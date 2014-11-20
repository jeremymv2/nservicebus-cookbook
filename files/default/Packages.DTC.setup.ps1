Set-StrictMode -Version 2

function SecurityConfigurationUpdated() { 
    $regKey = "Software\Microsoft\MSDTC\Security"
    $valuesToCheck = @("NetworkDtcAccess", "NetworkDtcAccessOutbound", "NetworkDtcAccessTransactions", "XaTransactions")

    $updated = $false
    $changesMade = $false
    Write-Host "Checking if DTC is configured correctly." 

    foreach ($valueName in $HKLM.GetValueNames($regKey) | ? { $valuesToCheck -contains $_ }) {
          $val = $HKLM.ReadValue($regKey, $valueName, $null, $false) 
          if ($val -ne 1) {
              $updated = $true
              $HKLM.WriteValue($regKey, $valueName, 1, [Microsoft.Win32.RegistryValueKind]::DWord)
              Write-Host "Updating KEY: HKLM:$regkey "
              Write-Host "         VALUE: $ValueName was 0 now 1"
          }
    }
    return $updated
}

$currentIdentity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
$currentPrincipal = new-object System.Security.Principal.WindowsPrincipal($currentIdentity)
$adminRole = [ System.Security.Principal.WindowsBuiltInRole]::Administrator

if (-not $currentPrincipal.IsInRole($adminRole)) {
	Write-Host "Elevated permissions are required to run this script"
	Write-Host "Use an elevated command prompt to complete these tasks"
	exit 1
}

$transcribe = ($args.Count -gt 0)

try {
    if ($transcribe) {
        Start-Transcript -Path $args[0] -Force
    }
    if (!$PSVersionTable.PSVersion.Major -lt 3) {
         $PSScriptRoot = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
    }
    Push-Location $PSScriptRoot
    #-- .Net 2 Registry Wrapper which can working with 64bit registry
    $FullPathToRegHelper = (Get-Item -Path ".\" -Verbose).FullName + "\RegHelper.cs"
    if (Test-Path $FullPathToRegHelper) {
        Add-Type -Path $FullPathToRegHelper
    } else {
        throw "RegHelper.cs is missing. This script cannot continue without this file"
    }
    $64bitOS = (Get-WmiObject -Class Win32_Processor | Select-Object -ExpandProperty AddressWidth) -eq 64
    $HKLM = [reghelper]::LocalMachine($64bitOS)
    if (SecurityConfigurationUpdated -or ((Get-Service "MSDTC" | Select-Object -ExpandProperty Status) -ne "Running")) {
        Restart-Service -Name "MSDTC" -Force -Verbose 
    } else {
        Write-Host "MSDTC is configured and running"
    }
}
catch{
	$_ 
	throw $_
}
finally
{
    if ($transcribe) {
        Stop-Transcript
    }
}
