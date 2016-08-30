/* Filename:    RemoveHL7Item.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Credits:     Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        30/08/2016
 * 
 * Notes:       Implements the cmdlet to update the value of a specific item from a HL7 v2 message.
 * 
 */

namespace HL7Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;

    // CmdLet: Remove-HL7Item
    // Removes the value of a specific item from the message 
    [Cmdlet(VerbsCommon.Remove, "HL7Item")]
    public class RemoveHL7Item : PSCmdlet
    {
        private string[] itemPosition;
        private string[] paths;
        private bool expandWildcards = false;
        private string[] filter = new string[] { };
        private bool filterConditionsMet = true;
        private bool allrepeats;

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

        //  A parameter for position of the item to remove
        [Parameter(
            Mandatory = true,
            Position = 1,
            HelpMessage = "Position of the item to return, e.g. PID-3.1"
        )]
        [Alias("Item")]
        public string[] ItemPosition
        {
            get { return this.itemPosition; }
            set { this.itemPosition = value; }
        }


        // Parameter to optionally filter the messages based on matching message contents
        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "Filter on message contents")]
        public string[] Filter
        {
            get { return this.filter; }
            set { this.filter = value; }
        }

        // Do not wait for ACKs responses if this switch is set
        [Parameter(
            Mandatory = false,
            HelpMessage = "Update all repeats of an item"
         )]
        public SwitchParameter UpdateAllRepeats
        {
            get { return this.allrepeats; }
            set { this.allrepeats = value; }
        }

        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
        /// </summary>
        protected override void ProcessRecord()
        {

            // validate the that all of the list of locations to remove are valid.
            foreach (string item in this.itemPosition)
            {
                // confirm each filter is formatted correctly
                if (!Common.IsItemLocationValid(item))
                {
                    ArgumentException ex = new ArgumentException(item + " does not appear to be a valid HL7 location. Ensure the -ItemPosition list is formatted correctly.");
                    ErrorRecord error = new ErrorRecord(ex, "InvalidFilter", ErrorCategory.InvalidArgument, item);
                    this.WriteError(error);
                    return;
                }
            }
         
            // confirm the filter parameter is valid before processing any files
            foreach (string currentFilter in this.filter)
            {
                // confirm each filter is formatted correctly
                if (!Common.IsFilterValid(currentFilter))
                {
                    ArgumentException ex = new ArgumentException(currentFilter + " does not appear to be a valid filter");
                    ErrorRecord error = new ErrorRecord(ex, "InvalidFilter", ErrorCategory.InvalidArgument, currentFilter);
                    this.WriteError(error);
                    return;
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
                    if (Common.IsFileSystemPath(provider, path) == false)
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

     /*               // if the ItemPosition parameter is not in the correct format display an error and return
                    if (!Common.IsItemLocationValid(this.itemPosition))
                    {
                        ArgumentException argException = new ArgumentException("The -ItemPosition parameter does not appear to be in the correct format.", this.itemPosition);
                        ErrorRecord parameterError = new ErrorRecord(argException, "ParameterNotValid", ErrorCategory.InvalidArgument, this.itemPosition);
                        WriteError(parameterError);
                        return;
                    }
    */
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
                                string filterItem = Common.GetFilterItem(currentFilter);
                                string filterValue = Common.GetFilterValue(currentFilter);
                                // for repeating fields, only one of the items returned has to match for the filter to be evaluated as true.
                                foreach (string itemValue in message.GetHL7ItemValue(filterItem))
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
                            foreach (string locationItem in this.itemPosition)
                            {
                                List<HL7Item> hl7Items = message.GetHL7Item(locationItem);
                                // if the hl7Items array is  empty, the item was not found in the message
                                if (hl7Items.Count == 0)
                                {
                                    WriteWarning("Item " + this.itemPosition + " not found in the message " + filePath);
                                }

                                //  items were located in the message, so proceed with replacing the original value with the new value.
                                else
                                {
                                    // update all repeats/occurances of the specified item
                                    if (this.allrepeats)
                                    {
                                        foreach (HL7Item item in hl7Items)
                                        {
                                            RemoveHL7ItemResult result = new RemoveHL7ItemResult(item.ToString(), filePath, locationItem);
                                            item.SetValueFromString(string.Empty);
                                            WriteObject(result);
                                        }
                                    }
                                    // update only the first occurrance. This is the default action.
                                    else
                                    {
                                        RemoveHL7ItemResult result = new RemoveHL7ItemResult(hl7Items.ElementAt(0).ToString(), filePath, locationItem);
                                        hl7Items.ElementAt(0).SetValueFromString(string.Empty);
                                        WriteObject(result);
                                    }
                                    // save the changes back to the original file
                                    System.IO.File.WriteAllText(filePath, message.ToString());
                                }
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
    }

    /// <summary>
    /// An object containing the results to be returned to the pipeline. 
    /// </summary>
    public class RemoveHL7ItemResult
    {
        private string oldValue;
        private string location;
        private string filename;

        /// <summary>
        /// The previous value of the HL7 item that was removed
        /// </summary>
        public string DeletedValue
        {
            get { return this.oldValue; }
            set { this.oldValue = value; }
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
        /// The location of the HL7 item that was changed. e.g. PID-3.1
        /// </summary>
        public string HL7Item
        {
            get { return this.location.ToUpper(); }
            set { this.location = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <param name="Filename"></param>
        public RemoveHL7ItemResult(string OldValue, string Filename, string HL7Item)
        {
            this.oldValue = OldValue;
            this.filename = Filename;
            this.location = HL7Item;
        }
    }

}
