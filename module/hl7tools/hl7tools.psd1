#
# Module manifest for module 'Hl7Tools'
#
# Generated by: Rob Holme
#
# Generated on: 28/07/2016
#

@{

# Script module or binary module file associated with this manifest
RootModule = if($PSEdition -eq 'Core')
{
    'netcore\hl7tools.dll'
}
else # Desktop
{
    'netframework\hl7tools.dll'
}

# Version number of this module.
ModuleVersion = '1.7.3'

# ID used to uniquely identify this module
GUID = 'd5c6a068-96fd-49cb-aa72-92c6e3d090f2'

# Author of this module
Author = 'Rob Holme'

# Company or vendor of this module
CompanyName = 'Unknown'

# Copyright statement for this module
Copyright = '(c) 2020 Rob Holme. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Powershell module for analysing and editing HL7 v2.x files'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '4.0'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = 'hl7tools.format.ps1xml'

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @()

# Functions to export from this module
FunctionsToExport = @()

# Cmdlets to export from this module
CmdletsToExport = @('Remove-HL7Identifiers',
	'Remove-HL7Item',
	'Select-HL7Item',
	'Send-HL7Message',
	'Set-HL7Item',
	'Split-HL7BatchFile',
	'Receive-HL7Message',
	'Show-HL7MessageTimeline')

# Variables to export from this module
VariablesToExport = @()

# Aliases to export from this module
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('HL7','PSEdition_Desktop', 'PSEdition_Core', 'Windows', 'Linux', 'MacOS')

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/RobHolme/HL7-Powershell-Module/blob/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/RobHolme/HL7-Powershell-Module'

        # A URL to an icon representing this module.
        IconUri = 'https://github.com/RobHolme/vscode-hl7tools/raw/master/images/hl7tools-icon.png'

        # ReleaseNotes of this module
		ReleaseNotes = '## 1.7.2 - Fixed IconURI' 
    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
HelpInfoURI = 'https://github.com/RobHolme/HL7-Powershell-Module/blob/master/readme.md#cmdlet-usage'

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}

