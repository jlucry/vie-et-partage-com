using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Post group association.
    /// </summary>
    public class JsonPostGroup
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonPostGroup()
        {
        }
        public JsonPostGroup(PostGroup group)
        {
            // Set...
            if (group != null)
            {
                Id = group.Id;
                PostId = group.Post?.Id ?? 0;
                GroupId = group.GroupId;
            }
        }
        public JsonPostGroup(Post post, SiteClaim claim)
        {
            // Set...
            if (post != null && claim != null)
            {
                Id = 0;
                PostId = post.Id;
                GroupId = claim.Id;
                Name = claim.StringValue;
            }
        }

        /// <summary>
        /// Association id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Post of the association.
        /// </summary>
        public  int PostId { get; set; }

        /// <summary>
        /// Groug id.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Used only for frontend post edition.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Used only for frontend post edition.
        /// </summary>
        public bool Checked { get; set; }
    }

    /// <summary>
    /// Post groups extensions.
    /// </summary>
    public static class JsonPostGroupExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonPostGroup> groups)
        {
            if (groups != null && groups.Count > 0)
            {
                foreach (JsonPostGroup group in groups)
                {
                    if (group.Checked == true)
                        yield return group.GroupId;
                }
            }
        }
    }
}