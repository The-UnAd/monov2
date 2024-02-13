resource "aws_elasticache_subnet_group" "redis" {
  name       = "unad-elasticache-subnet-group"
  subnet_ids = tolist(aws_subnet.private_subnet.*.id)
}

resource "aws_security_group" "redis" {
  name        = "unad-elasticache-sg"
  description = "Allow traffic for Elasticache"
  vpc_id      = aws_vpc.vpc.id

  ingress {
    from_port   = 6379
    to_port     = 6379
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
  }
}

resource "aws_elasticache_cluster" "unad" {
  cluster_id                   = "unad-redis"
  engine                       = "redis"
  node_type                    = "cache.t2.micro"
  num_cache_nodes              = 1
  # preferred_availability_zones = local.availability_zones
  # az_mode                      = "cross-az"
  parameter_group_name         = "default.redis7"
  subnet_group_name            = aws_elasticache_subnet_group.redis.name
  security_group_ids           = [aws_security_group.redis.id]
  engine_version               = "7.1"
}

resource "aws_ssm_parameter" "redis_hosts" {
  name  = "/redis/hosts"
  type  = "String"
  value = join(",", [for endpoint in aws_elasticache_cluster.unad.cache_nodes : "redis://${endpoint.address}:${aws_elasticache_cluster.unad.port}"])
}

resource "aws_ssm_parameter" "redis_connection_string" {
  name  = "/redis/connection_string"
  type  = "SecureString"
  value = "${join(",", [for endpoint in aws_elasticache_cluster.unad.cache_nodes : "${endpoint.address}:${aws_elasticache_cluster.unad.port}"])},ssl=false,abortConnect=false"
}

output "redis_endpoints" {
  value = join(",", [for endpoint in aws_elasticache_cluster.unad.cache_nodes : "${endpoint.address}:${aws_elasticache_cluster.unad.port}"])
}

output "redis_connection_string" {
  sensitive = true
  value     = "${join(",", [for endpoint in aws_elasticache_cluster.unad.cache_nodes : "${endpoint.address}:${aws_elasticache_cluster.unad.port}"])},ssl=false,abortConnect=false"
}
