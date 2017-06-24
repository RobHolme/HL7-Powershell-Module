
namespace HL7Tools
{
    using System;
    using System.Text;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Collections.Concurrent;

    class HL7TCPListener
    {
        int TCP_TIMEOUT; // timeout value for receiving TCP data in millseconds
        private TcpListener tcpListener;
        private Thread tcpListenerThread;
        private int listernerPort;
        private string archivePath = null;
        private bool sendACK = true;
        private string passthruHost = null;
        private int passthruPort;
        private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
        private bool runThread = true;
        private ConcurrentQueue<ReceivedMessageResult> objectQueue;
        private ConcurrentQueue<string> warningQueue;
        private ConcurrentQueue<string> debugQueue;


        /// <summary>
        /// Constructor, includes wreferences to return warnings and result objects
        /// </summary>
        public HL7TCPListener(int port, ref ConcurrentQueue<ReceivedMessageResult> messageQueueRef, ref ConcurrentQueue<string> warningQueueRef, ref ConcurrentQueue<string> verboseQueueRef, int TimeOut = 30000)
        {
            this.listernerPort = port;
            warningQueue = warningQueueRef;
            objectQueue = messageQueueRef;
            debugQueue = verboseQueueRef;
            TCP_TIMEOUT = TimeOut;
        }

        /// <summary>
        /// Start the TCP listener. Log the options set.
        /// </summary>
        public bool Start()
        {
            // start a new thread to listen for new TCP conmections
            this.tcpListener = new TcpListener(IPAddress.Any, this.listernerPort);
            this.tcpListenerThread = new Thread(new ThreadStart(StartListener));
            this.tcpListenerThread.Start();
            return true;
        }

        /// <summary>
        /// Stop the all threads
        /// </summary>
        public void RequestStop()
        {
            this.runThread = false;
        }

        /// <summary>
        /// Start listening for new connections
        /// </summary>
        private void StartListener()
        {
            try {
                this.tcpListener.Start();
                // run the thread unless a request to stop is received
                while (this.runThread) {
                    // waits for a client connection to the listener
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    this.LogDebug("New client connection accepted from " + client.Client.RemoteEndPoint);
                    // create a new thread. This will handle communication with a client once connected
                    Thread clientThread = new Thread(new ParameterizedThreadStart(ReceiveData));
                    clientThread.Start(client);
                }
            }
            catch (Exception e) {
                LogWarning("An error occurred while attempting to start the listener on port " + this.listernerPort);
                LogWarning(e.Message);
                LogWarning("HL7Listener exiting.");
            }
        }

        /// <summary>
        /// Receive data from a client connection, look for MLLP HL7 message.
        /// </summary>
        /// <param name="client"></param>
        private void ReceiveData(object client)
        {
            // generate a random sequence number to use for the file names
            Random random = new Random(Guid.NewGuid().GetHashCode());
            int filenameSequenceStart = random.Next(0, 1000000);

            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            clientStream.ReadTimeout = TCP_TIMEOUT;
            clientStream.WriteTimeout = TCP_TIMEOUT;

            byte[] messageBuffer = new byte[4096];
            int bytesRead;
            String messageData = "";
            int messageCount = 0;

            while (true) {
                bytesRead = 0;
                try {
                    // Wait until a client application submits a message
                    bytesRead = clientStream.Read(messageBuffer, 0, 4096);
                }
                catch (Exception) {
                    // A network error has occurred
                    LogDebug("Connection from " + tcpClient.Client.RemoteEndPoint + " has ended");
                    break;
                }
                if (bytesRead == 0) {
                    // The client has disconected
                    LogDebug("The client " + tcpClient.Client.RemoteEndPoint + " has disconnected");
                    break;
                }
                // Message buffer received successfully
                messageData += Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
                // Find a VT character, this is the beginning of the MLLP frame
                int start = messageData.IndexOf((char)0x0B);
                if (start >= 0) {
                    // Search for the end of the MLLP frame (a FS character)
                    int end = messageData.IndexOf((char)0x1C);
                    if (end > start) {
                        messageCount++;
                        try {
                            // queue the message to sent to the passthru host if the -PassThru option has been set
                            if (passthruHost != null) {
                                messageQueue.Enqueue(messageData.Substring(start + 1, end - (start + 1)));
                            }
                            // create a HL7message object from the message recieved. Use this to access elements needed to populate the ACK message and file name of the archived message
                            HL7Message message = new HL7Message(messageData.Substring(start + 1, end - (start + 1)));
                            messageData = ""; // reset the message data string for the next message
                            string messageTrigger = message.GetHL7ItemValue("MSH-9")[0];
                            string messageControlID = message.GetHL7ItemValue("MSH-10")[0];
                            //string acceptAckType = message.GetHL7Item("MSH-15")[0];
                            string dateStamp = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2,'0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + DateTime.Now.Hour.ToString().PadLeft(2, '0') + DateTime.Now.Minute.ToString().PadLeft(2, '0');
                            string filename = dateStamp + "_" + (filenameSequenceStart + messageCount).ToString("D6") + "_" + messageTrigger + ".hl7"; //  increment sequence number for each filename
                            // Write the HL7 message to file.
                            WriteMessagetoFile(message.ToString(), System.IO.Path.Combine(this.archivePath,filename));
                            ReceivedMessageResult resultObject = new ReceivedMessageResult(messageTrigger, System.IO.Path.Combine(this.archivePath, filename), tcpClient.Client.RemoteEndPoint.ToString());
                            objectQueue.Enqueue(resultObject);
                            // send ACK message is MSH-15 is set to AL and ACKs not disbaled by -NOACK command line switch
                            //if ((this.sendACK) && (acceptAckType.ToUpper() == "AL"))
                            if (this.sendACK) {
                                LogDebug("Sending ACK (Message Control ID: " + messageControlID + ")");
                                // generate ACK Message and send in response to the message received
                                string response = GenerateACK(message.ToString());  // TO DO: send ACKS if set in message header, or specified on command line
                                byte[] encodedResponse = Encoding.UTF8.GetBytes(response);
                                // Send response
                                try {
                                    clientStream.Write(encodedResponse, 0, encodedResponse.Length);
                                    clientStream.Flush();
                                }
                                catch (Exception e) {
                                    // A network error has occurred
                                    LogDebug("An error has occurred while sending an ACK to the client " + tcpClient.Client.RemoteEndPoint);
                                    LogDebug(e.Message);
                                    break;
                                }
                            }
                        }
                        catch (Exception e) {
                            messageData = ""; // reset the message data string for the next message
                            LogWarning("An exception occurred while parsing the HL7 message");
                            LogWarning(e.Message);
                            break;
                        }
                    }
                }
            }
            LogDebug("Total messages received:" + messageCount);
            clientStream.Close();
            clientStream.Dispose();
            tcpClient.Close();
        }

        /// <summary>
        /// /// <summary>
        /// Write the HL7 message recieved to file. Optionally provide the file path, otherwise use the working directory.     
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filePath"></param>
        private void WriteMessagetoFile(string message, string filename)
        {
            // write the HL7 message to file
            try {
                LogDebug("Received message. Saving to file " + filename);
                System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
                file.Write(message);
                file.Close();
            }
            catch (Exception e) {
                LogWarning("Failed to write file " + filename);
                LogWarning(e.Message);
            }
        }

        /// <summary>
        /// Generate a string containing the ACK message in response to the original message. Supply a string containing the original message (or at least the MSH segment).
        /// </summary>
        /// <returns></returns>
        string GenerateACK(string originalMessage)
        {
            // create a HL7Message object using the original message as the source to obtain details to reflect back in the ACK message
            HL7Message tmpMsg = new HL7Message(originalMessage);
            string trigger = tmpMsg.GetHL7ItemValue("MSH-9.2")[0];
            string originatingApp = tmpMsg.GetHL7ItemValue("MSH-3")[0];
            string originatingSite = tmpMsg.GetHL7ItemValue("MSH-4")[0];
            string messageID = tmpMsg.GetHL7ItemValue("MSH-10")[0];
            string processingID = tmpMsg.GetHL7ItemValue("MSH-11")[0];
            string hl7Version = tmpMsg.GetHL7ItemValue("MSH-12")[0];
            string ackTimestamp = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();

            StringBuilder ACKString = new StringBuilder();
            ACKString.Append((char)0x0B);
            ACKString.Append("MSH|^~\\&|HL7Listener|HL7Listener|" + originatingSite + "|" + originatingApp + "|" + ackTimestamp + "||ACK^" + trigger + "|" + messageID + "|" + processingID + "|" + hl7Version);
            ACKString.Append((char)0x0D);
            ACKString.Append("MSA|CA|" + messageID);
            ACKString.Append((char)0x1C);
            ACKString.Append((char)0x0D);
            return ACKString.ToString();
        }

        public bool IsRunning()
        {
            return this.runThread;
        }

        /// <summary>
        /// Set and get the values of the SendACK option. This can be used to overide sending of ACK messages. 
        /// </summary>
        public bool SendACK
        {
            get { return this.sendACK; }
            set { this.sendACK = value; }
        }


        /// <summary>
        /// The PassthruHost property identifies the host to pass the messages through to
        /// </summary>
        public string PassthruHost
        {
            set { this.passthruHost = value; }
            get { return this.passthruHost; }
        }


        /// <summary>
        /// The PassthruPort property identies the remote port to pass the messages thought to.
        /// </summary>
        public int PassthruPort
        {
            set { this.passthruPort = value; }
            get { return this.passthruPort; }
        }


        /// <summary>
        /// The FilePath property contains the path to archive the received messages to
        /// </summary>
        public string FilePath
        {
            set { this.archivePath = value; }
            get { return this.archivePath; }
        }

        /// <summary>
        /// Write debug events to the console.
        /// </summary>
        /// <param name="message"></param>
        private void LogDebug(string message)
        {
            debugQueue.Enqueue(message);
        }


        /// <summary>
        /// Write a warning events to the console
        /// </summary>
        /// <param name="message"></param>
        private void LogWarning(string message)
        {
            warningQueue.Enqueue(message);
        }
    }
}
