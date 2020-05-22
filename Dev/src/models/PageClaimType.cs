using System;
using System.Collections.Generic;

namespace Models
{
    /// <summary>
    /// Page claim type.
    /// </summary>
    public class PageClaimType : ClaimType
    {
        public const string FilterPostId = "postid";
        public const string ShowChildsCategories = "showchildscat";
        public const string PostFiltering = "postFiltering";
    }

    public class PageFiltering
    {
        public const string ExcludeEvent = "excludeEvent";
    }
}