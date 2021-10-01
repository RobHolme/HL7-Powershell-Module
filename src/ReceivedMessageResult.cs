namespace HL7Tools
{
    /// <summary>
    /// Simple class to store the result of the received HL7 message. This ibject is queued from the recieving thread to the CmdLet to output the object to the powershell pipeline.
    /// </summary>
    class ReceivedMessageResult
    {
        private string trigger;
        private string fileName;
        private string remoteConnection;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReceivedMessageResult(string MessageTrigger, string MessageFilename, string MessageRemoteConnection)
        {
            this.trigger = MessageTrigger;
            this.fileName = MessageFilename;
            this.remoteConnection = MessageRemoteConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Trigger
        {
            set { this.trigger = value; }
            get { return this.trigger; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Filename
        {
            set { this.fileName = value; }
            get { return this.fileName; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string RemoteConnection
        {
            set { this.remoteConnection = value; }
            get { return this.remoteConnection; }
        }

    }
}
