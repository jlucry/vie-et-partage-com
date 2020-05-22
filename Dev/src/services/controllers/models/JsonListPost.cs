using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Represents a list of posts.
    /// </summary>
    public class JsonListPost
    {
        private readonly IEnumerable<JsonPost> _posts;
        private readonly JsonListSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="posts"></param>
        public JsonListPost(JsonListSettings settings, IEnumerable<JsonPost> posts)
        {
            _posts = posts;
            _settings = settings;
        }

        /// <summary>
        /// Settings of the list.
        /// </summary>
        public JsonListSettings Settings { get { return _settings; } }

        /// <summary>
        /// Post of the list.
        /// </summary>
        public IEnumerable<JsonPost> Posts { get { return _posts; } }
    }
}