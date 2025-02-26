---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version:
schema: 2.0.0
---

# Select-HL7Item

## SYNOPSIS
Queries a HL7 file (or list of files) to return a specific HL7 Item.

## SYNTAX

### Literal
```
Select-HL7Item -LiteralPath <String[]> [-ItemPosition] <String[]> [[-Filter] <String[]>] [[-Encoding] <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Path
```
Select-HL7Item [-Path] <String[]> [-ItemPosition] <String[]> [[-Filter] <String[]>] [[-Encoding] <String>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This CmdLet returns the values of specific HL7 items from a file (or group of files).
This is intended to aid with integration work to validate the values of fields over a large sample of HL7 messages.
Some basic filtering is available to include specific messages from a larger sample.

Output can be piped to other PowerShell CmdLets to refine the results.
e.g. returning only the unique values across a range of files:

## EXAMPLES

### Example 1
```powershell
PS C:\> Select-HL7Item -Path c:\test -ItemPosition PID-3.1
```

Display the Patient ID values for all hl7 files in c:\test

### Example 2
```powershell
PS C:\> get-childitem *.hl7 | Select-HL7Item -ItemPosition PID-3.1,PID-5
```

Pipe all files with .hl7 extensions to the CmdLet, return Patient ID values (PID-3.1) and Patient Names (PID-5).

### Example 3
```powershell
PS C:\> Select-HL7Item -Path c:\test -ItemPosition PID-5 -Filter PV1-2=INPATIENT
```

Display all PID-5 values where the value of PV1-2 is INPATIENT.

### Example 4
```powershell
PS C:\> (Select-HL7Item -Path c:\test -ItemPosition PID-3.1).ItemValue | Sort-Object -Unique
```

Only return unique values

## PARAMETERS

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
A string, or list of strings, identifying the location of the item(s) in the HL7 message you wish to retrieve the value for.
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

[Online help](https://github.com/RobHolme/HL7-Powershell-Module#select-hl7item)