using Models;
using System;

namespace Services
{
    /// <summary>
    /// Post claim.
    /// </summary>
    public class JsonPostClaim
    {
        public JsonPostClaim(PostClaim claim)
        {
            // Set...
            if (claim != null)
            {
                Id = claim?.Id ?? 0;
                Type = claim?.Type;
                Value = claim?.Value;
                StringValue = claim?.StringValue;
                DateTimeValue = claim?.DateTimeValue;
            }
        }

        /// <summary>
        /// Claim id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Claim type:
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Claim value.
        /// </summary>
        public int? Value { get; set; }

        /// <summary>
        /// Claim string value.
        /// </summary>
        public string StringValue { get; set; }

        /// <summary>
        /// Claim DateTime value.
        /// </summary>
        public DateTime? DateTimeValue { get; set; }

        /// <summary>
        /// Post of the association.
        /// </summary>
        public  int PostId { get; set; }
    }
}