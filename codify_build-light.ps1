#Requires -Modules PSFramework

function Set-Led {
    #Requires -Modules Microsoft.PowerShell.IoT
    param (
        [string][Parameter(Mandatory, Position=0)] [ValidateSet('White', 'Black', 'Red', 'Green', 'Blue', 'Purple', 'Yellow', 'Cyan')] $Colour,
        [int] $redpin = $env:PI_RED,
        [int] $greenpin = $env:PI_GREEN,
        [int] $bluepin = $env:PI_BLUE
    )

    Write-PSFMessage -Level Output -Message "Setting LED to $Colour"
    
    # See RGB colour ven diagram https://br24.com/en/rgb-cmyk-differences/
    switch ($Colour) {
        "White" {
            Set-GpioPin -Id $redpin -Value High
            Set-GpioPin -Id $greenpin -Value High
            Set-GpioPin -Id $bluepin -Value High
        }
        "Black" {
            Set-GpioPin -Id $redpin -Value Low
            Set-GpioPin -Id $greenpin -Value Low
            Set-GpioPin -Id $bluepin -Value Low
        }
        "Red" {
            Set-GpioPin -Id $redpin -Value High
            Set-GpioPin -Id $greenpin -Value Low
            Set-GpioPin -Id $bluepin -Value Low
        }
        "Green" {
            Set-GpioPin -Id $redpin -Value Low
            Set-GpioPin -Id $greenpin -Value High
            Set-GpioPin -Id $bluepin -Value Low
        }
        "Blue" {
            Set-GpioPin -Id $redpin -Value Low
            Set-GpioPin -Id $greenpin -Value Low
            Set-GpioPin -Id $bluepin -Value High
        }
        "Purple" {
            Set-GpioPin -Id $redpin -Value High
            Set-GpioPin -Id $greenpin -Value Low
            Set-GpioPin -Id $bluepin -Value High
        }
        "Yellow" {
            Set-GpioPin -Id $redpin -Value High
            Set-GpioPin -Id $greenpin -Value High
            Set-GpioPin -Id $bluepin -Value Low
        }
        "Cyan" {
            Set-GpioPin -Id $redpin -Value Low
            Set-GpioPin -Id $greenpin -Value High
            Set-GpioPin -Id $bluepin -Value High
        }
    }
}

function Get-PipelineResult {
    param (
        [string] [Parameter(Position=0)] $AzureDevOpsOrganization = "",
        [string] [Parameter(Position=1)] $AzureDevOpsProject = "",
        [string] [Parameter(Position=2)] $AzureDevOpsPAT = ""
    )

    Write-PSFMessage -Level Output -Message "Running Get-PipelineResult function"
   
    # Results object
    $Results = [PSCustomObject]@{
        error       = $false
        unknown     = $false
        inProgress  = $false
        cancel      = $false
        failed      = $false
        succeeded   = $false
    }

    $AzureDevOpsAuthenicationHeader = @{Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($AzureDevOpsPAT)")) }
    $AzureDevOpsApiVersion = "api-version=6.1-preview.1"

    Write-PSFMessage -Level Output -Message "Getting project pipelines"

    $Uri = "https://dev.azure.com/" + $AzureDevOpsOrganization + "/" + $AzureDevOpsProject + "/_apis/pipelines?" + $AzureDevOpsApiVersion

    Write-PSFMessage -Level Output -Message "Calling $Uri"

    $Pipelines = Invoke-RestMethod -Uri $Uri -Method get -Headers $AzureDevOpsAuthenicationHeader 

    foreach ($Pipeline in $Pipelines.value) {
        Write-PSFMessage -Level Output -Message "Getting $($Pipeline.Name) status"
        
        # https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/get?view=azure-devops-rest-6.1
        $Uri = "https://dev.azure.com/" + $AzureDevOpsOrganization + "/" + $AzureDevOpsProject + "/_apis/pipelines/" + $Pipeline.id + "/runs" + "?" + $AzureDevOpsApiVersion

        Write-PSFMessage -Level Output -Message "Calling $Uri"
        
        $Runs = Invoke-RestMethod -Uri $Uri -Method get -Headers $AzureDevOpsAuthenicationHeader 

        $Latest = 0

        foreach ($Run in $Runs.value) {
            # Find the status of the latest run
            If ($Run.id -gt $Latest) {
                $Latest = $Run.id

                # Run State https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/list?view=azure-devops-rest-6.1#runstate
                switch ($Run.state) {
                    "inProgress" {$Results.inProgress = $true; break}
                    "canceling" {$Results.cancel = $true; break}
                    "unknown" {$Results.unknown = $true; break}
                    "completed" {
                        # Run Result - https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/list?view=azure-devops-rest-6.1#runresult
                        switch ($run.result) {
                            "canceled" {$Results.cancel = $true; break}
                            "failed" {$Results.failed = $true; break}
                            "succeeded" {$Results.succeeded = $true; break}
                            "unknown" {$Results.unknown = $true; break}
                            Default {$Results.error = $true}
                        }
                    }
                    Default {$Results.error = $true}
                }
            }
        }
    }
    Write-PSFMessage -Level Output -Message "Returning results"
    return $Results
}

function Set-CodifyBuiltLight {
    param (
        [string] $AzureDevOpsOrganization = $env:AzDevOpsOrg,
        [string] $AzureDevOpsProject = $env:AzDevOpsProject,
        [string] $AzureDevOpsPAT = $env:AzDevOpsPAT
    )

    Write-PSFMessage -Level Output -Message "Azure DevOps Organisation - $AzureDevOpsOrganization"
    Write-PSFMessage -Level Output -Message "Azure DevOps Project - $AzureDevOpsProject"
    Write-PSFMessage -Level Output -Message "Azure DevOps PAT - $($AzureDevOpsPAT.Substring(0, [Math]::Min($AzureDevOpsPAT.Length, 3)))*************" # This is a secret

    Write-PSFMessage -Level Output -Message "Running Set-CodifyBuiltLight function"

    # Update loop, set LED white
    Write-PSFMessage -Level Output -Message "Checking pipelines"
    Set-Led -Colour White
    
    $Results = Get-PipelineResult $AzureDevOpsOrganization $AzureDevOpsProject $AzureDevOpsPAT

    while ($true) {
        # The LED can only show one status (colour). This block establishs the result hierarchy and breaks from the loop once set
        # Top of the hierarchy
        if ($Results.error) {
            Write-PSFMessage -Level Output -Message "A pipeline show an ERROR result"
            Set-Led -Colour Purple
            break
        }
        if ($Results.unknown) {
            Write-PSFMessage -Level Output -Message "A pipeline show an UNKNOWN result"
            Set-Led -Colour Cyan
            break
        }
        if ($Results.inProgress) {
            Write-PSFMessage -Level Output -Message "A pipeline show a INPROGRESS result"
            Set-Led -Colour Blue
            break
        }
        if ($Results.cancel) {
            Write-PSFMessage -Level Output -Message "A pipeline show a CANCEL result"
            Set-Led -Colour Yellow
            break
        }
        if ($Results.failed) {
            Write-PSFMessage -Level Output -Message "A pipeline show a FAILED result"
            Set-Led -Colour Red
            break
        }
        # Bottom of the hierarchy
        if ($Results.succeeded) {
            Write-PSFMessage -Level Output -Message "A pipeline show a SUCCESS result"
            Set-Led -Colour Green
            break
        }
        
        # Something else went wrong set LED to error
        Write-PSFMessage -Level Output -Message "Unable to process pipeline result"
        Set-Led -Colour Purple
        
        # Finally break to end loop
        break
    }
}

# Main
while ($true) {
    Set-CodifyBuiltLight
    Start-Sleep -Seconds 300
}
