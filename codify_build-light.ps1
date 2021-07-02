function Set-Led {
    #Requires -Modules Microsoft.PowerShell.IoT
    param (
        [bool] [Parameter(Position=0)] $red = 0,
        [bool] [Parameter(Position=1)] $green = 0,
        [bool] [Parameter(Position=2)] $blue = 0,
        [int] $redpin = 1,
        [int] $greenpin = 3,
        [int] $bluepin = 5
    )

    If ($red) {
        # On
        Set-GpioPin -Id $redpin -Value High
    } else {
        Set-GpioPin -Id $redpin -Value Low
    }
    
    If ($green) {
        # On
        Set-GpioPin -Id $greenpin -Value High
    } else {
        Set-GpioPin -Id $greenpin -Value Low
    }

    If ($blue) {
        # On
        Set-GpioPin -Id $bluepin -Value High
    } else {
        Set-GpioPin -Id $bluepin -Value Low
    }

}

function Get-PipelineResult {
    param (
        [string] [Parameter(Position=0)] $AzureDevOpsOrganization = "",
        [string] [Parameter(Position=1)] $AzureDevOpsProject = "",
        [string] [Parameter(Position=2)] $AzureDevOpsPAT = ""
    )

    $AzureDevOpsApiVersion = "api-version=6.1-preview.1"

    # Result object
    $result = [PSCustomObject]@{
        error       = $false
        unknown     = $false
        inProgress  = $false
        cancel      = $false
        failed      = $false
        succeeded   = $false
    }

    $AzureDevOpsAuthenicationHeader = @{Authorization = 'Basic ' + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$($AzureDevOpsPAT)")) }

    $Uri = "https://dev.azure.com/" + $AzureDevOpsOrganization + "/" + $AzureDevOpsProject + "/_apis/pipelines?" + $AzureDevOpsApiVersion

    $pipelines = Invoke-RestMethod -Uri $Uri -Method get -Headers $AzureDevOpsAuthenicationHeader 


    foreach ($pipeline in $pipelines.value) {
        # https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/get?view=azure-devops-rest-6.1
        $Uri = "https://dev.azure.com/" + $AzureDevOpsOrganization + "/" + $AzureDevOpsProject + "/_apis/pipelines/" + $pipeline.id + "/runs" + "?" + $AzureDevOpsApiVersion
        
        $runs = Invoke-RestMethod -Uri $Uri -Method get -Headers $AzureDevOpsAuthenicationHeader 

        foreach ($run in $runs.value) {
            # Find the status of the latest run
            If ($run.id -gt $runIdLatest) {
                $runIdLatest = $run.id

                # Run State https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/list?view=azure-devops-rest-6.1#runstate
                switch ($run.state) {
                    "inProgress" {$result.inProgress = $true; break}
                    "canceling" {$result.cancel = $true; break}
                    "unknown" {$result.unknown = $true; break}
                    "completed" {
                        # Run Result - https://docs.microsoft.com/en-us/rest/api/azure/devops/pipelines/runs/list?view=azure-devops-rest-6.1#runresult
                        switch ($run.result) {
                            "canceled" {$result.cancel = $true; break}
                            "failed" {$result.failed = $true; break}
                            "succeeded" {$result.succeeded = $true; break}
                            "unknown" {$result.unknown = $true; break}
                            Default {$result.error = $true}
                        }
                    }
                    Default {$result.error = $true}
                }
            }
        }
    }

    return $result
}

function Set-CodifyBuiltLight {
    param (
        [string] [Parameter(Position=0)] $AzureDevOpsOrganization = "",
        [string] [Parameter(Position=1)] $AzureDevOpsProject = "",
        [string] [Parameter(Position=2)] $AzureDevOpsPAT = ""
    )

    # Update loop, set LED white
    Set-Led  1 1 1
    write-host "checking..."

    $pipelineResults = Get-PipelineResult $AzureDevOpsOrganization $AzureDevOpsProject $AzureDevOpsPAT

    # Convert $pipelineResults to an array to support switch
    $pipelineResultsArray = `
        $(if($pipelineResults.error) {"error"}), `
        $(if($pipelineResults.unknown) {"unknown"}), `
        $(if($pipelineResults.inProgress) {"inProgress"}), `
        $(if($pipelineResults.cancel) {"cancel"}), `
        $(if($pipelineResults.failed) {"failed"}), `
        $(if($pipelineResults.succeeded) {"succeeded"})

    # Enum results and set LEDs
        switch ($pipelineResultsArray) {
        "error" {Set-Led  1 0 1; break}
        "unknown" {Set-Led  0 1 1; break}
        "inProgress" {Set-Led  0 0 1; break}
        "cancel" {Set-Led  1 1 0; break}
        "failed" {Set-Led  1 0 0; break}
        "succeeded" {Set-Led  0 1 0; break}
        Default {Set-Led  1 0 1} # Error
    }
}

# Create timer instance
$timer = New-Object Timers.Timer
$timer.Interval = 300000 # 5 minutes
$timer.AutoReset = $true
$timer.Enabled = $true

## register your event
Register-ObjectEvent -InputObject $timer -EventName Elapsed -SourceIdentifier Set-CodifyBuiltLight -Action {Set-CodifyBuiltLight $AzureDevOpsOrganization $AzureDevOpsProject $AzureDevOpsPAT}
