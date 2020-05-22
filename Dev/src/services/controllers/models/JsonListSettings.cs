using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Represents settings of a list of elements.
    /// </summary>
    public class JsonListSettings
    {
        /// <summary>
        /// Default filters.
        /// </summary>
        public Dictionary<string, string> DefaultFilters { get; set; }

        /// <summary>
        /// List filters.
        /// </summary>
        public Dictionary<string, string> Filters { get; set; }

        /// <summary>
        /// Total of element of the list based on the filters.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Skip of the list.
        /// </summary>
        public int Skip { get; set; }

        /// <summary>
        /// Take of the list.
        /// </summary>
        public int Take { get; set; }
    }
}