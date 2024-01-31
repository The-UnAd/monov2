####################
# ECS
# This module specifies all the required resources to run a containerized application on ECS.
# It creates a cluster, a task definition, a service, and a load balancer.
# It also creates a CloudWatch log group and a CloudWatch log stream for the service.
# It optionally creates a service discovery namespace and a service discovery service.
# It optionally creates a service connect namespace and a service connect service.
####################

resource "aws_service_discovery_private_dns_namespace" "this" {
  name = "unad.local"
  vpc  = aws_vpc.vpc.id
}

resource "aws_ecs_cluster" "cluster" {
  name = "UnAd-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }
  service_connect_defaults {
    namespace = aws_service_discovery_private_dns_namespace.this.arn
  }
}

resource "aws_iam_role" "ecs_task_execution_role" {
  name = "ecsTaskExecutionRole"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_policy" "ecr_policy" {
  name = "ecr_policy"
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetAuthorizationToken",
          "logs:CreateLogStream",
          "logs:PutLogEvents",
          "ecr:BatchCheckLayerAvailability",
          "ecr:GetDownloadUrlForLayer",
          "ecr:GetRepositoryPolicy",
          "ecr:DescribeRepositories",
          "ecr:ListImages",
          "ecr:DescribeImages",
          "ecr:BatchGetImage",
          "ssm:GetParameters",
          "secretsmanager:GetSecretValue",
          "kms:Decrypt",
          "cognito-idp:AdminGetUser" # for cognito user pool.  This needs to be more granular in the future
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_ecr_policy" {
  policy_arn = aws_iam_policy.ecr_policy.arn
  role       = aws_iam_role.ecs_task_execution_role.name
}

resource "aws_security_group" "ecs_private" {
  name   = "ecs-private"
  vpc_id = aws_vpc.vpc.id

  tags = {
    Name = "ecs-private"
  }
}

resource "aws_security_group_rule" "ecs_egress_redis" {
  type              = "egress"
  from_port         = aws_memorydb_cluster.UnAd.port
  to_port           = aws_memorydb_cluster.UnAd.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_security_group_rule" "ecs_egress_tls" {
  # we need this for the ECS service to be able to access SSM and Secrets Manager
  type              = "egress"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_security_group_rule" "ecs_egress_http" {
  # we need this for the ECS service to be able to access other ECS services (Gateways to GraphQL APIs, etc.)
  type              = "egress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_security_group_rule" "ecs_ingress_http" {
  # we need this for the ECS service to be able to access SSM and Secrets Manager
  type              = "ingress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_security_group_rule" "ecs_egress_rds" {
  type              = "egress"
  from_port         = aws_rds_cluster.aurora.port
  to_port           = aws_rds_cluster.aurora.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_security_group_rule" "ecs_egress_msk" {
  type              = "egress"
  from_port         = 9092
  to_port           = 9098
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

resource "aws_acm_certificate" "wildcard" {
  domain_name       = "*.${var.dns_zone}"
  validation_method = "DNS"

  lifecycle {
    create_before_destroy = true
  }
}

module "signup-site" {
  source                     = "./ecs"
  region                     = data.aws_region.current.name
  project_name               = "signup-site"
  task_role_arn              = aws_iam_role.ecs_task_execution_role.arn
  execution_role_arn         = aws_iam_role.ecs_task_execution_role.arn
  public_subnet_ids          = aws_subnet.public_subnet.*.id
  vpc_id                     = aws_vpc.vpc.id
  vpc_cidr                   = var.vpc_cidr
  private_subnet_ids         = aws_subnet.private_subnet.*.id
  service_security_group_ids = [aws_security_group.ecs_private.id]
  desired_count              = 1
  cluster_arn                = aws_ecs_cluster.cluster.arn
  health_check_path          = "/health"
  task_cpu                   = 256
  task_memory                = 512
  ssl_certificate_arn        = aws_acm_certificate.wildcard.arn
  container_secrets = [{
    name      = "DB_PASSWORD"
    valueFrom = "${aws_ssm_parameter.rds_cluster_password.arn}"
    }, {
    name      = "DB_USER"
    valueFrom = "${aws_ssm_parameter.rds_cluster_user.arn}"
    }, {
    name      = "DB_PORT"
    valueFrom = "${aws_ssm_parameter.rds_cluster_db_port.arn}"
    }, {
    name      = "REDIS_KEY"
    valueFrom = "${aws_ssm_parameter.redis_password.arn}"
    }
  ]
  container_environment = [{
    name  = "NODE_ENV"
    value = "production"
    }, {
    name  = "PORT"
    value = "80"
    }, {
    name  = "REDIS_USER"
    value = "${aws_memorydb_user.UnAd_redis_user.user_name}"
    }, {
    name  = "REDIS_CLUSTER_NODES"
    value = "${join(",", [for node in local.redis_nodes : "${node.address}:${node.port}"])}"
  }]
}

# module "graphql-gateway" {
#   source                       = "./ecs"
#   region                       = data.aws_region.current.name
#   project_name                 = "graphql-gateway"
#   task_role_arn                = aws_iam_role.ecs_task_execution_role.arn
#   execution_role_arn           = aws_iam_role.ecs_task_execution_role.arn
#   public_subnet_ids            = aws_subnet.public_subnet.*.id
#   vpc_id                       = aws_vpc.vpc.id
#   vpc_cidr                     = var.vpc_cidr
#   private_subnet_ids           = aws_subnet.private_subnet.*.id
#   service_security_group_ids   = [aws_security_group.ecs_private.id]
#   desired_count                = 1
#   cluster_arn                  = aws_ecs_cluster.cluster.arn
#   task_cpu                     = 256
#   task_memory                  = 512
#   health_check_path            = "/health"
#   enable_service_connect       = true
#   service_connect_namespace    = aws_service_discovery_private_dns_namespace.this.arn
#   service_connect_namespace_id = aws_service_discovery_private_dns_namespace.this.id
#   ssl_certificate_arn          = aws_acm_certificate.wildcard.arn
#   container_secrets = [
#     # {
#     #   name      = "COGNITO_ENDPOINT"
#     #   valueFrom = "${aws_ssm_parameter.cognito_pool_endpoint.arn}"
#     # },
#     {
#       name      = "REDIS_URL"
#       valueFrom = "${aws_ssm_parameter.redis_connection_string.arn}"
#     }
#   ]
#   container_environment = [{
#     name  = "ASPNETCORE_ENVIRONMENT"
#     value = "Production"
#     }, {
#     name  = "ASPNETCORE_URLS"
#     value = "http://+:80"
#   }]
# }

# module "user-api" {
#   source                       = "./ecs"
#   region                       = data.aws_region.current.name
#   project_name                 = "user-api"
#   task_role_arn                = aws_iam_role.ecs_task_execution_role.arn
#   execution_role_arn           = aws_iam_role.ecs_task_execution_role.arn
#   vpc_id                       = aws_vpc.vpc.id
#   vpc_cidr                     = var.vpc_cidr
#   private_subnet_ids           = aws_subnet.private_subnet.*.id
#   service_security_group_ids   = [aws_security_group.ecs_private.id]
#   desired_count                = 1
#   cluster_arn                  = aws_ecs_cluster.cluster.arn
#   task_cpu                     = 256
#   task_memory                  = 512
#   health_check_path            = "/health"
#   enable_service_connect       = true
#   service_connect_namespace    = aws_service_discovery_private_dns_namespace.this.arn
#   service_connect_namespace_id = aws_service_discovery_private_dns_namespace.this.id
#   container_secrets = [
#     {
#       name      = "DB_CONNECTIONSTRING"
#       valueFrom = "${aws_ssm_parameter.rds_cluster_db_connection_string.arn}"
#     },
#     {
#       name      = "COGNITO_ENDPOINT"
#       valueFrom = "${aws_ssm_parameter.cognito_pool_endpoint.arn}"
#     },
#     {
#       name      = "REDIS_URL"
#       valueFrom = "${aws_ssm_parameter.redis_connection_string.arn}"
#     }
#   ]
#   container_environment = [{
#     name  = "ASPNETCORE_ENVIRONMENT"
#     value = "Production"
#     }, {
#     name  = "ASPNETCORE_URLS"
#     value = "http://+:80"
#   }, {
#     name  = "DB_NAME"
#     value = "userdb"
#   }]
# }

# module "graph-monitor" {
#   source                     = "./ecs"
#   region                     = data.aws_region.current.name
#   project_name               = "graph-monitor"
#   task_role_arn              = aws_iam_role.ecs_task_execution_role.arn
#   execution_role_arn         = aws_iam_role.ecs_task_execution_role.arn
#   public_subnet_ids          = aws_subnet.public_subnet.*.id
#   vpc_id                     = aws_vpc.vpc.id
#   vpc_cidr                   = var.vpc_cidr
#   private_subnet_ids         = aws_subnet.private_subnet.*.id
#   service_security_group_ids = [aws_security_group.ecs_private.id]
#   desired_count              = 1
#   cluster_arn                = aws_ecs_cluster.cluster.arn
#   task_cpu                   = 256
#   task_memory                = 512
#   health_check_path          = "/health"
#   enable_service_connect     = false
#   ssl_certificate_arn        = aws_acm_certificate.wildcard.arn
#   container_secrets = [
#     {
#       name      = "REDIS_URL"
#       valueFrom = "${aws_ssm_parameter.redis_connection_string.arn}"
#     }
#   ]
#   container_environment = [{
#     name  = "ASPNETCORE_ENVIRONMENT"
#     value = "Production"
#     }, {
#     name  = "ASPNETCORE_URLS"
#     value = "http://+:80"
#     }, {
#     name  = "ApiKeyAuthenticationOptions:ApiKey"
#     value = "${random_password.graph_montitor_api_key.result}"
#   }]
# }

# resource "random_password" "graph_montitor_api_key" {
#   length  = 32
#   special = false
# }

# output "graph_montitor_api_key" {
#   value = random_password.graph_montitor_api_key.result
# }



