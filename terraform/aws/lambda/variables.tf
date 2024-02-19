variable "region" {
  type = string
}

variable "vpc_cidr" {
  type = string
}

variable "security_group_ids" {
  type = list(string)
}

variable "subnet_ids" {
  type = list(string)
}

variable "certificate_arn" {
  type = string
}

variable "domain_name" {
  type = string
}

variable "redis_connection_string" {
  type = string
  sensitive = true
}

variable "zone_id" {
  type = string
}
