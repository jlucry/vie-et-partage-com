// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Services
{
    /// <summary>
    /// ApplicationUserExtension extensions.
    /// </summary>
    public static class ApplicationUserExtension
    {
        /// <summary>
        /// Has regions.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static bool HasRegions(this ApplicationUser user, List<int> regions)
        {
            // Checking...
            if (user == null || regions == null)
            {
                return false;
            }
            // Check if the user is from any inputs region...
            foreach (int region in regions)
            {
                if (user.Region1 == 0 || user.Region1 == region
                    || user.Region2 == 0 || user.Region2 == region
                    || user.Region3 == 0 || user.Region3 == region
                    || user.Region4 == 0 || user.Region4 == region
                    || user.Region5 == 0 || user.Region5 == region
                    || user.Region6 == 0 || user.Region6 == region
                    || user.Region7 == 0 || user.Region7 == region
                    || user.Region8 == 0 || user.Region8 == region
                    || user.Region9 == 0 || user.Region9 == region
                    || user.Region10 == 0 || user.Region10 == region)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Has all regions.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static bool HasAllRegions(this ApplicationUser user, List<int> regions)
        {
            // Checking...
            if (user == null || regions == null)
            {
                return false;
            }
            // Check if the user is from all region...
            if (user.Region1 == 0
                || user.Region2 == 0
                || user.Region3 == 0
                || user.Region4 == 0
                || user.Region5 == 0
                || user.Region6 == 0
                || user.Region7 == 0
                || user.Region8 == 0
                || user.Region9 == 0
                || user.Region10 == 0)
            {
                return true;
            }
            // Check if the user is from all inputs region...
            foreach (int region in regions)
            {
                if (!(user.Region1 == region
                    || user.Region2 == region
                    || user.Region3 == region
                    || user.Region4 == region
                    || user.Region5 == region
                    || user.Region6 == region
                    || user.Region7 == region
                    || user.Region8 == region
                    || user.Region9 == region
                    || user.Region10 == region))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get user roles.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetRoles(this ApplicationUser user)
        {
            // Checking...
            if (user == null || user.Claims == null)
            {
                return null;
            }
            return user?.Claims?.Where(c => c.ClaimType == UserClaimType.Role).Select(c => c.ClaimValue)?.ToList();
        }

        /// <summary>
        /// Has roles.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool HasRoles(this ApplicationUser user)
        {
            // Checking...
            if (user == null || user.Claims == null)
            {
                return false;
            }
            return ((user?.Claims?.Where(c => c.ClaimType == UserClaimType.Role).Count() ?? 0)
                != 0) ? true : false;
        }

        /// <summary>
        /// Has role.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool HasRole(this ApplicationUser user, string role)
        {
            // Checking...
            if (user == null || user.Claims == null
                || string.IsNullOrWhiteSpace(role) == true)
            {
                return false;
            }
            // Check if the user have the input role...
            return (user.Claims.Where(c => c.ClaimType == UserClaimType.Role && c.ClaimValue == role).Count() != 0);
        }

        /// <summary>
        /// Get user higher role.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string HigherRole(this ApplicationUser user)
        {
            if (user == null || user.Claims == null)
            {
                return null;
            }
            else if (user.HasRole(ClaimValueRole.Administrator) == true)
            {
                return ClaimValueRole.Administrator;
            }
            else if (user.HasRole(ClaimValueRole.Publicator) == true)
            {
                return ClaimValueRole.Publicator;
            }
            else if (user.HasRole(ClaimValueRole.Contributor) == true)
            {
                return ClaimValueRole.Contributor;
            }
            else if (user.HasRole(ClaimValueRole.Reader) == true)
            {
                return ClaimValueRole.Reader;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user group ids.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<int> GroupsId(this ApplicationUser user)
        {
            // Checking...
            if (user == null)
            {
                return null;
            }
            List<int> dataGroups = new List<int>();
            if (user.Group1 != -1) dataGroups.Add(user.Group1);
            if (user.Group2 != -1) dataGroups.Add(user.Group2);
            if (user.Group3 != -1) dataGroups.Add(user.Group3);
            if (user.Group4 != -1) dataGroups.Add(user.Group4);
            if (user.Group5 != -1) dataGroups.Add(user.Group5);
            if (user.Group6 != -1) dataGroups.Add(user.Group6);
            if (user.Group7 != -1) dataGroups.Add(user.Group7);
            if (user.Group8 != -1) dataGroups.Add(user.Group8);
            if (user.Group9 != -1) dataGroups.Add(user.Group9);
            if (user.Group10 != -1) dataGroups.Add(user.Group10);
            return dataGroups;
        }

        /// <summary>
        /// Get user group ids as string.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GroupsIdAsString(this ApplicationUser user)
        {
            // Checking...
            if (user == null)
            {
                return null;
            }
            List<string> dataGroups = new List<string>();
            if (user.Group1 != -1) dataGroups.Add(user.Group1.ToString());
            if (user.Group2 != -1) dataGroups.Add(user.Group2.ToString());
            if (user.Group3 != -1) dataGroups.Add(user.Group3.ToString());
            if (user.Group4 != -1) dataGroups.Add(user.Group4.ToString());
            if (user.Group5 != -1) dataGroups.Add(user.Group5.ToString());
            if (user.Group6 != -1) dataGroups.Add(user.Group6.ToString());
            if (user.Group7 != -1) dataGroups.Add(user.Group7.ToString());
            if (user.Group8 != -1) dataGroups.Add(user.Group8.ToString());
            if (user.Group9 != -1) dataGroups.Add(user.Group9.ToString());
            if (user.Group10 != -1) dataGroups.Add(user.Group10.ToString());
            return dataGroups;
        }

        /// <summary>
        /// Is member of all groups.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool MemberOfAllGroup(this ApplicationUser user)
        {
            // Checking...
            if (user == null)
            {
                return false;
            }
            return (user.Group1 == -12346789);
        }

        /// <summary>
        /// Has group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool MemberOf(this ApplicationUser user, ApplicationUser data)
        {
            // Checking...
            if (user == null || data == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of the data groups...
            return _MemberOf(user.GroupsId(), data.GroupsId());
        }

        /// <summary>
        /// Has all group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool MemberOfAll(this ApplicationUser user, ApplicationUser data)
        {
            // Checking...
            if (user == null || user.Claims == null || data == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of all data groups...
            return _MemberOfAll(user.GroupsId(), data.GroupsId());
        }

        /// <summary>
        /// Has group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool MemberOf(this ApplicationUser user, Page page)
        {
            // Checking...
            if (user == null || page == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of the data groups...
            return _MemberOf(user.GroupsId(), page.GroupsId());
        }

        /// <summary>
        /// Has all group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public static bool MemberOfAll(this ApplicationUser user, Page page)
        {
            // Checking...
            if (user == null || page == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of all data groups...
            return _MemberOfAll(user.GroupsId(), page.GroupsId());
        }

        /// <summary>
        /// Has group.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        public static bool MemberOf(this ApplicationUser user, Post post)
        {
            // Checking...
            if (user == null || post == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of the data groups...
            return _MemberOf(user.GroupsId(), post.GroupsId());
        }

        /// <summary>
        /// Has all groups.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        public static bool MemberOfAll(this ApplicationUser user, Post post)
        {
            // Checking...
            if (user == null || post == null)
            {
                return false;
            }
            else if (user.MemberOfAllGroup() == true)
            {
                return true;
            }
            // Check if the user is part of all data groups...
            return _MemberOfAll(user.GroupsId(), post.GroupsId());
        }
        
        /// <summary>
        /// Filtering specified group and return only group of the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="post"></param>
        public static IEnumerable<int> Filter(this ApplicationUser user, IEnumerable<int> groups)
        {
            if (groups != null && groups.Count() > 0)
            {
                List<int> userGroups = user.GroupsId();
                if (userGroups != null && userGroups.Count() > 0)
                {
                    foreach (int group in groups)
                    {
                        if (userGroups.Contains(group) == true)
                        { 
                            yield return group;
                            break;
                        }
                    }
                }
            }
        }

        private static bool _MemberOf(List<int> userGroups, List<int> dataGroups)
        {
            if (userGroups == null || dataGroups == null)
            {
                // Invalid case...
                return false;
            }
            else if (dataGroups.Count == 0)
            {
                // No group for data...
                return true;
            }
            else
            {
                foreach (int userGroup in userGroups)
                {
                    if (dataGroups.Contains(userGroup) == true)
                    {
                        // User is part of one group...
                        return true;
                    }
                }
                // User is part of any group...
                return false;
            }
        }

        private static bool _MemberOfAll(List<int> userGroups, List<int> dataGroups)
        {
            if (userGroups == null || dataGroups == null)
            {
                // Invalid case...
                return false;
            }
            else if (dataGroups.Count == 0)
            {
                // No group for data...
                return true;
            }
            else if (userGroups.Count == dataGroups.Count)
            {
                foreach (int userGroup in userGroups)
                {
                    if (dataGroups.Contains(userGroup) == false)
                    {
                        // User is not part of one group...
                        return false;
                    }
                }
                // User is part of all groups...
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
