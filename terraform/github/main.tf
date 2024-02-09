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


variable "aurora_host" {
  type     = string
  nullable = false
}

variable "aurora_port" {
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
