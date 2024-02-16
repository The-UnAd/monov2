resource "aws_ecr_repository" "graph_monitor_ecr_repo" {
  name         = "unad/graph-monitor"
  force_delete = true
}

resource "random_password" "graph_monitor_api_key" {
  length  = 32
  special = false
}

output "graph_monitor_api_key" {
  value = random_password.graph_monitor_api_key.result
}

data "aws_iam_policy_document" "lambda_invoke" {
  statement {
    actions   = ["lambda:InvokeFunction"]
    resources = [aws_lambda_function.graph_monitor_authorizer.arn]
  }
}

resource "aws_iam_policy" "lambda_invoke_policy" {
  name        = "lambda_invoke_policy"
  description = "Policy to allow lambda to invoke other lambda functions"
  policy      = data.aws_iam_policy_document.lambda_invoke.json
}

resource "aws_iam_role" "lambda_execution_role" {
  name = "lambda_execution_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}


resource "aws_iam_role" "api_role" {
  name = "api_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy" "lambda_vpc_access" {
  name = "lambda_vpc_access"
  role = aws_iam_role.lambda_execution_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow",
      Action = [
        "ec2:CreateNetworkInterface",
        "ec2:DescribeNetworkInterfaces",
        "ec2:DeleteNetworkInterface"
      ],
      Resource : "*"
    }]
  })
}

resource "aws_iam_role_policy" "lambda_ecr_access" {
  name = "lambda_ecr_access"
  role = aws_iam_role.lambda_execution_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow",
      Action = [
        "ecr:SetRepositoryPolicy",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage"
      ],
      Resource = "*"
    }]
  })
}

resource "aws_iam_role_policy" "lambda_invoke_authorizer" {
  name = "lambda_invoke_authorizer"
  role = aws_iam_role.api_role.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow",
      Action = [
        "apigateway:InvokeAuthorizer"
      ],
      Resource = "*"
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_execution_attach" {
  role       = aws_iam_role.lambda_execution_role.name
  policy_arn = aws_iam_policy.lambda_invoke_policy.arn
}

resource "aws_cloudwatch_log_group" "graph_monitor_log_group" {
  name              = "/aws/lambda/graph-monitor"
  retention_in_days = 7
}

resource "aws_lambda_permission" "graph_monitor_get_log_permission" {
  statement_id  = "AllowExecutionFromCloudWatch"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_get.function_name
  principal     = "logs.${var.region}.amazonaws.com"

  source_arn = aws_lambda_function.graph_monitor_get.arn
}

resource "aws_lambda_permission" "graph_monitor_post_log_permission" {
  statement_id  = "AllowExecutionFromCloudWatch"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_post.function_name
  principal     = "logs.${var.region}.amazonaws.com"

  source_arn = aws_lambda_function.graph_monitor_post.arn
}

resource "aws_iam_role_policy_attachment" "lambda_logs" {
  role       = aws_iam_role.lambda_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_security_group" "lambda_private" {
  name   = "lambda-private"
  vpc_id = aws_vpc.vpc.id

  tags = {
    Name = "lambda-private"
  }
}

resource "aws_security_group_rule" "lambda_egress_redis" {
  type              = "egress"
  from_port         = aws_elasticache_cluster.unad.port
  to_port           = aws_elasticache_cluster.unad.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.lambda_private.id
}


resource "random_id" "bucket_id" {
  byte_length = 8
}

resource "aws_s3_bucket" "lambda_bucket" {
  bucket = "unad-code-bucket-${random_id.bucket_id.hex}"
}

resource "aws_lambda_function" "graph_monitor_authorizer" {
  function_name = "graph-monitor-authorizer"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "GraphMonitor::GraphMonitor.Authorizer_Authorize_Generated::Authorize"
  role          = aws_iam_role.lambda_execution_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = aws_subnet.private_subnet.*.id
    security_group_ids = [aws_security_group.lambda_private.id]
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      API_KEY                = random_password.graph_monitor_api_key.result
    }
  }

  provisioner "local-exec" {
    command     = "deploy-graph-monitor-authorizer.ps1"
    working_dir = "../../serverless/GraphMonitor/GraphMonitor"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "graph_monitor_post" {
  function_name = "graph-monitor-post"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "GraphMonitor::GraphMonitor.StoreUrlFunction_StoreUrl_Generated::StoreUrl"
  role          = aws_iam_role.lambda_execution_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = aws_subnet.private_subnet.*.id
    security_group_ids = [aws_security_group.lambda_private.id]
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      REDIS_URL              = aws_ssm_parameter.redis_connection_string.value # TODO: if we need a password later, secure this
    }
  }

  provisioner "local-exec" {
    command     = "deploy-graph-monitor-authorizer.ps1"
    working_dir = "../../serverless/GraphMonitor/GraphMonitor"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "graph_monitor_get" {
  function_name = "graph-monitor-get"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "GraphMonitor::GraphMonitor.GetUrlFunction_GetUrl_Generated::GetUrl"
  role          = aws_iam_role.lambda_execution_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = aws_subnet.private_subnet.*.id
    security_group_ids = [aws_security_group.lambda_private.id]
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      REDIS_URL              = aws_ssm_parameter.redis_connection_string.value # TODO: if we need a password later, secure this
    }
  }

  provisioner "local-exec" {
    command     = "deploy-graph-monitor-authorizer.ps1"
    working_dir = "../../serverless/GraphMonitor/GraphMonitor"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

# resource "aws_apigatewayv2_domain_name" "graph_monitor_domain" {
#   domain_name      = "monitor.${var.dns_zone}"
#   domain_name_configuration {
#     certificate_arn = aws_acm_certificate.wildcard.arn
#     endpoint_type   = "REGIONAL"
#     security_policy = "TLS_1_2"
#   }
# }
# resource "aws_apigatewayv2_api_mapping" "api_mapping" {
#   api_id      = aws_apigatewayv2_api.graph_monitor_api.id
#   domain_name = aws_apigatewayv2_domain_name.graph_monitor_domain.id
#   stage       = "$default"
# }

resource "aws_apigatewayv2_api" "graph_monitor_api" {
  name          = "graph-monitor-http-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_authorizer" "graph_monitor_authorizer" {
  api_id                            = aws_apigatewayv2_api.graph_monitor_api.id
  authorizer_type                   = "REQUEST"
  authorizer_uri                    = aws_lambda_function.graph_monitor_authorizer.invoke_arn
  authorizer_result_ttl_in_seconds  = 300
  name                              = "api-key-authorizer"
  authorizer_payload_format_version = "2.0"
  enable_simple_responses           = true
  identity_sources                  = ["$request.header.Authorization"]
  authorizer_credentials_arn        = aws_iam_role.api_role.arn
}

resource "aws_apigatewayv2_integration" "lambda_integration_post" {
  api_id           = aws_apigatewayv2_api.graph_monitor_api.id
  integration_type = "AWS_PROXY"

  connection_type        = "INTERNET"
  description            = "Lambda integration"
  integration_method     = "POST"
  integration_uri        = aws_lambda_function.graph_monitor_post.invoke_arn
  payload_format_version = "2.0"
  credentials_arn        = aws_iam_role.api_role.arn
}

resource "aws_apigatewayv2_integration" "lambda_integration_get" {
  api_id           = aws_apigatewayv2_api.graph_monitor_api.id
  integration_type = "AWS_PROXY"

  connection_type        = "INTERNET"
  description            = "Lambda integration"
  integration_method     = "POST"
  integration_uri        = aws_lambda_function.graph_monitor_get.invoke_arn
  payload_format_version = "2.0"
  credentials_arn        = aws_iam_role.api_role.arn

}

resource "aws_lambda_permission" "apigw_post" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_post.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.graph_monitor_api.execution_arn}/*/*"
}

resource "aws_lambda_permission" "apigw_get" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_get.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.graph_monitor_api.execution_arn}/*/*"
}

resource "aws_apigatewayv2_route" "post_route" {
  api_id             = aws_apigatewayv2_api.graph_monitor_api.id
  route_key          = "POST /{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.lambda_integration_post.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.graph_monitor_authorizer.id
}

resource "aws_apigatewayv2_route" "get_route" {
  api_id             = aws_apigatewayv2_api.graph_monitor_api.id
  route_key          = "ANY /{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.lambda_integration_get.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.graph_monitor_authorizer.id
}

resource "aws_cloudwatch_log_group" "api_logs" {
  name = "/aws/apigateway/graph-monitor-http-api"
}

resource "aws_apigatewayv2_stage" "default_stage" {
  api_id      = aws_apigatewayv2_api.graph_monitor_api.id
  name        = "$default"
  auto_deploy = true
  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.api_logs.arn
    format          = "$context.identity.sourceIp - - [$context.requestTime] \"$context.httpMethod $context.routeKey $context.protocol\" $context.status $context.responseLength $context.requestId $context.error.message $context.authorizer.error"
  }
}

output "graph_monitor_api_url" {
  value = aws_apigatewayv2_api.graph_monitor_api.api_endpoint
}
