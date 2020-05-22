using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// User group association.
    /// </summary>
    public class JsonUserGroup
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonUserGroup()
        {
        }
        public JsonUserGroup(UserGroup group)
        {
            // Set...
            if (group != null)
            {
                Id = group.Id;
                UserId = group.User?.Id;
                GroupId = group.GroupId;
            }
        }
        public JsonUserGroup(ApplicationUser user, SiteClaim claim)
        {
            // Set...
            if (user != null && claim != null)
            {
                Id = 0;
                UserId = user.Id;
                GroupId = claim.Id;
                Name = claim.StringValue;
            }
        }

        /// <summary>
        /// Association id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User of the association.
        /// </summary>
        public  string UserId { get; set; }

        /// <summary>
        /// Groug id.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Used only for frontend user edition.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Used only for frontend user edition.
        /// </summary>
        public bool Checked { get; set; }
    }

    /// <summary>
    /// User groups extensions.
    /// </summary>
    public static class JsonUsertGroupExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonUserGroup> groups)
        {
            if (groups != null && groups.Count > 0)
            {
                foreach (JsonUserGroup group in groups)
                {
                    if (group.Checked == true)
                        yield return group.GroupId;
                }
            }
        }
    }
}