NServiceBus Cookbook
========================

This cookbook configures NServiceBus on a Windows system

Notes used for install:

Platform Tools required: serviceinsight, servicepulse, servicecontrol, nservicebus

Not Required: servicematrix

Offline Install howto: (http://docs.particular.net/platform/installer/offline)

msmq (https://github.com/Particular/Packages.Msmq/blob/master/src/tools/setup.ps1)

msdtc (https://github.com/Particular/Packages.DTC/blob/master/src/tools/setup.ps1,  https://github.com/Particular/Packages.DTC/blob/master/src/tools/RegHelper.cs)

performance counters (https://github.com/Particular/Packages.PerfCounters/blob/master/src/tools/setup.ps1)

Individual installers for tools above can be executed unattended like so: ```Particular.ServiceControl-1.3.0.exe /quiet```

Installing the license: (http://docs.particular.net/nservicebus/license-management)

Example attributes.rb:
```
default['nservicebus']['license']['url'] = "http://artifactory.acme.com/artifactory/chef/nServicebus/License/License.xml"
default['nservicebus']['serviceinsight']['sha256checksum'] = '0806ded164cb7f19d1f7c2b2e500f3dd68c7ad95a5e77c648713be551b0c3acf'
default['nservicebus']['serviceinsight']['url'] = "https://github.com/Particular/ServiceInsight/releases/download/1.2.4/Particular.ServiceInsight-1.2.4.exe"
default['nservicebus']['servicepulse']['sha256checksum'] = 'c3d559042dcb279f9b948e3293ce693bcbdff61987e2fe8a8b4076c2d869c110'
default['nservicebus']['servicepulse']['url'] = "https://github.com/Particular/ServicePulse/releases/download/1.1.1/Particular.ServicePulse-1.1.1.exe"
default['nservicebus']['servicecontrol']['sha256checksum'] = '8e37a1cc27bc66fdad8eb9d7fabde77d765b03567aaefc3f59db540fd98ad3db'
default['nservicebus']['servicecontrol']['url'] = "https://github.com/Particular/ServiceControl/releases/download/1.4.0/Particular.ServiceControl-1.4.0.exe"```
