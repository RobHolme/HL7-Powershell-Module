/* Filename:    Common.cs
 * 
 * Author:      Rob Holme (rob@holme.com.au) 
 *              
 * Date:        29/08/2016
 * 
 * Notes:       Implements static functions common to more than one CmdLet class
 * 
 */

namespace HL7Tools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.PowerShell.Commands;

    public static class Common
    {
        /// <summary>
        /// Confirm that the HL7 item location string is in a valid format. It does not check to see if the item referenced exists or not.
        /// </summary>
        /// <param name="hl7ItemLocation"></param>
        /// <returns></returns>
        public static bool IsItemLocationValid(string hl7ItemLocation)
        {
            // make sure the location requested mactches the regex of a valid location string. This does not check to see if segment names exit, or items are present in the message
            if (System.Text.RegularExpressions.Regex.IsMatch(hl7ItemLocation, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)) // regex to confirm the HL7 element location string is valid
            {
                // make sure field, component and subcomponent values are not 0
                if (System.Text.RegularExpressions.Regex.IsMatch(hl7ItemLocation, "([.]0)|([-]0)", RegexOptions.IgnoreCase)) {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Make sure the filter string matches the expected pattern for a filter
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public static bool IsFilterValid(string filterString)
        {
            if (Regex.IsMatch(filterString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?=", RegexOptions.IgnoreCase)) {
                return true;
            }

            // the value provided after the -filter switch did not match the expected format of a message trigger.
            else {
                return false;
            }
        }

        /// <summary>
        /// return true if the string representing the HL7 location is valid. This does not confirm if the items exists, it only checks the formating of the string.
        /// </summary>
        /// <param name="HL7LocationString">The string identifying the location of the item within the message. eg PID-3.1</param>
        /// <returns></returns>
        public static bool IsHL7LocationStringValid(string HL7LocationString)
        {
            return (Regex.IsMatch(HL7LocationString, "^[A-Z]{2}([A-Z]|[0-9])([[]([1-9]|[1-9][0-9])[]])?(([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3}[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?[.][0-9]{1,3})|([-][0-9]{1,3}([[]([1-9]|[1-9][0-9])[]])?))?$", RegexOptions.IgnoreCase)); // segment([repeat])? or segment([repeat)?-field([repeat])? or segment([repeat)?-field([repeat])?.component or segment([repeat)?-field([repeat])?.component.subcomponent 
        }

        /// <summary>
        /// Check that this provider is the filesystem
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileSystemPath(ProviderInfo provider, string path)
        {
            bool isFileSystem = true;
            if (provider.ImplementingType != typeof(FileSystemProvider)) {
                // tell the caller that the item was not on the filesystem
                isFileSystem = false;
            }
            return isFileSystem;
        }

        /// <summary>
        /// return the portion of the filter string that identifies the HL7 Item to filter on
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public static string GetFilterItem(string filterString)
        {
            if (IsFilterValid(filterString)) {
                string[] tempString = (filterString).Split('=');
                return tempString[0];
            }
            else {
                return null;
            }
        }

        /// <summary>
        /// return the portion of the filter string that identifies the value to filter on
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        public static string GetFilterValue(string filterString)
        {
            if (IsFilterValid(filterString)) {
                string[] tempString = (filterString.Split('='));
                if (tempString.Length > 1) {
                    return tempString[1];
                }
                else {
                    return null;
                }
            }
            else {
                return null;
            }

        }
    }
}
