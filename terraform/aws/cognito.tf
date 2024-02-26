# resource "aws_cognito_user_pool" "cognito_pool" {
#   name = "unad-cognito-pool"

#   username_attributes      = ["email"]
#   auto_verified_attributes = ["email"]

#   account_recovery_setting {
#     recovery_mechanism {
#       name     = "verified_email"
#       priority = 1
#     }
#   }

#   schema {
#     attribute_data_type = "String"
#     name                = "email"
#     required            = true
#   }

#   password_policy {
#     minimum_length    = 8
#     require_lowercase = true
#     require_uppercase = true
#     require_numbers   = true
#     require_symbols   = true
#   }

#   # This bit is a workaround for a bug in the AWS provider.
#   # See https://github.com/hashicorp/terraform-provider-aws/issues/21654#issuecomment-1058245347
#   lifecycle {
#     ignore_changes = [
#       password_policy,
#       schema
#     ]
#   }
# }

# # resource "aws_cognito_identity_pool" "cognito_identity_pool" {
# #   identity_pool_name               = "${var.env_name}-identity-pool"
# #   allow_unauthenticated_identities = true

# #   cognito_identity_providers {
# #     client_id               = aws_cognito_user_pool_client.cognito_client.id
# #     provider_name           = "cognito-idp.${var.region}.amazonaws.com/${aws_cognito_user_pool.cognito_pool.id}"
# #     server_side_token_check = false
# #   }
# # }

# resource "random_password" "test_user_password" {
#   length      = 16
#   special     = true
#   min_numeric = 1
#   min_special = 1
# }

# resource "aws_cognito_user" "test_user" {
#   username       = "testuser@example.com"
#   password       = random_password.test_user_password.result
#   user_pool_id   = aws_cognito_user_pool.cognito_pool.id
#   message_action = "SUPPRESS"
#   attributes = {
#     email          = "testuser@example.com"
#     email_verified = true
#   }
# }

# resource "aws_cognito_user_pool_client" "cognito_client" {
#   name                                 = "unad-user-pool-client"
#   user_pool_id                         = aws_cognito_user_pool.cognito_pool.id
#   generate_secret                      = false
#   explicit_auth_flows                  = ["USER_PASSWORD_AUTH"]
# #   allowed_oauth_flows                  = ["code", "implicit"]
# #   allowed_oauth_flows_user_pool_client = true
# #   allowed_oauth_scopes                 = ["phone", "email", "openid", "profile", "aws.cognito.signin.user.admin"]
# #   callback_urls                        = ["https://www.example.com/callback"]
# #   logout_urls                          = ["https://www.example.com/logout"]
# }

# resource "aws_cognito_user_pool_domain" "main" {
#   domain       = "unad"
#   user_pool_id = aws_cognito_user_pool.cognito_pool.id
# }

# output "cognito_pool_id" {
#   value = aws_cognito_user_pool.cognito_pool.id
# }
# output "cognito_pool_domain" {
#   value = aws_cognito_user_pool_domain.main.domain
# }
# output "cognito_pool_domain_id" {
#   value = aws_cognito_user_pool_domain.main.id
# }
# output "cognito_pool_endpoint" {
#   value = aws_cognito_user_pool.cognito_pool.endpoint
# }
# output "cognito_client_id" {
#   value = aws_cognito_user_pool_client.cognito_client.id
# }
# output "cognito_client_secret" {
#   value = aws_cognito_user_pool_client.cognito_client.client_secret
# }

# output "test_user_credentials" {
#   value = {
#     username = aws_cognito_user.test_user.username
#     password = random_password.test_user_password.result
#     userId   = aws_cognito_user.test_user.id
#   }
# }

# resource "aws_ssm_parameter" "cognito_pool_endpoint" {
#   name  = "/congito/endpoint"
#   type  = "String"
#   value = aws_cognito_user_pool.cognito_pool.endpoint
# }


# resource "aws_ssm_parameter" "cognito_client_id" {
#   name  = "/congito/client_id"
#   type  = "String"
#   value = aws_cognito_user_pool_client.cognito_client.id
# }
