variable "region" {
  type     = string
  nullable = false
}

variable "vpc_cidr" {
  default = "10.0.0.0/16"
}

variable "signup_dns_zone" {
  type     = string
  nullable = false
}

variable "subscribe_dns_zone" {
  type     = string
  nullable = false
}

variable "portal_dns_zone" {
  type     = string
  nullable = false
}

variable "environment" {
  type     = string
  nullable = false
}
