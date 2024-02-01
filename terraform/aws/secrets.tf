
resource "tls_private_key" "jwt_key" {
  algorithm = "RSA"
}


resource "aws_ssm_parameter" "jwt_private_key" {
  name  = "/jwt/private_key"
  type  = "SecureString"
  value = tls_private_key.jwt_key.private_key_pem
}

# data "aws_ssm_parameter" "twilio_account_sid" {
#   name = "/unad/global/twilio"
# }

# data "aws_secretsmanager_secret" "twilio_auth_token" {
#   name = "/unad/global/stripe"
# }

data "aws_ssm_parameter" "twilio_account_sid" {
  name  = "/twilio/account_sid"
}

data "aws_ssm_parameter" "twilio_auth_token" {
  name  = "/twilio/auth_token"
}

data "aws_ssm_parameter" "stripe_api_key" {
  name  = "/stripe/api_key"
}

data "aws_ssm_parameter" "stripe_publishable_key" {
  name  = "/stripe/stripe_publishable_key"
}

resource "aws_ssm_parameter" "redis_password" {
  name  = "/redis/password"
  type  = "SecureString"
  value = random_password.redis_password.result
}

resource "aws_ssm_parameter" "redis_username" {
  name  = "/redis/username"
  type  = "String"
  value = aws_memorydb_user.unad_redis_user.user_name
}

resource "aws_ssm_parameter" "redis_hosts" {
  name  = "/redis/hosts"
  type  = "String"
  value = join(",", [for node in local.redis_nodes : "${node.address}:${node.port}"])
}

resource "aws_ssm_parameter" "redis_connection_string" {
  name  = "/redis/connection_string"
  type  = "SecureString"
  value = "${join(",", [for node in local.redis_nodes : "${node.address}:${node.port}"])},ssl=true,abortConnect=false,user=${aws_memorydb_user.unad_redis_user.user_name},password=${random_password.redis_password.result}"
}

output "jwt_public_key" {
  value = tls_private_key.jwt_key.public_key_openssh
}

output "jwt_private_key" {
  value = tls_private_key.jwt_key.private_key_pem
}
