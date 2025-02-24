---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version: https://github.com/RobHolme/HL7-Powershell-Module#send-hl7message
schema: 2.0.0
---

# Send-HL7Message

## SYNOPSIS
Send a HL7 file to a remote host via TCP (MLLP).

## SYNTAX

### Literal
```
Send-HL7Message -LiteralPath <string[]> [-HostName] <string> [-Port] <int> [[-Delay] <int>] [[-Encoding] {UTF-8 | ISO-8859-1}] [-NoACK] [-UseTLS] [-SkipCertificateCheck] [<CommonParameters>]
```

### Path
```
Send-HL7Message [-Path] <string[]> [-HostName] <string> [-Port] <int> [[-Delay] <int>] [[-Encoding] {UTF-8 | ISO-8859-1}] [-NoACK] [-UseTLS] [-SkipCertificateCheck] [<CommonParameters>]
```

### MessageString
```
Send-HL7Message [-MessageString] <string[]> [-HostName] <string> [-Port] <int> [[-Delay] <int>] [[-Encoding] {UTF-8| ISO-8859-1} ] [-NoACK] [-UseTLS] [-SkipCertificateCheck] [<CommonParameters>]
```

## DESCRIPTION
Send a HL7 v2.x message from a file (or list of files) via TCP to a remote endpoint.
Messages are framed using MLLP (Minimal Lower Layer Protocol).

## EXAMPLES

### Example 1
@{paragraph=PS C:\\\>}

```
Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\message1.hl7
```

### Example 2
@{paragraph=PS C:\\\>}

```
Send-Hl7Message -Hostname 192.168.0.10 -Port 1234 -Path c:\HL7Files\*.hl7 -Encoding ISO-8859-1
```

Send all .hl7 files in the c:\HL7Files folder.
Use ISO-8859-1 (Wester European) text encoding.

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

Required: False
Position: 4
Default value: UTF-8
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

### -InformationAction
@{Text=}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: infa

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InformationVariable
@{Text=}

```yaml
Type: String
Parameter Sets: (All)
Aliases: iv

Required: False
Position: Named
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
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```
### -MessageString
Provide the message contents (as a string) instead of a file.
```yaml
type: String[]
Parameter Sets: MessageString
Aliases:

Required: True
Possition: 0
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#send-hl7message)

