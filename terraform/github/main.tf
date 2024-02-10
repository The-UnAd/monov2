terraform {
  required_providers {
    github = {
      source  = "integrations/github"
      version = "~> 5.0"
    }
  }
}

# Configure the GitHub Provider
provider "github" {}

data "github_repository" "monov2" {
  full_name = "The-UnAd/monov2"
}

data "github_repository_environment" "development" {
  repository  = data.github_repository.repo.name
  environment = "development"
}

resource "github_actions_environment_secret" "jumpbox_ssh_key" {
  repository      = data.github_repository.monov2.full_name
  environment     = data.github_repository_environment.development.name
  secret_name     = "JUMPBOX_SSH_KEY"
  plaintext_value = var.jumpbox_ssh_key
}

resource "github_actions_environment_variable" "db_host" {
  repository    = data.github_repository.monov2.full_name
  environment   = data.github_repository_environment.development.name
  variable_name = "DB_HOST"
  value         = var.db_host
}

variable "db_host" {
  type     = string
  nullable = false
}

variable "db_port" {
  type     = number
  nullable = false
}

variable "jumpbox_host" {
  type     = string
  nullable = false
}

variable "jumpbox_ssh_key" {
  type      = string
  nullable  = false
  sensitive = true
}
