//// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
//#define DENORMALIZE

//using Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;

//namespace Services
//{
//    /// <summary>
//    /// ClaimsPrincipal extensions.
//    /// </summary>
//    public static class ClaimsPrincipalExtension
//    {
//        ///// <summary>
//        ///// Get user name.
//        ///// </summary>
//        ///// <param name="user"></param>
//        ///// <param name="role"></param>
//        ///// <returns></returns>
//        //public static string GetUserName(this ClaimsPrincipal user)
//        //{
//        //    // Check if the user have the input role...
//        //    return user?.GetUserName();
//        //}

//        /// <summary>
//        /// Get user group ids.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <returns></returns>
//        public static List<int> GetGroupsId(this ClaimsPrincipal user)
//        {
//            return user?.Claims?.Where(c => c.Type == SiteClaimType.Group).Select(c => Convert.ToInt32(c.Value))?.ToList();
//        }

//        /// <summary>
//        /// Get user roles.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <returns></returns>
//        public static List<string> GetRoles(this ClaimsPrincipal user)
//        {
//            // Checking...
//            if (user == null || user.Claims == null)
//            {
//                return null;
//            }
//            return user?.Claims?.Where(c => c.Type == UserClaimType.Role).Select(c => c.Value)?.ToList();
//        }

//        /// <summary>
//        /// Has role.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="role"></param>
//        /// <returns></returns>
//        public static bool HasRole(this ClaimsPrincipal user, string role)
//        {
//            // Checking...
//            if (user == null || user.Claims == null
//                || string.IsNullOrWhiteSpace(role) == true)
//            {
//                return false;
//            }
//            // Check if the user have the input role...
//            return user.HasClaim(UserClaimType.Role, role);
//        }

//#if !DENORMALIZE
//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="groups"></param>
//        /// <returns></returns>
//        public static bool MemberOf(this ClaimsPrincipal user, ICollection<PageGroup> groups)
//        {
//            // Checking...
//            if (user == null || user.Claims == null
//                || groups == null || groups.Count == 0)
//            {
//                return false;
//            }
//            // Check if the user have at least one common group with the input claims...
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Type == SiteClaimType.Group
//                    && groups.Count(g => g.GroupId.ToString() == userClaim.Value) != 0)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="groups"></param>
//        /// <returns></returns>
//        public static bool MemberOf(this ClaimsPrincipal user, ICollection<PostGroup> groups)
//        {
//            // Checking...
//            if (user == null || user.Claims == null
//                || groups == null || groups.Count == 0)
//            {
//                return false;
//            }
//            // Check if the user have at least one common group with the input claims...
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Type == SiteClaimType.Group
//                    && groups.Count(g => g.GroupId.ToString() == userClaim.Value) != 0)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }
//#       else
//        /// <summary>
//        /// Is member of all group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <returns></returns>
//        public static bool MemberOfAllGroup(this ClaimsPrincipal user)
//        {
//            // Checking...
//            if (user == null || user.Claims == null)
//            {
//                return false;
//            }
//            // Check if the user have at least one common group with the input claims...
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Type == SiteClaimType.Group)
//                {
//                    if (userClaim.Value == "\x1b")
//                    {
//                        // User is part of all group...
//                        return true;
//                    }
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="page"></param>
//        /// <returns></returns>
//        public static bool MemberOf(this ClaimsPrincipal user, Page page)
//        {
//            // Checking...
//            if (user == null || user.Claims == null || page == null)
//            {
//                return false;
//            }
//            // Check if the user have at least one common group with the input claims...
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Type == SiteClaimType.Group)
//                {
//                    int userGroupId = 0;
//                    if (userClaim.Value == "\x1b")
//                    {
//                        // User is part of all group...
//                        return true;
//                    }
//                    else if (int.TryParse(userClaim.Value, out userGroupId) == true)
//                    {
//                        if ((page.Group1 != -1 && userGroupId == page.Group1)
//                            || (page.Group2 != -1 && userGroupId == page.Group2)
//                            || (page.Group3 != -1 && userGroupId == page.Group3)
//                            || (page.Group4 != -1 && userGroupId == page.Group4)
//                            || (page.Group5 != -1 && userGroupId == page.Group5)
//                            || (page.Group6 != -1 && userGroupId == page.Group6)
//                            || (page.Group7 != -1 && userGroupId == page.Group7)
//                            || (page.Group8 != -1 && userGroupId == page.Group8)
//                            || (page.Group9 != -1 && userGroupId == page.Group9)
//                            || (page.Group10 != -1 && userGroupId == page.Group10))
//                        {
//                            return true;
//                        }
//                    }
//                }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="page"></param>
//        /// <returns></returns>
//        public static bool MemberOfAll(this ClaimsPrincipal user, Page page)
//        {
//            // Checking...
//            if (user == null || user.Claims == null || page == null)
//            {
//                return false;
//            }
//            List<string> userGroups = new List<string>();
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Value == "\x1b")
//                {
//                    // User is part of all group...
//                    return true;
//                }
//                else if (userClaim.Type == SiteClaimType.Group)
//                {
//                    userGroups.Add(userClaim.Value);
//                }
//            }
//            List<string> dataGroups = new List<string>();
//            if (page.Group1 != -1) dataGroups.Add(page.Group1.ToString());
//            if (page.Group2 != -1) dataGroups.Add(page.Group2.ToString());
//            if (page.Group3 != -1) dataGroups.Add(page.Group3.ToString());
//            if (page.Group4 != -1) dataGroups.Add(page.Group4.ToString());
//            if (page.Group5 != -1) dataGroups.Add(page.Group5.ToString());
//            if (page.Group6 != -1) dataGroups.Add(page.Group6.ToString());
//            if (page.Group7 != -1) dataGroups.Add(page.Group7.ToString());
//            if (page.Group8 != -1) dataGroups.Add(page.Group8.ToString());
//            if (page.Group9 != -1) dataGroups.Add(page.Group9.ToString());
//            if (page.Group10 != -1) dataGroups.Add(page.Group10.ToString());

//            if (userGroups == null && dataGroups == null)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == 0 && dataGroups.Count == 0)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == dataGroups.Count)
//            {
//                foreach (string group in userGroups)
//                {
//                    if (dataGroups.Contains(group) == false)
//                    {
//                        return false;
//                    }
//                }
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="post"></param>
//        /// <returns></returns>
//        public static bool MemberOf(this ClaimsPrincipal user, Post post)
//        {
//            // Checking...
//            if (user == null || user.Claims == null || post == null)
//            {
//                return false;
//            }
//            // Check if the user have at least one common group with the input claims...
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Value == "\x1b")
//                {
//                    // User is part of all group...
//                    return true;
//                }
//                else if (userClaim.Type == SiteClaimType.Group)
//                {
//                    int userGroupId = 0;
//                    if (int.TryParse(userClaim.Value, out userGroupId) == true)
//                    {
//                        if ((post.Group1 != -1 && userGroupId == post.Group1)
//                            || (post.Group2 != -1 && userGroupId == post.Group2)
//                            || (post.Group3 != -1 && userGroupId == post.Group3)
//                            || (post.Group4 != -1 && userGroupId == post.Group4)
//                            || (post.Group5 != -1 && userGroupId == post.Group5)
//                            || (post.Group6 != -1 && userGroupId == post.Group6)
//                            || (post.Group7 != -1 && userGroupId == post.Group7)
//                            || (post.Group8 != -1 && userGroupId == post.Group8)
//                            || (post.Group9 != -1 && userGroupId == post.Group9)
//                            || (post.Group10 != -1 && userGroupId == post.Group10))
//                        {
//                            return true;
//                        }
//                    }
//                }
//            }
//            return false;
//        }
        
//        /// <summary>
//        /// Has group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="post"></param>
//        /// <returns></returns>
//        public static bool MemberOfAll(this ClaimsPrincipal user, Post post)
//        {
//            // Checking...
//            if (user == null || user.Claims == null || post == null)
//            {
//                return false;
//            }
//            List<string> userGroups = new List<string>();
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Value == "\x1b")
//                {
//                    // User is part of all group...
//                    return true;
//                }
//                else if (userClaim.Type == SiteClaimType.Group)
//                {
//                    userGroups.Add(userClaim.Value);
//                }
//            }
//            List<string> dataGroups = new List<string>();
//            if (post.Group1 != -1) dataGroups.Add(post.Group1.ToString());
//            if (post.Group2 != -1) dataGroups.Add(post.Group2.ToString());
//            if (post.Group3 != -1) dataGroups.Add(post.Group3.ToString());
//            if (post.Group4 != -1) dataGroups.Add(post.Group4.ToString());
//            if (post.Group5 != -1) dataGroups.Add(post.Group5.ToString());
//            if (post.Group6 != -1) dataGroups.Add(post.Group6.ToString());
//            if (post.Group7 != -1) dataGroups.Add(post.Group7.ToString());
//            if (post.Group8 != -1) dataGroups.Add(post.Group8.ToString());
//            if (post.Group9 != -1) dataGroups.Add(post.Group9.ToString());
//            if (post.Group10 != -1) dataGroups.Add(post.Group10.ToString());

//            if (userGroups == null && dataGroups == null)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == 0 && dataGroups.Count == 0)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == dataGroups.Count)
//            {
//                foreach (string group in userGroups)
//                {
//                    if (dataGroups.Contains(group) == false)
//                    {
//                        return false;
//                    }
//                }
//                return true;
//            }

//            return false;
//        }
//#endif

//        /// <summary>
//        /// Has all group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="dataUser"></param>
//        /// <returns></returns>
//        public static bool MemberOfAll(this ClaimsPrincipal user, ApplicationUser dataUser)
//        {
//            // Checking...
//            if (user == null || user.Claims == null || dataUser == null)
//            {
//                return false;
//            }
//            List<string> userGroups = new List<string>();
//            foreach (System.Security.Claims.Claim userClaim in user.Claims)
//            {
//                if (userClaim.Value == "\x1b")
//                {
//                    // User is part of all group...
//                    return true;
//                }
//                else if (userClaim.Type == SiteClaimType.Group)
//                {
//                    userGroups.Add(userClaim.Value);
//                }
//            }
//            List<string> dataGroups = new List<string>();
//            foreach (Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string> userClaim in dataUser.Claims)
//            {
//                if (userClaim.ClaimType == SiteClaimType.Group)
//                {
//                    dataGroups.Add(userClaim.ClaimValue);
//                }
//            }

//            if (userGroups == null && dataGroups == null)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == 0 && dataGroups.Count == 0)
//            {
//                // No group for both...
//                return true;
//            }
//            else if (userGroups.Count == dataGroups.Count)
//            {
//                foreach (string group in userGroups)
//                {
//                    if (dataGroups.Contains(group) == false)
//                    {
//                        return false;
//                    }
//                }
//                return true;
//            }

//            return false;
//        }

//        /// <summary>
//        /// Filters specified group.
//        /// </summary>
//        /// <param name="user"></param>
//        /// <param name="post"></param>
//        public static IEnumerable<int> Filter(this ClaimsPrincipal user, IEnumerable<int> groups)
//        {
//            if (groups != null && groups.Count() > 0)
//            {
//                List<int> userGroups = user.GetGroupsId();
//                if (userGroups != null && userGroups.Count() > 0)
//                {
//                    foreach (int group in groups)
//                    {
//                        if (userGroups.Contains(group) == true)
//                        { 
//                            yield return group;
//                            break;
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
