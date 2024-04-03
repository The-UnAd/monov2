provider "aws" {
  alias  = "virginia"
  region = "us-east-1"
}

locals {
  admin_site_domain_name     = "portal.${data.aws_route53_zone.portal.name}"
  admin_site_certificate_arn = aws_acm_certificate.portal_wildcard.arn
  cloudfront_origin          = "S3-${aws_s3_bucket.admin_site.id}"
}

resource "aws_s3_bucket" "admin_site" {
  bucket        = "unad-admin-site"
  force_destroy = var.environment == "production" ? false : true
}

resource "aws_s3_bucket_website_configuration" "admin_site" {
  bucket = aws_s3_bucket.admin_site.id

  index_document {
    suffix = "index.html"
  }

  error_document {
    key = "error.html"
  }
}

resource "aws_s3_bucket_acl" "admin_site_acl" {
  bucket = aws_s3_bucket.admin_site.id

  acl = "public-read"
}

resource "aws_s3_bucket_policy" "admin_site_bucket_policy" {
  bucket = aws_s3_bucket.admin_site.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action    = ["s3:GetObject"]
        Effect    = "Allow"
        Resource  = "${aws_s3_bucket.admin_site.arn}/*"
        Principal = "*"
      },
    ]
  })
}

resource "aws_globalaccelerator_accelerator" "global_accelerator" {
  name = "unad-global-accelerator"

  ip_address_type = "IPV4"
  enabled         = true

  attributes {
    flow_logs_enabled   = true
    flow_logs_s3_bucket = "unad-accelerator-logs"
  }
}

resource "aws_globalaccelerator_listener" "global_listener" {
  accelerator_arn = aws_globalaccelerator_accelerator.global_accelerator.id
  protocol        = "TCP"
  port_range {
    from_port = 443
    to_port   = 443
  }
}

resource "aws_cloudfront_distribution" "admin_site_distribution" {
  origin {
    domain_name = aws_s3_bucket.admin_site.bucket_regional_domain_name
    origin_id   = local.cloudfront_origin

    s3_origin_config {
      origin_access_identity = "origin-access-identity/cloudfront/${aws_cloudfront_origin_access_identity.s3_oai.id}"
    }
  }

  enabled             = true
  default_root_object = "index.html"

  aliases = [
    local.admin_site_domain_name,
  ]

  default_cache_behavior {
    allowed_methods  = ["GET", "HEAD"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = local.cloudfront_origin

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  price_class = "PriceClass_100"

  viewer_certificate {
    acm_certificate_arn      = local.admin_site_certificate_arn
    ssl_support_method       = "sni-only"
    minimum_protocol_version = "TLSv1.2_2019"
  }

  restrictions {
    geo_restriction {
      restriction_type = "whitelist"
      locations        = ["US"]
    }
  }

  provider = aws.virginia
}

resource "aws_globalaccelerator_endpoint_group" "admin_endpoint_group" {
  listener_arn          = aws_globalaccelerator_listener.global_listener.id
  endpoint_group_region = data.aws_region.current
  endpoint_configuration {
    endpoint_id = aws_cloudfront_distribution.admin_site_distribution.id
    weight      = 100
  }
}

resource "aws_route53_record" "admin_site" {
  zone_id = data.aws_route53_zone.portal.zone_id
  name    = local.admin_site_domain_name
  type    = "A"

  alias {
    name                   = aws_globalaccelerator_accelerator.global_accelerator.dns_name
    zone_id                = aws_globalaccelerator_accelerator.global_accelerator.hosted_zone_id
    evaluate_target_health = false
  }

  provider = aws.virginia
}


resource "aws_cloudfront_origin_access_identity" "s3_oai" {
  comment = "OAI for ${aws_s3_bucket.admin_site.id}"
}

output "admin_site_bucket_name" {
  value = aws_s3_bucket.admin_site.bucket
}
