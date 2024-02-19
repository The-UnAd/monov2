resource "local_sensitive_file" "env_ec2" {
  filename = "../dotenv/.env.ec2"
  content  = <<-EOT
    # This file is automatically generated by terraform.
    # Do not edit this file directly.
    # Instead, update your project-specific .env.local file
    JUMPBOX_HOST="${module.aws.jumpbox_host}"
  EOT
}

resource "local_sensitive_file" "env_redis" {
  filename = "../dotenv/.env.redis"
  content  = <<-EOT
    # This file is automatically generated by terraform.
    # Do not edit this file directly.
    # Instead, update your project-specific .env.local file
    REDIS_URL="${module.aws.redis_endpoints}"
  EOT
}

resource "local_sensitive_file" "env_lambda" {
  filename = "../dotenv/.env.lambda"
  content  = <<-EOT
    # This file is automatically generated by terraform.
    # Do not edit this file directly.
    # Instead, update your project-specific .env.local file
    GRAPH_MONITOR_URL="${module.aws.graph_monitor_api_url}"
    GRAPH_MONITOR_API_KEY="${module.aws.graph_monitor_api_key}"
    FUNCTIONS_URL="${module.aws.unad_functions_api_url}"
    FUNCTIONS_API_KEY="${module.aws.unad_functions_api_key}"
  EOT
}

resource "local_sensitive_file" "env_rds" {
  filename = "../dotenv/.env.rds"
  content  = <<-EOT
    # This file is automatically generated by terraform.
    # Do not edit this file directly.
    # Instead, update your project-specific .env.local file
    DB_HOST="${module.aws.rds_cluster_endpoint}"
    DB_PASS="${module.aws.rds_cluster_password}"
    DB_PORT="${module.aws.rds_cluster_port}"
  EOT
}
