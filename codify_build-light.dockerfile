# Using Microsoft PowerShell Core image
FROM mcr.microsoft.com/powershell:lts-arm32v7-ubuntu-bionic

# Add PowerShell IOT module for controlling Raspberry Pi GPIO
RUN pwsh -Command Set-PSRepository -Name 'PSGallery' -InstallationPolicy Trusted
RUN pwsh -Command Install-Module Microsoft.PowerShell.IoT -Force

# Create profile to import IOT module
RUN mkdir -p /root/.config/powershell/
RUN echo "Import-Module Microsoft.PowerShell.IoT"> /root/.config/powershell/Microsoft.PowerShell_profile.ps1
