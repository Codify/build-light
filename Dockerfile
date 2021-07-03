# Using Microsoft PowerShell Core image
FROM mcr.microsoft.com/powershell:lts-arm32v7-ubuntu-bionic

# Add PowerShell modules
RUN pwsh -Command Set-PSRepository -Name 'PSGallery' -InstallationPolicy Trusted && \
    pwsh -Command Install-Module Microsoft.PowerShell.IoT -Force && \
    pwsh -Command Install-Module PSFramework -Force

# Create profile to import IOT module
RUN mkdir -p /root/.config/powershell/ && \
    echo "Import-Module Microsoft.PowerShell.IoT"> /root/.config/powershell/Microsoft.PowerShell_profile.ps1 && \
    echo "Import-Module PSFramework"> /root/.config/powershell/Microsoft.PowerShell_profile.ps1

# Environment variables for Azure DevOps
ENV AzDevOpsOrg=orgname \
    AzDevOpsProject=projectname \
    AzDevOpsPAT=secret

# Environment variables for RGB LED 
ENV PI_RED=1 \
    PI_GREEN=3 \
    PI_BLUE=5

# Copy build-light script
WORKDIR /app/codify/build-light
COPY codify_build-light.ps1 .

# Start
ENTRYPOINT ["pwsh", "-file", "/app/codify/build-light/codify_build-light.ps1"]

LABEL org.label-schema.name = "codify/build-light" \
      org.label-schema.url="https://www.codify.com/build-light" \
      org.label-schema.vcs-url = "https://github.com/Codify/build-light" \
      org.label-schema.vendor = "Codify Pty Ltd" \
      org.label-schema.docker.cmd= "docker run --device /dev/gpiomem -e AzDevOpsOrg=orgname -e AzDevOpsProject=projectname -e AzDevOpsPAT=secret" \
      org.label-schema.docker.params = "AzDevOpsOrg=orgname, AzDevOpsProject=projectname, AzDevOpsPAT=secret, PI_RED=1, PI_GREEN=3, PI_BLUE=5"
