# build-light

build-light displays the status of a Visual Studio Online build and drives awareness across the team in a simple way.

A Universal App that can run on Windows 10 and Windows 10 IoT Core, and __with a little effort__ can be hooked up to an LED light strip making the ultimate [headless] build-light

[Read More](http://www.codify.com/visual-studio-build-monitor-first-play-windows-iot/)

We have tested the app running on a Raspberry Pi 2 with Windows 10 IoT Core RTM

## So many colours
**_Red_**
Uh-oh build is broken

**_Green_**
Build is good

**_Blue_**
Build is in progress

**_Purple_**
Build is unknown. Often seen when a build is queued but yet to be allocated to a build agent

**_Yellow_**
Build kinda worked [partially succeeded]

**_White_**
Polling

**_Off_**
Check the power? Maybe raise an issue.

## Installation

### Get your PC ready
You will need to follow these [instructions](https://ms-iot.github.io/content/en-US/win10/SetupPCRPI.htm) to get you PC ready to compile build-light

### Get Windows 10 IoT running
Not finished yet, get your [RPi2 running Windows 10 IoT](https://ms-iot.github.io/content/en-US/win10/SetupRPI.htm)

### Build and Deploy

Before you start, your RPi2 must be:
* On
* Has an IP address
* Visible in WindowsIoTCoreWatcher
 
 
1. Load the Codify.VisualStudioOnline.BuildLight.sln into Visual Studio 2015
2. Change the build type to **Release** and platform to **ARM**
3. Set the target to **Remote Machine** and specify the **IP address** of your RPi2
4. Click **Deploy**

## All your exceptions belong to us
The app will send telemetry (device and status) and exceptions are sent to our own Azure Application Insights. This helps us help you :)

Opt-out by removing the code before you deploy.

## Feedback
Always welcome please raise any issues

