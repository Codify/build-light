# build-light

![Image of build-light](http://www.codify.com/wp-content/uploads/2015/08/lights.jpg)

build-light displays the status of a Visual Studio Online build and drives a better development process by visually indicating the build quality to your team in a simple way.

build-light is a Windows 10 Universal App designed to run on Windows 10 IoT Core, and _with a little effort_ can be hooked up to an LED light strip making the ultimate [headless] build-light.

Read more on our [blog](http://www.codify.com/visual-studio-build-monitor-first-play-windows-iot/).

## So many colours
**_Red_**
- Uh-oh, build is broken

**_Green_**
- Build is good

**_Blue_**
- Build is in progress

**_Purple_**
- Build is unknown. Often seen when a build is queued but yet to be allocated to a build agent

**_Yellow_**
- Build kinda worked [partially succeeded]

**_White_**
- Polling

**_Off_**
- Check the power? Maybe raise an issue.

## Installation

#### Get your PC ready
You will need to follow these [instructions](https://ms-iot.github.io/content/en-US/win10/SetupPCRPI.htm) to get you PC ready to compile build-light

#### Get Windows 10 IoT running
Not finished yet, get your [RPi2 running Windows 10 IoT](https://ms-iot.github.io/content/en-US/win10/SetupRPI.htm)

#### Build and Deploy

Before you start, your RPi2 must be:
* On and at the Windows 10 IoT launcher
* Have an IP address
* Visible in WindowsIoTCoreWatcher

With this in place you are now ready to deploy build-light.

1. Load the **Codify.VisualStudioOnline.BuildLight.sln** into Visual Studio 2015
2. Change the build type to **Release** and platform to **ARM**
3. Set the target to **Remote Machine** and specify the **IP address** of your RPi2
4. Click **Deploy**

## A little more effort

Coming soon - how to hook up an RGB LED light strip to build-light

## All your exceptions belong to us
The app will send telemetry (device and status) and exceptions are sent to our own Azure Application Insights. This helps us help you :)

Opt-out by removing the code before you deploy.