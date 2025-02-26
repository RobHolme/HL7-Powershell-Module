# Project Description
This PowerShell module is a small collection of CmdLets to manipulate HL7 v2.x files, including:

* __Select-HL7Item__: queries a HL7 file (or list of files) to return a specific HL7 Item.
* __Set-HL7Item__: changes the value of an existing item within a HL7 file
* __Remove-HL7Item__: delete the value for a nominated item in the HL7 message.
* __Send-HL7Messsage__: send a HL7 file to a remote host via TCP.
* __Remove-HL7Identifiers__: remove personal identifiers for patients and next of kin from HL7 files.
* __Split-HL7BatchFile__: Split a batch file containing multiple messages into a separate file per message.
* __Receive-HL7Message__: Receive a HL7 v2.x message via a TCP connection
* __Show-HL7MessageTimeline__: List messages chronologically based on the header timestamp (MSH-7)

# Installation Instructions
You may install the module from existing build releases, from the Powershell Gallery, or build from source. 
## Install from Powershell Gallery
From a powershell console, run:

```Powershell
Install-Module -Name hl7tools
```
## Install from GitHub releases
Follow these instructions if you plan on installing the module from a prebuilt release instead of building from source.
1.  Download the zip file from the latest release from: https://github.com/RobHolme/HL7-Powershell-Module/releases. 
2.  Extract the 'hl7tools' folder from the archive to your PowerShell modules folder. You can confirm the location of your modules folder by running `$env:PSModulePath` from a PowerShell console. 
2.  Run the PowerShell command `Unblock-File` against all files extracted from the download.
3.  Open a powershell console, the module will be imported when the console opens and the CmdLets available to use. Alternatively, if you don't wish to load the module with all new powershell sessions, run `import-module .\hl7tools\hl7tools.psd1` to use the module for the current session only.

## Build from source
The solution will build versions for .Net 6.0 (Microsoft Powershell 6+) and .Net Framework 4.52 (Windows Powershell 5.1). The powershell module will load the relevant dll based on the Powershell environment it is run from. If you only wish to target .Net standard or .Net Framework alone, the .csproj file will need to be edited accordingly to specify a single TargetFramework.
1. Install the .Net 6 SDK and the .Net Framework 4.5.2 SDK. Install instructions for the SDK for each platform are available from:
* Linux: https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x
* Windows: https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x
* MacOS: https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x
1. Open a command console, navigate to the root folder of this solution (containing hl7tools.csproj). Run the following commands to build the release and publish to the /module/lib folder:
   
`dotnet publish --configuration release --framework net6.0 --output .\module\hl7tools\lib\net6.0\`

`dotnet publish --configuration release --framework net48 --output .\module\hl7tools\lib\net48\`

(or run the .\publish.cmd batch file)

2. The build process will copy each version of hl7tools.dll to subfolders of `module\hl7tools\lib\`. The contents of the `\module\hl7tools` folder will contain the Powershell Module.
3. Move the `\hl7tools` folder to your Powershell Module Path (query the path from the environment variable by running `$env:PSModulePath` from a powershell console). Restart powershell. Alternatively, if you don't wish to load the module with all new powershell sessions, run `import-module .\hl7tools\hl7tools.psd1` to use the module for the current session only.

# CmdLet Usage 
## Select-HL7Item
This CmdLet returns the values of specific HL7 items from a file (or group of files). This is intended to aid with integration work to validate the values of fields over a large sample of HL7 messages. Some basic filtering is available to only include specific messages from a larger sample.

Output can be piped to other powershell CmdLets to refine the results. e.g. returning only the unique values across a range of files:

```
Select-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string[]> [-Filter <string[]>] [[-Encoding] <String>] [<CommonParameters>]

Select-HL7Item [-Path] <string[]> [-ItemPosition] <string[]> [-Filter <string[]>] [[-Encoding] <String>] [<CommonParameters>]
```
Example:

* `Select-HL7Item -Path c:\test -ItemPosition PID-3.1` 
Display the Patient ID values for all hl7 files in c:\test
* `get-childitem *.hl7 | Select-HL7Item -ItemPosition PID-3.1` 
Pipe all files with .hl7 extentions to the CmdLet, return Patient ID values
* `Select-HL7Item -Path c:\test -ItemPosition PID-5 -Filter PV1-2=INPATIENT` 
Display all PID-5 values where the value of PV1-2 is INPATIENT
* `(Select-HL7Item -Path c:\test -ItemPosition PID-3.1).ItemValue | Sort-Object -Unique` 
Only return unique values
* `Select-HL7Item -Path c:\test\*.hl7 -ItemPosition PID-3.1,PID-5` 
Display the Patient ID and Patient Name values for all hl7 files in c:\test

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't identified as a HL7 v2.x file. This parameter accepts a list of files, separate each file file with a ','

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-ItemPosition \<string[]\>__: A string, or list of strings, identifying the location of the item(s) in the HL7 message you wish to retrieve the value for.

ItemPosition Examples:
* `-ItemPosition PID`  All PID segments
* `-ItemPosition PID-3`  The value for the 3rd PID field. If this item is a list all PID-3 values in the list will be returned.
* `-ItemPosition PID-3[1]`  The value of the 3rd PID field, but for only the first occurrence of PID-3 (assuming PID-3 is a list)
* `-ItemPosition PID-3.1`  The value for the 1st component of the PID-3 field. If the component belongs  to a list  of fields, all PID-3.1 components will be returned. 
* `-ItemPosition PID-3[2].1` The value for the 1st component of the second occurrence of the PID-3 field (assuming the field is a list).
* `-ItemPosition PID-3.1.1` The value for the first sub-component of the first component of the 3rd field of the PID segment. 
* `-ItemPosition PID-3.1,PID-5` The value for the 1st component of the PID-3 field, and the value of the PID-5 field.

__-Filter <string[]>__: Only includes messages where a HL7 item equals a specific value. The format is: HL7Item=value. The HL7Item part of the filter is of the same format as the -ItemPosition parameter. The -Filter parameter accepts a list of filters (separated by a comma). If a list of filters is provided then a message must match all conditions to be included. 

If a filter references a repeating element, then a match is returned as successful if any of the repeats match the condition. A position within the repeating element can be nominated if the order is known. 

>The filter is intended to filter at the message/file level when querying a large number of files (to include/exclude the messages from the results). It will not filter on individual repeating segments or fields within a message - the results will still include __all__ fields requested from the messages matched by the filter condition. e.g. if you filter on OBX-3=NM, all OBX-3 values from all OBX segments will be returned, but only for messages queried that contain at least 1 OBX-3 value equal to 'NM'. Message not matching the filter condition are excluded from the results.

Filter Examples:
* `-Filter MSH-9=ADT^A05` This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.
* `-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT` This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT" 
* `-Filter PID-3[2].1=Z9999` This filter would only include messages with the second repeat of the PID-3 fields matching 'Z9999'

__-Encoding \<string\>__: Specify the character encoding. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

> Note: Items that cannot be located in the message will display a warning. 
> To suppress the warning messages use the `-WarningAction Ignore` common parameter.

## Send-HL7Message
Send a HL7 v2.x message from a file (or list of files) via TCP to a remote endpoint. Messages are framed using MLLP (Minimal Lower Layer Protocol).

```
    Send-HL7Message [-LiteralPath] <String[]> [-HostName] <String> [-Port] <Int32> [[-Delay] <Int32>] [[-Encoding] {UTF-8 | ISO-8859-1}] [-NoACK] [-SkipCertificateCheck] [-UseTLS] [-ProgressAction <ActionPreference>] [<CommonParameters>]

    Send-HL7Message [-MessageString] <String[]> [-HostName] <String> [-Port] <Int32> [[-Delay] <Int32>] [[-Encoding] {UTF-8 | ISO-8859-1}] [-NoACK] [-SkipCertificateCheck] [-UseTLS] [-ProgressAction <ActionPreference>] [<CommonParameters>]

    Send-HL7Message [-Path] <String[]> [-HostName] <String> [-Port] <Int32> [[-Delay] <Int32>] [[-Encoding] {UTF-8 | ISO-8859-1}] [-NoACK] [-SkipCertificateCheck] [-UseTLS] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```
example:

`Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\message1.hl7`

`Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\*.hl7 -Encoding ISO-8859-1`

`Send-Hl7Message -Hostname secure-hl7.company.com -Port 5000 -Path c:\HL7Files\*.hl7 -Encoding UTF-8 -UseTLS`

`$msg = get-content c:\hl7\test.hl7; Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -MessageString $msg`

### Parameters
__-Hostname \<string\>__:  The  IP Address or host name of the remote host to send the HL7 message to

__-Port \<int\>__: The TCP port of the listener on the remote host to send the HL7 message to.

__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' (no spaces). 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-MessageString <String[]>__: Supply the message contents as a string value instead of a filename. Accepts either a string[] array [string[]] (one one segment per array item) or a single [string] value (with Carriage Returns between each         segment in a single string.)

__-NoACK__: This switch instructs the CmdLet not to wait for an ACK response from the remote host.

__-Delay \<int\>__: The delay (in seconds) between sending each message.

__-Encoding \<string\>__: Specify the character encoding used when sending the message. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

__-UseTLS__: Secure the connection to the remote host via TLS. Requires the remote host to support TLS. Experimental, not widely tested.

__-SkipCertificateCheck__: Ignore TLS certificate validation errors (trust, validity, etc). Used in conjunction with -UseTLS switch, has not impact otherwise.

## Remove-HL7Identifiers
Removes names, addresses and other personally identifiable details from a HL7 v2.x Message.  

> Note:  Identifier codes in {"PID-3"} remain. This masks the following fields from a HL7 v2.x file:
> * All PID fields except {"PID-1, PID-2, PID-3"}
> * All NK1 fields except {"NK1-1, NK1-3"}
> * All IN1 fields 
> * All IN2 fields 

By default a new file is saved in the same location as the original file with "MASKED_" prefixed  to the file name. If the  optional "-OverwriteFile" switch is  set the original file will be modified.

```
Remove-HL7Identifiers [-LiteralPath] <string[]> [[-CustomItemsList] <string[]>] [[-MaskChar] <char>] [[-OverwriteFile]] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]

Remove-HL7Identifiers [-Path] <string[]> [[-CustomItemsList] <string[]>] [[-MaskChar] <char>] [[-OverwriteFile]] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```
examples:

`Remove-HL7Identifiers -Path c:\hl7files\*.hl7 -OverwriteFile`

`Remove-Hl7Identifiers -Path c:\test.txt`

`Remove-HL7Identifiers -Path c:\test\testfile.hl7 -CustomItemsList PID-3.1,NK1,DG1`

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory, or a list of files. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-CustomItemsList <string[]>__: A list of the HL7 items to mask instead of the default segments and fields. List each item in a comma separated list (no spaces). Items can include a mix of segments (e.g. PV1), Fields (e.g. PID-3), Components (e.g. {"PID-3.1"}) or Subcomponents (e.g. {"PV1-42.2.2"}).  For repeating segments and fields a specific repeat can be identified (index starts at 1). e.g. {"PID-3[1].1"} will mask out the first occurrence of {"PID-3.1"}, leaving other repeats of {"PID-3.1"} unchanged. Likewise {"IN1[2]"} will mask out the second occurrence of the IN1 segments only.

CustomItemsList Examples:
* `-CustomItemsList PID-3.1,PID-5,PID-13,NK1`


__-MaskChar \<char\>__: The character used  to mask the identifier fields,  Defaults to '*' if not supplied

__-OverwriteFile__: If this  switch is set, the original file is modified.

__-Encoding \<string\>__: Specify the character encoding. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

## Set-HL7Item

This CmdLet changes the value of an existing HL7 item from a file (or group of files). Some basic filtering is available to only include specific messages within a large sample. By default only the first occurrence of an item will be changed unless the -UpdateAllRepeats switch is set.

```
Set-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string> [-Value] <string> [-Filter <string[]>]  [-UpdateAllRepeats] [-AppendToExistingValue] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]

Set-HL7Item [-Path] <string[]> [-ItemPosition] <string> [-Value] <string> [-Filter <string[]>]  [-UpdateAllRepeats] [-AppendToExistingValue] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```
examples:

`Set-HL7Item -Path c:\hl7files\hl7file.txt -ItemPosition PID-3.1 -Value A1234567`

`Set-HL7Item -Path c:\hl7files\*.hl7 -ItemPosition PV1-3.1 -Value A1234567 -Filter PV1-2=INPATIENT`

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-ItemPosition \<string\>__: A string identifying the location of the item in the HL7 message you wish to  retrieve the value for. 

ItemPosition Examples
* `-ItemPosition PID` All PID segments
* `-ItemPosition PID-3` The value for the 3rd PID field. If this item is a list all PID-3 values in the list will be returned.
* `-ItemPosition PID-3[1]` The value of the 3rd PID field, but for only the first occurrence of PID-3 (assuming PID-3 is a list)
* `-ItemPosition PID-3.1` The value for the 1st component of the PID-3 field. If the component belongs  to a list  of fields, all PID-3.1 components will be returned. 
* `-ItemPosition PID-3[2].1` The value for the 1st component of the second occurrence of the PID-3 field (assuming the field is a list).
* `-ItemPosition PID-3.1.1` The value for the first sub-component of the first component of the 3rd field of the PID segment. 

__-Value <string[]>__: The new value to set the item to.

__-UpdateAllRepeats__: Update all occurrences identified by -ItemPosition

__-AppendToExistingValue__: The value supplied by the '-Value' parameter is appended to the original value in the message (instead of replacing it).

__-Filter <string[]>__ Only includes messages where a HL7 item equals a  specific value.  The format is: HL7Item=value. The HL7Item part  of the filter is of  the  same  format as the  -ItemPosition parameter. The -Filter parameter accepts a list of filters (separated by a comma). If a list of filters is provided then a message must match +all+ conditions to be included. 

>The filter is intended to filter at the message/file level when querying a large number of files (to include/exclude the messages from the results). It will not filter on individual repeating segments or fields within a message - the results will still include __all__ fields requested from the messages matched by the filter condition. e.g. if you filter on OBX-3=NM, all OBX-3 values from all OBX segments will be returned, but only for messages queried that contain at least 1 OBX-3 value equal to 'NM'. Message not matching the filter condition are excluded from the results.

Filter Examples:
* `-Filter MSH-9=ADT^A05` This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.
* `-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT` This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT" 

__-Encoding \<string\>__: Specify the character encoding. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

> Note: items that cannot be located in the message will display a warning. To suppress the warning messages use the "-WarningAction Ignore" common parameter.


## Remove-HL7Item
Deletes the value of specified items from the HL7 message. A list of items, or single item can be specified.

```
Remove-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string[]> [[-Filter] <string[]>] [-RemoveAllRepeats] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>]

Remove-HL7Item [-Path] <string[]> [-ItemPosition] <string[]> [[-Filter] <string[]>] [-RemoveAllRepeats] [[-Encoding] <String>] [-WhatIf] [-Confirm] [<CommonParameters>] 
```

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-RemoveAllRepeats__: Delete all repeats identified, Defaults to only deleting the first occurrence if this switch is not set.

__-ItemPosition <string[]>__: A string identifying the location of the item in the HL7 message you wish to  delete the value for. Provide a list of items to identify more than one location to delete. 

ItemPosition Examples:
* `-ItemPosition PID` All PID segments
* `-ItemPosition PID-3` The 3rd PID field. .
* `-ItemPosition PID-3,PID-4,NK1-2` deletes the values for PID-3, PID-4 and NK-2.
* `-ItemPosition PID-3[1]` The value of the 3rd PID field, but for only the first occurrence of PID-3 (assuming PID-3 is a list)
* `-ItemPosition PID-3.1.1` The value for the first sub-component of the first component of the 3rd field of the PID segment. 

__-Filter <string[]>__ Only includes messages where a HL7 item equals a  specific value.  The format is: HL7Item=value. The HL7Item part  of the filter is of  the  same  format as the  -ItemPosition parameter. The -Filter parameter accepts a list of filters (separated by a comma). If a list of filters is provided then a message must match +all+ conditions to be included. 

>The filter is intended to filter at the message/file level when querying a large number of files (to include/exclude the messages from the results). It will not filter on individual repeating segments or fields within a message - the results will still include __all__ fields requested from the messages matched by the filter condition. e.g. if you filter on OBX-3=NM, all OBX-3 values from all OBX segments will be returned, but only for messages queried that contain at least 1 OBX-3 value equal to 'NM'. Message not matching the filter condition are excluded from the results.

Filter Examples:
* `-Filter MSH-9=ADT^A05` This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.
* `-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT` This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT" 

__-Encoding \<string\>__: Specify the character encoding. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

## Split-HL7BatchFile
Splits a HL7 batch file into a separate file per HL7 message

```
Split-HL7BatchFile -LiteralPath <string[]> [-OverwriteFile] [-WhatIf] [<CommonParameters>]

Split-HL7BatchFile [-Path] <string[]> [-OverwriteFile] [-WhatIf] [<CommonParameters>]
```

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-OverwriteFile__: Don't warn when overwriting existing files

## Receive-HL7Message
Receives a HL7 v2.x message via a TCP connection (MLLP framing).

```
Receive-HL7Message [-Path] <String> [-Port] <Int32> [[-Timeout] <Int32>] [[-Encoding] <String>] [-NoACK <SwitchParameter>] [-InformationAction <ActionPreference>] [-InformationVariable <String>] [<CommonParameters>]
```

### Parameters
__-Path \<string\>__: The path to store the messages received. Must be literal path.

__-Port \<int\>__: The the TCP port to listen for messages on.

__-Timeout \<int\>__: The timeout in seconds before idle connections are dropped (defaults to 60 seconds if not specified).

__-Encoding \<string\>__: The text encoding to use when receiving the message. Supports "UTF-8" or "ISO-8859-1" (Western European). Defaults to "UTF-8" if parameter not supplied.

__-NoACK \<SwitchParameter\>__: Set this switch parameter to suppress acknowledgement (ACK) messages from being sent in reposne to messages received.


## Show-HL7MessageTimeline
Lists messages chronologically based on the message header receive date/time field (MSH-7)

```
Show-HL7MessageTimeline -Path <string> [-Descending] [<CommonParameters>]
```

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-Descending__: Switch to show messages in descending chronological order (defaults to ascending without this switch).
