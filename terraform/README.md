# Things Not Created by Terraform

- Secrets Manager Secret at `/unad/ec2/jumpbox/ssh-keys`.  A JSON object that contains an ed_25519 key pair for SSH access to the jumpbox.  This must be generated locally and set in Secrets Manager manually per-environment.  Should be generated with `ssh-keygen -t ed25519 -C "info@theunad.com"`
