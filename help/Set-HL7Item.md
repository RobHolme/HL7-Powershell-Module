---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version: https://github.com/RobHolme/HL7-Powershell-Module#set-hl7item
schema: 2.0.0
---

# Set-HL7Item

## SYNOPSIS
Sets the value of an existing item within a HL7 file.

## SYNTAX

### Literal
```
Set-HL7Item -LiteralPath <String[]> [-ItemPosition] <String> [-Value] <String> [[-Filter] <String[]>]
 [[-Encoding] <String>] [-UpdateAllRepeats] [-AppendToExistingValue] [-InformationAction <ActionPreference>]
 [-InformationVariable <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Path
```
Set-HL7Item [-Path] <String[]> [-ItemPosition] <String> [-Value] <String> [[-Filter] <String[]>]
 [[-Encoding] <String>] [-UpdateAllRepeats] [-AppendToExistingValue] [-InformationAction <ActionPreference>]
 [-InformationVariable <String>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
This CmdLet changes the value of an existing HL7 item from a file (or group of files).
Some basic filtering is available to only include specific messages within a large sample.
By default only the first occurrence of an item will be changed unless the -UpdateAllRepeats switch is set.
Items that cannot be located in the message will display a warning.
To suppress the warning messages use the "-WarningAction Ignore" common parameter.

## EXAMPLES

### Example 1
@{paragraph=PS C:\\\>}

```
Set-HL7Item -Path c:\hl7files\hl7file.txt -ItemPosition PID-3.1 -Value A1234567
```

### Example 2
@{paragraph=PS C:\\\>}

```
Set-HL7Item -Path c:\hl7files\*.hl7 -ItemPosition PV1-3.1 -Value A1234567 -Filter PV1-2=INPATIENT
```

## PARAMETERS

### -AppendToExistingValue
The value supplied by the '-Value' parameter is appended to the original value in the message (instead of replacing it).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Append

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Encoding
Specify the character encoding.
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

### -Filter
Only includes messages where a HL7 item equals a specific value.
The format is: HL7Item=value.
The HL7Item part of the filter is of the same format as the -ItemPosition parameter.
The -Filter parameter accepts a list of filters (separated by a comma).
If a list of filters is provided then a message must match all conditions to be included.
e.g.

-Filter MSH-9=ADT^A05   This filter would only include messages that had "ADT^A05" as the value for the MSH-9 field.

-Filter MSH-9=ADT^A05,PV1-2=OUTPATIENT   This filter would only include messages where both the MSH-9 field contained "ADT^A04" and the PV1-2 field contained "OUTPATIENT"

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
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

### -ItemPosition
A string identifying the location of the item in the HL7 message you wish to retrieve the value for.
e.g.

PID            Identifies the PID Segment
PID-3          Identifies the PID-3 Field
PID-3.1        Identifies the PID-3.1 Component
PID-3.1.1      Identifies the PID-3.1.1 Subcomponent
PID-3\[2\].1     Identifies the PID-3.1 Component for second occurrence of the PID-3 Field

```yaml
Type: String
Parameter Sets: (All)
Aliases: Item

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
Aliases: PSPath, Name, Filename, Fullname

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Path
The full or relative path a single HL7 file or directory.
This may include wildcards in the path name.
If a directory is provide, all files within the directory will be examined.
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

### -UpdateAllRepeats
Update all occurrences identified by -ItemPosition

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

### -Value
The new value to set the item to.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
@{Text=}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
@{Text=}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

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

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#set-hl7item)

