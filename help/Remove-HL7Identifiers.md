---
external help file: hl7tools.dll-Help.xml
Module Name: hl7tools
online version:
schema: 2.0.0
---

# Remove-HL7Identifiers

## SYNOPSIS
Remove personal identifiers for patients and next of kin from HL7 files.

## SYNTAX

### Literal
```
Remove-HL7Identifiers -LiteralPath <String[]> [[-CustomItemsList] <String[]>] [[-MaskChar] <Char>]
 [-OverwriteFile] [[-Encoding] <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### Path
```
Remove-HL7Identifiers [-Path] <String[]> [[-CustomItemsList] <String[]>] [[-MaskChar] <Char>] [-OverwriteFile]
 [[-Encoding] <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes names, addresses and other personally identifiable details from a HL7 v2.x Message.

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-HL7Identifiers -Path c:\hl7files\*.hl7 -OverwriteFile
```

### Example 1
```powershell
PS C:\> Remove-Hl7Identifiers -Path c:\test.txt
```

### Example 1
```powershell
PS C:\> Remove-HL7Identifiers -Path c:\test\testfile.hl7 -CustomItemsList PID-3.1,NK1,DG1
```

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

### -CustomItemsList
A list of the HL7 items to mask instead of the default segments and fields.
List each item in a comma separated list (no spaces).
Items can include a mix of segments (e.g. PV1), Fields (e.g. PID-3), Components (e.g. "PID-3.1") or Subcomponents (e.g. "PV1-42.2.2").
For repeating segments and fields a specific repeat can be identified (index starts at 1).
e.g. "PID-3\[1\].1" will mask out the first occurrence of "PID-3.1", leaving other repeats of "PID-3.1" unchanged.
Likewise "IN1\[2\]" will mask out the second occurrence of the IN1 segments only.
e.g. -CustomItemsList PID-3.1,PID-5,PID-13,NK1

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
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
Position: 4
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

### -MaskChar
The character used to mask the identifier fields, Defaults to '*' if not supplied.

```yaml
Type: Char
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OverwriteFile
If this switch is set, the original file is modified.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
The full or relative path a single HL7 file or directory, or a list of files.
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

[Online Help](https://github.com/RobHolme/HL7-Powershell-Module#remove-hl7identifiers)