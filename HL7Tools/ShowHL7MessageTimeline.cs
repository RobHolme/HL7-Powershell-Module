/* Filename:    SelectHl7Item.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Credits:     Code to handle the Path and LiteralPath parameter sets, and expansion of wildcards is based
 *              on Oisin Grehan's post: http://www.nivot.org/blog/post/2008/11/19/Quickstart1ACmdletThatProcessesFilesAndDirectories
 * 
 * Date:        15/06/2017
 * 
 * Notes:       Orders a group of messages chronologically based on the MSH-7 (Received DateTime) field.
 * 
 */

namespace HL7Tools
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text.RegularExpressions;

    // CmdLet: Show-HL7MessageTimeline
    // Returns a specific item from the message based on the location
    [Cmdlet(VerbsCommon.Show, "HL7MessageTimeline")]
    public class ShowHL7MessageTimeline : PSCmdlet
    {
        private string[] paths;
        private bool expandWildcards = false;
        private bool descending = false;
        List<ShowHL7MessageTimelineResult> messageTimestampResults = new List<ShowHL7MessageTimelineResult>();

        // Parameter set for the -Path and -LiteralPath parameters. A parameter set ensures these options are mutually exclusive.
        // A LiteralPath is used in situations where the filename actually contains wild card characters (eg File[1-10].txt) and you want
        // to use the literaral file name instead of treating it as a wildcard search.
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

        // Switch to order the message timestamps in descending order instead
        [Parameter(
            Mandatory = false,
            HelpMessage = "Order messages in descending chronological order"
         )]
        [Alias("Desc")]
        public SwitchParameter Descending
        {
            get { return this.descending; }
            set { this.descending = value; }
        }

        /// <summary>
        /// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
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

                // if the path provided is a directory, expand the files in the directory and add these to the list.
                if (Directory.Exists(path)) {
                    filePaths.AddRange(Directory.GetFiles(path));
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
                    try {
                        // load the file into a HL7Message object for processing
                        string fileContents = File.ReadAllText(filePath);
                        HL7Message message = new HL7Message(fileContents);

                        // get the message trigger from MSH-9 (only interested in MSH-9.1 and MSH-9.2 - ignore any document types if included).                       
                        string[] messageTrigger = message.GetHL7ItemValue("MSH-9");
                        string[] splitTrigger = messageTrigger[0].Split(message.ComponentDelimeter);
                        string simplifiedTrigger;
                        if (splitTrigger.Length  > 1) {
                            simplifiedTrigger = splitTrigger[0] + message.ComponentDelimeter + splitTrigger[1];
                        }
                        else {
                            simplifiedTrigger = splitTrigger[0];
                        }

                        // get the message timestamp
                        string[] messageDateTime = message.GetHL7ItemValue("MSH-7");
                        // if the hl7Items array is  empty, the item was not found in the message
                        if (messageDateTime.Length == 0) {
                            WriteWarning("MSH-7 does not contain a value for " + filePath);
                        }
                        //  items were returned
                        else {
                            string timePattern = "[0-9]{12}([0-9]{2})?";
                            string timezonePattern = "(?<=(/+|-))[0-9]{4}";

                            Match timeMatch = Regex.Match(messageDateTime[0], timePattern);
                            Match timezoneMatch = Regex.Match(messageDateTime[0], timezonePattern);

                            if (timeMatch.Success) {
                                // time zone is includes
                                if (timezoneMatch.Success) {
                                    // time includes seconds
                                    if (timeMatch.Value.Length == 14) {
                                        ShowHL7MessageTimelineResult resultListItem = new ShowHL7MessageTimelineResult(DateTime.ParseExact(timeMatch.Value + timezoneMatch.Groups[1].Value + timezoneMatch.Value, "yyyyMMddHHmmsszzzz", null), filePath, simplifiedTrigger);
                                        messageTimestampResults.Add(resultListItem);
                                    }
                                    // time only resolves to minutes
                                    else {
                                        ShowHL7MessageTimelineResult resultListItem = new ShowHL7MessageTimelineResult(DateTime.ParseExact(timeMatch.Value + timezoneMatch.Groups[1].Value + timezoneMatch.Value, "yyyyMMddHHmmzzzz", null), filePath, simplifiedTrigger);
                                        messageTimestampResults.Add(resultListItem);
                                    }
                                }
                                // timezone is not included
                                else {
                                    // time includes seconds
                                    if (timeMatch.Value.Length == 14) {
                                        ShowHL7MessageTimelineResult resultListItem = new ShowHL7MessageTimelineResult(DateTime.ParseExact(timeMatch.Value, "yyyyMMddHHmmss", null), filePath, simplifiedTrigger);
                                        messageTimestampResults.Add(resultListItem);
                                    }
                                    // time only resolves to minutes
                                    else {
                                        ShowHL7MessageTimelineResult resultListItem = new ShowHL7MessageTimelineResult(DateTime.ParseExact(timeMatch.Value, "yyyyMMddHHmm", null), filePath, simplifiedTrigger);
                                        messageTimestampResults.Add(resultListItem);
                                    }


                                }
                            }
                            // timestamp missing, or does not resolve down to at least minutes
                            else {
                                WriteWarning("Timestamp missing from " + filePath);
                            }

                        }

                    }
                    // if the file does not start with a MSH segment, the constructor will throw an exception. 
                    catch (System.ArgumentException) {
                        ArgumentException argException = new ArgumentException("The file does not appear to be a valid HL7 v2 message", filePath);
                        ErrorRecord invalidFileError = new ErrorRecord(argException, "FileNotValid", ErrorCategory.InvalidData, filePath);
                        WriteError(invalidFileError);
                        return;
                    }
                }
            }
        }


        /// <summary>
        /// write the results for all files piped into the CmdLet
        /// </summary>
        protected override void EndProcessing()
        {
            IEnumerable<ShowHL7MessageTimelineResult> orderedResults;

            // order the list of results by the timestamp property in descending order if the -descending switch is provided
            if (this.descending) {
                orderedResults = messageTimestampResults.OrderByDescending(ShowHL7MessageTimelineResult => ShowHL7MessageTimelineResult.MessageTimestamp);
            }
            // default to ordering the timestamps in ascending order if the -descending swithc is not provided
            else {
                orderedResults = messageTimestampResults.OrderBy(ShowHL7MessageTimelineResult => ShowHL7MessageTimelineResult.MessageTimestamp);
            }
            foreach (ShowHL7MessageTimelineResult item in orderedResults) {
                WriteObject(item);
            }
        }
    }

    /// <summary>
    /// An object containing the results to be returned to the pipeline
    /// </summary>
    public class ShowHL7MessageTimelineResult
    {
        private DateTime messageTimestamp;
        private string filename;
        private string messageTrigger;

        /// <summary>
        /// The value of the HL7 item
        /// </summary>
        public DateTime MessageTimestamp
        {
            get { return this.messageTimestamp; }
            set { this.messageTimestamp = value; }
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
        /// The HL7 message trigger (MSH-9)
        /// </summary>
        public string MessageTrigger
        {
            get { return this.messageTrigger; }
            set { this.messageTrigger = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <param name="Filename"></param>
        public ShowHL7MessageTimelineResult(DateTime timestamp, string Filename, string Trigger)
        {
            this.messageTimestamp = timestamp;
            this.filename = Filename;
            this.messageTrigger = Trigger;
        }
    }

}
