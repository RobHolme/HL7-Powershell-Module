---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version:
schema: 2.0.0
---

# Send-HL7Message

## SYNOPSIS
Send a HL7 file to a remote host via TCP (MLLP).

## SYNTAX

### Literal
```
Send-HL7Message [-LiteralPath] <String[]> [-HostName] <String> [-Port] <Int32> [-NoACK] [[-Delay] <Int32>]
 [[-Encoding] <String>] [-UseTLS] [-SkipCertificateCheck] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Path
```
Send-HL7Message [-Path] <String[]> [-HostName] <String> [-Port] <Int32> [-NoACK] [[-Delay] <Int32>]
 [[-Encoding] <String>] [-UseTLS] [-SkipCertificateCheck] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### MessageString
```
Send-HL7Message [-MessageString] <String[]> [-HostName] <String> [-Port] <Int32> [-NoACK] [[-Delay] <Int32>]
 [[-Encoding] <String>] [-UseTLS] [-SkipCertificateCheck] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Send a HL7 v2.x message from a file (or list of files) via TCP to a remote endpoint.
Messages are framed using MLLP (Minimal Lower Layer Protocol).

## EXAMPLES

### Example 1
```powershell
PS C:\> Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\message1.hl7
```

### Example 2
```powershell
PS C:\> 
Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\*.hl7 -Encoding ISO-8859-1
```

Send all .hl7 files in the c:\HL7Files folder. Use ISO-8859-1 (Wester European) text encoding.

### Example 3
```powershell
PS C:\> $msg = get-content c:\hl7\test.hl7; Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -MessageString $msg
```

Supply the message contents as a parameter. Accepts a string array [string[]] (one one segment per array item) or a single string value [string] (with Carriage Returns between each segment.) 

## PARAMETERS

### -Delay
The delay (in seconds) between sending each message.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Encoding
Set the text encoding.
Supports "UTF-8" or "ISO-8859-1" (Western European).
Defaults to "UTF-8" if parameter not supplied.


```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: UTF-8, ISO-8859-1

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HostName
The IP Address or host name of the remote host to send the HL7 message to.

```yaml
Type: String
Parameter Sets: (All)
Aliases: ComputerName, Server, IPAddress

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LiteralPath
Same as -Path, only wildcards are not expanded.
Use this if the literal path includes a wildcard character you do not intent to expand.

```yaml
Type: String[]
Parameter Sets: Literal
Aliases: PSPath, Name, Filename

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -MessageString
Supply the message contents as a string value instead of a filename. Accepts either a string[] array [string[]] (one one segment per array item) or a single [string] value (with Carriage Returns between each segment in a single string.) 

```yaml
Type: String[]
Parameter Sets: MessageString
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoACK
This switch instructs the CmdLet not to wait for an ACK response from the remote host.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
The full or relative path a single HL7 file or directory.
This may include wildcards in the path name.
If a directory is provide, all files within the directory will be examined.
Exceptions will be raised if a file isn't identified as a HL7 v2.x file.
This parameter accepts a list of files, separate each file file with a ',' (no spaces).

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Port
The TCP port of the listener on the remote host to send the HL7 message to.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SkipCertificateCheck
Ignore TLS certificate errors.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseTLS
Use TLS to secure the connection (if supported by server).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#send-hl7message)