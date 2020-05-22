using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public class CRoute
    {
        public const string RegionTagName = "regionName";
        public const string PageTitleTagName = "pageTitle";
        public const string PageIdTagName = "pageId";
        public const string PostTitleTagName = "postTitle";
        public const string PostIdTagName = "postId";
        public const string CatIdTagName = "catId";
        public const string CatTitleTagName = "catTitle";

        public const string RouteAccountLogin = "/account/login";
        public const string RouteAccountRegister = "/account/register";
        public const string RouteStaticFile_Lib = "/lib/";
        public const string RouteStaticFile_Admin = "/admin";
        public const string RouteStaticFile_Css = "/css/";
        public const string RouteStaticFile_Images = "/images/";
        public const string RouteStaticFile_Js = "/js/";
        public const string RouteStaticFile_Theme = "/theme/";
        public const string RouteStaticFile_Post = "/post/";
        public const string RouteStaticFile_PostPub = "/post/pub/";
        public const string RouteStaticFile_User = "/user/";
    }
}
