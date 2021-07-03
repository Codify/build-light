# build-light

## Project Updated to use a PowerShell Core container for Azure DevOps

build-light displays the status of your Azure DevOps build pipelines and drives better development process by prompting the build status to your team in a simple way.

build-light runs on a Raspberry Pi as a PowerShell Core container that retrieves build status and _with a little effort_ can be hooked up to an LED light strip making the ultimate [headless] build-light.

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
TODO

### RGB LED Lights
TODO
important powershell-iot uses https://github.com/PowerShell/PowerShell-IoT/blob/master/docs/rpi3_pin_layout.md

#### Codify build-light hat pins
$redPin = 1
$greenPin = 3
$bluePin = 5

### Build and Deploy
TODO