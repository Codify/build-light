# build-light

**This project now runs as a Docker container with PowerShell script to monitor the status of Azure DevOps pipelines**

Codify's build-light project drives better software development practices by prompting the health of your projects build/pipeline status.

build-light is designed to run on a Raspberry Pi with an LED light strip that can be mounted centrally in your work area and provide a visual indication of its health.

## So many colours
**_Red_**
- Uh-oh, pipeline failed and is broken

**_Green_**
- Pipeline succeeded; all is good

**_Blue_**
- Pipeline in progress

**_Purple_**
- There was an error and you should check you logs

**_Yellow_**
- Caution your Pipeline is canceling/ed; find out why

**_White_**
- Checking status of your pipelines

**_Cyan_**
- We got a status, but we don't know what it is

**_Off_**
- Container running? Maybe raise an issue.

## Setup
### Raspberry Pi
- Raspberry Pi 3
- Raspbian OS
  - Connected to the internet
  - SSH enabled
- Docker, follow [this guide](https://dev.to/elalemanyo/how-to-install-docker-and-docker-compose-on-raspberry-pi-1mo) to setup
- (Optional) [Portainer](https://www.portainer.io/)

### Azure DevOps
Configure Azure DevOps parameters for the build-light script via environment variables in the Docker Run script.

| Environment Variable | Description |
| ----------- | ----------- |
| **AzDevOpsOrg** | This is your Azure DevOps organisation/account name. It forms part of the URL you use to access DevOps - https://dev.azure.com/_yourorg_ |
| **AzDevOpsProject** | Your Azure DevOps project name. |
| **AzDevOpsPAT** | Personal Access Token used to authenticate the build-light script to your instance. Follow these instructions to [create a PAT](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=preview-page#create-a-pat)
Build (Read) & Release (Read) permissions are required for build-light |

**Update the environment variables in the Docker Run command below**

### Container
1. SSH to your Raspberry Pi
1. Build Docker image from GitHub project
'docker build github.com/codify/build-light --tag codify/build-light:latest'
1. Run container from the image
'docker run -d \'
'--name codify_build-light \'
'--restart unless-stopped \'
'--device /dev/gpiomem \'
'-e AzDevOpsOrg=orgname \'
'-e AzDevOpsProject=projectname \'
'-e AzDevOpsPAT=secret \'
'codify/build-light'



### RGB LED Lights
TODO

important powershell-iot uses https://github.com/PowerShell/PowerShell-IoT/blob/master/docs/rpi3_pin_layout.md
