<#  Rob Holme  11/07/2024
.SYNOPSIS
    Copy the module files from the current folder to the PowerShell module home.
.DESCRIPTION
    Copy the module files from the current folder to the PowerShell module home. Ignore dot folders (such as .git).
.PARAMETER Scope
    Install the module to either the current user module path or the all users module path
.EXAMPLE
    Install-Module -Scope CurrentUser
.EXAMPLE
    Install-Module -Scope AllUsers    
#>

[CmdletBinding()]
param (
    [Parameter(
        Position = 0,
        Mandatory = $False,
        ValueFromPipeline = $false,
        ValueFromPipelineByPropertyName = $True
    )]
    [ValidateSet("CurrentUser", "AllUsers")]
    [string] $Scope = "CurrentUser"
)

# Get the module version number from the module manifest file.
# Return $null if the module version can not be parsed.
function Get-ModuleVersion() {
    $moduleManifestFile = Get-ModuleManifestFile
    if ($null -ne $moduleManifestFile) {
        $versionElement = Get-Content $moduleManifestFile | Select-String "ModuleVersion(\s){0,}=(\s){0,}('|"")\d{1,}(\.{1}\d{1,}){0,}"
        if ($versionElement.Matches.Count -ne 1) {
            Write-Error "ModuleVersion element not detected in manifest"
            return $null
        }
        else {
            # match version number string
            $moduleVersionMatch = ([regex]::Match($versionElement, "\d{1,}(\.{1}\d{1,}){0,}"))
            if ($moduleVersionMatch.Success) {
                return $moduleVersionMatch[0].Value
            }
            else {
                return $null
            }
        }
    }
    return $null
}

# Get the module manifest file
# Return $null if not found, or more than 1 manaifest file present in the current directory. 
function Get-ModuleManifestFile {
    try {
        $moduleFile = Get-ChildItem *.psd1
    }
    catch {
        Write-Error "Exception raised while detecting module file, exiting. Use -Debug switch to view exception" 
        Write-Debug $_.Exception
        return $null
    }

    # Make sure only one module file is found, otherwise exit.
    if ($moduleFile.Count -eq 1) {
        return $moduleFile
    } 
    else {
        Write-Error "Single module manifest file expected, none or more than 1 found." 
        return $null
    }
}

# Get the module path based on scope and platform
function Get-PSModulePath {
    param (
        [Parameter(
            Position = 0,
            Mandatory = $True,
            ValueFromPipeline = $False,
            ValueFromPipelineByPropertyName = $False
        )]
        [ValidateSet("CurrentUser", "AllUsers")]
        [string] $moduleScope
    )

    if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
        if ($IsCoreCLR) { 
            $powerShellType = "PowerShell" 
        } 
        else { 
            $powerShellType = "WindowsPowerShell" 
        }
        $localUserDir = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::MyDocuments)) $powerShellType
        $allUsersDir = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::ProgramFiles)) $powerShellType
        
    }
    else {
        # Paths are the same for both Linux and macOS
        $localUserDir = Join-Path (Get-HomeFolder) ".local/share/powershell"
        $allUsersDir = "/usr/local/share/powershell"
    }
    if ($moduleScope -eq "AllUsers") {
        return $allUsersDir
    }
    else {
        return $localUserDir
    }
    
}

# Helper function to get home directory
function Get-HomeFolder {
    $envHome = [System.Environment]::GetEnvironmentVariable("HOME") ?? $null

    if ($null -ne $envHome) {
        return $envHome
    }
    # Return an empty string in this case so the process working directory will be used.
    else {
        return ""
    }
}

function Get-ModuleName {
    $moduleManifestFile = Get-ModuleManifestFile
    if ($null -ne $moduleManifestFile) {
        return $moduleManifestFile.BaseName
    }
    return $null
}


# The module manaifest (and module) is located in the 'module\hl7tools' folder 
cd module\hl7tools

# Obtain the module name, version, and root modules path - all needed to construct the desitination path
$moduleVersion = Get-ModuleVersion
$moduleRootPath = Join-Path -Path (Get-PSModulePath -moduleScope $Scope) -ChildPath "Modules"
$moduleName = Get-ModuleName
Write-Verbose "Module name:`t`t $($moduleName)"
Write-Verbose "Module path:`t`t $($moduleRootPath)"
Write-Verbose "Module version:`t $($moduleVersion)"

if (($null -ne $moduleVersion) -and ($null -ne $moduleRootPath) -and ($null -ne $moduleName)) {
    # construct the module path ($env:psmodulepath\modulename\version)
    $destinationPath = Join-Path -Path $moduleRootPath -ChildPath $moduleName -AdditionalChildPath $moduleVersion
    # create the folder if it does not already exist
    if (!(Test-Path -Path $destinationPath)) {
        Write-Verbose "Creating folder $destinationPath"
        try {
            New-Item $destinationPath -ItemType Directory -ErrorAction Stop| Out-Null
        }
        catch {
            Write-Error "Unable to create directory $destinationPath. Use -Debug switch for details"
            Write-Debug $_.Exception
            exit
        }
    } 
    # copy all files, exclude dot folders (e.g. .git)
    Write-Host "Installing module to $destinationPath"
    try {
        Copy-Item -Path (Join-Path -Path (Get-Location).Path -ChildPath '*') -Recurse -Destination $destinationPath.ToString() -Exclude '.*','images' -Force -ErrorAction Stop
    }
    catch {
        Write-Error "Unable to copy files to $destinationPath. Use -Debug switch for details"
        Write-Debug $_.Exception
        exit
    }
}
    




