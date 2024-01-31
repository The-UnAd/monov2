# Considerations for Docker

Each component in this system destined to be run in a container has an associated `Dockerfile` within it's project directory.  This file is used to construct the images that will be deployed to their respective pods in Kubernetes.

## Conventions

Due to using [AWS CodeArtifact](https://aws.amazon.com/codeartifact/) as the package source for NPM packages, each Node project in this repository contains a `.npmrc` file specifying the repository URL.  In addition, users of those projects must create or generate a `.npmrc` file in their home directory authenticating them with the artifact repository.  This is not only key for managing packages locally, but also for building the Docker containers.  Due to this requirement, for convenience and consistency, each project that must product an image also contains a `docker-build.ps1` script, similar to the following.

```powershell
$npmrc = Resolve-Path $HOME/.npmrc
Write-Host "using npmrc at: $npmrc"
"docker build --secret id=npmrc,src=$npmrc -t UnAd/device-api  -f .\Dockerfile ." | Invoke-Expression
```

Note the `--secret` flag in the `docker build` command.  This is the key to allowing the docker build process to access the CodeArtifact repository.  Without this piece, private packages would not be accessible.

In order to push a new version to ECR first tag the build in question with the ECR Repository URI as the prefix, first ensure you are authenticated with the ECR system:

```powershell
aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin 626564493841.dkr.ecr.us-east-2.amazonaws.com
```


```powershell
docker tag device-api:latest 626564493841.dkr.ecr.us-east-2.amazonaws.com/UnAd/device-api:latest
```


