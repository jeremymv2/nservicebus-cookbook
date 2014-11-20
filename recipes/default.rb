# Cookbook Name:: nservicebus
# Recipe:: default

# Copyright 2014, Jeremy Miller
#
# All rights reserved - Do Not Redistribute
#
raise 'This recipe only supports Windows' if node['platform_family'] == 'windows'

directory 'C:/Program Files (x86)/Particular Software/' do
  action :create
end

cookbook_file "Packages.Msmq.setup.ps1" do
  path "#{Chef::Config[:file_cache_path]}/Packages.Msmq.setup.ps1"
  action :create
end

cookbook_file "Packages.DTC.setup.ps1" do
  path "#{Chef::Config[:file_cache_path]}/Packages.DTC.setup.ps1"
  action :create
end

cookbook_file "Packages.DTC.RegHelper.cs" do
  path "#{Chef::Config[:file_cache_path]}/RegHelper.cs"
  action :create
end

cookbook_file "Packages.PerfCounters.setup.ps1" do
  path "#{Chef::Config[:file_cache_path]}/Packages.PerfCounters.setup.ps1"
  action :create
end

ruby_block "Exec powershell Packages.Msmq.setup.ps1" do
  block do
    Chef::Resource::RubyBlock.send(:include, Chef::Mixin::PowershellOut)
    result = powershell_out("#{Chef::Config[:file_cache_path]}/Packages.Msmq.setup.ps1")
    Chef::Log.info("powershell exit: #{result.exitstatus}")
    Chef::Log.info("powershell error: #{result.stderr}")
    Chef::Log.info("powershell stdout: #{result.stdout}")
    if result.exitstatus == 0
      File.open("#{Chef::Config[:file_cache_path]}/Packages.Msmq.setup.ps1.RAN", 'w') {|f| f.write(1) }
    end
  end
  not_if { File.exist?("#{Chef::Config[:file_cache_path]}/Packages.Msmq.setup.ps1.RAN") }
end

ruby_block "Exec powershell Packages.DTC.setup.ps1" do
  block do
    Chef::Resource::RubyBlock.send(:include, Chef::Mixin::PowershellOut)
    result = powershell_out("#{Chef::Config[:file_cache_path]}/Packages.DTC.setup.ps1")
    Chef::Log.info("powershell exit: #{result.exitstatus}")
    Chef::Log.info("powershell error: #{result.stderr}")
    Chef::Log.info("powershell stdout: #{result.stdout}")
    if result.exitstatus == 0
      File.open("#{Chef::Config[:file_cache_path]}/Packages.DTC.setup.ps1.RAN", 'w') {|f| f.write(1) }
    end
  end
  not_if { File.exist?("#{Chef::Config[:file_cache_path]}/Packages.DTC.setup.ps1.RAN") }
end

ruby_block "Exec powershell Packages.PerfCounters.setup.ps1" do
  block do
    Chef::Resource::RubyBlock.send(:include, Chef::Mixin::PowershellOut)
    result = powershell_out("#{Chef::Config[:file_cache_path]}/Packages.PerfCounters.setup.ps1")
    Chef::Log.info("powershell exit: #{result.exitstatus}")
    Chef::Log.info("powershell error: #{result.stderr}")
    Chef::Log.info("powershell stdout: #{result.stdout}")
    if result.exitstatus == 0
      File.open("#{Chef::Config[:file_cache_path]}/Packages.PerfCounters.setup.ps1.RAN", 'w') {|f| f.write(1) }
    end
  end
  not_if { File.exist?("#{Chef::Config[:file_cache_path]}/Packages.PerfCounters.setup.ps1.RAN") }
end

windows_package 'Particular Software ServiceInsight' do
  source node['nservicebus']['serviceinsight']['url']
  checksum node['nservicebus']['serviceinsight']['sha256checksum']
  options '/quiet'
  installer_type :custom
  action :install
end

windows_package 'Particular Software ServicePulse' do
  source node['nservicebus']['servicepulse']['url']
  checksum node['nservicebus']['servicepulse']['sha256checksum']
  options '/quiet'
  installer_type :custom
  action :install
end

windows_package 'Particular Software ServiceControl' do
  source node['nservicebus']['servicecontrol']['url']
  checksum node['nservicebus']['servicecontrol']['sha256checksum']
  options '/quiet'
  installer_type :custom
  action :install
end

remote_file "NServiceBus License" do
  source node['nservicebus']['license']['url']
  path "#{Chef::Config[:file_cache_path]}/nsbuslicense.xml"
  action :create_if_missing
end

powershell_script "Add NserviceBus liscense to Registry" do
  code <<-EOH
    $content = Get-Content #{Chef::Config[:file_cache_path]}/nsbuslicense.xml | Out-String
    New-ItemProperty -Path HKLM:\\Software\\ParticularSoftware -Name License -Value "$content" -PropertyType MultiString -Force
  EOH
  not_if { registry_value_exists?("HKEY_LOCAL_MACHINE\\SOFTWARE\\ParticularSoftware",{:name => "License", :type => :multi_string }, :machine) }
end
