resource "aws_ecr_repository" "lambda_ecr_repo" {
  name         = "unad/graph-monitor"
  force_delete = true
}

# resource "aws_ecr_lifecycle_policy" "lasttwoimages" {
#   repository = aws_ecr_repository.lambda_ecr_repo.name
#   policy     = jsonencode({
#     "rules" : [
#       {
#         "rulePriority" : 1,
#         "description" : "Keep only the last 2 images",
#         "selection" : {
#           "tagStatus" : "any",
#           "countType" : "imageCountMoreThan",
#           "countNumber" : 2
#         },
#         "action" : {
#           "type" : "expire"
#         }
#       }
#     ]
#   })
# }

resource "random_password" "graph_montitor_api_key" {
  length  = 32
  special = false
}

output "graph_montitor_api_key" {
  value = random_password.graph_montitor_api_key.result
}

# module "lambda_function" {
#   source = "terraform-aws-modules/lambda/aws"

#   function_name  = "graph-monitor"
#   create_package = false
#   runtime        = "dotnet8"

#   image_uri    = module.docker_image.image_uri
#   package_type = "Image"
#   environment_variables = {
#     "ASPNETCORE_ENVIRONMENT" = "Production"
#     "ASPNETCORE_URLS" = "http://+:80"
#     "ApiKeyAuthenticationOptions:ApiKey": "${random_password.graph_montitor_api_key.result}",
#     "REDIS_URL": "${aws_ssm_parameter.redis_connection_string.value}"
#   } # TODO: figure out how to get the redis connection string from the SSM parameter store
# }

# #{
# #     name  = "ASPNETCORE_ENVIRONMENT"
# #     value = "Production"
# #     }, {
# #     name  = "ASPNETCORE_URLS"
# #     value = "http://+:80"
# #     }, {
# #     name  = "ApiKeyAuthenticationOptions:ApiKey"
# #     value = "${random_password.graph_montitor_api_key.result}"
# #   }

# module "docker_image" {
#   source = "terraform-aws-modules/lambda/aws//modules/docker-build"

#   create_ecr_repo = false
#   ecr_repo        = "unad/graph-monitor"

#   use_image_tag = true
#   image_tag     = "latest" # TODO: use git commit hash or something

#   source_path = "../../dotnet/GraphMonitor/GraphMonitor"  
# }

