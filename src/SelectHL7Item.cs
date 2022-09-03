/* Filename:    SelectHl7Item.cs
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

// TO DO: create help XML file https://msdn.microsoft.com/en-us/library/bb525433(v=vs.85).aspx

namespace HL7Tools {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Management.Automation;
	using System.Text;

	// CmdLet: Select-HL7Item
	// Returns a specific item from the message based on the location
	[Cmdlet(VerbsCommon.Select, "HL7Item")]
	public class SelectHL7Item : PSCmdlet {
		private string[] itemPosition;
		private string[] paths;
		private bool expandWildcards = false;
		private string[] filter = new string[] { };
		private bool filterConditionsMet = true;
		private string encoding = "UTF-8";
		private bool abortProcessing = false;

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
		public string[] LiteralPath {
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
		public string[] Path {
			get { return this.paths; }
			set {
				this.expandWildcards = true;
				this.paths = value;
			}
		}

		//  A parameter for position of the item to return
		[Parameter(
			Mandatory = true,
			Position = 1,
			HelpMessage = "Position of the item(s) to return, e.g. PID-3.1"
		)]
		[Alias("Item")]
		public string[] ItemPosition {
			get { return this.itemPosition; }
			set { this.itemPosition = value; }
		}

		// Parameter to optionally filter the messages based on matching message contents
		[Parameter(
			Mandatory = false,
			Position = 2,
			HelpMessage = "Filter on message contents")]
		public string[] Filter {
			get { return this.filter; }
			set { this.filter = value; }
		}

		// Parameter to specify the message character encoding format
		[Parameter(
			Mandatory = false,
			Position = 3,
			HelpMessage = "Text encoding ('UTF-8' | 'ISO-8859-1'")]
		[ValidateSet("UTF-8", "ISO-8859-1")]
		public string Encoding {
			get { return this.encoding; }
			set { this.encoding = value; }
		}

		/// <summary>
		/// Powershell BeginProcessing. Validate paremeters are formatted correctly.
		/// </summary>		
		protected override void BeginProcessing() {
			base.BeginProcessing();

			// confirm the item location parameter is valid before processing any files
			foreach (string position in this.itemPosition) {
				if (!Common.IsItemLocationValid(position)) {
					ArgumentException ex = new ArgumentException(position + " does not appear to be a valid HL7 item possition");
					ErrorRecord error = new ErrorRecord(ex, "InvalidElement", ErrorCategory.InvalidArgument, position);
					this.WriteError(error);
					abortProcessing = true;
					return;
				}
			}

			// confirm the filter parameter is valid before processing any files
			foreach (string currentFilter in this.filter) {
				// confirm each filter is formatted correctly
				if (!Common.IsFilterValid(currentFilter)) {
					ArgumentException ex = new ArgumentException(currentFilter + " does not appear to be a valid filter");
					ErrorRecord error = new ErrorRecord(ex, "InvalidFilter", ErrorCategory.InvalidArgument, currentFilter);
					this.WriteError(error);
					abortProcessing = true;
					return;
				}
			}
		}


		/// <summary>
		/// get the HL7 item provided via the cmdlet parameter HL7ItemPosition
		/// </summary>
		protected override void ProcessRecord() {
			base.ProcessRecord();

			if (abortProcessing) {
				return;
			}

			// set the text encoding
			Encoding encoder = System.Text.Encoding.GetEncoding(this.encoding);
			WriteVerbose("Encoding: " + encoder.EncodingName);

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
						// assume the filter is true, until a failed match is found
						this.filterConditionsMet = true;
						// load the file into a HL7Message object for processing
						string fileContents = File.ReadAllText(filePath, encoder);
						HL7Message message = new HL7Message(fileContents);
						// if a filter was supplied, evaluate if the file matches the filter condition
						if (this.filter != null) {
							// check to see is all of the filter conditions are met (ie AND all filters supplied). 
							foreach (string currentFilter in this.filter) {
								bool anyItemMatch = false;
								string filterItem = Common.GetFilterItem(currentFilter);
								string filterValue = Common.GetFilterValue(currentFilter);
								// for repeating fields, only one of the items returned has to match for the filter to be evaluated as true.
								foreach (string itemValue in message.GetHL7ItemValue(filterItem)) {
									// convert both values to upper case for a case insentive match
									if (itemValue.ToUpper() == filterValue.ToUpper()) {
										anyItemMatch = true;
									}
								}
								// if none of the repeating field items match, then fail the filter match for this file. 
								if (!anyItemMatch) {
									this.filterConditionsMet = false;
								}
							}
						}

						// if the filter supplied matches this message (or no filter provided) then process the file to optain the HL7 item requested
						if (filterConditionsMet) {
							// create a PSObject								
							PSObject result = new PSObject();
							foreach (string hl7ItemPosition in itemPosition) {
								string[] hl7Items = message.GetHL7ItemValue(hl7ItemPosition);
								// if the hl7Items array is  empty, the item was not found in the message. Write null property otherwise powershell will omit columns in output if firest value does nto include all properties
								if (hl7Items.Length == 0) {
									result.Properties.Add(new PSNoteProperty(hl7ItemPosition.ToString(), null));
									WriteVerbose("Item " + hl7ItemPosition + " not found in the message " + filePath);
								}
								//  items were returned, add properties to PSObject
								else {
									// if only a single value returned, save it as a string, not a string[]
									if (hl7Items.Length == 1) {
										result.Properties.Add(new PSNoteProperty(hl7ItemPosition.ToString(), hl7Items[0]));
									}
									else {
										result.Properties.Add(new PSNoteProperty(hl7ItemPosition.ToString(), hl7Items));
									}
								}
							}
							result.Properties.Add(new PSNoteProperty("Filename", filePath));
							WriteObject(result);
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
	}


}
