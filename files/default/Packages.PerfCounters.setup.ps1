Set-StrictMode -Version 2

function AddCounters {

    $counterCollection = new-object System.Diagnostics.CounterCreationDataCollection
    $counterCollection.AddRange($counters)

    [void] [System.Diagnostics.PerformanceCounterCategory]::Create($categoryName, "NServiceBus statistics", [System.Diagnostics.PerformanceCounterCategoryType]::MultiInstance, $counterCollection)
    [System.Diagnostics.PerformanceCounter]::CloseSharedResources()    # http://blog.dezfowler.com/2007/08/net-performance-counter-problems.html
    Write-Host -ForegroundColor Green "Successfully Added Counters" | Out-Default
}

function DeleteCategory {
    [void] [System.Diagnostics.PerformanceCounterCategory]::Delete($categoryName)
}

function DoesCategoryExist {
    [System.Diagnostics.PerformanceCounterCategory]::Exists($categoryName)
}

$categoryName = "NServiceBus"

$counters = @(
    (new-object System.Diagnostics.CounterCreationData( "Critical Time", 
                                                        "Age of the oldest message in the queue.", 
                                                        [System.Diagnostics.PerformanceCounterType]::NumberOfItems32)),

    (new-object System.Diagnostics.CounterCreationData( "SLA violation countdown", 
                                                        "Seconds until the SLA for this endpoint is breached.", 
                                                        [System.Diagnostics.PerformanceCounterType]::NumberOfItems32)),

    (new-object System.Diagnostics.CounterCreationData( "# of msgs successfully processed / sec", 
                                                        "The current number of messages processed successfully by the transport per second.",
                                                        [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32)),

    (new-object System.Diagnostics.CounterCreationData( "# of msgs pulled from the input queue /sec", 
                                                        "The current number of messages pulled from the input queue by the transport per second.", 
                                                        [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32)),

    (new-object System.Diagnostics.CounterCreationData("# of msgs failures / sec",
                                                       "The current number of failed processed messages by the transport per second.", 
                                                      [System.Diagnostics.PerformanceCounterType]::RateOfCountsPerSecond32))
)

$currentIdentity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$currentPrincipal = new-object System.Security.Principal.WindowsPrincipal($currentIdentity)
$adminRole = [System.Security.Principal.WindowsBuiltInRole]::Administrator

if (-not $currentPrincipal.IsInRole($adminRole)) {
	Write-Host "Elevated permissions are required to run this script"
	Write-Host "Use an elevated command prompt to complete these tasks"
	exit 1
}

$transcribe = ($args.Count -gt 0)

if ($transcribe) {
    Start-Transcript -Path $args[0] -Force
}
try
{
    Write-Host "Checking Performance Counters"
    if (DoesCategoryExist) {
        $missing = $counters | ? { [System.Diagnostics.PerformanceCounterCategory]::CounterExists($_.CounterName, $categoryName) -eq $false } 
        if ($missing)
        {
            DeleteCategory
            AddCounters    
        }
        else
        {
            Write-Host -ForegroundColor Green "Counter are already installed"
        }
    } else {
        AddCounters
    }
}
finally {
    if ($transcribe) {
        Stop-Transcript
    }
}
