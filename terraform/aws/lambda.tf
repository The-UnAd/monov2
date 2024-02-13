resource "aws_ecr_repository" "graph_monitor_ecr_repo" {
  name         = "unad/graph-monitor"
  force_delete = true
}

resource "random_password" "graph_montitor_api_key" {
  length  = 32
  special = false
}

output "graph_montitor_api_key" {
  value = random_password.graph_montitor_api_key.result
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

resource "aws_lambda_function" "graph_monitor" {
  function_name = "graph-monitor"
  package_type  = "Image"
  image_uri     = "${aws_ecr_repository.graph_monitor_ecr_repo.repository_url}:latest"
  role          = aws_iam_role.lambda_execution_role.arn
  timeout       = 60

  image_config {
    entry_point = [
      "GraphMonitor::GraphMonitor.StoreUrlFunction_StoreUrl_Generated::StoreUrl",
      "GraphMonitor::GraphMonitor.GetUrlFunction_GetUrl_Generated::GetUrl"
    ]
  }
}

# Optionally, create an API Gateway to trigger the Lambda
resource "aws_apigatewayv2_api" "graph_monitor_api" {
  name          = "graph-monitor-http-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id           = aws_apigatewayv2_api.graph_monitor_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.graph_monitor.arn
}

resource "aws_apigatewayv2_route" "default_route" {
  api_id    = aws_apigatewayv2_api.graph_monitor_api.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_apigatewayv2_stage" "default_stage" {
  api_id      = aws_apigatewayv2_api.graph_monitor_api.id
  name        = "$default"
  auto_deploy = true
}

output "http_api_url" {
  value = aws_apigatewayv2_api.graph_monitor_api.api_endpoint
}
