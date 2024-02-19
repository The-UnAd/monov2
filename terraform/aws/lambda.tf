resource "random_password" "graph_monitor_api_key" {
  length  = 32
  special = false
}

output "graph_monitor_api_key" {
  value = random_password.graph_monitor_api_key.result
}

data "aws_iam_policy_document" "lambda_assume_role_policy" {
  statement {
    actions = ["sts:AssumeRole"]

    principals {
      type        = "Service"
      identifiers = ["lambda.amazonaws.com"]
    }
  }
}

resource "aws_iam_role" "lambda_role" {
  name               = "lambda_role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume_role_policy.json
}

resource "aws_iam_role_policy_attachment" "lambda_execution_attach" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
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

resource "aws_cloudwatch_log_group" "authorizer_logs" {
  name              = "/aws/lambda/${aws_lambda_function.graph_monitor_authorizer.function_name}"
  retention_in_days = 14
}

resource "aws_cloudwatch_log_group" "get_logs" {
  name              = "/aws/lambda/${aws_lambda_function.graph_monitor_get.function_name}"
  retention_in_days = 14
}

resource "aws_cloudwatch_log_group" "post_logs" {
  name              = "/aws/lambda/${aws_lambda_function.graph_monitor_post.function_name}"
  retention_in_days = 14
}

resource "aws_lambda_function" "graph_monitor_authorizer" {
  function_name = "graph-monitor-authorizer"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "GraphMonitor::GraphMonitor.Authorizer_Authorize_Generated::Authorize"
  role          = aws_iam_role.lambda_role.arn
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

  tracing_config {
    mode = "Active"
  }

  provisioner "local-exec" {
    command     = "deploy-graph-monitor-authorizer.ps1"
    working_dir = "../serverless/GraphMonitor/GraphMonitor"
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
  role          = aws_iam_role.lambda_role.arn
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
    working_dir = "../serverless/GraphMonitor/GraphMonitor"
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
  role          = aws_iam_role.lambda_role.arn
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
    working_dir = "../serverless/GraphMonitor/GraphMonitor"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_apigatewayv2_domain_name" "graph_monitor_domain" {
  domain_name = "monitor.${data.aws_route53_zone.main.name}"
  domain_name_configuration {
    certificate_arn = aws_acm_certificate.wildcard.arn
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_api_mapping" "api_mapping" {
  api_id      = aws_apigatewayv2_api.graph_monitor_api.id
  domain_name = aws_apigatewayv2_domain_name.graph_monitor_domain.id
  stage       = aws_apigatewayv2_stage.default_stage.name
}

resource "aws_route53_record" "graph_monitor" {
  name    = aws_apigatewayv2_domain_name.graph_monitor_domain.domain_name
  type    = "A"
  zone_id = data.aws_route53_zone.main.zone_id

  alias {
    name                   = aws_apigatewayv2_domain_name.graph_monitor_domain.domain_name_configuration[0].target_domain_name
    zone_id                = aws_apigatewayv2_domain_name.graph_monitor_domain.domain_name_configuration[0].hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_apigatewayv2_api" "graph_monitor_api" {
  name          = "graph-monitor-http-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_authorizer" "graph_monitor_authorizer" {
  name                              = "api-key-authorizer"
  api_id                            = aws_apigatewayv2_api.graph_monitor_api.id
  authorizer_type                   = "REQUEST"
  authorizer_uri                    = aws_lambda_function.graph_monitor_authorizer.invoke_arn
  authorizer_result_ttl_in_seconds  = 0
  authorizer_payload_format_version = "2.0"
  enable_simple_responses           = true
}

resource "aws_apigatewayv2_integration" "lambda_integration_post" {
  api_id           = aws_apigatewayv2_api.graph_monitor_api.id
  integration_type = "AWS_PROXY"

  connection_type        = "INTERNET"
  description            = "Lambda integration"
  integration_method     = "POST"
  integration_uri        = aws_lambda_function.graph_monitor_post.invoke_arn
  payload_format_version = "2.0"
}

resource "aws_apigatewayv2_integration" "lambda_integration_get" {
  api_id           = aws_apigatewayv2_api.graph_monitor_api.id
  integration_type = "AWS_PROXY"

  connection_type        = "INTERNET"
  description            = "Lambda integration"
  integration_method     = "POST"
  integration_uri        = aws_lambda_function.graph_monitor_get.invoke_arn
  payload_format_version = "2.0"

}

resource "aws_lambda_permission" "apigw_post" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_post.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.graph_monitor_api.execution_arn}/*/*"
}

resource "aws_lambda_permission" "execute" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.graph_monitor_authorizer.function_name
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
  name = "/aws/apigateway/${aws_apigatewayv2_api.graph_monitor_api.name}"
}

resource "aws_apigatewayv2_stage" "default_stage" {
  api_id      = aws_apigatewayv2_api.graph_monitor_api.id
  name        = "$default"
  auto_deploy = true
  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.api_logs.arn
    format          = "$context.identity.sourceIp - - [$context.requestTime] \"$context.httpMethod $context.routeKey $context.protocol\" $context.status $context.responseLength $context.requestId $context.error.message $context.integration.error $context.integrationErrorMessage $context.authorizer.error $context.integration.integrationStatus"
  }
}

output "graph_monitor_api_url" {
  value = aws_apigatewayv2_api.graph_monitor_api.api_endpoint
}
