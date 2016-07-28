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

// TO DO:   How to handle items that are not present in the message
//          manifest file to format the output

using System;
using System.IO;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.PowerShell.Commands;

namespace HL7Tools
{
    // TO DO: create help XML file https://msdn.microsoft.com/en-us/library/bb525433(v=vs.85).aspx

    // CmdLet: Select-HL7Item
    // Returns a specific item from the message based on the location
    [Cmdlet(VerbsCommon.Select, "HL7Item")]
    public class SelectHL7Item : PSCmdlet
    {
        private string itemPosition;
        private string[] paths;
        private bool expandWildcards = false;
        private string[] filter;
        private bool filterConditionsMet = true;

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
            get { return this.paths; }
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

        // Parameter to optionally filter the messages based on matching message contents
        [Parameter(
            HelpMessage = "Filter on message contents")]
        public string[] Filter
        {
            get { return this.filter; }
            set { this.filter = value; }
        }

        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
        /// </summary>
        protected override void ProcessRecord()
        {
            // confirm the item location parameter is valid before processing any files
            if (!this.IsHL7LocationStringValid(this.itemPosition))
            {
                ArgumentException ex = new ArgumentException(this.itemPosition + " does not appear to be a valid HL7 item");
                ErrorRecord error = new ErrorRecord(ex, "InvalidElement", ErrorCategory.InvalidArgument, this.itemPosition);
                this.WriteError(error);
                return;
            }

            // confirm the filter parameter is valid before processing any files
            if (this.filter != null)
            {
                foreach (string currentFilter in this.filter)
                {
                    // confirm each filter is formatted correctly
                    if (!this.IsFilterValid(currentFilter))
                    {
                        ArgumentException ex = new ArgumentException(currentFilter + " does not appear to be a valid filter");
                        ErrorRecord error = new ErrorRecord(ex, "InvalidFilter", ErrorCategory.InvalidArgument, currentFilter);
                        this.WriteError(error);
                        return;
                    }
                }
            }

            // expand the file or directory information provided in the -Path or -LiteralPath parameters
            foreach (string path in paths)
            {
                // This will hold information about the provider containing the items that this path string might resolve to.                
                ProviderInfo provider;
             
                // This will be used by the method that processes literal paths
                PSDriveInfo drive;
                
                // this contains the paths to process for this iteration of the loop to resolve and optionally expand wildcards.
                List<string> filePaths = new List<string>();
                
                // if the path provided is a directory, expand the files in the directy and add these to the list.
                if (Directory.Exists(path))
                {
                    filePaths.AddRange(Directory.GetFiles(path));
                }
                
                // not a directory, could be a wildcard or literal filepath 
                else
                {
                    // expand wildcards. This assumes if the user listed a directory it is literal
                    if (expandWildcards)
                    {
                        // Turn *.txt into foo.txt,foo2.txt etc. If path is just "foo.txt," it will return unchanged. If the filepath expands into a directory ignore it.
                        foreach (string expandedFilePath in this.GetResolvedProviderPathFromPSPath(path, out provider))
                        {
                            if (!Directory.Exists(expandedFilePath))
                            {
                                filePaths.Add(expandedFilePath);
                            }
                        }
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
                    if (!this.IsItemLocationValid(this.itemPosition))
                    {
                        ArgumentException argException = new ArgumentException("The -ItemPosition parameter does not appear to be in the correct format.", this.itemPosition);
                        ErrorRecord parameterError = new ErrorRecord(argException, "ParameterNotValid", ErrorCategory.InvalidArgument, this.itemPosition);
                        WriteError(parameterError);
                        return;
                    }

                    // process the message
                    try
                    {
                        // assume the filter is true, until a failed match is found
                        this.filterConditionsMet = true;
                        // load the file into a HL7Message object for processing
                        string fileContents = File.ReadAllText(filePath);
                        HL7Message message = new HL7Message(fileContents);
                        // if a filter was supplied, evaluate if the file matches the filter condition
                        if (this.filter != null)
                        {
                            // check to see is all of the filter conditions are met (ie AND all filters supplied). 
                            foreach (string currentFilter in this.filter)
                            {
                                bool anyItemMatch = false;
                                string filterItem = this.GetFilterItem(currentFilter);
                                string filterValue = this.GetFilterValue(currentFilter);
                                // for repeating fields, only one of the items returned has to match for the filter to be evaluated as true.
                                foreach (string itemValue in message.GetHL7Item(filterItem))
                                {
                                    if (itemValue == filterValue)
                                    {
                                        anyItemMatch = true;
                                    }
                                }
                                // if none of the repeating field items match, then fail the filter match for this file. 
                                if (!anyItemMatch)
                                {
                                    this.filterConditionsMet = false;
                                }
                            }
                        }
                        
                        // if the filter supplied matches this message (or no filter provided) then process the file to optain the HL7 item requested
                        if (filterConditionsMet)
                        {
                            string[] hl7Items = message.GetHL7Item(itemPosition);
                            // if the hl7Items array is  empty, the item was not found in the message
                            if (hl7Items.Length == 0)
                            {
                                WriteWarning("Item " + this.itemPosition + " not found in the message " + filePath);
                            }
                            
                            //  items were returned
                            else
                            {
                                SelectHL7ItemResult result = new SelectHL7ItemResult(hl7Items, filePath);
                                WriteObject(result);
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

        /// <summary>
        ///  Make sure the filter string matches the expected pattern for a filter
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private bool IsFilterValid(string filterString)
        {
            if (Regex.IsMatch(filterString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?=", RegexOptions.IgnoreCase))
            {
                return true;
            }

            // the value provided after the -filter switch did not match the expected format of a message trigger.
            else 
            {
                return false;
            }
        }

        /// <summary>
        /// return the portion of the filter string that identifies the HL7 Item to filter on
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private string GetFilterItem(string filterString)
        {
            if (IsFilterValid(filterString))
            {
                string[] tempString = (filterString).Split('=');
                return tempString[0]; 
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// return the portion of the filter string that identifies the value to filter on
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private string GetFilterValue(string filterString)
        {
            if (IsFilterValid(filterString))
            {
                string[] tempString = (filterString.Split('='));
                if (tempString.Length > 1)
                {
                    return tempString[1];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
          
        }

        /// <summary>
        /// return true if the string representing the HL7 location is valid. This does not confirm if the items exists, it only checks the formating of the string.
        /// </summary>
        /// <param name="HL7LocationString">The string identifying the location of the item within the message. eg PID-3.1</param>
        /// <returns></returns>
        private bool IsHL7LocationStringValid (string HL7LocationString)
        {
            return (Regex.IsMatch(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)); // segment([repeat])? or segment([repeat)?-field([repeat])? or segment([repeat)?-field([repeat])?.component or segment([repeat)?-field([repeat])?.component.subcomponent 
        }
    }

    /// <summary>
    /// An object containing the results to be returned to the pipeline
    /// </summary>
    public class SelectHL7ItemResult
    {
        private string[] hl7Itemvalue;
        private string filename;

        /// <summary>
        /// The value of the HL7 item
        /// </summary>
        public string[] HL7Item
        {
            get { return this.hl7Itemvalue; }
            set { this.hl7Itemvalue = value; }
        }

        /// <summary>
        /// The filename containing the item returned
        /// </summary>
        public string Filename
        {
            get { return this.filename; }
            set { this.filename = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemValue"></param>
        public SelectHL7ItemResult(string[] ItemValue)
        {
            this.hl7Itemvalue = ItemValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <param name="Filename"></param>
        public SelectHL7ItemResult(string[] ItemValue, string Filename)
        {
            this.hl7Itemvalue = ItemValue;
            this.filename = Filename;
        }
    }
}
