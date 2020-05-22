using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Framework
{
    public class Email
    {
        /// <summary>
        /// Regular expression pattern for valid email
        /// addresses, allows for the following domains:
        /// com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
        /// </summary>
        public const string EmailCheckRegexPattern = @"^.*@.*\.([a-zA-Z][a-zA-Z]*)$";//fr|eu|mobi|name|ws|com|info|tv|biz|be|cc|uk|es|re|pt|edu|gov|int|mil|net|org|name|museum|coop|aero|pro|tv|
        /// <summary>
        /// 
        /// </summary>
        private static Regex _EmailRegex = null;

        /// <summary>
        /// method for determining is the user provided a valid email address
        /// We use regular expressions in this check, as it is a more thorough
        /// way of checking the address provided
        /// </summary>
        /// <param name="email">email address to validate</param>
        /// <returns>true is valid, false if not valid</returns>
        public static bool IsValidEmail_T(string email)
        {
            // Regular expression object
            if (_EmailRegex == null)
            {
                _EmailRegex = new Regex(EmailCheckRegexPattern, RegexOptions.IgnorePatternWhitespace);
            }
            // Make sure an email address was provided
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            // Use IsMatch to validate the address
            return _EmailRegex.IsMatch(email);
        }
    }
}
