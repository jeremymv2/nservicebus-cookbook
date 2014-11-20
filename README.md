NServiceBus Cookbook
========================

This cookbook configures NServiceBus on a Windows system

These are the notes I used:

platform tools required:
  serviceinsight
  servicepulse
  servicecontrol
  nservicebus

not required:
  servicematrix

offline:
  http://docs.particular.net/platform/installer/offline
  msmq https://github.com/Particular/Packages.Msmq/blob/master/src/tools/setup.ps1
  msdtc https://github.com/Particular/Packages.DTC/blob/master/src/tools/setup.ps1
        https://github.com/Particular/Packages.DTC/blob/master/src/tools/RegHelper.cs
  performance counters https://github.com/Particular/Packages.PerfCounters/blob/master/src/tools/setup.ps1

  individual installers for tools above:
    For example: Particular.ServiceControl-1.3.0.exe /quiet APPDIR=“C:\MyTagretDirectory”

    serviceinsight (https://github.com/Particular/ServiceInsight/releases)
      https://github.com/Particular/ServiceInsight/releases/download/1.2.4/Particular.ServiceInsight-1.2.4.exe
    servicepulse (https://github.com/Particular/ServicePulse/releases)
      https://github.com/Particular/ServicePulse/releases/download/1.1.1/Particular.ServicePulse-1.1.1.exe
    servicecontrol (https://github.com/Particular/ServiceControl/releases)
      https://github.com/Particular/ServiceControl/releases/download/1.4.0/Particular.ServiceControl-1.4.0.exe

  installing the license
    http://docs.particular.net/nservicebus/license-management
