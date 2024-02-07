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
  name = "unad-cluster"

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
  from_port         = aws_memorydb_cluster.unad.port
  to_port           = aws_memorydb_cluster.unad.port
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

# resource "aws_security_group_rule" "ecs_egress_rds" {
#   type              = "egress"
#   from_port         = aws_rds_cluster.aurora.port
#   to_port           = aws_rds_cluster.aurora.port
#   protocol          = "tcp"
#   cidr_blocks       = [var.vpc_cidr]
#   security_group_id = aws_security_group.ecs_private.id
# }

resource "aws_security_group_rule" "ecs_egress_msk" {
  type              = "egress"
  from_port         = 9092
  to_port           = 9098
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.ecs_private.id
}

data "aws_secretsmanager_secret" "twilio_secrets" {
  name = "/unad/global/twilio"
}

data "aws_secretsmanager_secret" "stripe_secrets" {
  name = "/unad/global/stripe"
}

module "signup-site" {
  source                     = "./ecs"
  region                     = data.aws_region.current.name
  project_name               = "signup-site"
  container_port             = 3000
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
  ssl_certificate_arn        = aws_acm_certificate_validation.wildcard.certificate_arn
  container_secrets = [{
    name      = "REDIS_KEY"
    valueFrom = "${aws_ssm_parameter.redis_password.arn}"
    }, {
    name      = "TWILIO_ACCOUNT_SID"
    valueFrom = "${data.aws_ssm_parameter.twilio_account_sid.arn}"
    }, {
    name      = "TWILIO_AUTH_TOKEN"
    valueFrom = "${data.aws_ssm_parameter.twilio_auth_token.arn}"
    }, {
    name      = "STRIPE_API_KEY"
    valueFrom = "${data.aws_ssm_parameter.stripe_api_key.arn}"
    }, {
    name      = "NEXT_PUBLIC_STRIPE_PUBLIC_KEY"
    valueFrom = "${data.aws_ssm_parameter.stripe_publishable_key.arn}"
    }, {
    name      = "JWT_PRIVATE_KEY"
    valueFrom = "${aws_ssm_parameter.jwt_private_key.arn}"
    }
  ]
  container_environment = [{
    name  = "NEXT_PUBLIC_JWT_PUBLIC_KEY"
    value = "${tls_private_key.jwt_key.public_key_openssh}"
    }, {
    name  = "TWILIO_MESSAGE_SERVICE_SID"
    value = "MG286e5e74c953d5720fbc002c41a2bdd6"
    }, {
    name  = "NEXT_PUBLIC_SESSION_LENGTH"
    value = "86400"
    }, {
    name  = "SESSION_LENGTH"
    value = "86400"
    }, {
    name  = "OTP_WINDOW"
    value = "10"
    }, {
    name  = "OTP_STEP"
    value = "1000"
    }, {
    name  = "SHARE_HOST"
    value = "https://${aws_route53_record.signup-site.name}"
    }, {
    name  = "SUBSCRIBE_HOST"
    value = "https://${aws_route53_record.signup-site.name}/subscribe"
    }, {
    name  = "SITE_HOST"
    value = "https://${aws_route53_record.signup-site.name}"
    }, {
    name  = "STRIPE_PORTAL_HOST"
    value = "https://pay.theunad.com/p/login/test_9AQ8Ag7pwgGg0c84gg"
    }, {
    name  = "STRIPE_PRODUCT_BASIC_PRICING_TABLE"
    value = "prctbl_1N76h5E8A2efFCQS9I8E5lvT"
    }, {
    name  = "PORT"
    value = "3000"
    }, {
    name  = "REDIS_USER"
    value = "${aws_memorydb_user.unad_redis_user.user_name}"
    }, {
    name  = "REDIS_USE_TLS",
    value = "true"
    }, {
    name  = "REDIS_CLUSTER_NODES"
    value = "${join(",", [for node in local.redis_nodes : "${node.address}:${node.port}"])}"
  }]
}

resource "aws_route53_record" "signup-site" {
  allow_overwrite = true
  name            = "signup.${aws_route53_zone.main.name}"
  records         = [module.signup-site.load_balancer_dns_name]
  ttl             = 60
  type            = "CNAME"
  zone_id         = aws_route53_zone.main.zone_id
}

output "signup_site_dns" {
  value = module.signup-site.load_balancer_dns_name

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



