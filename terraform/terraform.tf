terraform {
  # https://developer.hashicorp.com/terraform/cli/cloud/settings#the-cloud-block
  # 
  # The cloud block only affects Terraform CLI's behavior. When Terraform Cloud uses a
  # configuration that contains a cloud block - for example, when a workspace is
  # configured to use a VCS provider directly - it ignores the block and behaves according
  # to its own workspace settings.
  # 
  # What I hope this means is that I can play with config here, `apply`, all I want,
  # and when I push to github, the cloud will use the settings in the environment-specfic
  # workspace.
  cloud {
    organization = "theunad"

    workspaces {
      project = "monov2"
      name    = "local"
    }
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.37.0"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2"
    }
    external = {
      source  = "hashicorp/external"
      version = "~> 2"
    }
  }
}

provider "aws" {
  region = var.AWS_REGION
}

module "aws" {
  source   = "./aws"
  region   = var.AWS_REGION
  dns_zone = var.DNS_ZONE
}

module "github" {
  source                = "./github"
  db_host               = module.aws.rds_cluster_endpoint
  db_port               = module.aws.rds_cluster_port
  db_pass               = module.aws.rds_cluster_password
  jumpbox_host          = module.aws.jumpbox_host
  token                 = var.GITHUB_TOKEN
  graph_monitor_url     = module.aws.graph_monitor_api_url
  graph_monitor_api_key = module.aws.graph_monitor_api_key
}


