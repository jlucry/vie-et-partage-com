using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Post region association.
    /// </summary>
    public class JsonPostRegion
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonPostRegion()
        {
        }
        public JsonPostRegion(PostRegion region)
        {
            // Set...
            if (region != null)
            {
                Id = region.Id;
                PostId = region.Post?.Id ?? 0;
                RegionId = region.RegionId;
            }
        }
        public JsonPostRegion(Post post, SiteClaim claim)
        {
            // Set...
            if (post != null && claim != null)
            {
                Id = 0;
                PostId = post.Id;
                RegionId = claim.Id;
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
        public int PostId { get; set; }

        /// <summary>
        /// Region id.
        /// 0 for all region.
        /// </summary>
        public int RegionId { get; set; }

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
    /// Post regions extensions.
    /// </summary>
    public static class JsonPostRegionExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonPostRegion> regions)
        {
            if (regions != null && regions.Count > 0)
            {
                foreach (JsonPostRegion region in regions)
                {
                    if (region.Checked == true)
                        yield return region.RegionId;
                }
            }
        }
    }
}