using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Represents a list of users.
    /// </summary>
    public class JsonListUser
    {
        private readonly IEnumerable<JsonUser> _users;
        private readonly JsonListSettings _settings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="users"></param>
        public JsonListUser(JsonListSettings settings, IEnumerable<JsonUser> users)
        {
            _users = users;
            _settings = settings;
        }

        /// <summary>
        /// Settings of the list.
        /// </summary>
        public JsonListSettings Settings { get { return _settings; } }

        /// <summary>
        /// User of the list.
        /// </summary>
        public IEnumerable<JsonUser> Users { get { return _users; } }
    }
}