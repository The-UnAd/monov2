terraform {
  required_providers {
    github = {
      source  = "integrations/github"
      version = "~> 5.0"
    }
  }
}

# Configure the GitHub Provider
provider "github" {
  token = var.token
  owner = "The-UnAd"
}

resource "github_actions_environment_variable" "db_host" {
  repository    = "monov2"
  environment   = "development"
  variable_name = "DB_HOST"
  value         = var.db_host
}

resource "github_actions_environment_variable" "jumpbox_host" {
  repository      = "monov2"
  environment     = "development"
  variable_name     = "JUMPBOX_HOST"
  value = var.jumpbox_host
}

resource "github_actions_environment_variable" "db_port" {
  repository      = "monov2"
  environment     = "development"
  variable_name     = "DB_PORT"
  value = var.db_port
}

resource "github_actions_environment_secret" "db_pass" {
  repository      = "monov2"
  environment     = "development"
  secret_name     = "DB_PASS"
  plaintext_value = var.db_pass
}

resource "github_actions_environment_secret" "jumpbox_ssh_key" {
  repository      = "monov2"
  environment     = "development"
  secret_name     = "DB_PASS"
  plaintext_value = var.db_pass
}

variable "token" {
  type      = string
  sensitive = true
  nullable  = false
}

variable "db_host" {
  type     = string
  nullable = false
}

variable "db_pass" {
  type      = string
  sensitive = true
  nullable  = false
}

variable "db_port" {
  type     = number
  nullable = false
}

variable "jumpbox_host" {
  type     = string
  nullable = false
}
