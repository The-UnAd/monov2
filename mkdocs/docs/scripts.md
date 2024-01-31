# Shared Scripts Glossary

The `scripts` directory contains a set of convenience [Powershell](https://learn.microsoft.com/en-us/powershell/) scripts to make developers' lives easier when working in this repo.

!!! tip "Further Development"
    Any such scripts moving forward must, if possible, be written in Powershell.  This is so that the scripts can be executed in any environment, regardless of platform.

## `init`

The `init.ps1` script (the only one that lives at the root of this project) is a convenience script for loading the most common environment variables in a terminal session.  It reads from the `.env.default` file [set up here](/#initial-setup).  

!!! info
    **Ensure this has been run in any new terminal session, as just about every other script depends on it's behavior.**

## `login-to-aws-npm`

This script contains the AWS CLI command required to generate a valid `~/.npmrc` entry for the [CodeArtifact NPM repo](https://docs.aws.amazon.com/codeartifact/latest/ug/using-npm.html).  See [NPM Usage and Conventions](npm) for more information.

## `login-to-ecr`

This script contains the AWS CLI command required to set the correct authentication configuration for pushing images to [Elastic Container Registry](https://console.aws.amazon.com/ecr/home/).  See [Considerations for Docker](docker) for more information.



