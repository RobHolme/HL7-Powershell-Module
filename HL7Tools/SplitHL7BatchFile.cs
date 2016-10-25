/* Filename:    SplitHL7BatchFile.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        24/10/2016
 * 
 * Notes:       Implements the CmdLet Split-HL7Batchfile. Splits batch mode HL7 messages from a single bacth file
 *              into multiple files.
 * 
 */

namespace HL7Tools
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Management.Automation;

    // CmdLet: Update-HL7Item
    // Replaces the value of a specific item from the message 
    [Cmdlet(VerbsCommon.Split, "HL7BatchFile", SupportsShouldProcess = true)]
    public class SplitHl7BatchFile : PSCmdlet
    {
        private string[] paths;
        private bool expandWildcards = false;
        private bool overwriteFile;
        private bool yesToAll;
        private bool noToAll;

        // Parameter set for the -Path and -LiteralPath parameters. A parameter set ensures these options are mutually exclusive.
        // A LiteralPath is used in situations where the filename actually contains wild card characters (eg File[1-10].txt) and you want
        // to use the literaral file name instead of treating it as a wildcard search.
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Literal")
        ]
        [Alias("PSPath", "Name", "Filename", "Fullname")]
        [ValidateNotNullOrEmpty]
        public string[] LiteralPath
        {
            get { return this.paths; }
            set { this.paths = value; }
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Path")

        ]
        [ValidateNotNullOrEmpty]
        public string[] Path
        {
            get { return this.paths; }
            set
            {
                this.expandWildcards = true;
                this.paths = value;
            }
        }

        // Switch to supress warnings if a file is overwritten by the CmdLet
        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not warn if overwriting existing files?"
        )]
        [Alias("Overwrite", "Force")]
        public SwitchParameter OverwriteFile
        {
            get { return this.overwriteFile; }
            set { this.overwriteFile = value; }
        }

        /// <summary>
        /// Process each file from the pipeline
        /// </summary>
        protected override void ProcessRecord()
        {
            // expand the file or directory information provided in the -Path or -LiteralPath parameters
            foreach (string path in paths) {
                // This will hold information about the provider containing the items that this path string might resolve to.                
                ProviderInfo provider;

                // This will be used by the method that processes literal paths
                PSDriveInfo drive;

                // this contains the paths to process for this iteration of the loop to resolve and optionally expand wildcards.
                List<string> filePaths = new List<string>();

                // if the path provided is a directory, return an error.
                if (Directory.Exists(path)) {
                    IOException fileException = new FileNotFoundException("The path provided is a directory, not a file", path);
                    ErrorRecord fileNotFoundError = new ErrorRecord(fileException, "SaveFailed", ErrorCategory.OpenError, path);
                    WriteError(fileNotFoundError);
                    return;
                }

                // not a directory, could be a wildcard or literal filepath 
                else {
                    // expand wildcards. This assumes if the user listed a directory it is literal
                    if (expandWildcards) {
                        // Turn *.txt into foo.txt,foo2.txt etc. If path is just "foo.txt," it will return unchanged. If the filepath expands into a directory ignore it.
                        foreach (string expandedFilePath in this.GetResolvedProviderPathFromPSPath(path, out provider)) {
                            if (!Directory.Exists(expandedFilePath)) {
                                filePaths.Add(expandedFilePath);
                            }
                        }
                    }
                    else {
                        // no wildcards, so don't try to expand any * or ? symbols.                    
                        filePaths.Add(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive));
                    }
                    // ensure that this path (or set of paths after wildcard expansion)
                    // is on the filesystem. A wildcard can never expand to span multiple providers.
                    if (Common.IsFileSystemPath(provider, path) == false) {
                        // no, so skip to next path in paths.
                        continue;
                    }
                }

                // At this point, we have a list of paths on the filesystem, process each file. 
                foreach (string filePath in filePaths) {
                    // If the file does not exist display an error and return.
                    if (!File.Exists(filePath)) {
                        FileNotFoundException fileException = new FileNotFoundException("File not found", filePath);
                        ErrorRecord fileNotFoundError = new ErrorRecord(fileException, "FileNotFound", ErrorCategory.ObjectNotFound, filePath);
                        WriteError(fileNotFoundError);
                        return;
                    }

                    // process the message
                    int fileCount = 0;
                    string newFilePath;
                    
                    string tempHL7Message = string.Empty;

                    // get the file contents, split on carriage return to return each line. If the file contains <CR><LF> end of line characters, the convert to <CR> only (as per HL7 spec).
                    string[] fileContents = File.ReadAllText(filePath).Replace("\r\n", "\r").Split('\r');
                    foreach (string currentLine in fileContents) {
                        // ignore the file and batch headers/footers
                        if ((Regex.IsMatch(currentLine, "^FHS", RegexOptions.IgnoreCase)) || (Regex.IsMatch(currentLine, "^BHS", RegexOptions.IgnoreCase)) || (Regex.IsMatch(currentLine, "^BTS", RegexOptions.IgnoreCase)) || (Regex.IsMatch(currentLine, "^FTS", RegexOptions.IgnoreCase))) {
                            WriteVerbose("Batch file header/footer detected");
                        }
                        else {
                            if (Regex.IsMatch(currentLine, "^MSH", RegexOptions.IgnoreCase)) {
                                // special  case for the first MSH segment
                                if (fileCount == 0) {
                                    tempHL7Message = currentLine;
                                    fileCount++;
                                }
                                else {
                                    // save the changes to a new file (append unique id to original filename)
                                    newFilePath = AppendFilenameSuffix(filePath, fileCount.ToString());
                                    this.SaveFile(newFilePath, tempHL7Message);
                                    // start storing the next message
                                    fileCount++;
                                    tempHL7Message = currentLine;
                                }
                            }
                            else {
                                tempHL7Message += "\r" + currentLine;
                            }
                        }
                    }
                    // save the last message
                    newFilePath = AppendFilenameSuffix(filePath, fileCount.ToString());
                    this.SaveFile(newFilePath, tempHL7Message);
                }
            }
        }

        /// <summary>
        /// Save the hl7 message to a file
        /// </summary>
        /// <param name="Filename"></param>
        /// <param name="Hl7Message"></param>
        private void SaveFile(string Filename, string Hl7Message)
        {
            SplitHL7BatchFileResult result;
            
            // if the -WhatIf switch is supplied don't commit changes to file
            if (this.ShouldProcess(Filename, "Saving HL7 message to file")) {
                try {
                    // prompt the user to overwrite the file if it exists (and the -OverwriteFile switch is not set)
                    if (!this.overwriteFile && File.Exists(Filename)) {
                        if (this.ShouldContinue("File " + Filename + " exists.  Are you sure you want to overwrite the file?", "Overwrite file", ref this.yesToAll, ref this.noToAll)) {
                            System.IO.File.WriteAllText(Filename, Hl7Message);
                            result = new SplitHL7BatchFileResult(Filename);
                            WriteObject(result);
                        }
                    }
                    else {
                        System.IO.File.WriteAllText(Filename, Hl7Message);
                        result = new SplitHL7BatchFileResult(Filename);
                        WriteObject(result);
                    }
             
                }
                // write error if any exceptions raised when saving the file
                catch {
                    IOException fileException = new FileNotFoundException("File not found", Filename);
                    ErrorRecord fileNotFoundError = new ErrorRecord(fileException, "SaveFailed", ErrorCategory.WriteError, Filename);
                    WriteError(fileNotFoundError);
                }
            }
        }

        /// <summary>
        /// Append a suffix to a file path. The suffix is added between the filename and extention.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="fileSuffix"></param>
        /// <returns></returns>
        private string AppendFilenameSuffix(string FilePath, string fileSuffix)
        {
            string newFilePath;
            string pathName = System.IO.Path.GetDirectoryName(FilePath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(FilePath);
            string fileExtension = System.IO.Path.GetExtension(FilePath);

            newFilePath = System.IO.Path.Combine(pathName, fileName + "_" + fileSuffix);
            if (fileExtension.Length > 0) {
                newFilePath = newFilePath + fileExtension;
            }
            return newFilePath;
        }
    }

    /// <summary>
    /// An object containing the results to be returned to the pipeline. 
    /// </summary>
    public class SplitHL7BatchFileResult
    {
        private string newFilename;
    
        /// <summary>
        /// The location of the HL7 item that was changed. e.g. PID-3.1
        /// </summary>
        public string Filename
        {
            get { return this.newFilename; }
            set { this.newFilename = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <param name="Filename"></param>
        public SplitHL7BatchFileResult(string NewFilename)
        {
            this.newFilename = NewFilename;
        }
    }
}
                   
           
