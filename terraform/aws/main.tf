
terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "5.37.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3"
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

data "aws_availability_zones" "available" {}

locals {
  availability_zones = data.aws_availability_zones.available.names
  region = var.region
  vpc_cidr = var.vpc_cidr
}
