---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version:
schema: 2.0.0
---

# Receive-HL7Message

## SYNOPSIS
Receive HL7 v2.x messages via a TCP connection (MLLP).

## SYNTAX

```
Receive-HL7Message [-Path] <String> [-Port] <Int32> [[-Timeout] <Int32>] [[-Encoding] <String>] [-NoACK]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Receives a HL7 v2.x message via a TCP connection (MLLP framing). Pres 'esc' to close the listener.

## EXAMPLES

### Example 1
```powershell
PS C:\> Receive-HL7Message -Path c:\hl7 -Port 5000
```

Start a MLLP listener on port 5000. Save received files to c:\hl7\ 

## PARAMETERS

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
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoACK
Do not send an acknowledgement (ACK) message.

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
The path to save received messages to.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Port
The TCP port to listen on.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Timeout
The timeout to end idle TCP connections in seconds.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
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

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#receive-hl7message)