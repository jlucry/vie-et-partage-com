using Models;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// User region association.
    /// </summary>
    public class JsonUserRegion
    {
        /// <summary>
        /// Default constructor for JSON deserialization.
        /// </summary>
        public JsonUserRegion()
        {
        }
        public JsonUserRegion(UserRegion region)
        {
            // Set...
            if (region != null)
            {
                Id = region.Id;
                UserId = region.User?.Id;
                RegionId = region.RegionId;
            }
        }
        public JsonUserRegion(ApplicationUser user, SiteClaim claim)
        {
            // Set...
            if (user != null && claim != null)
            {
                Id = 0;
                UserId = user.Id;
                RegionId = claim.Id;
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
        public string UserId { get; set; }

        /// <summary>
        /// Region id.
        /// 0 for all region.
        /// </summary>
        public int RegionId { get; set; }

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
    /// User regions extensions.
    /// </summary>
    public static class JsonUserRegionExtensions
    {
        /// <summary>
        /// To Id collection
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public static IEnumerable<int> ToIdArray(this ICollection<JsonUserRegion> regions)
        {
            if (regions != null && regions.Count > 0)
            {
                foreach (JsonUserRegion region in regions)
                {
                    if (region.Checked == true)
                        yield return region.RegionId;
                }
            }
        }
    }
}