using Contracts;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Services factory.
    /// </summary>
    public class Factory
    {
        /// <summary>
        /// Registerd modules.
        /// </summary>
        static Dictionary<string, IModule> _Modules = new Dictionary<string, IModule>();

        /// <summary>
        /// Get modules.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, IModule> GetModules()
        {
            return _Modules;
        }

        /// <summary>
        /// Get the specified module.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static IModule GetModule(string domain)
        {
            // Checking...
            if (string.IsNullOrEmpty(domain) == true)
            {
                return null;
            }
            else if (_Modules.ContainsKey(domain) == false)
            {
                // Search for domain alias...
                foreach (KeyValuePair<string, IModule> mod in Factory.GetModules())
                {
                    if ((mod.Value?.DomainAlias?.Contains(domain) ?? false) == true)
                    {
                        return mod.Value;
                    }
                }
                return null;
            }
            else
            {
                return _Modules[domain];
            }
        }

        /// <summary>
        /// Set the specified module.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static bool RegisterModule(string domain, IModule module)
        {
            // Checking...
            if (module == null
                || string.IsNullOrEmpty(domain) == true
                || _Modules.ContainsKey(domain) == true)
            {
                return false;
            }
            // Add...
            _Modules.Add(domain, module);
            return true;
        }
    }
}
