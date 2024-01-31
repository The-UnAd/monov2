resource "aws_route53_zone" "main" {
  name = var.dns_zone
}

# resource "aws_route53_record" "main-ns" {
#   zone_id = aws_route53_zone.main.zone_id
#   name    = var.dns_zone
#   type    = "NS"
#   ttl     = "30"
#   records = aws_route53_zone.main.name_servers
# }
