// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Contracts;
using Framework;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace Services
{
    /// <summary>
    /// Post extensions.
    /// </summary>
    public static class PostExtensions
    {
        /// <summary>
        /// Get post claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<PostClaim> GetClaims(this Post post, string type = null)
        {
            return (string.IsNullOrWhiteSpace(type) == true)
                ? post?.PostClaims
                : post?.PostClaims?.Where(s => s.Type == type);
        }

        /// <summary>
        /// Get post claim.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PostClaim GetClaim(this Post post, int id)
        {
            return post?.PostClaims?.FirstOrDefault(s => s.Id == id);
        }

        ///// <summary>
        ///// Get post groups.
        ///// </summary>
        ///// <param name="post"></param>
        ///// <returns></returns>
        //public static ICollection<Models.Claim> GetGroups(this Post post)
        //{
        //    return post?.PostClaims?.Where(s => s.Type == ClaimType.Group).Cast<Models.Claim>()?.ToList();
        //}

        ///// <summary>
        ///// Get post regions.
        ///// </summary>
        ///// <param name="post"></param>
        ///// <returns></returns>
        //public static ICollection<PostClaim> GetRegions(this Post post)
        //{
        //    return post?.PostClaims?.Where(s => s.Type == ClaimType.Region)?.ToList();
        //}

        /// <summary>
        /// Get post regions as string.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetRegionsAsString(this Post post, WcmsAppContext appctx)
        {
            string asString = string.Empty;
            List<int> ids = new List<int>();
            ICollection<PostRegion> postRegions = null;

#           if !DENORMALIZE
            if ((postRegions = post?.PostRegions) != null)
#           else
            if ((postRegions = post?.PostRegions(appctx)) != null)
#           endif
            {
                if ((postRegions?.Count ?? 0) == appctx.RegionCount)
                {
                    return "Toutes les régions";
                }
                foreach (PostRegion region in postRegions)
                {
                    if (region.RegionId == 0)
                    {
                        return "Toutes";
                    }
                    ids.Add(region.RegionId);
                }
                asString = appctx?.Site?.GetRegionsAsString(ids) ?? string.Empty;
            }
            return asString;
        }

        ///// <summary>
        ///// Get post categories.
        ///// </summary>
        ///// <param name="post"></param>
        ///// <returns></returns>
        //public static ICollection<PostClaim> GetCategories(this Post post)
        //{
        //    return post?.PostClaims?.Where(s => s.Type == ClaimType.Categorie)?.ToList();
        //}

#if !DENORMALIZE
        /// <summary>
        /// Has the specified category.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="catId"></param>
        /// <returns></returns>
        public static bool HasCategory(this Post post, int catId)
        {
            //TODO: Need to have categories in the post data...
            return (post?.PostCategorys?.Count(pc => pc.CategoryId == catId) ?? 0) != 0;
        }
#else
        /// <summary>
        /// Has the specified category.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <param name="catId"></param>
        /// <returns></returns>
        public static bool HasCategory(this Post post, WcmsAppContext appctx, int catId)
        {
            //TODO: Need to have categories in the post data...
            return (post?.PostCategorys(appctx)?.Count(pc => pc.CategoryId == catId) ?? 0) != 0;
        }
#endif

        ///// <summary>
        ///// Get post tags.
        ///// </summary>
        ///// <param name="post"></param>
        ///// <returns></returns>
        //public static ICollection<PostClaim> GetTags(this Post post)
        //{
        //    return post?.PostClaims?.Where(s => s.Type == ClaimType.Tag)?.ToList();
        //}

        /// <summary>
        /// Get post title.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetTitle(this Post post, int size)
        {
            return HtmlString.TitleReSize(post?.Title, size);
        }

        /// <summary>
        /// Get post url.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetUrl(this Post post, WcmsAppContext appctx)
        {
            string region = appctx?.Region?.StringValue;
            string catExtension = (appctx?.Cat != null) ? $"ct{appctx.Cat.Id}/" : string.Empty;
            string url = $"/{Framework.String.ToUrl(post.Title)}/pg{appctx?.Page?.Id ?? 0}/{catExtension}pt{post.Id}";
            return (region != null)
                ? $"/{Framework.String.ToUrl/*Framework.String.RemoveAccents*/(region).ToLower()}{url}"
                : url;
        }

        /// <summary>
        /// Get post file url
        /// </summary>
        /// <param name="post"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileUrl(this Post post, PostFile file)
        {
            if (post == null || file == null)
            {
                return string.Empty;
            }
            else if (file.Url != null)
            {
                return post.GetFileUrl($"{file.Url}");
            }
            string ext = Path.GetExtension(file.Title);
            return post.GetFileUrl($"{file.Id}{ext}");
        }

        /// <summary>
        /// Get post file url
        /// </summary>
        /// <param name="post"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileUrl(this Post post, string fileName)
        {
            if (post == null || fileName == null)
            {
                return string.Empty;
            }
            else if (fileName.StartsWith("http:") == true)
            {
                if (fileName.Contains("youtube") == true)
                {
                    fileName = fileName.Replace("watch?v=", "embed/");
                }
                return fileName;
            }
            string postFolder = (post.Private == false && post.State == State.Valided)
                ? CRoute.RouteStaticFile_PostPub
                : CRoute.RouteStaticFile_Post;
            return $"{postFolder}{post.CreationDate.Year}/{post.CreationDate.Month}/{post.Id}/{fileName}";
        }

        /// <summary>
        /// Get post cover url.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetCoverUrl(this Post post, bool crop = false)
        {
            if (post?.Cover == null)
            {
                return string.Empty;
            }
            string cover = (crop == true) ? "cover.crop" : "cover";
            return (post.Cover.StartsWith("http:") == true)
                ? post.Cover
                : post.GetFileUrl($"{cover}{post.Cover}");
        }

        /// <summary>
        /// Get post root path.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetRootPath(this Post post, WcmsAppContext appctx)
        {
            return post?.GetFilePath(appctx, string.Empty);
        }

        /// <summary>
        /// Get post file path.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePath(this Post post, WcmsAppContext appctx, string fileName)
        {
            string fileUrl = null;
            if (post.Site == null || (fileUrl = post.GetFileUrl(fileName)) == null)
            {
                return null;
            }
            IFileInfo filInf = appctx?.HostingEnvironment?.WebRootFileProvider?.GetFileInfo($"/{post.Site.Id}/{fileUrl}");
            appctx?.Log?.LogDebug("GetFilePath: {0} to {1}", fileName, filInf?.PhysicalPath);
            return filInf?.PhysicalPath;
        }

        /// <summary>
        /// Get post cover path.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static string GetCoverPath(this Post post, WcmsAppContext appctx)
        {
            if (post?.Cover == null)
            {
                return null;
            }
            return (post.Cover.StartsWith("http:") == true)
                ? null
                : post.GetFilePath(appctx, $"cover{post.Cover}");
        }

        /// <summary>
        /// Post publication date.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetPublicationDate(this Post post)
        {
            try
            {
                string dtstr = post?.ValidationDate?.ToString("dd/MM/yyyy", new CultureInfo("fr-FR"));
                if (string.IsNullOrEmpty(dtstr) == false)
                {
                    string[] split = dtstr.Split(' ');
                    if ((split?.Length ?? 0) >= 1)
                    {
                        return split[0];
                    }
                }
            }
            catch { }

            return "!!!";
        }

        /// <summary>
        /// Post publication short date.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetPublicationShortDate(this Post post)
        {
            return post.GetPublicationDate();
            /*try
            {
                return post?.ValidationDate?.ToShortDateString() ?? "!!!";
            }
            catch
            {
                return "!!!";
            }*/
        }

        /// <summary>
        /// Post start date.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="withTime"></param>
        /// <returns></returns>
        public static string GetStartDate(this Post post, bool withTime = false)
        {
            try
            {
                // Retourner la date de début...
                //return dt.ToShortDateString();
                if (withTime == false)
                    return post?.StartDate?.ToString("d MMM yyyy", new CultureInfo("fr-FR")) ?? "!!!";
                else
                    return post?.StartDate?.ToString("d MMM yyyy HH:mm", new CultureInfo("fr-FR")) ?? "!!!";
            }
            catch
            {
                return "!!!";
            }
        }

        /// <summary>
        /// Post end date.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="withTime"></param>
        /// <returns></returns>
        public static string GetEndDate(this Post post, bool withTime = false)
        {
            try
            {
                // Retourner la date de fin...
                if (withTime == false)
                    return post?.EndDate?.ToString("d MMM yyyy", new CultureInfo("fr-FR")) ?? "!!!";
                else
                    return post?.EndDate?.ToString("d MMM yyyy HH:mm", new CultureInfo("fr-FR")) ?? "!!!";
            }
            catch
            {
                return "!!!";
            }
        }

#if DENORMALIZE
        /// <summary>
        /// Get post group ids.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static List<int> GroupsId(this Post post)
        {
            // Checking...
            if (post == null)
            {
                return null;
            }
            List<int> dataGroups = new List<int>();
            if (post.Group1 != -1) dataGroups.Add(post.Group1);
            if (post.Group2 != -1) dataGroups.Add(post.Group2);
            if (post.Group3 != -1) dataGroups.Add(post.Group3);
            if (post.Group4 != -1) dataGroups.Add(post.Group4);
            if (post.Group5 != -1) dataGroups.Add(post.Group5);
            if (post.Group6 != -1) dataGroups.Add(post.Group6);
            if (post.Group7 != -1) dataGroups.Add(post.Group7);
            if (post.Group8 != -1) dataGroups.Add(post.Group8);
            if (post.Group9 != -1) dataGroups.Add(post.Group9);
            if (post.Group10 != -1) dataGroups.Add(post.Group10);
            return dataGroups;
        }

        /// <summary>
        /// Get post groups.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<PostGroup> PostGroups(this Post post, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<PostGroup> groups = new List<PostGroup>();
            if ((claims = appctx?.Site?.GetGroups()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (claim.Id == post.Group1 || claim.Id == post.Group2
                        || claim.Id == post.Group3 || claim.Id == post.Group4
                        || claim.Id == post.Group5 || claim.Id == post.Group6
                        || claim.Id == post.Group7 || claim.Id == post.Group8
                        || claim.Id == post.Group9 || claim.Id == post.Group10)
                    {
                        groups.Add(new PostGroup { GroupId = claim.Id, Post = post });
                    }
                }
            }
            return groups;
        }

        /// <summary>
        /// Set post tags.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static void PostGroups(this Post post, IEnumerable<int> groups)
        {
            int length = (groups == null) ? 0 : groups.Count();
            post.Group1 = (length >= 1) ? groups.ElementAt(0) : -1;
            post.Group2 = (length >= 2) ? groups.ElementAt(1) : -1;
            post.Group3 = (length >= 3) ? groups.ElementAt(2) : -1;
            post.Group4 = (length >= 4) ? groups.ElementAt(3) : -1;
            post.Group5 = (length >= 5) ? groups.ElementAt(4) : -1;
            post.Group6 = (length >= 6) ? groups.ElementAt(5) : -1;
            post.Group7 = (length >= 7) ? groups.ElementAt(6) : -1;
            post.Group8 = (length >= 8) ? groups.ElementAt(7) : -1;
            post.Group9 = (length >= 9) ? groups.ElementAt(8) : -1;
            post.Group10 = (length >= 10) ? groups.ElementAt(9) : -1;
        }

        /// <summary>
        /// Get post regions.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<PostRegion> PostRegions(this Post post, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<PostRegion> regions = new List<PostRegion>();
            if ((claims = appctx?.Site?.GetRegions()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (post.Region1 == 0
                        || claim.Id == post.Region1 || claim.Id == post.Region2
                        || claim.Id == post.Region3 || claim.Id == post.Region4
                        || claim.Id == post.Region5 || claim.Id == post.Region6
                        || claim.Id == post.Region7 || claim.Id == post.Region8
                        || claim.Id == post.Region9 || claim.Id == post.Region10)
                    {
                        regions.Add(new PostRegion { RegionId = claim.Id, Post = post });
                    }
                }
            }
            return regions;
        }

        /// <summary>
        /// Set post regions.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static void PostRegions(this Post post, IEnumerable<int> regions)
        {
            int length = (regions == null) ? 0 : regions.Count();
            post.Region1 = (length >= 1) ? regions.ElementAt(0) : -1;
            post.Region2 = (length >= 2) ? regions.ElementAt(1) : -1;
            post.Region3 = (length >= 3) ? regions.ElementAt(2) : -1;
            post.Region4 = (length >= 4) ? regions.ElementAt(3) : -1;
            post.Region5 = (length >= 5) ? regions.ElementAt(4) : -1;
            post.Region6 = (length >= 6) ? regions.ElementAt(5) : -1;
            post.Region7 = (length >= 7) ? regions.ElementAt(6) : -1;
            post.Region8 = (length >= 8) ? regions.ElementAt(7) : -1;
            post.Region9 = (length >= 9) ? regions.ElementAt(8) : -1;
            post.Region10 = (length >= 10) ? regions.ElementAt(9) : -1;
        }

        /// <summary>
        /// Get post categories.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<PostCategory> PostCategorys(this Post post, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<PostCategory> categories = new List<PostCategory>();
            if ((claims = appctx?.Site?.GetCategories()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (claim.Id == post.Category1 || claim.Id == post.Category2
                        || claim.Id == post.Category3 || claim.Id == post.Category4
                        || claim.Id == post.Category5 || claim.Id == post.Category6
                        || claim.Id == post.Category7 || claim.Id == post.Category8
                        || claim.Id == post.Category9 || claim.Id == post.Category10)
                    {
                        categories.Add(new PostCategory { CategoryId = claim.Id, Post = post });
                    }
                }
            }
            return categories;
        }

        /// <summary>
        /// Set post categories.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="categorys"></param>
        /// <returns></returns>
        public static void PostCategorys(this Post post, IEnumerable<int> categorys)
        {
            int length = (categorys == null) ? 0 : categorys.Count();
            post.Category1 = (length >= 1) ? categorys.ElementAt(0) : -1;
            post.Category2 = (length >= 2) ? categorys.ElementAt(1) : -1;
            post.Category3 = (length >= 3) ? categorys.ElementAt(2) : -1;
            post.Category4 = (length >= 4) ? categorys.ElementAt(3) : -1;
            post.Category5 = (length >= 5) ? categorys.ElementAt(4) : -1;
            post.Category6 = (length >= 6) ? categorys.ElementAt(5) : -1;
            post.Category7 = (length >= 7) ? categorys.ElementAt(6) : -1;
            post.Category8 = (length >= 8) ? categorys.ElementAt(7) : -1;
            post.Category9 = (length >= 9) ? categorys.ElementAt(8) : -1;
            post.Category10 = (length >= 10) ? categorys.ElementAt(9) : -1;
        }

        /// <summary>
        /// Get post tags.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        /// <returns></returns>
        public static ICollection<PostTag> PostTags(this Post post, WcmsAppContext appctx)
        {
            ICollection<SiteClaim> claims = null;
            ICollection<PostTag> tags = new List<PostTag>();
            if ((claims = appctx?.Site?.GetTags()) != null)
            {
                foreach (SiteClaim claim in claims)
                {
                    if (claim.Id == post.Tag1 || claim.Id == post.Tag2
                        || claim.Id == post.Tag3 || claim.Id == post.Tag4
                        || claim.Id == post.Tag5 || claim.Id == post.Tag6
                        || claim.Id == post.Tag7 || claim.Id == post.Tag8
                        || claim.Id == post.Tag9 || claim.Id == post.Tag10)
                    {
                        tags.Add(new PostTag { TagId = claim.Id, Post = post });
                    }
                }
            }
            return tags;
        }

        /// <summary>
        /// Set post tags.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static void PostTags(this Post post, IEnumerable<int> tags)
        {
            int length = (tags == null) ? 0 : tags.Count();
            post.Tag1 = (length >= 1) ? tags.ElementAt(0) : -1;
            post.Tag2 = (length >= 2) ? tags.ElementAt(1) : -1;
            post.Tag3 = (length >= 3) ? tags.ElementAt(2) : -1;
            post.Tag4 = (length >= 4) ? tags.ElementAt(3) : -1;
            post.Tag5 = (length >= 5) ? tags.ElementAt(4) : -1;
            post.Tag6 = (length >= 6) ? tags.ElementAt(5) : -1;
            post.Tag7 = (length >= 7) ? tags.ElementAt(6) : -1;
            post.Tag8 = (length >= 8) ? tags.ElementAt(7) : -1;
            post.Tag9 = (length >= 9) ? tags.ElementAt(8) : -1;
            post.Tag10 = (length >= 10) ? tags.ElementAt(9) : -1;
        }
#endif

        /// <summary>
        /// Has inscription enabled.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static bool HasInscription(this Post post)
        {
            PostClaim[] stClaims = post.GetClaims(PostClaimType.Registration)?.ToArray();
            if ((stClaims?.Length ?? 0) == 1) {
                if (stClaims [0].StringValue == "true" || stClaims[0].StringValue == "yes") {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get post text.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string GetText(this Post post)
        {
            if (post.PostTexts != null)
            {
                foreach (PostText tag in post.PostTexts)
                {
                    //PostTexts.Add(new JsonPostText(tag));
                    if (tag.Type == PostTextType.Contain)
                    {
                        return WebUtility.HtmlDecode(tag.Value);
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Init post folder.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="appctx"></param>
        public static bool InitDirectory(this Post post, WcmsAppContext appctx)
        {
            string postPath = null;
            if ((postPath = post.GetRootPath(appctx)) == null)
            {
                appctx?.Log?.LogDebug("postPath=null");
                return false;
            }
            appctx?.Log?.LogDebug("postPath={0}", postPath);
            //string[] postPaths = Directory.GetDirectories();
            if (Directory.Exists(postPath) == false)
            {
                Directory.CreateDirectory(postPath);
            }
            return true;
        }

        /// <summary>
        /// Get registartion fields.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="id"></param>
        /// <param name="registrationFields"></param>
        /// <returns></returns>
        public static ICollection<PostRegistrationField> GetRegistrationField(this Post post)
        {
            try
            {
                List<PostClaim> registrationFields = post?.GetClaims(PostClaimType.RegistrationField)?.ToList();
                string registrationField = ((registrationFields?.Count ?? 0) > 0
                    && string.IsNullOrWhiteSpace(registrationFields[0]?.StringValue) == false) ? registrationFields[0].StringValue : null;
                // Check before deserialize...
                if (registrationField == null)
                {
                    return null;
                }
                // Deserialize...
                ICollection<PostRegistrationField> fields = JsonConvert.DeserializeObject<ICollection<PostRegistrationField>>(registrationField);
                return fields;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
