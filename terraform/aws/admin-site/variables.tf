
variable "environment" {
  description = "The environment to deploy to"
  type        = string 
}

variable "postfix" {
  description = "Postfix to apply to names that have to be globally unique"
  type     = string
  nullable = false
}
