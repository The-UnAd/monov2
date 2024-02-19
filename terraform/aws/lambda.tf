

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


module "lambda" {
  source = "./lambda"
  security_group_ids = [aws_security_group.lambda_private.id]
  subnet_ids = aws_subnet.private_subnet.*.id
  region = data.aws_region.current.name
  vpc_cidr = var.vpc_cidr
  certificate_arn = aws_acm_certificate.wildcard.arn
  domain_name = data.aws_route53_zone.main.name
  redis_connection_string = aws_ssm_parameter.redis_connection_string.value
  zone_id = data.aws_route53_zone.main.zone_id
}

output "graph_monitor_api_key" {
  value = module.lambda.graph_monitor_api_key
}
output "graph_monitor_api_url" {
  value = module.lambda.graph_monitor_api_url
}


output "unad_functions_api_key" {
  value = module.lambda.unad_functions_api_key
}
output "unad_functions_api_url" {
  value = module.lambda.unad_functions_api_url
}
