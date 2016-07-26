/* Filename:    RemoveHL7Identifiers.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 * 
 * Date:        07/03/2016 - Initial Version
 *              23/07/2016 - Added GetMLLPFramedMessage()
 * 
 * Notes:       Implements a class to store and manipulate a HL7 v2 message.
 * 
 */

// TO DO: Add range checking to GetHL7Item

namespace HL7Tools
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    
    /// <summary>
    /// Class representing a HL7 sub component
    /// </summary>
    public class SubComponent
    {
        private string subComponentValue;

        /// <summary>
        /// Initialises a new instance of the <see cref="SubComponet"/> class.
        /// </summary>
        /// <param name="SubComponentValue">The value to asign to the sub componet object</param>
        public SubComponent(string SubComponentValue)
        {
            this.subComponentValue = SubComponentValue;
        }

        /// <summary>
        /// Gets and sets the value of the sub component
        /// </summary>
        public string Value
        {
            get { return subComponentValue; }
            set { this.subComponentValue = Value; }
        }

        /// <summary>
        /// convert value to string
        /// </summary>
        /// <returns>Returns the SubComponent value as a string</returns>
        public override string ToString()
        {
            return this.subComponentValue;
        }

        /// <summary>
        /// Mask out the text with a mask character
        /// </summary>
        /// <param name="maskCharacter">The character to use as the mask</param>
        public void Mask(char maskCharacter = '*')
        {
            if (this.subComponentValue != "\"\"") // ignore null fields (deletes) - ""
            {
                string maskString = new string(maskCharacter, this.subComponentValue.Length);
                this.subComponentValue = maskString;
            }
        }
    }

    /// <summary>
    /// Class representing a HL7 component
    /// </summary>
    public class Component
    {
        private List<SubComponent> subComponents = new List<SubComponent>();
        private char subComponentDelimter;
        
        /// <summary>
        /// Initialises a new instance of the <see cref="Componet"/> class.
        /// </summary>
        /// <param name="ComponentValue"></param>
        /// <param name="SubComponentDelimter">The charcter used as the delimiter for sub components. Defaults to '&'</param>
        public Component(string ComponentValue, char SubComponentDelimter = '&')
        {
            this.subComponentDelimter = SubComponentDelimter; // the character used to delimit sub components in the HL7 message

            // split the string into sub components, then save
            string[] splitSubCompoents = ComponentValue.Split(subComponentDelimter);
            foreach (string subComponent in splitSubCompoents)
            {
                subComponents.Add(new SubComponent(subComponent));
            }
        }

        /// <summary>
        /// Gets the list of SubComponents 
        /// </summary>
        public List<SubComponent> SubComponents
        {
            get { return this.subComponents; }
        }

        /// <summary>
        /// Gets and sets the list of SubComponents that make up a Component object.
        /// </summary>
        public Component Value
        {
            get { return this; }
            set { this.subComponents = Value.SubComponents; }
        }

        /// <summary>
        /// Return the sub component if in range, else return null. Index starts at 1
        /// </summary>
        /// <param name="ID">ID repesents the possition of a single SubComponet in the list of SubComponets (starts at 1)</param>
        /// <returns>Returns a SubComponent object</returns>
        public SubComponent GetSubComponent(int ID)
        {
            if ((ID > 0) && (ID <= subComponents.Count))
            {
                return this.subComponents[ID - 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Mask out the component value
        /// </summary>
        /// <param name="MaskCharacter">The character to use to as the mask</param>
        public void Mask(char MaskCharacter = '*')
        {
            foreach (SubComponent item in this.subComponents)
            {
                item.Mask(MaskCharacter);
            }
        }

        /// <summary>
        /// Set the SubComponent value at a spcecific index
        /// </summary>
        /// <param name="ID">The index repesenting the item in the lis of SubComponents that is set</param>
        /// <param name="SubComponentValue">The value to assign to the SubComponet</param>
        public void SetSubComponent(int ID, SubComponent SubComponentValue)
        {
            if ((ID > 0) && (ID <= subComponents.Count))
            {
                this.subComponents[ID - 1] = SubComponentValue;
            }
            
            // if the sub component is out of the range of current subcomponents, add in empty sub components until the range is large enough
            if (ID > subComponents.Count)
            {
                while (ID > subComponents.Count + 1)
                {
                    this.subComponents.Add(new SubComponent(""));
                }
                this.subComponents.Add(SubComponentValue);
            }
        }

        /// <summary>
        /// set the sub component at a specific index using a string value
        /// </summary>
        /// <param name="ID">The index (position) of the item to set the value of (index starts at 1)</param>
        /// <param name="SubComponentStringValue">The value to assign the SubComponent</param>
        public void SetSubComponent(int ID, string SubComponentStringValue)
        {
            if ((ID > 0) && (ID <= this.subComponents.Count))
            {
                SubComponent tempSubComponent = new SubComponent(SubComponentStringValue);
                this.subComponents[ID - 1] = tempSubComponent;
            }

            // if the sub component is out of the range of current subcomponents, add in empty sub components until the range is large enough
            if (ID > this.subComponents.Count)
            {
                while (ID > subComponents.Count + 1) 
                {
                    this.subComponents.Add(new SubComponent(""));
                }
                this.subComponents.Add(new SubComponent(SubComponentStringValue));
            }
        }

        /// <summary>
        /// Convert the component value to a string
        /// </summary>
        /// <returns>Returns a string containing the vale of the component</returns>
        public override string ToString()
        {
            if (this.subComponents.Count == 0)
            {
                return "";
            }
            else if (this.subComponents.Count == 1)
            {
                return this.subComponents[0].ToString();
            }
            else
            {
                string returnString = this.subComponents[0].ToString();
                for (int i = 1; i < this.subComponents.Count; i++)
                {
                    returnString += subComponentDelimter + this.subComponents[i].ToString();
                }
                return returnString;
            }
        }
    }

    /// <summary>
    /// private class representing a single HL7 field item
    /// </summary>
    public class FieldItem
    {
        private List<Component> components = new List<Component>();
        private char componentDelimeter;
        private char subCompenentDelimeter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldItem"/> class.
        /// </summary>
        /// <param name="FieldValue">The value to assign the FieldItem</param>
        /// <param name="ComponentDelimeter">The character to use as the component delimiter. Defaults to '^'</param>
        /// <param name="SubComponentDelimeter">The character to use as the sub component delimiter. Defaults to '&'</param>
        public FieldItem(string FieldValue, char ComponentDelimeter = '^', char SubComponentDelimeter = '&')
        {
            // set the HL7 delimeter characters
            this.componentDelimeter = ComponentDelimeter;
            this.subCompenentDelimeter = SubComponentDelimeter;

            // split the string into components, then add to component list
            string[] splitSubCompoents = FieldValue.Split(componentDelimeter);
            foreach (string field in splitSubCompoents)
            {
                components.Add(new Component(field, subCompenentDelimeter));
            }
        }

        /// <summary>
        /// Gets the list of components that make up a field item
        /// </summary>
        public List<Component> Components
        {
            get { return this.components; }
        }

        /// <summary>
        /// Gets and set the FieldItem value
        /// </summary>
        public FieldItem Value
        {
            get { return this; }
            set { this.components = Value.Components; }
        }

        /// <summary>
        /// Convert the field value to a string
        /// </summary>
        /// <returns>Returns a string of the field value</returns>
        public override string ToString()
        {
            if (this.components.Count == 0)
            {
                return "";
            }
            else if (this.components.Count == 1)
            {
                return this.components[0].ToString();
            }
            else
            {
                string returnString = this.components[0].ToString();
                for (int i = 1; i < this.components.Count; i++)
                {
                    returnString += componentDelimeter + this.components[i].ToString();
                }
                return returnString;
            }
        }

        /// <summary>
        /// Mask out the field value
        /// </summary>
        /// <param name="MaskCharacter">This defines the character to use as a mask. Defaults to '*'</param>
        public void Mask(char MaskCharacter = '*')
        {
            foreach (Component item in this.Components)
            {
                item.Mask(MaskCharacter);
            }
        }
    }

    /// <summary>
    /// Class representing a HL7 field item, or list of field items
    /// </summary>
    public class Field
    {

        private List<FieldItem> fieldItems = new List<FieldItem>();
        private char fieldRepeatDelimeter;
        private char compoenentDelimeter;
        private char subComponentDelimiter;

        /// <summary>
        /// Initialises a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="FieldValue"></param>
        /// <param name="FieldRepeatDelimeter"></param>
        public Field(string FieldValue, char FieldRepeatDelimeter = '~', char ComponentDelimeter = '^', char SubComponentDelimter = '&')
        {
            // set the HL7 delimeter characters
            this.fieldRepeatDelimeter = FieldRepeatDelimeter;
            this.compoenentDelimeter = ComponentDelimeter;
            this.subComponentDelimiter = SubComponentDelimter;
            // split repeating field values, add each value to the list of SinglField objects
            string[] splitFields = FieldValue.Split(fieldRepeatDelimeter);
            foreach (string fieldItem in splitFields)
            {
                this.fieldItems.Add(new FieldItem(fieldItem, this.compoenentDelimeter, this.subComponentDelimiter));
            }

        }

        /// <summary>
        /// convert the field values to a string
        /// </summary>
        /// <returns>Returns a the field value as a string</returns>
        public override string ToString()
        {
            if (this.fieldItems.Count == 0)
            {
                return "";
            }
            else if (this.fieldItems.Count == 1)
            {
                return this.fieldItems[0].ToString();
            }
            else
            {
                string returnString = this.fieldItems[0].ToString();
                for (int i = 1; i < this.fieldItems.Count; i++)
                {
                    returnString += this.fieldRepeatDelimeter + this.fieldItems[i].ToString();
                }
                return returnString;
            }
        }

        /// <summary>
        /// Gets or sets the value of the Field object
        /// </summary>
        public Field Value
        {
            get { return this; }
            set { this.fieldItems = Value.FieldItems; }
        }

        /// <summary>
        /// Gets the list of FieldItems that make up a Field object
        /// </summary>
        public List<FieldItem> FieldItems
        {
            get { return this.fieldItems; }
            //           set { this.field = Value; }
        }

        /// <summary>
        /// Mask out the field value
        /// </summary>
        /// <param name="MaskCharacter">The character to use as the mask, defaults to '*'</param>
        public void Mask(char MaskCharacter = '*')
        {
            foreach (FieldItem item in this.FieldItems)
            {
                item.Mask(MaskCharacter);
            }
        }
    }


    /// <summary>
    /// Class representing a HL7 message segment
    /// </summary>
    class Segment
    {
        private List<Field> fields = new List<Field>();
        private char fieldDelimeter;
        private char fieldRepeatDelimter;
        private char compoenentDelimeter;
        private char subComponentDelimiter;
        private string segmentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Segment"/> class. Create a list of fields representing the segment based on a string containing the segment
        /// </summary>
        /// <param name="SegmentValue">A string representing the value of the segment</param>
        /// <param name="FieldDelimeter">The character used as the field delimiter. Defaults to '|'</param>
        /// <param name="FieldRepeatDelimeter">The character used as the field repeat delimiter. Defaults to '~'</param>
        /// <param name="ComponentDelimeter">The character used as the component delimiter. Defaults to '^'</param>
        /// <param name="SubComponentDelimter">The character used as the sub component delimiter. Defaults to '&'</param>
        public Segment(string SegmentValue, char FieldDelimeter = '|', char FieldRepeatDelimeter = '~', char ComponentDelimeter = '^', char SubComponentDelimter = '&')
        {
            this.fieldDelimeter = FieldDelimeter;
            this.fieldRepeatDelimter = FieldRepeatDelimeter;
            this.compoenentDelimeter = ComponentDelimeter;
            this.subComponentDelimiter = SubComponentDelimter;

            // split repeating field values, add each value to the list of SinglField objects
            string[] splitSegment = SegmentValue.Split(FieldDelimeter);
            this.segmentName = splitSegment[0];
            // special case for message header, the field delimeter is MSH-1
            if (this.segmentName == "MSH")
            {
                this.fields.Add(new Field(this.fieldDelimeter.ToString(), this.fieldRepeatDelimter, this.compoenentDelimeter, this.subComponentDelimiter));

            }
            // now add the remaining fields for all segment types
            for (int i = 1; i < splitSegment.Length; i++)
            {
                this.fields.Add(new Field(splitSegment[i], this.fieldRepeatDelimter, this.compoenentDelimeter, this.subComponentDelimiter));
            }
        }

        /// <summary>
        /// Gets the value of the Segment
        /// </summary>
        public Segment Value
        {
            get { return this; }
        }

        /// <summary>
        /// Gets or sets a list of Fields that the segment comprises of
        /// </summary>
        public List<Field> Fields
        {
            get { return this.fields; }
            set { this.fields = Fields; }
        }

        /// <summary>
        /// Gets the name of the segment
        /// </summary>
        public string Name
        {
            get { return this.segmentName; }
        }

        /// <summary>
        /// Converts the value of a Segment object to a sting
        /// </summary>
        /// <returns>Returns a copy of the segment value as a string</returns>
        public override string ToString()
        {
            if (this.fields.Count == 0)
            {
                return "";
            }

            string returnString = "";
            returnString = this.segmentName;
            // the MSH segment is different because MSH-1 is the field delimeter
            if (this.segmentName == "MSH")
            {
                for (int i = 1; i < this.fields.Count; i++)
                {
                    returnString += fieldDelimeter + this.fields[i].ToString();
                }
            }
            else
            {
                for (int i = 0; i < this.fields.Count; i++)
                {
                    returnString += fieldDelimeter + this.fields[i].ToString();
                }
            }
            return returnString;
        }
    }

    /// <summary>
    /// Class representing a HL7 message. Items are treated as text only, no strict HL7 message schema
    /// </summary>
    class HL7Message
    {
        private List<Segment> segments = new List<Segment>();
        private char fieldDelimeter;
        private char fieldRepeatDelimeter;
        private char componentDelimeter;
        private char subComponentDelimeter;

       /// <summary>
       /// Initializes a new instance of the <see cref="HL7Message"/> class.
       /// </summary>
       /// <param name="Message">A string containing the full message text used to create the HL7Message object</param>
        public HL7Message(string Message)
        {
            // If the file has been edited in a text editor, <CR><LF> may have been inserted at the end of each line. HL7 segments should only be delimited by <CR> characters, so replace <CR><LF> with <CR>
            string crlf = ((char)0x0D).ToString() + ((char)0x0A).ToString();
            string cr = ((char)0x0D).ToString();
            Message = Message.Replace(crlf, cr);
            string[] segmentStrings = Message.Split((char)0x0D);
            // set the field, component, sub component and repeat delimters
            int startPos = Message.IndexOf("MSH");
            if (startPos >= 0)
            {
                startPos = startPos + 2;
                this.fieldDelimeter = Message[startPos + 1];
                this.componentDelimeter = Message[startPos + 2];
                this.fieldRepeatDelimeter = Message[startPos + 3];
                this.subComponentDelimeter = Message[startPos + 5];
            }
            // throw an exception if a MSH segment is not included in the message. 
            else
            {
                throw new ArgumentException("MSH segment not present.");
            }
            // add each segment
            foreach (string segmentItem in segmentStrings)
            {
                this.segments.Add(new Segment(segmentItem, this.fieldDelimeter, this.fieldRepeatDelimeter, this.componentDelimeter, this.subComponentDelimeter));
            }

        }


        /// <summary>
        /// Gets a list of all Segments from the message
        /// </summary>
        public List<Segment> Segments
        {
            get { return this.segments; }
        }

        /// <summary>
        /// Gets or sets the segments contained in the message
        /// </summary>
        public HL7Message Value
        {
            get { return this; }
            set { this.segments = Value.Segments; }
        }


        /// <summary>
        /// Return the HL7 message contents as a string.
        /// </summary>
        /// <returns>returns a copy of the HL7Message as a string</returns>
        public override string ToString()
        {
            if (this.segments.Count == 0)
            {
                return "";
            }
            else
            {
                string returnString = "";
                foreach (Segment item in this.segments)
                {
                    returnString += item.ToString() + (char)0x0D;
                }
                return returnString;
            }
        }

        /// <summary>
        /// Return a list of segments in the message matching a segment name (eg PID).
        /// </summary>
        /// <param name="segmentName">The 3 letter name of the Segment to return. eg 'MSH', 'PID', 'PV1'</param>
        /// <returns>returns a list of one or more segments matching the segmentName</returns>
        public List<Segment> GetSegment(string segmentName)
        {
            List<Segment> segmentList = new List<Segment>();
            foreach (Segment item in this.segments)
            {
                if (item.Name.ToUpper() == segmentName.ToUpper())
                {
                    segmentList.Add(item);
                }
            }
            return segmentList;
        }

        /// <summary>
        /// De Identify message fields.
        /// All PID fields except PID-1, PID-2, PID-3
        /// All NK1 fields except NK1-1, NK1-3
        /// All IN1 fields 
        /// All IN2 fields 
        /// </summary>
        /// <param name="MaskCharacter">The character used to mask the values, defaults to '*'</param>
        public void DeIdentify(char MaskCharacter = '*')
        {
            List<Segment> PIDSegments = this.GetSegment("PID");
            List<Segment> NK1Segments = this.GetSegment("NK1");
            List<Segment> IN1Segments = this.GetSegment("IN1");
            List<Segment> IN2Segments = this.GetSegment("IN2");

            // mask PID segments
            foreach (Segment segmentItem in PIDSegments)
            {
                // ignore PID-1, PID-2, PID-3
                for (int i = 3; i < segmentItem.Fields.Count; i++) 
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

            // mask NK1 segments
            foreach (Segment segmentItem in NK1Segments)
            {
                // ignore NK1-1
                for (int i = 1; i < segmentItem.Fields.Count; i++) 
                {
                    // ignore NK-3
                    if (i != 2) 
                    {
                        segmentItem.Fields[i].Mask(MaskCharacter);
                    }
                }
            }

            // mask IN1 segments
            foreach (Segment segmentItem in IN1Segments)
            {
                // mask all fields
                for (int i = 0; i < segmentItem.Fields.Count; i++) 
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

            // mask IN1 segments
            foreach (Segment segmentItem in IN2Segments)
            {
                // mask all fields
                for (int i = 0; i < segmentItem.Fields.Count; i++) 
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

        }

        /// <summary>
        /// Return the value for the corresponding HL7 item. HL7LocationString is formatted as Segment-Field.Componet.SubComponent eg PID-3 or PID-5.1.1
        /// </summary>
        /// <param name="HL7LocationString">A string representing the location on the item within the message. e.g. PID-3.1, MSH-4, PID-13[1].1</param>
        /// <returns>Returns a copy of the nominated HL7 item as a string</returns>
        public string[] GetHL7Item(string HL7LocationString)
        {
            string segmentName = "";
            int fieldNumber = 0;
            int componentNumber = 0;
            int subcomponentNumber = 0;
            int segmentRepeatNumber = 0;
            int fieldRepeatNumber = 0;

            //  use regular expressions to determine if the item location string is valid.
            if (Regex.IsMatch(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)) // segment([repeat])? or segment([repeat)?-field([repeat])? or segment([repeat)?-field([repeat])?.component or segment([repeat)?-field([repeat])?.component.subcomponent 
            {
                
                // Obtain the segment repeat number if specified
                Match checkRepeatingSegmentNumber = System.Text.RegularExpressions.Regex.Match(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])[[][1-9]{1,3}[]]", RegexOptions.IgnoreCase);
                if (checkRepeatingSegmentNumber.Success == true)
                {
                    string tmpStr = checkRepeatingSegmentNumber.Value.Split('[')[1];
                    segmentRepeatNumber = Int32.Parse(tmpStr.Split(']')[0]);

                }

                // Obtain the field repeat number if specified
                Match checkRepeatingFieldNumber = System.Text.RegularExpressions.Regex.Match(HL7LocationString, "[-][0-9]{1,3}[[]([1-9]|[1-9][0-9])[]]", RegexOptions.IgnoreCase);
                if (checkRepeatingFieldNumber.Success == true)
                {
                    string tmpStr = checkRepeatingFieldNumber.Value.Split('[')[1];
                    fieldRepeatNumber = Int32.Parse(tmpStr.Split(']')[0]);
                }
                
                // retrieve the field, component and sub componnent values. If they don't exist, set to 0
                string[] tempString = HL7LocationString.Split('-');
                segmentName = tempString[0].Substring(0, 3); // the segment name
                if (tempString.Count() > 1) // confirm values other than the segment were provided.
                {
                    string[] tempString2 = tempString[1].Split('.');
                    if (tempString2.Count() >= 1) // field exists, possibly more. Set the field value.
                    {
                        fieldNumber = Int32.Parse(tempString2[0].Split('[')[0]); // if the field contains a repeat number, ignore the repeat value and braces
                    }
                    if (tempString2.Count() >= 2) // field and component, possibly more. Set the component value
                    {
                        componentNumber = Int32.Parse(tempString2[1]);
                    }
                    if (tempString2.Count() == 3) // field, compoment and sub component exist. Set the value of thesub component.
                    {
                        subcomponentNumber = Int32.Parse(tempString2[2]);
                    }
                }
            }

            List<Segment> segments = this.GetSegment(segmentName);
            List<string> returnString = new List<string>();

            // Subcomponent value requested
            if (subcomponentNumber != 0)
            {
                // a specific repeat of a segment was requested
                if (segmentRepeatNumber != 0)
                {
                    // a specific field repeat was requested
                    if (fieldRepeatNumber != 0) 
                    {
                        // check the element requested us within range (ie present in the message)
                        if (segmentRepeatNumber < segments.Count())
                        {
                            if (fieldNumber < segments[segmentRepeatNumber - 1].Fields.Count())
                            {
                                if (fieldRepeatNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count())
                                {
                                    if(componentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components.Count())
                                    {
                                        if (subcomponentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents.Count())
                                        {
                                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                                        }
                                    }
                                }
                            }
                        } 
                    }
                    
                    // all field repeats requested
                    else
                    {
                        // check the element requested us within range (ie present in the message)
                        if (segmentRepeatNumber < segments.Count())
                        {
                            if (fieldNumber < segments[segmentRepeatNumber - 1].Fields.Count())
                            {
                                // loop through all FieldItems since the request is not for a specific repeat
                                for (int fieldItemCount = 0; fieldItemCount < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                                {
                                    if (componentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components.Count())
                                    {
                                        if (subcomponentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents.Count())
                                        {
                                            // add the element to the list of elements returned
                                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // no specific segment specified, return all
                else
                {
                    foreach (Segment segmentItem in segments)
                    {
                        // a specific field repeat was requested. Check the range to ensure the item exists in the message first
                        if (fieldRepeatNumber != 0)
                        {
                            if (fieldNumber < segmentItem.Fields.Count())
                            {
                                if (fieldRepeatNumber < segmentItem.Fields[fieldNumber - 1].FieldItems.Count())
                                {
                                    if (componentNumber < segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components.Count())
                                    {
                                        if (subcomponentNumber < segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents.Count())
                                        {
                                            returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                                        }
                                    }
                                }
                            }
                        }

                        // no specific field repeat specified, return all field repeats
                        else
                        {
                            // check  that the item is in range (ie present in the message)
                            if (fieldNumber < segmentItem.Fields.Count())
                            {
                                // for  all FieldItems
                                for (int fieldItemCount = 0; fieldItemCount < segmentItem.Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                                {
                                    if (componentNumber < segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components.Count())
                                    {
                                        if (subcomponentNumber < segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents.Count())
                                        {
                                            returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Component value requested
            else if (componentNumber != 0)
            {
                // specific segment repeat requested
                if (segmentRepeatNumber != 0)
                {
                    // a specific field repeat was requested.  Confirm the item is  with in range (ie contained in the message)
                    if (fieldRepeatNumber != 0)
                    {
                        if (segmentRepeatNumber < segments.Count())
                        {
                            if (fieldNumber < segments[segmentRepeatNumber - 1].Fields.Count())
                            {
                                if (fieldRepeatNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count())
                                {
                                    if (componentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components.Count())
                                    {
                                        returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].ToString());
                                    }
                                }
                            }
                        }

                        // no repeating field item specified, return all fields
                        else
                        {
                            // do range checking (make sure the item exists  in the mesage)
                            if (segmentRepeatNumber < segments.Count())
                            {
                                if (fieldNumber < segments[segmentRepeatNumber - 1].Fields.Count())
                                {
                                    for (int fieldItemCount = 0; fieldItemCount < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                                    {
                                        if (componentNumber < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components.Count())
                                        {
                                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // no segment repeat requested, return all
                else
                {
                    foreach (Segment segmentItem in segments)
                    {
                        ////////////            // TO DO: add in range checking
                        // a specific field repeat was requested                
                        if (fieldRepeatNumber != 0)
                        {
                            returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].ToString());
                        }

                        // no specific field repeat specified, return all field repeats
                        else
                        {
                            for (int fieldItemCount = 0; fieldItemCount < segmentItem.Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                            {
                                returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].ToString());
                            }
                        }
                    }
                }
                }

                // Field value requested
                else if (fieldNumber != 0)
                {
                    // segment repeat identified, only return the single segment
                    if (segmentRepeatNumber != 0)
                    {
                        // a specific field repeat was requested
                        // TO DO: add in range checking
                        if (fieldRepeatNumber != 0)
                        {
                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].ToString()); // TO DO: add in range checking, return null if out of range
                        }

                        // no repeating field item specified, return all fields
                        else
                        {
                            for (int fieldItemCount = 0; fieldItemCount < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                            {
                                returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].ToString()); // TO DO: add in range checking, return null if out of range
                            }
                        }
                    }

                    // Return all segments
                    else
                    {
                        foreach (Segment segmentItem in segments)
                        {
                            // make sure the field number requested is in range (ie present in the message segment)
                            if (fieldNumber < segmentItem.Fields.Count())
                            {
                                // a specific field repeat was requested                
                                if (fieldRepeatNumber != 0)
                                {
                                    // make sure the field repeat requested is in range (ie present in the message segment
                                    if (fieldRepeatNumber < segmentItem.Fields[fieldNumber - 1].FieldItems.Count())
                                    {
                                        returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].ToString());
                                    }
                                }

                                // no specific field repeat specified, return all field repeats
                                else
                                {
                                    for (int fieldItemCount = 0; fieldItemCount < segmentItem.Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                                    {
                                        returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].ToString());
                                    }
                                }
                            }
                        }
                    }
                }

                // Segment value requested
                else if (segmentName != null)
                {
                    // segment repeat identified, only return the single segment
                    if (segmentRepeatNumber != 0)
                    {
                        returnString.Add(segments[segmentRepeatNumber - 1].ToString());  // TO DO: add in range checking, return null if out of range
                    }

                    // no segment repeat identified, so return all matching segments.
                    else
                    {
                        foreach (Segment segmentItem in segments)
                        {
                            returnString.Add(segmentItem.ToString());
                        }
                    }
                }
            return returnString.ToArray();
        }


        /// <summary>
        /// Return the HL7 message with MLLP framing added
        /// </summary>
        /// <returns>Returns the message contents as a string framed in the MLLP control characters</returns>
        public string GetMLLPFramedMessage()
        {
            StringBuilder mllpMessage = new StringBuilder();
            mllpMessage.Append((char)0x0B);
            mllpMessage.Append(this.ToString());
            mllpMessage.Append((char)0x1C);
            mllpMessage.Append((char)0x0D);
            return mllpMessage.ToString();
        }
    }
}
