variable "region" {
  type = string
  nullable = false
}

variable "vpc_cidr" {
  default = "10.0.0.0/16"
}

variable "dns_zone" {
  type = string
  nullable = false
}

variable "environment" {
  type = string
  nullable = false
}
