using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public enum AuthenticationType
    {
        ExtLive,
        ExtGoogle,
    };

    /// <summary>
    /// Module authentication.
    /// </summary>
    public class ModuleAuthentication
    {
        public AuthenticationType Type { get; set; }
        //public string AuthenticationType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        //public string CallbackPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }
    }
}
