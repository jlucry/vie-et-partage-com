namespace Framework
{
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    //**************************************************
    // Class CHtmlString
    //
    // Description :
    //  The approach that doesn't use Regex performs better than all the Regex approaches.
    //  Using RegexOptions.Compiled and a separate Regex object helps make #3 much faster than #2.
    //  RegexOptions.Compiled has some drawbacks, however. It can reduce startup time by 10x in some cases.
    //  More material is available pertaining to make Regexes simpler and faster to run.
    //
    //***************************************************
    public class HtmlString
    {
        static Regex _reg = new Regex("<.*?>", RegexOptions.Compiled);


        //***************************************************
        // Description : 1.Remove all HTML tags from string.
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string StripTags(string text)
        {
            StringBuilder b = new StringBuilder(text.Length);
            bool inside = false;
            for (int i = 0; i < text.Length; i++)
            {
                char let = text[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (inside == false)
                {
                    b.Append(let);
                }
            }
            return b.ToString();
        }

        //***************************************************
        // Description : 2. Common method used to remove all HTML tags from string with Regex.
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string RemoveHtmlTag(string text)
        {
            return Regex.Replace(text, "<.*?>", string.Empty);
            //return Regex.Replace(text, @"<(.|\n)*?>", string.Empty);
        }

        //***************************************************
        // Description : 3. Optimized Regex method to remove all HTML tags from string with Regex.
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string RemoveHtmlTag2(string text)
        {
            return _reg.Replace(text, string.Empty);
        }

        /// <summary>
        /// Removes all FONT and SPAN tags, and all Class and Style attributes.
        /// Designed to get rid of non-standard Microsoft Word HTML tags.
        /// </summary>
        public static string CleanHtml(string html)
        {
            // start by completely removing all unwanted tags  
            html = Regex.Replace(html, @"<[/]?(font|xml|del|ins|[ovwxp]:\w+)[^>]*?>", "", RegexOptions.IgnoreCase); //"<[/]?(font|span|xml|del|ins|[ovwxp]:\w+)[^>]*?>"
            // then run another pass over the html (twice), removing unwanted attributes  
            //html = Regex.Replace(html, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
            //html = Regex.Replace(html, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
            return html;
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string TitleReSize(string str, int intSize)
        {
            try
            {
                //Verification...
                if (str == null)
                    return "";
                str = WebUtility.HtmlDecode(str);
                //Retirer le formatage html...
                str = CleanHtml(str);
                //Tronquer à la taille spécifié...
                if (intSize != -1)
                {
                    str = String.reSize(str, intSize);
                }
                return _EncodeSpecialCar(str);
            }
            catch
            {
                return "";
            }
        }

        //***************************************************
        // Description :
        // Param[in]   :
        // Param[out]  :
        // Return      :
        //***************************************************
        public static string reSize(string str, int intSize)
        {
            try
            {
                //Verification...
                if (str == null)
                    return "";
                // Remplacer la simple quote...
                str = str.Replace("'", "&#145;");
                if (str.Length <= intSize)
                    return str;
                //Tronquer sur le dernier espace...
                int space = str.IndexOf(" ", intSize);
                int space2 = str.IndexOf("&nbps", intSize);
                if (space == -1 && space2 == -1)
                {
                    //Tronquer à la taille spécifié...
                    return String.reSize(str, intSize);
                }
                //Tronquer à l'espace le plus proche de la taille spécifié...
                return String.reSize(str, (space2 == -1 || space <= space2) ? space : space2);
            }
            catch
            {
                return "";
            }
        }

        private static string _EncodeSpecialCar(string str)
        {
            //Verification...
            if (str == null)
                return "";
            return str.Replace("'", "&#145;").Replace("\"", "&quot;");
        }
    }
}