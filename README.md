[![Build Status](https://dfds.visualstudio.com/DevelopmentExcellence/_apis/build/status/Harald-pipeline?branchName=master)](https://dfds.visualstudio.com/DevelopmentExcellence/_build/latest?definitionId=1416&branchName=master)
[![Build Status](https://dfds.visualstudio.com/DevelopmentExcellence/_apis/build/status/Harald-pipeline?branchName=master&stageName=CI)](https://dfds.visualstudio.com/DevelopmentExcellence/_build/latest?definitionId=1416&branchName=master)

# Harald

The notification service aka herold aka herald.

## Getting started

### Prerequisites:

1. dotnet core 2.2 sdk
2. docker
3. docker-compose
4. bash

### Directory outline

The most **significant** items found in the repository/directory root are:

```text
.
├── Dockerfile
├── README.md
├── add-migration.sh
├── api-contracts/
├── db/
├── docker-compose.yml
├── docs/
├── k8s/
├── pipeline.sh
└── src/
```
_Please note: you'd might also find other items in the repository/directory root._

Here is a small description for each of the items:

| Item               | Description                                                                                                                                                                |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Dockerfile         | Describes how the runtime environment for the actual application should look like.                                                                                         |
| README.md          | _This_ readme file                                                                                                                                                         |
| add-migration.sh   | Util for quickly generating a database migration script that follows default conventions. Run it like this: `./add-migration.sh "<small description of you change here>"`. |
| api-contracts/     | Directory that contains the current OpenApi contract(s) exposed from the service.                                                                                          |
| db/                | Directory that contains all things related to the database setup e.g. Dockerfile for init migration container, migration script etc.                                       |
| docker-compose.yml | Docker-compose file for bringing all external dependencies up (some in a 'faked-out' version).                                                                             |
| docs/              | Directory for any documents that take part in documenting the service e.g. domain events.                                                                                  |
| k8s/               | Directory containing all Kubernetes manifests used to describe the desired runtime state for the service in Kubernetes.                                                    |
| pipeline.sh        | _The_ shell script used to implement the _continous integration_ pipeline. The script also generates a docker container image as a deployment artifact.                    |
| src/               | The _main_ directory for all the source code for the service.                                                                                                              |

### Running the service

First restore dependencies by runing the `./pipeline` script located in the repository root or by navigating to the `./src` folder and run `dotnet restore` like shown below:

#### Pipeline script

```bash
./pipeline.sh
```
#### Manual restore

```bash
cd src
dotnet restore
```

#### Start the application

Then the application can be executed by the following (navigate to the `./src` folder):
```bash
dotnet run --project Harald.WebApi/
```

## Database

The database will initially start as empty. The image is constructed so that files can be added through the command below, and these will be run in date order (at least if you name the file right).

### Local Development

To add a migration, run:

```sh
./add-migration.sh create capability table
```

Will create an empty migration file (e.g. `20181017194326_create_capability_table.sql`) in the `./db/migrations` folder. The file will be prefixed with YYMMDDHHMMSS.

To bring up a local postgres database with all migration scripts applied against it, set the environment variables in `docker-compose.yml` as needed (or use defaults), and run:

```sh
docker-compose up --build
```

After adding new migrations, run `docker-compose down` and re-run the above command.

## Domain Events

Domain events consumed is following:
* capability_created
* member_joined_capability
* member_left_capability

For information about these domain events refers to the [domain events in capability service](https://github.com/dfds/team-service/blob/master/docs/domain_events.md).

## Integrations

### Slack

In order to integrate with Slack, there has to be an OAuth Access Token defined.
This is constructed upon creation of _App_ in the [Admin-section](https://api.slack.com/apps) of Slack. If the key has to be rolled over, one need to reinstall the app in Slack.

Ensure that the "user" installing the app is a dedicated user just for that purpose. Due to some funkiness with how the Slack API works, some actions will be seen as done by the "app" and some by the user who has "installed" the app.

## TODO

* Improve resiliency.
* Make sure all integrations are idempotent.
* Better test coverage.
