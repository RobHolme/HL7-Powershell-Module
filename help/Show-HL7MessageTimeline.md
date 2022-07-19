---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version: https://github.com/RobHolme/HL7-Powershell-Module#show-hl7messagetimeline
schema: 2.0.0
---

# Show-HL7MessageTimeline

## SYNOPSIS
List messages chronologically based on the header timestamp (MSH-7).

## SYNTAX

### Literal
```
Show-HL7MessageTimeline -LiteralPath <String[]> [-Descending] [-InformationAction <ActionPreference>]
 [-InformationVariable <String>] [<CommonParameters>]
```

### Path
```
Show-HL7MessageTimeline [-Path] <String[]> [-Descending] [-InformationAction <ActionPreference>]
 [-InformationVariable <String>] [<CommonParameters>]
```

## DESCRIPTION
Lists messages chronologically based on the message header receive date/time field (MSH-7).

## PARAMETERS

### -Descending
Switch to show messages in descending chronological order (defaults to ascending without this switch).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Desc

Required: False
Position: Named
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

### -Path
The full or relative path a single HL7 file or directory.
This may include wildcards in the path name.
If a directory is provided, all files within the directory will be examined.
Exceptions will be raised if a file isn't identified as a HL7 v2.x file.
This parameter accepts a list of files, separate each file file with a ','.

```yaml
Type: String[]
Parameter Sets: Path
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

## NOTES

## RELATED LINKS

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#show-hl7messagetimeline)

