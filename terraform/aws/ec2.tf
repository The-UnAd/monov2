
data "aws_ami" "ubuntu" {
  most_recent = true

  filter {
    name   = "name"
    values = ["ubuntu/images/hvm-ssd/ubuntu-jammy-22.04-amd64-server-20230516"]
  }
}

data "aws_secretsmanager_secret" "jumpbox_ssh_keys" {
  name = "/unad/ec2/jumpbox/ssh-keys"
}

data "aws_secretsmanager_secret_version" "jumpbox_ssh_keys" {
  secret_id = data.aws_secretsmanager_secret.jumpbox_ssh_keys.id
}

resource "aws_key_pair" "jumpbox" {
  key_name   = "jumpbox"
  public_key = jsondecode(data.aws_secretsmanager_secret_version.jumpbox_ssh_keys.secret_string)["public_key"]
}

resource "aws_instance" "jumpbox" {
  ami                    = data.aws_ami.ubuntu.id
  instance_type          = "t2.micro"
  key_name               = aws_key_pair.jumpbox.key_name
  vpc_security_group_ids = [aws_security_group.jumpbox.id]

  connection {
    type        = "ssh"
    user        = "ec2-user"
    private_key = jsondecode(data.aws_secretsmanager_secret_version.jumpbox_ssh_keys.secret_string)["private_key"]
    host        = self.public_ip
  }

  subnet_id                   = aws_subnet.public_subnet[0].id
  associate_public_ip_address = true

  tags = {
    Name = "jumpbox"
  }
}

resource "aws_security_group" "jumpbox" {
  name_prefix = "jumpbox-"
  vpc_id      = aws_vpc.vpc.id
}

resource "aws_security_group_rule" "jumpbox_ingress_ssh" {
  type              = "ingress"
  from_port         = 22
  to_port           = 22
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.jumpbox.id
}

resource "aws_security_group_rule" "jumpbox_egress_tls" {
  type              = "egress"
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.jumpbox.id
}

resource "aws_security_group_rule" "jumpbox_egress_http" {
  type              = "egress"
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.jumpbox.id
}

resource "aws_security_group_rule" "jumpbox_egress_msk" {
  type              = "egress"
  from_port         = 9092
  to_port           = 9098
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.jumpbox.id
}

# resource "aws_security_group_rule" "jumpbox_egress_postgres" {
#   type              = "egress"
#   from_port         = aws_rds_cluster.aurora.port
#   to_port           = aws_rds_cluster.aurora.port
#   protocol          = "tcp"
#   cidr_blocks       = [var.vpc_cidr]
#   security_group_id = aws_security_group.jumpbox.id
# }

resource "aws_security_group_rule" "jumpbox_egress_redis" {
  type              = "egress"
  from_port         = aws_memorydb_cluster.unad.port
  to_port           = aws_memorydb_cluster.unad.port
  protocol          = "tcp"
  cidr_blocks       = [var.vpc_cidr]
  security_group_id = aws_security_group.jumpbox.id
}

resource "aws_security_group_rule" "jumpbox_egress_dns_tcp" {
  type              = "egress"
  from_port         = 53
  to_port           = 53
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.jumpbox.id
}

resource "aws_security_group_rule" "jumpbox_egress_dns_udp" {
  type              = "egress"
  from_port         = 53
  to_port           = 53
  protocol          = "udp"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.jumpbox.id
}

output "jumpbox_host" {
  value = aws_instance.jumpbox.public_dns
}

