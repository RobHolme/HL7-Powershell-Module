# Project Description
This PowerShell module is a small collection of CmdLets to manipulate HL7 v2.x files, including:

* __Select-HL7Item__: queries a HL7 file (or list of files) to return a specific HL7 Item.
* __Set-HL7Item__: changes the value of an existing item within a HL7 file
* __Remove-HL7Item__: delete the value for a nominated item in the HL7 message.
* __Send-HL7Messsage__: send a HL7 file to a remote host via TCP.
* __Remove-HL7Identifiers__: remove personal identifiers for patients and next of kin from HL7 files.
* __Split-HL7BatchFile__: Split a batch file containing multiple messages into a separate file per message.
* __Receive-HL7Message__: Receive a HL7 v2.x message via a TCP connection

# Installation Instructions

1.  Download hl7tools.zip and extract all files (hl7tools.dll, HL7Tools.Format.ps1xml, Hl7Tools.psd1) to a folder named “HL7Tools” in your PowerShell modules folder. You can confirm the location of your modules folder by running `$env:PSModulePath` from a PowerShell console.
2.  Run the PowerShell command `Unblock-File` against all files extracted from the download (or unblock the files via Windows Explorer file properties).
3.  Open a powershell console, the module will be imported when the console opens and the CmdLets available to use.

# CmdLet Usage 
## Select-HL7Item
This CmdLet returns the values of specific HL7 items from a file (or group of files). This is intended to aid with integration work to validate the values of fields over a large sample of HL7 messages. Some basic filtering is available to only include specific messages from a larger sample.

Output can be piped to other powershell CmdLets to refine the results. e.g. returning only the unique values across a range of files:

```
Select-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string> [-Filter <string[]>] [<CommonParameters>]

Select-HL7Item [-Path] <string[]> [-ItemPosition] <string> [-Filter <string[]>] [<CommonParameters>]
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

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't identified as a HL7 v2.x file. This parameter accepts a list of files, separate each file file with a ','

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-ItemPosition \<string\>__: A string identifying the location of the item in the HL7 message you wish to retrieve the value for.

ItemPosition Examples:
* `-ItemPosition PID`  All PID segments
* `-ItemPosition PID-3`  The value for the 3rd PID field. If this item is a list all PID-3 values in the list will be returned.
* `-ItemPosition PID-3[1]`  The value of the 3rd PID field, but for only the first occurrence of PID-3 (assuming PID-3 is a list)
* `-ItemPosition PID-3.1`  The value for the 1st component of the PID-3 field. If the component belongs  to a list  of fields, all PID-3.1 components will be returned. 
* `-ItemPosition PID-3[2].1` The value for the 1st component of the second occurrence of the PID-3 field (assuming the field is a list).
* `-ItemPosition PID-3.1.1` The value for the first sub-component of the first component of the 3rd field of the PID segment. 

__-Filter <string[]>__: Only includes messages where a HL7 item equals a  specific value.  The format is: HL7Item=value. The HL7Item part  of the filter is of  the  same  format as the  -ItemPosition parameter. The -Filter parameter accepts a list of filters (separated by a comma). If a list of filters is provided then a message must match all conditions to be included. 

Filter Examples:
* `-Filter MSH-9=ADT^A05` This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.
* `-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT` This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT" 

> Note: Items that cannot be located in the message will display a warning. 
> To suppress the warning messages use the `-WarningAction Ignore` common parameter.

## Send-HL7Message
Send a HL7 v2.x message from a file (or list of files) via TCP to a remote endpoint. Messages are framed using MLLP (Minimal Lower Layer Protocol).

```
Send-HL7Message [-HostName] <string> [-Port] <int> [-LiteralPath] <string[]> [-NoACK]  [<CommonParameters>]

Send-HL7Message [-HostName] <string> [-Port] <int> [-Path] <string[]> [-NoACK]  [<CommonParameters>]
```
example:

`Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\message1.hl7`

### Parameters
__-Hostname \<string\>__:  The  IP Address or host name of the remote host to send the HL7 message to

__-Port \<int\>__: The TCP port of the listener on the remote host to send the HL7 message to.

__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' (no spaces). 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-NoACK__: This switch instructs the CmdLet not to wait for an ACK response from the remote host.

## Remove-HL7Identifiers
Removes names, addresses and other personally identifiable details from a HL7 v2.x Message.  

> Note:  Identifier codes in {"PID-3"} remain. This masks the following fields from a HL7 v2.x file:
> * All PID fields except {"PID-1, PID-2, PID-3"}
> * All NK1 fields except {"NK1-1, NK1-3"}
> * All IN1 fields 
> * All IN2 fields 

By default a new file is saved in the same location as the original file with "MASKED_" prefixed  to the file name. If the  optional "-OverwriteFile" switch is  set the original file will be modified.

```
Remove-HL7Identifiers [-LiteralPath] <string[]> [[-CustomItemsList] <string[]>] [[-MaskChar] <char>] [[-OverwriteFile]] [-WhatIf] [-Confirm] [<CommonParameters>]

Remove-HL7Identifiers [-Path] <string[]> [[-CustomItemsList] <string[]>] [[-MaskChar] <char>] [[-OverwriteFile]] [-WhatIf] [-Confirm] [<CommonParameters>]
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

## Set-HL7Item

This CmdLet changes the value of an existing HL7 item from a file (or group of files). Some basic filtering is available to only include specific messages within a large sample. By default only the first occurrence of an item will be changed unless the -UpdateAllRepeats switch is set.

```
Set-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string> [-Value] <string> [-Filter <string[]>]  [-UpdateAllRepeats] [-AppendToExistingValue] [-WhatIf] [-Confirm] [<CommonParameters>]

Set-HL7Item [-Path] <string[]> [-ItemPosition] <string> [-Value] <string> [-Filter <string[]>]  [-UpdateAllRepeats] [-AppendToExistingValue] [-WhatIf] [-Confirm] [<CommonParameters>]
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

__-UpdateAllRepeats__: Update all occurances  identified by -ItemPosition

__-AppendToExistingValue__: The value supplied by the '-Value' parameter is appended to the original value in the message (instead of replacing it).

___-Filter <string[]>___ Only includes messages where a HL7 item equals a  specific value.  The format is: HL7Item=value. The HL7Item part  of the filter is of  the  same  format as the  -ItemPosition parameter. The -Filter parameter accepts a list of filters (separated by a comma). If a list of filters is provided then a message must match +all+ conditions to be included. 

Filter Examples:
* `-Filter MSH-9=ADT^A05` This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.
* `-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT` This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT" 

> Note: items that cannot be located in the message will display a warning. To suppress the warning messages use the "-WarningAction Ignore" common parameter.


## Remove-HL7Item
Deletes the value of specified items from the HL7 message. A list of items, or single item can be specified.

```
Remove-HL7Item [-LiteralPath] <string[]> [-ItemPosition] <string[]> [[-Filter] <string[]>] [-RemoveAllRepeats] [-WhatIf] [-Confirm] [<CommonParameters>]

Remove-HL7Item [-Path] <string[]> [-ItemPosition] <string[]> [[-Filter] <string[]>] [-RemoveAllRepeats] [-WhatIf] [-Confirm] [<CommonParameters>] 
```

### Parameters
__-Path <string[]>__: The full or relative path a single HL7 file or directory. This may include wildcards in the path name. If a directory is provide, all files within the directory will be examined. Exceptions will be raised if a file isn't  identified as  a HL7 v2.x file. This parameter accepts a list of files, separate each file  file with a ',' 

__-LiteralPath <string[]>__: Same as -Path, only wildcards are not expanded. Use this if the literal path includes a wildcard character you do not intent to expand.

__-RemoveAllRepeats__: Delete all repeats identified, Defaults to only deleting the first occurrence if this switch is not set.

__-ItemPosition <string[]>__: A string identifying the location of the item in the HL7 message you wish to  delete the value for. Provide a list of items to identify more than one location to delete. 

ItemPosition Examples:
* `-ItemPosition PID` All PID segments
* `-ItemPosition PID-3` The 3rd PID field. .
* `-ItemPosition PID-3,PID-4,NK1-2` deletes the calues for PID-3, PID-4 and NK-2.
* `-ItemPosition PID-3[1]` The value of the 3rd PID field, but for only the first occurrence of PID-3 (assuming PID-3 is a list)
* `-ItemPosition PID-3.1.1` The value for the first sub-component of the first component of the 3rd field of the PID segment. 

## Split-HL7BatchFile
Splits a HL7 batch file into a speperate file per HL7 message

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
Receive-HL7Message -Path <string> -Port <int> [-Timeout] <int> [<CommonParameters>]
```

### Parameters
__-Path \<string\>__: The path to store the messages received.

__-Port \<int\>__: The the TCP port to listen for messages on.

__-Timeout \<int\>__: The timeout in seconds before idle connections are dropped (defaults to 60 seconds if not specified).
