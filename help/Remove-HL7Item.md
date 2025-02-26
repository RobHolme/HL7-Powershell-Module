---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version:
schema: 2.0.0
---

# Remove-HL7Item

## SYNOPSIS
Delete the value for a nominated item in the HL7 message.

## SYNTAX

### Literal
```
Remove-HL7Item -LiteralPath <String[]> [-ItemPosition] <String[]> [[-Filter] <String[]>] [-RemoveAllRepeats]
 [[-Encoding] <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Path
```
Remove-HL7Item [-Path] <String[]> [-ItemPosition] <String[]> [[-Filter] <String[]>] [-RemoveAllRepeats]
 [[-Encoding] <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Deletes the value of specified items from the HL7 message.
A list of items, or single item can be specified.

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-HL7Item -Path c:\hl7\test.hl7 -ItemPosition PID-3.1
```

remove the PID-3.1 component

### Example 2

```powershell
PS C:\> Remove-HL7Item -Path c:\hl7\test.hl7 -ItemPosition PID-3.1 -Filter "MSH-9.2=ADT^A01"
```

remove the PID-3.1 component for all ADT^A01 messages

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

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

### -Encoding
Text encoding ('UTF-8' | 'ISO-8859-1'). Defaults to "UTF-8".

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
Position: 2
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
Type: String[]
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

### -RemoveAllRepeats
Delete all repeats identified, Defaults to only deleting the first occurrence if this switch is not set.

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

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#remove-hl7item)