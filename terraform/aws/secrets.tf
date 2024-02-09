
resource "tls_private_key" "jwt_key" {
  algorithm = "RSA"
}


resource "aws_ssm_parameter" "jwt_private_key" {
  name  = "/jwt/private_key"
  type  = "SecureString"
  value = tls_private_key.jwt_key.private_key_pem
}

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

output "jwt_public_key" {
  value = tls_private_key.jwt_key.public_key_openssh
}

output "jwt_private_key" {
  value = tls_private_key.jwt_key.private_key_pem
}
