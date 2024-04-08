data "aws_route53_zone" "portal" {
  name = var.main_dns_zone

  provider = aws.virginia
}

resource "aws_acm_certificate" "main_wildcard" {
  domain_name       = "*.${data.aws_route53_zone.portal.name}"
  validation_method = "DNS"

  provider = aws.virginia
}

resource "aws_route53_record" "main_wildcard" {
  for_each = {
    for dvo in aws_acm_certificate.main_wildcard.domain_validation_options : dvo.domain_name => {
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

  provider = aws.virginia
}

resource "aws_acm_certificate_validation" "main_wildcard" {
  certificate_arn         = local.admin_site_certificate_arn
  validation_record_fqdns = [for record in aws_route53_record.main_wildcard : record.fqdn]

  provider = aws.virginia
}

output "admin_site" {
    value = {
        domain_name = local.admin_site_domain_name
        certificate_arn = local.admin_site_certificate_arn
    }
}
