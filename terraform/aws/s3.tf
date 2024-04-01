resource "aws_s3_bucket" "admin_site" {
  bucket = "unad-admin-site"
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

resource "aws_route53_record" "admin_site" {
  name    = "portal.${data.aws_route53_zone.main.name}"
  type    = "CNAME"
  zone_id = data.aws_route53_zone.main.zone_id
  records = [aws_s3_bucket.admin_site.bucket_regional_domain_name]
  ttl     = "300"
}

output "admin_site_bucket_name" {
  value = aws_s3_bucket.admin_site.bucket
}
