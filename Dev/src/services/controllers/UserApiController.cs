using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Services
{
    /// <summary>
    /// User api.
    /// </summary>
    [Authorize]
    [Route("api/user")]
    public class UserApiController : BaseController
    {
        private IHostingEnvironment _environment;

        /// <summary>
        /// The User api controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public UserApiController(IHostingEnvironment environment, Services.WcmsAppContext appContext)
            : base(appContext)
        {
        }
        
        // GET: api/user/list
        [AllowAnonymous]
        [HttpPost("List")]
        public async Task<JsonListUser> Get([FromBody]JsonListSettings settings)
        {
            try
            {
                // Initialisation...
                if (settings == null)
                {
                    settings = new JsonListSettings()
                    {
                        Count = 0,
                        Skip = 0,
                        Take = 20,
                        Filters = new Dictionary<string, string>() { }
                    };
                    if (settings == null)
                    {
                        _Log.LogCritical("Not enough memory to allocate a new JsonListSettings!");
                        return null;
                    }
                }
                // Get users based on filter...
                UserProvider provider = new UserProvider(AppContext);
                Dictionary<string, object> filters = provider?.ConvertFilter(settings.Filters);
                IEnumerable<ApplicationUser> users = await provider?.Get(filters, settings.Skip, settings.Take);
                JsonListUser list = new JsonListUser(settings, users?.Select(p => new JsonUser(AppContext, p)));
                if (settings.Skip == 0 && settings.Count == 0
                    && list != null)
                {
                    // Get users total count...
                    list.Settings.Count = await provider?.Count(filters);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        // GET api/user/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<JsonUser> Get(string id)
        {
            try
            {
                JsonUser jsnPost = null;
                // Get and return the user...
                UserProvider provider = new UserProvider(AppContext);
                ApplicationUser user = await provider?.Get(id);
                jsnPost = new JsonUser(AppContext, user, true);
                return jsnPost;
            }
            catch
            {
                return null;
            }
        }

        // POST api/user
        [HttpPost]
        public async Task<JsonUser> User(int type, [FromBody]JsonUser user)
        {
            try
            {
                if (user != null)
                {
                    ApplicationUser postEdited = null;
                    UserProvider provider = new UserProvider(AppContext);

                    // Update the user...
                    if (type == 1)
                        // Update user information...
                        postEdited = await provider?.Update(user.Id, user.FName, user.LName, user.Phone, user.Zip, user.UserRegions.ToIdArray(), user.Password);
                    else if (type == 3)
                        // Update user settings...
                        postEdited = await provider?.Update(user.Id, user.Email, user.UserGroups.ToIdArray(), user.Enabled);

                    // The result...
                    user = (postEdited == null)
                        ? new JsonUser(provider?.LastError)
                        : new JsonUser(AppContext, postEdited, true);
                }
                //Thread.Sleep(5 * 1000);
                return user;
            }
            catch
            {
                return new JsonUser(UserMessage.UnexpectedError);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return true;
        }

        // DELETE api/user/5/{id}/{fileId}
        [HttpDelete("{id}/{fileId}")]
        public async Task<bool> Delete(string id, int fileId)
        {
            try
            {
                UserProvider provider = new UserProvider(AppContext);
                return (provider == null)
                    ? false
                    : await provider.DeleteCover(id);
            }
            catch
            {
                return false;
            }
        }

        public class FileDescriptionShort
        {
            public int Id { get; set; }

            public string Description { get; set; }

            public string Name { get; set; }

            public ICollection<IFormFile> File { get; set; }
        }
    }
}
