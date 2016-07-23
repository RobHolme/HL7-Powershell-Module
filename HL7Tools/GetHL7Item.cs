/* Filename:    GetHl7Item.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Credits:     Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        21/07/2016
 * 
 * Notes:       Implements the cmdlet to retrieve a specific item from a HL7 v2 message.
 * 
 */

using System;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;

namespace HL7Tools
{

    // TO DO: create help XML file https://msdn.microsoft.com/en-us/library/bb525433(v=vs.85).aspx

    // CmdLet: Get-HL7Item
    // Returns a specific item from the message based on the location
    [Cmdlet(VerbsCommon.Get, "HL7Item")]
    public class GetHL7Item : PSCmdlet
    {
        private string itemPosition;
        private bool listFileName;
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


        //  A parameter for position of the item to return
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ValueFromPipeline = true,
            Position = 1,
            HelpMessage = "Position of the item to return, e.g. PID-3.1"
        )]
        public string ItemPosition
        {
            get { return this.itemPosition; }
            set { this.itemPosition = value; }
        }

        // Swtich to optionally list the filename
        [Parameter(
            HelpMessage = "Switch to enable display of filename inspected"
        )]
        public SwitchParameter ListFileName
        {
            get { return this.listFileName; }
            set { this.listFileName = value; }
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

                    // if the ItemPosition parameter is not in the correct format display an error and return
                    if (!IsItemLocationValid(this.itemPosition))
                    {
                        ArgumentException argException = new ArgumentException("The -ItemPosition parameter does not appear to be in the correct format.", this.itemPosition);
                        ErrorRecord parameterError = new ErrorRecord(argException, "ParameterNotValid", ErrorCategory.InvalidArgument, this.itemPosition);
                        WriteError(parameterError);
                        return;
                    }

                    // process the message
                    try
                    {
                        string fileContents = File.ReadAllText(filePath);
                        HL7Message message = new HL7Message(fileContents);
                        string[] hl7Items = message.GetHL7Item(itemPosition);
                        // if the hlyItems array is  empty, the item was not found in the message
                        if (hl7Items.Length == 0)
                        {
                            if (this.listFileName)
                            {
                                WriteObject(filePath + ": WARNING: Item does not exist in the message");
                            }
                            else
                            {
                                WriteObject("WARNING: Item does not exist in the message");
                            }
                        }
                        //  items were returned
                        else
                        {
                            if (this.listFileName)
                            {
                                foreach (string item in hl7Items)
                                {
                                    WriteObject(filePath + ": " + item.ToString());
                                }
                            }
                            else
                            {
                                WriteObject(hl7Items);
                            }
                        }
                    }
                    // if the file does not start with a MSH segment, the constructor will throw an exception. 
                    catch (System.ArgumentException)
                    {
                        ArgumentException argException = new ArgumentException("The file does not appear to be a valid HL7 v2 message", filePath);
                        ErrorRecord invalidFileError = new ErrorRecord(argException, "FileNotValid", ErrorCategory.InvalidData, filePath);
                        WriteError(invalidFileError);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Confirm that the HL7 item location string is in a valid format. It does not check to see if the item referenced exists or not.
        /// </summary>
        /// <param name="hl7ItemLocation"></param>
        /// <returns></returns>
        private bool IsItemLocationValid(string hl7ItemLocation)
        {
            // make sure the location requested mactches the regex of a valid location string. This does not check to see if segment names exit, or items are present in the message
            if (System.Text.RegularExpressions.Regex.IsMatch(hl7ItemLocation, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)) // regex to confirm the HL7 element location string is valid
            {
                // make sure field, component and subcomponent values are not 0
                if (System.Text.RegularExpressions.Regex.IsMatch(hl7ItemLocation, "([.]0)|([-]0)", RegexOptions.IgnoreCase))
                {
                    return false;
                }
                return true;
            }
            return false;
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
