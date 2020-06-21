/* Filename:    RemoveHL7Identifiers.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 * 
 *              Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        21/07/2016 - Intial version, masks predefined list of fields only.
 *              31/07/2016 - Implemented custom list of items to mask.
 * 
 * Notes:       Implements the cmdlet to mask out personaly identifiable information from HL7 v2 Messages
 * 
 */


namespace HL7Tools
{
    using System;
    using System.IO;
	using System.Text;
    using System.Collections.Generic;
    using System.Management.Automation;

    /// <summary>
    /// Class implementing the cmdlet Remove-HL7Identifiers
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "HL7Identifiers", SupportsShouldProcess = true)]
    public class RemoveHL7Identifiers : PSCmdlet
    {
        private char maskChar = '*';
        private bool overwriteFile = false;
        private string[] paths;
        private bool expandWildcards = false;
        private string[] customItemsList = new string[] { };
		private string encoding = "UTF-8";

        // Parameter set for the -Path and -LiteralPath parameters. A parameter set ensures these options are mutually exclusive.
        // A LiteralPath is used in situations where the filename actually contains wild card characters (eg File[1-10].txt) and you want
        // to use the literal file name instead of treating it as a wildcard search.
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Literal")
        ]
        [Alias("PSPath", "Name", "Filename")]
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

        // A list of HL7 items to mask, supplied by the user
        [Parameter(
            Mandatory = false,
            Position = 1,
            HelpMessage = "User supplied list of items to mask"
        )]
        public string[] CustomItemsList
        {
            get { return this.customItemsList; }
            set { this.customItemsList = value; }
        }

        // The mask character to use. Optional, defaults to '*'
        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "The mask character to use"
        )]
        public char MaskChar
        {
            get { return this.maskChar; }
            set { this.maskChar = value; }
        }

        // Switch to overwrite the original file, instead of writing the masked data to a new file. Optional, defaults to false.
        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "Switch to overwrite the original file"
        )]
        public SwitchParameter OverwriteFile
        {
            get { return this.overwriteFile; }
            set { this.overwriteFile = value; }
        }

        // Parameter to specify the message character encoding format
        [Parameter(
            Mandatory = false,
            Position = 4,
            HelpMessage = "Text encoding ('UTF-8' | 'ISO-8859-1'")]
        [ValidateSet("UTF-8", "ISO-8859-1")]
        public string Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        /// <summary>
        /// remove identifying fields
        /// </summary>
        protected override void ProcessRecord()
        {
            // validate the that the list of locations to mask is valid.
            foreach (string item in this.customItemsList) {
                // confirm each filter is formatted correctly
                if (!Common.IsItemLocationValid(item)) {
                    ArgumentException ex = new ArgumentException(item + " does not appear to be a valid HL7 location");
                    ErrorRecord error = new ErrorRecord(ex, "InvalidFilter", ErrorCategory.InvalidArgument, item);
                    this.WriteError(error);
                    return;
                }
            }

			// set the text encoding
            Encoding encoder = System.Text.Encoding.GetEncoding(this.encoding);
            WriteVerbose("Encoding: " + encoder.EncodingName);

            foreach (string path in paths) {
                // This will hold information about the provider containing the items that this path string might resolve to.                
                ProviderInfo provider;

                // This will be used by the method that processes literal paths
                PSDriveInfo drive;

                // this contains the paths to process for this iteration of the loop to resolve and optionally expand wildcards.
                List<string> filePaths = new List<string>();

                // if the path provided is a directory, expand the files in the directory and add these to the list.
                if (Directory.Exists(path)) {
                    filePaths.AddRange(Directory.GetFiles(path));
                }

                // not a directory, could be a wild-card or literal filepath 
                else {
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
                    try {
                        string fileContents = File.ReadAllText(filePath, encoder);
                        HL7Message message = new HL7Message(fileContents);
                        // if a custom list of items is provided, then mask out each nominated item
                        if (customItemsList.Length > 0) {
                            foreach (string item in customItemsList) {
                                message.MaskHL7Item(item, this.maskChar);
                            }
                        }

                        // otherwise mask out default items
                        else {
                            message.DeIdentify(this.maskChar);
                        }

						char pathSeparator = System.IO.Path.DirectorySeparatorChar;
						string newFilename = filePath.Substring(0, filePath.LastIndexOf(pathSeparator) + 1) + "MASKED_" + filePath.Substring(filePath.LastIndexOf(pathSeparator) + 1, filePath.Length - (filePath.LastIndexOf(pathSeparator) + 1));

                        // if the overwrite switch is set, then use the original file name.
                        if (this.overwriteFile) {
                            newFilename = filePath;
                        }
                        // Write changes to the file. Replace the segment delimeter <CR> with the system newline string as this is being written to a file.
						string cr = ((char)0x0D).ToString();
						string newline = System.Environment.NewLine;
                        if (this.ShouldProcess(newFilename, "Saving changes to file")) {
                            System.IO.File.WriteAllText(newFilename, message.ToString().Replace(cr,newline), encoder);
                        }
                        WriteObject("Masked file saved as " + newFilename);
                    }

                    // if the file does not start with a MSH segment, the constructor will throw an exception. 
                    catch (System.ArgumentException) {
                        ArgumentException argException = new ArgumentException("The file does not appear to be a valid HL7 v2 message", filePath);
                        ErrorRecord fileNotFoundError = new ErrorRecord(argException, "FileNotValid", ErrorCategory.InvalidData, filePath);
                        WriteError(fileNotFoundError);
                        return;
                    }
                }
            }
        }
    }
}
