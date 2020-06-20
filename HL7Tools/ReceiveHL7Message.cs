/* Filename:    ReceiveHL7Message.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *
 * Date:        25/02/2017
 * 
 * Notes:       Implements a cmdlet to receive a MLLP framed HL7 message via TCP. Messages reveived are written to disk.
 * 
 */

namespace HL7Tools
{
    using System.IO;
    using System.Management.Automation;
    using System.Threading;
    using System.Collections.Concurrent;

    [Cmdlet("Receive", "HL7Message")]
    public class ReceiveHL7Message : PSCmdlet
    {
        private int port;
        private bool noACK;
        private string path;
        private bool abortProcessing = false;
        private int timeout = 60000;  // default to  1 minute
		private string encoding = "UTF-8";
        private ConcurrentQueue<ReceivedMessageResult> objectQueue = new ConcurrentQueue<ReceivedMessageResult>();
        private ConcurrentQueue<string> warningQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<string> verboseQueue = new ConcurrentQueue<string>();

        [Parameter(
            Position = 0,
            HelpMessage = "The path to save received messages to",
            Mandatory = true)

        ]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get { return this.path; }
            set
            {
                this.path = value;
            }
        }

        // The port number to listen on
        [Parameter(
            Mandatory = true,
            Position = 1,
            HelpMessage = "The TCP port to listen on"
        )]
        [ValidateRange(1, 65535)]
        public int Port
        {
            get { return this.port; }
            set { this.port = value; }
        }

        // The timeout to terminate idle TCP connections in seconds (stored in milliseconds)
        [Parameter(
            Mandatory = false,
            Position = 2,
            HelpMessage = "The timeout to end idle TCP connections in seconds."
        )]
        [ValidateRange(1, 1800)]
        public int Timeout
        {
            get { return (this.timeout / 1000); }
            set { this.timeout = (value*1000); }
        }

        // The encoding used when receiving the message
        [Parameter(
            Mandatory = false,
            Position = 3,
            HelpMessage = "Message text encoding"
        )]
        [ValidateSet("UTF-8", "ISO-8859-1")]
        public string Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        // Do not wait for ACKs responses if this switch is set
        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not wait for ACK response"
         )]
        public SwitchParameter NoACK
        {
            get { return this.noACK; }
            set { this.noACK = value; }
        }

        /// <summary>
        /// Validate items before processing the pipeline. Only called once before all items in the pipeline processed by BeginProcessing()
        /// </summary>
        protected override void BeginProcessing()
        {
            if (!Directory.Exists(this.path)) {
                WriteWarning("The path " + this.path + " does not exist, or could not be accessed.");
                this.abortProcessing = true;
            }
			else {
				// expand the path
				this.path = System.IO.Path.GetFullPath(this.path);

			}
			WriteVerbose("Encoding: " + encoder.EncodingName);
        }

        /// <summary>
        /// Listen for incoming MLLP framed messages
        /// </summary>
        protected override void ProcessRecord()
        {
            // if any of the validation checks in BeginProcessing() has failed, return without processing.
            if (abortProcessing) {
                return;
            }
            WriteWarning("Listening for MLLP framed messages on port " + this.port + ". Close powershell console to exit.");
            
            // create a new instance of HL7TCPListener. Set optional properties to return ACKs, passthru messages, archive location. Start the listener.
            HL7TCPListener listener = new HL7TCPListener(port, ref objectQueue, ref warningQueue, ref verboseQueue, this.timeout, this.encoding);
            if (noACK) {
                listener.SendACK = false;
            }
            else {
                listener.SendACK = true;
            }
            if (path != null) {
                listener.FilePath = path;
				WriteVerbose("Saving received files to " + listener.FilePath);
            }
            if (!listener.Start()) {
                WriteWarning("Failed to start TCP Listener");
            }
            else {
                while (listener.IsRunning()) {
                    // dequeue and write any warning messages received from TCPHL7Listener
                    string tempMessage;
                    while (warningQueue.TryDequeue(out tempMessage)) {
                        WriteWarning(tempMessage);                    
                    }
                    // dequeue and write any result objects
                    ReceivedMessageResult tempResult;
                    while (objectQueue.TryDequeue(out tempResult)) {
                        WriteObject(tempResult);
                    }
                    // dequeue and write and verbose logging to the console
                    string tempVerbose;
                    while (verboseQueue.TryDequeue(out tempVerbose)) {
                        WriteVerbose(tempVerbose);
                    }
                    // sleep before checking the queues again
                    Thread.Sleep(1000);
                }
            }
        }

    }
}
