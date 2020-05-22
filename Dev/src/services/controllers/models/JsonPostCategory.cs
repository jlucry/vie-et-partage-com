using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Post category association.
    /// </summary>
    public class JsonPostCategory
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonPostCategory()
        {
        }
        public JsonPostCategory(PostCategory cat)
        {
            // Set...
            if (cat != null)
            {
                Id = cat.Id;
                PostId = cat.Post?.Id ?? 0;
                CategoryId = cat.CategoryId;
            }
        }
        public JsonPostCategory(Post post, SiteClaim claim)
        {
            // Set...
            if (post != null && claim != null)
            {
                Id = 0;
                PostId = post.Id;
                CategoryId = claim.Id;
                Name = claim.StringValue;
            }
        }
        public JsonPostCategory(Post post, JsonSiteClaim claim)
        {
            // Set...
            if (post != null && claim != null)
            {
                Id = 0;
                PostId = post.Id;
                CategoryId = claim.Id;
                Name = claim.StringValue;
                Deep = claim.Deep;
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
        /// Category id.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Used only for frontend post edition.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Used only for frontend post edition.
        /// </summary>
        public int Deep { get; set; }
        /// <summary>
        /// Used only for frontend post edition.
        /// </summary>
        public bool Checked { get; set; }
    }

    /// <summary>
    /// Post categorys extensions.
    /// </summary>
    public static class JsonPostCategoryExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="categorys"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonPostCategory> categorys)
        {
            if (categorys != null && categorys.Count > 0)
            {
                foreach (JsonPostCategory category in categorys)
                {
                    if (category.Checked == true)
                        yield return category.CategoryId;
                }
            }
        }
    }
}