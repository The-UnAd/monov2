# Welcome to UnAd

Welcome to the UnAd system repository.  This monorepo is contains all primary functional components of the UnAd system.

## Repository Structure

The UnAd system architecture is guided by Domain-Driven principals in terms of structure.  This repository structure attempts to reflect that approach as much as possible.  The primary directories of interest are as follows.

- `db` -- This directory contains code an ancillary files for database management
- `dotenv` -- This directory mostly contains ephemeral data used in local development.
- `dotnet` -- This directory contains all .NET code, as well as many related files, such as Dockerfiles.
- `env` -- This directory mostly contains ephemeral data used in local development.
- `packages` -- This directory contains JavaScript libraries used by other JS-based parts of the system.
- `mkdocs` -- This directory contains more extensive documentation on the system and how it is designed.
- `terraform` -- This directory contains all Terraform configurations for the infrastructure.
- `web` -- This directory contains the two main websites, the Admin Portal and the Signup Site.

## Viewing the Docs

The easiest way to view the full docs in the repo is to (assuming you have Python installed), navigate to the `docs` directory in a terminal and run the following commands:

```ps1
python -m venv .
pip install -r requirements.txt
./Scripts/Activate.ps1
mkdocs serve
```

This will launch a local server with the fully-rendered docs at http://localhost:8080/.
