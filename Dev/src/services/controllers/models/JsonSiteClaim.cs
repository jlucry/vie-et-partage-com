using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services
{
    /// <summary>
    /// Site claim.
    /// Claim type:
    ///     * region
    ///     * categorie
    ///     * tag
    ///     * group
    /// </summary>
    public class JsonSiteClaim
    {
        /// <summary>
        /// Json site claim contructor.
        /// </summary>
        /// <param name="post"></param>
        public JsonSiteClaim(SiteClaim claim)
        {
            // Set...
            if (claim != null)
            {
                Id = claim?.Id ?? 0;
                Type = claim?.Type;
                Value = claim?.Value;
                StringValue = claim?.StringValue;
                DateTimeValue = claim?.DateTimeValue;
                Parent = claim?.ParentId ?? 0;
                if (claim.Childs != null)
                {
                    Childs = new List<JsonSiteClaim>();
                    foreach (SiteClaim child in claim.Childs)
                    {
                        Childs.Add(new JsonSiteClaim(child));
                    }
                }
            }
        }

        /// <summary>
        /// Claim id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Claim type:
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Claim value.
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// Claim string value.
        /// </summary>
        public string StringValue { get; set; }

        /// <summary>
        /// Claim DateTime value.
        /// </summary>
        public DateTime? DateTimeValue { get; set; }

        /// <summary>
        /// Parent claim Id.
        /// </summary>
        public int Parent { get; set; }
        /// <summary>
        /// Childs pages.
        /// </summary>
        public List<JsonSiteClaim> Childs { get; set; }
        /// <summary>
        /// Deep.
        /// </summary>
        public int Deep { get; set; }

        /// <summary>
        /// To flat list
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="flat"></param>
        public static List<JsonSiteClaim> ToFlatList(IEnumerable<JsonSiteClaim> claims, List<JsonSiteClaim> flat, int deep = 0)
        {
            //string indentTag = "&nbsp;&nbsp;";
            //string indentTag = "--";
            if (claims != null && flat != null)
            {
                //string indent = string.Empty;
                //for (int i = 0; i < deep; i += 1)
                //{
                //    indent = indentTag + indent;
                //}

                foreach (JsonSiteClaim claim in claims)
                {
                    //if (deep != 0 && claim.StringValue != null)
                    //{
                    //    claim.StringValue = indent + string.Empty + claim.StringValue;
                    //    //claim.StringValue = string.Concat(new string('-', deep), claim.StringValue);
                    //    //claim.StringValue = claim.StringValue.PadLeft(claim.StringValue.Length + 2 * deep, '-');
                    //}
                    claim.Deep = deep;
                    flat.Add(claim);
                    if (claim.Childs != null && claim.Childs.Count() != 0)
                    {
                        ToFlatList(claim.Childs, flat, deep + 1);
                    }
                    claim.Childs = null;
                }
            }
            return flat;
        }
    }
}