using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class UserClaimType : ClaimType
    {
        public const string Role = "role";
        public const string FirstName = "fname"; //prénom
        public const string LastName = "lname";
        public const string Mailing = "mailing";
        public const string Ip = "ip";
        public const string Phone = "phone";
        public const string Zip = "zip";
    }
}
