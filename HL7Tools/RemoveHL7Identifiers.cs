/* Filename:    RemoveHL7Identifiers.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 * 
 *              Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        21/07/2016
 * 
 * Notes:       Implements the cmdlet to mask out personaly identifiable information from HL7 v2 Messages
 * 
 */

using System;
using System.IO;
using System.Management.Automation;
using Microsoft.PowerShell.Commands; 
using System.Collections.Generic;

namespace HL7Tools
{
    [Cmdlet(VerbsCommon.Remove, "HL7Identifiers")]
    public class RemoveHL7Identifiers : PSCmdlet
    {
        private char maskChar = '*';
        private bool overwriteFile = false;
        private string[] paths;
        private bool expandWildcards = false;

        // Paremeter set for the -Path and -LiteralPath parameters. A parameter set ensures these options are mutually exclusive.
        // A LiteralPath is used in situations where the filename actually contains wild card characters (eg File[1-10].txt) and you want
        // to use the literaral file name instead of treating it as a wildcard search.
        [Parameter(
            Position = 0,
            Mandatory = true,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = "Literal")
        ]
        [Alias("PSPath")]
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
            get { return paths; }
            set
            {
                this.expandWildcards = true;
                this.paths = value;
            }
        }

        // The mask character to use. Optional, defaults to '*'
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
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
            Position = 2,
            HelpMessage = "Swtich to overwrite the original file"
        )]
        public SwitchParameter OverwriteFile
        {
            get { return this.overwriteFile; }
            set { this.overwriteFile = value; }
        }
        

        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPossition
        /// </summary>
        protected override void ProcessRecord()
        {
            foreach (string path in paths)
            {
                // This will hold information about the provider containing the items that this path string might resolve to.                
                ProviderInfo provider;
                // This will be used by the method that processes literal paths
                PSDriveInfo drive;
                // this contains the paths to process for this iteration of the loop to resolve and optionally expand wildcards.
                List<string> filePaths = new List<string>();
                if (expandWildcards)
                {
                    // Turn *.txt into foo.txt,foo2.txt etc. If path is just "foo.txt," it will return unchanged.
                    filePaths.AddRange(this.GetResolvedProviderPathFromPSPath(path, out provider));
                }
                else
                {
                    // no wildcards, so don't try to expand any * or ? symbols.                    
                    filePaths.Add(this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive));
                }
                // ensure that this path (or set of paths after wildcard expansion)
                // is on the filesystem. A wildcard can never expand to span multiple providers.
                if (IsFileSystemPath(provider, path) == false)
                {
                    // no, so skip to next path in paths.
                    continue;
                }

                // At this point, we have a list of paths on the filesystem, process each file. 
                foreach (string filePath in filePaths)
                {
                    // If the file does not exist display an error and return.
                    if (!File.Exists(filePath))
                    {
                        FileNotFoundException fileException = new FileNotFoundException("File not found", filePath);
                        ErrorRecord fileNotFoundError = new ErrorRecord(fileException, "FileNotFound", ErrorCategory.ObjectNotFound, filePath);
                        WriteError(fileNotFoundError);
                        return;
                    }
                    try
                    {
                        string fileContents = File.ReadAllText(filePath);
                        HL7Message message = new HL7Message(fileContents);
                        // mask out identifiable fields
                        message.DeIdentify(this.maskChar);
                        string newFilename = filePath.Substring(0, filePath.LastIndexOf("\\") + 1) + "MASKED_" + filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - (filePath.LastIndexOf("\\") + 1));
                        // if the overwrite switch is set, then use the original file name.
                        if (this.overwriteFile)
                        {
                            newFilename = filePath;
                        }
                        System.IO.File.WriteAllText(newFilename, message.ToString());
                        WriteObject("Masked file saved as " + newFilename);
                    }
                    // if the file does not start with a MSH segment, the constructor will throw an exception. 
                    catch (System.ArgumentException)
                    {
                        ArgumentException argException = new ArgumentException("The file does not appear to be a valid HL7 v2 message", filePath);
                        ErrorRecord fileNotFoundError = new ErrorRecord(argException, "FileNotValid", ErrorCategory.InvalidData, filePath);
                        WriteError(fileNotFoundError);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Check that this provider is the filesystem
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileSystemPath(ProviderInfo provider, string path)
        {
            bool isFileSystem = true;
            if (provider.ImplementingType != typeof(FileSystemProvider))
            {
                // create a .NET exception wrapping the error text
                ArgumentException ex = new ArgumentException(path + " does not resolve to a path on the FileSystem provider.");
                ErrorRecord error = new ErrorRecord(ex, "InvalidProvider", ErrorCategory.InvalidArgument, path);
                this.WriteError(error);
                // tell the caller that the item was not on the filesystem
                isFileSystem = false;
            }
            return isFileSystem;
        }

    }
}
