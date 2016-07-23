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

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HL7Tools
{
    /// <summary>
    /// Class respesenting a HL7 sub component
    /// </summary>
    public class SubComponent
    {
        private string subComponentValue;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="SubComponentValue"></param>
        public SubComponent(string SubComponentValue)
        {
            this.subComponentValue = SubComponentValue;
        }

        /// <summary>
        /// get and set the value of the sub component
        /// </summary>
        public string Value
        {
            get { return subComponentValue; }
            set { subComponentValue = Value; }
        }

        /// <summary>
        /// convert value to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return subComponentValue;
        }

        /// <summary>
        /// Mask out the text with a mask charcter
        /// </summary>
        /// <param name="maskCharacter"></param>
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
    /// Class representing a HL7 componet
    /// </summary>
    public class Component
    {
        private List<SubComponent> subComponents = new List<SubComponent>();
        private char subComponentDelimter;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="ComponentValue"></param>
        public Component(string ComponentValue, char SubComponentDelimter = '&')
        {
            subComponentDelimter = SubComponentDelimter; // the character used to delimit sub components in the HL7 message

            // split the string into sub components, then save
            string[] splitSubCompoents = ComponentValue.Split(subComponentDelimter);
            foreach (string subComponent in splitSubCompoents)
            {
                subComponents.Add(new SubComponent(subComponent));
            }

        }

        /// <summary>
        /// get the sub components 
        /// </summary>
        public List<SubComponent> SubComponents
        {
            get { return subComponents; }
        }

        /// <summary>
        /// get and 
        /// </summary>
        public Component Value
        {
            get { return this; }
            set { this.subComponents = Value.SubComponents; }
        }

        /// <summary>
        /// return the sub component if in range, else return null. Index starts at 1
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public SubComponent GetSubComponent(int ID)
        {
            if ((ID > 0) && (ID <= subComponents.Count))
            {
                return subComponents[ID - 1];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// mask out the compoent value
        /// </summary>
        /// <param name="MaskCharacter"></param>
        public void Mask(char MaskCharacter = '*')
        {
            foreach (SubComponent item in this.subComponents)
            {
                item.Mask(MaskCharacter);
            }
        }

        // IS THIS NEXT FUNCTION NEEDED, JUST USE component.SubComponent[2].Value = "blah" instead?
        // HOWEVER I NEED AN AddSubComponent(int Index, string value) function. 
        /// <summary>
        /// set the sub component at a spcecific index
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="SubComponentValue"></param>
        public void SetSubComponent(int ID, SubComponent SubComponentValue)
        {
            if ((ID > 0) && (ID <= subComponents.Count))
            {
                subComponents[ID - 1] = SubComponentValue;
            }
            // if the sub component is out of the range of current subcomponents, add in empty sub components until the range is large enough
            if (ID > subComponents.Count)
            {
                while (ID > subComponents.Count + 1)
                {
                    subComponents.Add(new SubComponent(""));
                }
                this.subComponents.Add(SubComponentValue);
            }
        }

        /// <summary>
        /// set the sub component at a spcecific index using a string value
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="SubComponentStringValue"></param>
        public void SetSubComponent(int ID, string SubComponentStringValue)
        {
            if ((ID > 0) && (ID <= subComponents.Count))
            {
                SubComponent tempSubComponent = new SubComponent(SubComponentStringValue);
                subComponents[ID - 1] = tempSubComponent;
            }
            // if the sub component is out of the range of current subcomponents, add in empty sub components until the range is large enough
            if (ID > subComponents.Count)
            {
                while (ID > subComponents.Count + 1)
                {
                    subComponents.Add(new SubComponent(""));
                }
                this.subComponents.Add(new SubComponent(SubComponentStringValue));
            }
        }

        /// <summary>
        /// convert the component value to a string
        /// </summary>
        /// <returns></returns>
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
        /// constructor
        /// </summary>
        /// <param name="FieldValue"></param>
        /// <param name="ComponentDelimeter"></param>
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
        /// get and set the compent values of the field item
        /// </summary>
        public List<Component> Components
        {
            get { return components; }
            //            set { components = Components; }
        }

        /// <summary>
        /// get and set the FieldItem value
        /// </summary>
        public FieldItem Value
        {
            get { return this; }
            set { this.components = Value.Components; }
        }

        /// <summary>
        /// convert the field value to a string
        /// </summary>
        /// <returns></returns>
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
        /// mask out the field value
        /// </summary>
        /// <param name="MaskCharacter"></param>
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
        /// constructor
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
                fieldItems.Add(new FieldItem(fieldItem, this.compoenentDelimeter, this.subComponentDelimiter));
            }

        }

        /// <summary>
        /// convert the field values to a string
        /// </summary>
        /// <returns></returns>
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
                    returnString += fieldRepeatDelimeter + this.fieldItems[i].ToString();
                }
                return returnString;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Field Value
        {
            get { return this; }
            set { this.fieldItems = Value.FieldItems; }
        }

        /// <summary>
        /// get the list of FieldItems
        /// </summary>
        public List<FieldItem> FieldItems
        {
            get { return this.fieldItems; }
            //           set { this.field = Value; }
        }

        /// <summary>
        /// mask out the field value
        /// </summary>
        /// <param name="MaskCharacter"></param>
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
        /// Constructor. Create a list of fields representing the segment based on a string containing the segment
        /// </summary>
        /// <param name="SegmentValue"></param>
        /// <param name="FieldDelimeter"></param>
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
        /// 
        /// </summary>
        public Segment Value
        {
            get { return this; }
        }

        /// <summary>
        /// return a list of Fields that the segment comprises of
        /// </summary>
        public List<Field> Fields
        {
            get { return this.fields; }
            set { this.fields = Fields; }
        }

        /// <summary>
        /// return the name of the segment
        /// </summary>
        public string Name
        {
            get { return this.segmentName; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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

        // constructor
        public HL7Message(string Message)
        {
            // If the file has been edited in a text editor, <CR><LF> may have been inserted at the end of each line. HL7 segments should only be delimited by <CR> characters, so replace <CR><LF> with <CR>
            string crlf = ((char)0x0D).ToString() + ((char)0x0A).ToString();
            string cr = ((char)0x0D).ToString();
            Message = Message.Replace(crlf, cr);
            //TO DO: throw an exception if the message does not contain a MSH segment
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
        /// 
        /// </summary>
        public List<Segment> Segments
        {
            get { return this.segments; }
        }

        /// <summary>
        /// 
        /// </summary>
        public HL7Message Value
        {
            get { return this; }
            set { this.segments = Value.Segments; }
        }


        /// <summary>
        /// return the HL7 message contents as a string.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="segmentName"></param>
        /// <returns></returns>
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
        public void DeIdentify(char MaskCharacter = '*')
        {
            List<Segment> PIDSegments = this.GetSegment("PID");
            List<Segment> NK1Segments = this.GetSegment("NK1");
            List<Segment> IN1Segments = this.GetSegment("IN1");
            List<Segment> IN2Segments = this.GetSegment("IN2");

            // mask PID segments
            foreach (Segment segmentItem in PIDSegments)
            {
                for (int i = 3; i < segmentItem.Fields.Count; i++) // ignore PID-1, PID-2, PID-3
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

            // mask NK1 segments
            foreach (Segment segmentItem in NK1Segments)
            {
                for (int i = 1; i < segmentItem.Fields.Count; i++) // ignore NK1-1
                {
                    if (i != 2) // ignore NK-3
                    {
                        segmentItem.Fields[i].Mask(MaskCharacter);
                    }
                }
            }

            // mask IN1 segments
            foreach (Segment segmentItem in IN1Segments)
            {
                for (int i = 0; i < segmentItem.Fields.Count; i++) // mask all fields
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

            // mask IN1 segments
            foreach (Segment segmentItem in IN2Segments)
            {
                for (int i = 0; i < segmentItem.Fields.Count; i++) // mask all fields
                {
                    segmentItem.Fields[i].Mask(MaskCharacter);
                }
            }

        }

        /// <summary>
        /// return the value for the coresponding HL7 item. HL7LocationString is formatted as Segment-Field.Componet.SubComponent eg PID-3 or PID-5.1.1
        /// </summary>
        /// <param name="HL7LocationString"></param>
        /// <returns></returns>
        public string[] GetHL7Item(string HL7LocationString)
        {
            string segmentName = "";
            int fieldNumber = 0;
            int componentNumber = 0;
            int subcomponentNumber = 0;
            int segmentRepeatNumber = 0;
            int fieldRepeatNumber = 0;

            //  use regular expressions to determine what filter was provided
            if (Regex.IsMatch(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)) // segment([repeat])? or segment([repeat)?-field([repeat])? or segment([repeat)?-field([repeat])?.component or segment([repeat)?-field([repeat])?.component.subcomponent 
            {
                // check to see if a segment repeat number is specified
                Match checkRepeatingSegmentNumber = System.Text.RegularExpressions.Regex.Match(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])[[][1-9]{1,3}[]]", RegexOptions.IgnoreCase);
                if (checkRepeatingSegmentNumber.Success == true)
                {
                    string tmpStr = checkRepeatingSegmentNumber.Value.Split('[')[1];
                    segmentRepeatNumber = Int32.Parse(tmpStr.Split(']')[0]);

                }
                // check to see if a field repeat number is specified
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


            // TO DO: THIS SECTION IS WRONG. fieldRepeatNumber shoudl be on fielditems instead?

            // Subcomponent value requested
            if (subcomponentNumber != 0)
            {
                // a specific repeat of a segment was requested
                if (segmentRepeatNumber != 0)
                {
                    // TO DO: add in range checking, return null if out of range
                    // a specific field repeat was requested
                    if (fieldRepeatNumber != 0)
                    {
                        returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                    }
                    // all field repeats requested
                    else
                    {
                        for (int fieldItemCount = 0; fieldItemCount < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                        {
                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
                        }

                    }
                }
                // no specific segment specified, return all
                else
                {
                    foreach (Segment segmentItem in segments)
                    {
                        // a specific field repeat was requested
                        // TO DO: add in range checking
                        if (fieldRepeatNumber != 0)
                        {
                            returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());

                        }
                        // no specific field repeat specified, return all field repeats
                        else
                        {
                            for (int fieldItemCount = 0; fieldItemCount < segmentItem.Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                            {
                                returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].SubComponents[subcomponentNumber - 1].ToString());
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
                    // a specific field repeat was requested
                    // TO DO: add in range checking
                    if (fieldRepeatNumber != 0)
                    {
                        returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].Components[componentNumber - 1].ToString());
                    }
                    // no repeating field item specified, return all fields
                    else
                    {
                        for (int fieldItemCount = 0; fieldItemCount < segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems.Count(); fieldItemCount++)
                        {
                            returnString.Add(segments[segmentRepeatNumber - 1].Fields[fieldNumber - 1].FieldItems[fieldItemCount].Components[componentNumber - 1].ToString());
                        }
                    }

                }
                // no segment repeat requested, return all
                else
                {
                    foreach (Segment segmentItem in segments)
                    {
                        // TO DO: add in range checking
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
                // return all segments
                else
                {
                    foreach (Segment segmentItem in segments)
                    {
                        // TO DO: add in range checking
                        // a specific field repeat was requested                
                        if (fieldRepeatNumber != 0)
                        {
                            returnString.Add(segmentItem.Fields[fieldNumber - 1].FieldItems[fieldRepeatNumber - 1].ToString());
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
        /// <returns></returns>
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
