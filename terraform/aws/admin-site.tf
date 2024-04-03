
locals {
  admin_site_domain_name     = "portal.${data.aws_route53_zone.portal.name}"
  admin_site_certificate_arn = aws_acm_certificate.portal_wildcard.arn
}

data "aws_route53_zone" "portal" {
  name = var.portal_dns_zone
}

resource "aws_acm_certificate" "portal_wildcard" {
  domain_name       = "*.${data.aws_route53_zone.portal.name}"
  validation_method = "DNS"
}

resource "aws_route53_record" "portal_wildcard" {
  for_each = {
    for dvo in aws_acm_certificate.portal_wildcard.domain_validation_options : dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  }

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = data.aws_route53_zone.portal.zone_id
}

resource "aws_acm_certificate_validation" "portal_wildcard" {
  certificate_arn         = local.admin_site_certificate_arn
  validation_record_fqdns = [for record in aws_route53_record.portal_wildcard : record.fqdn]
}

resource "aws_s3_bucket" "admin_site" {
  bucket        = "unad-admin-site"
  force_destroy = var.environment == "production" ? false : true
}

# resource "aws_s3_bucket_website_configuration" "admin_site" {
#   bucket = aws_s3_bucket.admin_site.id

#   index_document {
#     suffix = "index.html"
#   }

#   error_document {
#     key = "error.html"
#   }
# }

# resource "aws_s3_bucket_acl" "admin_site_acl" {
#   bucket = aws_s3_bucket.admin_site.id

#   acl = "public-read"
# }

# resource "aws_s3_bucket_policy" "admin_site_bucket_policy" {
#   bucket = aws_s3_bucket.admin_site.id

#   policy = jsonencode({
#     Version = "2012-10-17"
#     Statement = [
#       {
#         Action    = ["s3:GetObject"]
#         Effect    = "Allow"
#         Resource  = "${aws_s3_bucket.admin_site.arn}/*"
#         Principal = "*"
#       },
#     ]
#   })
# }

# resource "aws_api_gateway_resource" "admin_site_resource" {
#   rest_api_id = aws_api_gateway_rest_api.admin_api.id
#   parent_id   = aws_api_gateway_rest_api.admin_api.root_resource_id
#   path_part   = "{proxy+}"
# }

# resource "aws_api_gateway_method" "admin_site_method" {
#   rest_api_id   = aws_api_gateway_rest_api.admin_api.id
#   resource_id   = aws_api_gateway_resource.admin_site_resource.id
#   http_method   = "GET"
#   authorization = "NONE" # TODO: look into cognito authorizer for this
# }

# resource "aws_api_gateway_integration" "admin_site_integeration" {
#   rest_api_id = aws_api_gateway_rest_api.admin_api.id
#   resource_id = aws_api_gateway_resource.admin_site_resource.id
#   http_method = aws_api_gateway_method.admin_site_method.http_method

#   type                    = "HTTP_PROXY"
#   uri                     = "http://${aws_s3_bucket_website_configuration.admin_site.website_endpoint}/"
#   integration_http_method = aws_api_gateway_method.admin_site_method.http_method
# }

# resource "aws_api_gateway_deployment" "admin_site_deployment" {
#   rest_api_id = aws_api_gateway_rest_api.admin_api.id
#   stage_name  = var.environment

#   depends_on = [
#     aws_api_gateway_integration.admin_site_integeration
#   ]
# }

output "admin_site_bucket_name" {
  value = aws_s3_bucket.admin_site.bucket
}

