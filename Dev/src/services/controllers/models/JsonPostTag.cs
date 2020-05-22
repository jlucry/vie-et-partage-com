using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Post tag association.
    /// </summary>
    public class JsonPostTag
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonPostTag()
        {
        }
        public JsonPostTag(PostTag tag)
        {
            // Set...
            if (tag != null)
            {
                Id = tag.Id;
                PostId = tag.Post?.Id ?? 0;
                TagId = tag.TagId;
            }
        }
        public JsonPostTag(Post post, SiteClaim claim)
        {
            // Set...
            if (post != null && claim != null)
            {
                Id = 0;
                PostId = post.Id;
                TagId = claim.Id;
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
        /// Tag id.
        /// </summary>
        public int TagId { get; set; }

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
    /// Post tags extensions.
    /// </summary>
    public static class JsonPostTagExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonPostTag> tags)
        {
            if (tags != null && tags.Count > 0)
            {
                foreach (JsonPostTag tag in tags)
                {
                    if (tag.Checked == true)
                        yield return tag.TagId;
                }
            }
        }
    }
}