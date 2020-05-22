using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models
{
    public class ClaimValueRole
    {
        public const string Reader = "Reader";
        public const string Contributor = "Contributor";
        public const string Publicator = "Publicator";
        public const string Administrator = "Administrator";

        // Optional roles...
        public const string PostRegistrationTo = "PostRegistration";
    }
}
