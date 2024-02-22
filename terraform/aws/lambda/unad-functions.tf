resource "random_password" "unad_functions_api_key" {
  length  = 32
  special = false
}

output "unad_functions_api_key" {
  value = random_password.unad_functions_api_key.result
}


resource "aws_lambda_permission" "unad_functions_product_log_permission" {
  statement_id  = "AllowExecutionFromCloudWatch"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.unad_functions_product.function_name
  principal     = "logs.${var.region}.amazonaws.com"

  source_arn = aws_lambda_function.unad_functions_product.arn
}

resource "aws_cloudwatch_log_group" "unad_functions_authorizer_logs" {
  name              = "/aws/lambda/${aws_lambda_function.unad_functions_authorizer.function_name}"
  retention_in_days = 14
}

resource "aws_cloudwatch_log_group" "unad_functions_product_logs" {
  name              = "/aws/lambda/${aws_lambda_function.unad_functions_product.function_name}"
  retention_in_days = 14
}

resource "aws_lambda_function" "unad_functions_authorizer" {
  function_name = "unad-functions-authorizer"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "UnAd.Functions::UnAd.Functions.Authorizer_Authorize_Generated::Authorize"
  role          = aws_iam_role.lambda_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = var.security_group_ids
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT = "Production"
      API_KEY                = random_password.unad_functions_api_key.result
    }
  }

  tracing_config {
    mode = "Active"
  }

  provisioner "local-exec" {
    command     = "deploy-unad-functions-authorizer.ps1"
    working_dir = "../serverless/UnAd.Functions"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "unad_functions_product" {
  function_name = "unad-functions-product"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "UnAd.Functions::UnAd.Functions.StripeProductWebhook_Run_Generated::Run"
  role          = aws_iam_role.lambda_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = var.security_group_ids
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT              = "Production"
      REDIS_URL                           = "${var.redis_connection_string}"
      STRIPE_API_KEY                      = "sk_test_51MtKShE8A2efFCQSK8cxjP720Ya7fl0JFpvrPc1pUR1dqiOEhuOjC07cn9YLNBxPH38a1vZLMkGGhuApBQr90E3J00aqS8IsGu",
      STRIPE_SUBSCRIPTION_ENDPOINT_SECRET = "whsec_SzXQF1grYiTX4sbEWLlEGshdVlrf9NW5",
      STRIPE_PRODUCT_ENDPOINT_SECRET      = "whsec_hqJQbx0JyKKObi4Zl3oJM8Obx1zwSi78",
      STRIPE_PAYMENT_ENDPOINT_SECRET      = "whsec_oj97MjKzsm0ULffxkIOwEvejS0fob3fD",
      TWILIO_ACCOUNT_SID                  = "ACa47f561109b02f76ad0d06e6c409ed37",
      TWILIO_AUTH_TOKEN                   = "3fabad71f99120b1dbf36eefc324ce42",
      TWILIO_MESSAGE_SERVICE_SID          = "MG286e5e74c953d5720fbc002c41a2bdd6",
      ClientLinkBaseUri                   = "http://signup.unad.dev/subscribe",
      SMS_LINK_BASE_URL                   = "http://signup.unad.dev/announcement",
      StripePortalUrl                     = "https://pay.theunad.com/p/login/test_9AQ8Ag7pwgGg0c84gg",
      AccountUrl                          = "http://signup.unad.dev/account",
      MIXPANEL_TOKEN                      = "bc58f676986b86540e39ead3274931e8"
    }
  }

  provisioner "local-exec" {
    command     = "deploy-unad-functions-product.ps1"
    working_dir = "../serverless/UnAd.Functions"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "unad_functions_payment" {
  function_name = "unad-functions-payment"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "UnAd.Functions::UnAd.Functions.StripePaymentWebhook_Run_Generated::Run"
  role          = aws_iam_role.lambda_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = var.security_group_ids
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT              = "Production"
      REDIS_URL                           = "${var.redis_connection_string}"
      STRIPE_API_KEY                      = "sk_test_51MtKShE8A2efFCQSK8cxjP720Ya7fl0JFpvrPc1pUR1dqiOEhuOjC07cn9YLNBxPH38a1vZLMkGGhuApBQr90E3J00aqS8IsGu",
      STRIPE_SUBSCRIPTION_ENDPOINT_SECRET = "whsec_SzXQF1grYiTX4sbEWLlEGshdVlrf9NW5",
      STRIPE_PRODUCT_ENDPOINT_SECRET      = "whsec_hqJQbx0JyKKObi4Zl3oJM8Obx1zwSi78",
      STRIPE_PAYMENT_ENDPOINT_SECRET      = "whsec_oj97MjKzsm0ULffxkIOwEvejS0fob3fD",
      TWILIO_ACCOUNT_SID                  = "ACa47f561109b02f76ad0d06e6c409ed37",
      TWILIO_AUTH_TOKEN                   = "3fabad71f99120b1dbf36eefc324ce42",
      TWILIO_MESSAGE_SERVICE_SID          = "MG286e5e74c953d5720fbc002c41a2bdd6",
      ClientLinkBaseUri                   = "http://signup.unad.dev/subscribe",
      SMS_LINK_BASE_URL                   = "http://signup.unad.dev/announcement",
      StripePortalUrl                     = "https://pay.theunad.com/p/login/test_9AQ8Ag7pwgGg0c84gg",
      AccountUrl                          = "http://signup.unad.dev/account",
      MIXPANEL_TOKEN                      = "bc58f676986b86540e39ead3274931e8"
    }
  }

  provisioner "local-exec" {
    command     = "deploy-unad-functions-payment.ps1"
    working_dir = "../serverless/UnAd.Functions"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "unad_functions_messages" {
  function_name = "unad-functions-messages"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "UnAd.Functions::UnAd.Functions.MessageHandler_Run_Generated::Run"
  role          = aws_iam_role.lambda_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = var.security_group_ids
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT              = "Production"
      REDIS_URL                           = "${var.redis_connection_string}"
      STRIPE_API_KEY                      = "sk_test_51MtKShE8A2efFCQSK8cxjP720Ya7fl0JFpvrPc1pUR1dqiOEhuOjC07cn9YLNBxPH38a1vZLMkGGhuApBQr90E3J00aqS8IsGu",
      STRIPE_SUBSCRIPTION_ENDPOINT_SECRET = "whsec_SzXQF1grYiTX4sbEWLlEGshdVlrf9NW5",
      STRIPE_PRODUCT_ENDPOINT_SECRET      = "whsec_hqJQbx0JyKKObi4Zl3oJM8Obx1zwSi78",
      STRIPE_PAYMENT_ENDPOINT_SECRET      = "whsec_oj97MjKzsm0ULffxkIOwEvejS0fob3fD",
      TWILIO_ACCOUNT_SID                  = "ACa47f561109b02f76ad0d06e6c409ed37",
      TWILIO_AUTH_TOKEN                   = "3fabad71f99120b1dbf36eefc324ce42",
      TWILIO_MESSAGE_SERVICE_SID          = "MG286e5e74c953d5720fbc002c41a2bdd6",
      ClientLinkBaseUri                   = "http://signup.unad.dev/subscribe",
      SMS_LINK_BASE_URL                   = "http://signup.unad.dev/announcement",
      StripePortalUrl                     = "https://pay.theunad.com/p/login/test_9AQ8Ag7pwgGg0c84gg",
      AccountUrl                          = "http://signup.unad.dev/account",
      MIXPANEL_TOKEN                      = "bc58f676986b86540e39ead3274931e8"
    }
  }

  provisioner "local-exec" {
    command     = "deploy-unad-functions-messages.ps1"
    working_dir = "../serverless/UnAd.Functions"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_lambda_function" "unad_functions_subscription" {
  function_name = "unad-functions-subscription"
  package_type  = "Zip"
  s3_bucket     = aws_s3_bucket.lambda_bucket.bucket
  s3_key        = "GraphMonitor.zip"
  runtime       = "provided.al2023"
  handler       = "UnAd.Functions::UnAd.Functions.StripeSubscriptionWebhook_Run_Generated::Run"
  role          = aws_iam_role.lambda_role.arn
  timeout       = 60
  memory_size   = 256

  vpc_config {
    subnet_ids         = var.subnet_ids
    security_group_ids = var.security_group_ids
  }

  environment {
    variables = {
      ASPNETCORE_ENVIRONMENT              = "Production"
      REDIS_URL                           = "${var.redis_connection_string}"
      STRIPE_API_KEY                      = "sk_test_51MtKShE8A2efFCQSK8cxjP720Ya7fl0JFpvrPc1pUR1dqiOEhuOjC07cn9YLNBxPH38a1vZLMkGGhuApBQr90E3J00aqS8IsGu",
      STRIPE_SUBSCRIPTION_ENDPOINT_SECRET = "whsec_SzXQF1grYiTX4sbEWLlEGshdVlrf9NW5",
      STRIPE_PRODUCT_ENDPOINT_SECRET      = "whsec_hqJQbx0JyKKObi4Zl3oJM8Obx1zwSi78",
      STRIPE_PAYMENT_ENDPOINT_SECRET      = "whsec_oj97MjKzsm0ULffxkIOwEvejS0fob3fD",
      TWILIO_ACCOUNT_SID                  = "ACa47f561109b02f76ad0d06e6c409ed37",
      TWILIO_AUTH_TOKEN                   = "3fabad71f99120b1dbf36eefc324ce42",
      TWILIO_MESSAGE_SERVICE_SID          = "MG286e5e74c953d5720fbc002c41a2bdd6",
      ClientLinkBaseUri                   = "http://signup.unad.dev/subscribe",
      SMS_LINK_BASE_URL                   = "http://signup.unad.dev/announcement",
      StripePortalUrl                     = "https://pay.theunad.com/p/login/test_9AQ8Ag7pwgGg0c84gg",
      AccountUrl                          = "http://signup.unad.dev/account",
      MIXPANEL_TOKEN                      = "bc58f676986b86540e39ead3274931e8"
    }
  }

  provisioner "local-exec" {
    command     = "deploy-unad-functions-subscription.ps1"
    working_dir = "../serverless/UnAd.Functions"
    interpreter = ["pwsh"]
    when        = create
    on_failure  = fail
    quiet       = false
  }
}

resource "aws_apigatewayv2_domain_name" "unad_functions_domain" {
  domain_name = "funcs.${var.domain_name}"
  domain_name_configuration {
    certificate_arn = var.certificate_arn
    endpoint_type   = "REGIONAL"
    security_policy = "TLS_1_2"
  }
}

resource "aws_apigatewayv2_api_mapping" "unad_functions_api_mapping" {
  api_id      = aws_apigatewayv2_api.unad_functions_api.id
  domain_name = aws_apigatewayv2_domain_name.unad_functions_domain.id
  stage       = aws_apigatewayv2_stage.default_stage.name
}

resource "aws_route53_record" "unad_functions" {
  name    = aws_apigatewayv2_domain_name.unad_functions_domain.domain_name
  type    = "A"
  zone_id = var.zone_id

  alias {
    name                   = aws_apigatewayv2_domain_name.unad_functions_domain.domain_name_configuration[0].target_domain_name
    zone_id                = aws_apigatewayv2_domain_name.unad_functions_domain.domain_name_configuration[0].hosted_zone_id
    evaluate_target_health = false
  }
}

resource "aws_apigatewayv2_api" "unad_functions_api" {
  name          = "unad-functions-http-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_authorizer" "unad_functions_authorizer" {
  name                              = "unad-functions-api-key-authorizer"
  api_id                            = aws_apigatewayv2_api.unad_functions_api.id
  authorizer_type                   = "REQUEST"
  authorizer_uri                    = aws_lambda_function.unad_functions_authorizer.invoke_arn
  authorizer_result_ttl_in_seconds  = 0
  authorizer_payload_format_version = "2.0"
  enable_simple_responses           = true
}

resource "aws_apigatewayv2_integration" "lambda_integration_product" {
  api_id           = aws_apigatewayv2_api.unad_functions_api.id
  integration_type = "AWS_PROXY"

  connection_type        = "INTERNET"
  description            = "Lambda integration"
  integration_method     = "POST"
  integration_uri        = aws_lambda_function.unad_functions_product.invoke_arn
  payload_format_version = "2.0"
}

resource "aws_lambda_permission" "unad_functions_product" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.unad_functions_product.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.unad_functions_api.execution_arn}/*/*"
}

resource "aws_lambda_permission" "unad_functions_authorizer_execute" {
  statement_id  = "AllowExecutionFromAPIGateway"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.unad_functions_authorizer.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.unad_functions_api.execution_arn}/*/*"
}

resource "aws_apigatewayv2_route" "unad_functions_product" {
  api_id             = aws_apigatewayv2_api.unad_functions_api.id
  route_key          = "POST /{proxy+}"
  target             = "integrations/${aws_apigatewayv2_integration.lambda_integration_product.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.unad_functions_authorizer.id
}

resource "aws_cloudwatch_log_group" "unad_functions_api_logs" {
  name = "/aws/apigateway/${aws_apigatewayv2_api.unad_functions_api.name}"
}

resource "aws_apigatewayv2_stage" "unad_functions_product_default" {
  api_id      = aws_apigatewayv2_api.unad_functions_api.id
  name        = "$default"
  auto_deploy = true
  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.api_logs.arn
    format          = "$context.identity.sourceIp - - [$context.requestTime] \"$context.httpMethod $context.routeKey $context.protocol\" $context.status $context.responseLength $context.requestId $context.error.message $context.integration.error $context.integrationErrorMessage $context.authorizer.error $context.integration.integrationStatus"
  }
}

output "unad_functions_api_url" {
  value = aws_apigatewayv2_api.unad_functions_api.api_endpoint
}
