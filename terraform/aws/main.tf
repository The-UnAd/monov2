
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

