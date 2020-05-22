using Models;

namespace Services
{
    public class QueryFilter
    {
        public static string Categorie { get { return SiteClaimType.Categorie; } }
        public static string CategorieSingle { get { return SiteClaimType.Categorie + "Single"; } }
        public static string Tag { get { return SiteClaimType.Tag; } }
        public static string TagSingle { get { return SiteClaimType.Tag + "Single"; } }
        public const string Title = "title";
        public const string State = "state";
        public const string Highlight = "highlight";
        public const string StartDate = "startDate";
        public const string EndDate = "endDate";
        public const string Mine = "mine";
        public const string MineToo = "mineToo";
        public const string Group = "group";
        public static string TopCategorie = "TopCategorie";
        public static string ShowChildsCategoriesPosts = "ShowChildsCategoriesPosts";
        public const string ShowEventPostsOnly = "ShowEventPostsOnly";
        public const string ExcludePostsEvent = "ExcludePostsEvent";
    }
}
