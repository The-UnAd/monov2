variable "region" {
  type = string
}

variable "vpc_cidr" {
  default = "10.0.0.0/16"
}

variable "dns_zone" {
  type = string
}
