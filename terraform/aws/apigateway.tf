resource "aws_api_gateway_rest_api" "admin_api" {
  name        = "unad_admin_api"
  description = "API gateway for admin site"
}

resource "aws_api_gateway_resource" "graphql_resource" {
  rest_api_id = aws_api_gateway_rest_api.admin_api.id
  parent_id   = aws_api_gateway_rest_api.admin_api.root_resource_id
  path_part   = "graphql"
}

resource "aws_api_gateway_method" "graphql_method" {
  rest_api_id   = aws_api_gateway_rest_api.admin_api.id
  resource_id   = aws_api_gateway_resource.graphql_resource.id
  http_method   = "POST"
  authorization = "NONE" # TODO: look into cognito authorizer for this
}

resource "aws_api_gateway_domain_name" "admin_domain" {
  domain_name              = "portal.${data.aws_route53_zone.main.name}"
  regional_certificate_arn = aws_acm_certificate.main_wildcard.arn
  security_policy          = "TLS_1_2"
  endpoint_configuration {
    types = ["REGIONAL"]
  }
}

# resource "aws_route53_record" "admin_domain" {
#   name    = aws_api_gateway_domain_name.admin_domain.domain_name
#   type    = "A"
#   zone_id = data.aws_route53_zone.main.id

#   alias {
#     evaluate_target_health = true
#     name                   = aws_api_gateway_domain_name.admin_domain.cloudfront_domain_name
#     zone_id                = aws_api_gateway_domain_name.admin_domain.cloudfront_zone_id
#   }
# }

resource "aws_api_gateway_base_path_mapping" "graphql_base_path_mapping" {
  api_id      = aws_api_gateway_rest_api.admin_api.id
  stage_name  = aws_api_gateway_deployment.graphql_deployment.stage_name
  domain_name = aws_api_gateway_domain_name.admin_domain.domain_name
  base_path   = "graphql"
}
