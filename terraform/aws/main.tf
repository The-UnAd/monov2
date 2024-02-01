
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.34.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3"
    }
    docker = {
      source  = "kreuzwerker/docker"
      version = "3.0.2"
    }
    tls = {
      source = "hashicorp/tls"
      version = "4.0.5"
    }
  }
}

provider "aws" {
  region = var.region
}

data "aws_region" "current" {}

resource "aws_ssm_parameter" "region" {
  name  = "/global/region"
  type  = "String"
  value = var.region
}

