using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Module controllers.
    /// </summary>
    public class ModuleController
    {
        public string Name { get; set; }
        public bool HaveControllerAndView { get; set; }
        public bool HaveLayout { get; set; }
    }
}
