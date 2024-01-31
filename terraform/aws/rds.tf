
resource "random_password" "rds_password" { # TODO: Rename to rds_password.  WARNING: This will reset the passoword as well. :/
  length  = 32
  special = false
}

resource "aws_rds_cluster" "aurora" {
  cluster_identifier      = "UnAd-aurora"
  availability_zones      = data.aws_availability_zones.available.names
  database_name           = "UnAd"
  master_username         = "UnAd"
  master_password         = random_password.rds_password.result
  backup_retention_period = 7
  deletion_protection     = false # TODO: set to false for all but prod
  engine                  = "aurora-postgresql"
  engine_version          = "15"
  skip_final_snapshot     = true

  db_subnet_group_name   = aws_db_subnet_group.db_private_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds.id]

  tags = {
    Name = "UnAd-postgres"
  }
  lifecycle {
    prevent_destroy = false # TODO: set to false for all but prod
  }
}

resource "aws_rds_cluster_instance" "cluster_instances" {
  count               = 1
  identifier          = "UnAd-aurora-${count.index}"
  cluster_identifier  = aws_rds_cluster.aurora.id
  instance_class      = "db.t2.small"
  engine              = aws_rds_cluster.aurora.engine
  engine_version      = aws_rds_cluster.aurora.engine_version
  publicly_accessible = false
}

resource "aws_security_group" "rds" {
  name   = "rds-public"
  vpc_id = aws_vpc.vpc.id

  tags = {
    Name = "rds-public"
  }
}

resource "aws_security_group_rule" "rds_ingress" {
  type              = "ingress"
  from_port         = aws_rds_cluster.aurora.port
  to_port           = aws_rds_cluster.aurora.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.rds.id
}

resource "aws_security_group_rule" "rds_egress" {
  type              = "egress"
  from_port         = aws_rds_cluster.aurora.port
  to_port           = aws_rds_cluster.aurora.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.rds.id
}

resource "aws_db_subnet_group" "db_private_subnet_group" {
  subnet_ids = aws_subnet.private_subnet.*.id
  tags = {
    Name = "db-private-subnet-group"
  }
}

output "rds_cluster_endpoint" {
  value = aws_rds_cluster.aurora.endpoint
}

output "rds_cluster_password" {
  value = random_password.rds_password.result
}

resource "aws_ssm_parameter" "rds_cluster_password" {
  name  = "/rds/password"
  type  = "SecureString"
  value = random_password.rds_password.result
}

resource "aws_ssm_parameter" "rds_cluster_endpoint" {
  name  = "/rds/endpoint"
  type  = "String"
  value = aws_rds_cluster.aurora.endpoint
}

resource "aws_ssm_parameter" "rds_cluster_user" {
  name  = "/rds/user"
  type  = "String"
  value = "UnAd"
}

resource "aws_ssm_parameter" "rds_cluster_db_name" {
  name  = "/rds/db_name"
  type  = "String"
  value = "UnAd"
}

resource "aws_ssm_parameter" "rds_cluster_db_port" {
  name  = "/rds/db_port"
  type  = "String"
  value = aws_rds_cluster.aurora.port
}

resource "aws_ssm_parameter" "rds_cluster_db_connection_string" {
  name  = "/rds/rds_cluster_db_connection_string"
  type  = "String"
  value = "User ID=${"UnAd"};Password=${random_password.rds_password.result};Host=${aws_rds_cluster.aurora.endpoint};Port=${aws_rds_cluster.aurora.port}"
}



