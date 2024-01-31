

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.34.0"
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
  backend "s3" {
    bucket  = "terraform"
    key     = "terraform.tfstate"
    region  = "us-east-2"
    profile = "terraform"
  }
}

provider "aws" {
  region = var.region
}

module "aws" {
  source = "./aws"
  region = var.region
  dns_zone = var.DNS_ZONE
}


