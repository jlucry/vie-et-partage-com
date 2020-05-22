using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Module interface.
    /// </summary>
    public interface IModule
    {
        string Name { get; set; }
        string LongName { get; set; }
        string Domain { get; set; }
        IList<string> DomainAlias { get; set; }
        string DefaultDescription { get; set; }
        string DefaultPageKeywords { get; set; }
        bool UseArea { get; set; }
        List<ModuleAuthentication> Authentications { get; set; }
        Dictionary<string, ModuleController> Controllers { get; set; }
    }
}
