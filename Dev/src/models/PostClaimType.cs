using System;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Post claim type.
    /// </summary>
    public class PostClaimType : ClaimType
    {
        public const string Cover = "cover"; // Raw data or image url.
        //public const string Price = "price";
        public const string Registration = "registration";
        public const string RegistrationWebConfirmation = "registrationwebconfirmation";
        public const string RegistrationEmailConfirmation = "registrationemailconfirmation";
        public const string RegistrationField = "registrationfield";
        //public const string Author = "author";
        //public const string Editor = "editor";
        //public const string NbPages = "nbpages";
        //public const string Duration = "duration";
    }
}