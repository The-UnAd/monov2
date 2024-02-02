data "aws_region" "current" {}

resource "aws_ecr_repository" "this" {
  name         = "unad/${var.project_name}"
  force_delete = true
}

resource "aws_cloudwatch_log_group" "this_log_group" {
  name = "/ecs/${var.project_name}"

  retention_in_days = 7
}

resource "aws_ecs_task_definition" "this_task" {
  family                   = var.project_name
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = var.task_cpu
  memory                   = var.task_memory
  task_role_arn            = var.task_role_arn
  execution_role_arn       = var.execution_role_arn

  container_definitions = jsonencode([
    {
      name  = "${var.project_name}"
      image = "${aws_ecr_repository.this.repository_url}:latest"
      portMappings = [
        {
          containerPort = "${var.container_port}"
          protocol      = "tcp"
          name          = "${var.project_name}"
        }
      ]
      healthCheck = {
        command     = ["CMD-SHELL", "timeout 5s bash -c ':> /dev/tcp/127.0.0.1/${var.container_port}' || exit 1"]
        interval    = 30
        timeout     = 5
        startPeriod = 30
        retries     = 3
      }
      # TODO: Add depends_on for other services
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-region        = "${data.aws_region.current.name}",
          awslogs-group         = "${aws_cloudwatch_log_group.this_log_group.name}",
          awslogs-create-group  = "true",
          awslogs-stream-prefix = "ecs"
        }
      }
      environment = var.container_environment
      secrets     = var.container_secrets
    }
  ])
}

# TODO: add autoscaling

resource "aws_ecs_service" "this" {
  name            = var.project_name
  launch_type     = "FARGATE"
  cluster         = var.cluster_arn
  task_definition = aws_ecs_task_definition.this_task.arn
  desired_count   = 1

  force_new_deployment = true # set to true for debugging

  # triggers = {
  #   redeployment = timestamp()
  # }

  network_configuration {
    subnets          = var.private_subnet_ids
    security_groups  = var.service_security_group_ids
    assign_public_ip = false
  }

  dynamic "load_balancer" {
    for_each = length(var.public_subnet_ids) > 0 ? [1] : []
    content {
      target_group_arn = aws_lb_target_group.this_target_group[0].arn
      container_name   = var.project_name
      container_port   = var.container_port
    }
  }

  deployment_controller {
    type = "ECS"
  }

  dynamic "service_connect_configuration" {
    for_each = var.enable_service_connect ? [1] : []
    content {
      enabled   = var.enable_service_connect
      namespace = var.service_connect_namespace
      log_configuration {
        log_driver = "awslogs"
        options = {
          awslogs-region        = "${data.aws_region.current.name}"
          awslogs-group         = "${aws_cloudwatch_log_group.this_log_group.name}"
          awslogs-create-group  = "true"
          awslogs-stream-prefix = "service_connect"
        }
      }
    }
  }

  dynamic "service_registries" {
    for_each = var.enable_service_connect ? [1] : []
    content {
      registry_arn   = aws_service_discovery_service.this[0].arn
      container_name = var.project_name
    }
  }

  lifecycle {
    ignore_changes = [
      task_definition
    ]
  }
}

resource "aws_service_discovery_service" "this" {
  count        = var.enable_service_connect ? 1 : 0
  name         = var.project_name
  namespace_id = var.service_connect_namespace_id

  dynamic "health_check_custom_config" {
    for_each = length(var.health_check_path) > 0 ? [1] : []
    content {
      failure_threshold = 1
    }
  }
  dns_config {
    namespace_id = var.service_connect_namespace_id
    dns_records {
      ttl  = 10
      type = "A"
    }
    routing_policy = "MULTIVALUE"
  }
}

resource "aws_lb" "this_lb" {
  count              = length(var.public_subnet_ids) > 0 ? 1 : 0
  name               = "${var.project_name}-lb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.this[count.index].id]
  subnets            = var.public_subnet_ids
  tags = {
    Name = "${var.project_name}-lb"
  }
  enable_deletion_protection = false # TODO: turn on for production
}

resource "aws_lb_listener" "this_listener" {
  count             = length(var.public_subnet_ids) > 0 ? 1 : 0
  load_balancer_arn = aws_lb.this_lb[count.index].arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-2016-08"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.this_target_group[count.index].arn
  }
  
  certificate_arn = var.ssl_certificate_arn

  lifecycle {
    replace_triggered_by = [aws_lb_target_group.this_target_group[count.index].id]
  }
}

resource "aws_lb_target_group" "this_target_group" {
  count       = length(var.public_subnet_ids) > 0 ? 1 : 0
  name        = var.project_name
  port        = var.container_port
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "ip"

  dynamic "health_check" {
    for_each = var.health_check_path != null ? [1] : []
    content {
      path = var.health_check_path
      port = var.container_port
    }
  }
}
resource "aws_security_group" "this" {
  count  = length(var.public_subnet_ids) > 0 ? 1 : 0
  name   = "${var.project_name}-lb"
  vpc_id = var.vpc_id

  tags = {
    Name = "${var.project_name}-lb"
  }
}

resource "aws_security_group_rule" "this_egress_http" {
  count             = length(var.public_subnet_ids) > 0 ? 1 : 0
  type              = "egress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.this[count.index].id
  description       = "Allow inbound TLS traffic"
}

resource "aws_security_group_rule" "this_egress_tls" {
  count             = length(var.public_subnet_ids) > 0 ? 1 : 0
  type              = "egress"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.this[count.index].id
  description       = "Allow outbound TLS traffic"
}

resource "aws_security_group_rule" "this_ingress_tls" {
  count             = length(var.public_subnet_ids) > 0 ? 1 : 0
  type              = "ingress"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.this[count.index].id
  description       = "Allow TLS inbound traffic"
}

resource "aws_security_group_rule" "this_ingress_http" {
  count             = length(var.public_subnet_ids) > 0 ? 1 : 0
  type              = "ingress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.this[count.index].id
  description       = "Allow HTTP inbound traffic"
}

output "load_balancer_dns_name" {
  value = join("", aws_lb.this_lb.*.dns_name)
}



