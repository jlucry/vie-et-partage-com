using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Services
{
    /// <summary>
    /// Post api.
    /// </summary>
    [Authorize]
    [Route("api/post")]
    public class PostApiController : BaseController
    {
        private IHostingEnvironment _environment;
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// The Post api controller constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public PostApiController(IHostingEnvironment environment,
            Services.WcmsAppContext appContext, IEmailSender emailSender)
            : base(appContext)
        {
            _emailSender = emailSender;
        }
        
        // GET: api/post/list
        [AllowAnonymous]
        [HttpPost("List")]  
        public async Task<JsonListPost> Get([FromBody]JsonListSettings settings)
        {
            try
            {
                // Initialisation...
                Dictionary<string, string> defaultFilters = settings?.DefaultFilters;
                if (settings == null || settings.Filters == null)
                {
                    settings = new JsonListSettings()
                    {
                        Count = 0,
                        Skip = 0,
                        Take = 20,
                        Filters = new Dictionary<string, string>()
                        {
                            { QueryFilter.MineToo, "true" }
                        }
                    };
                    if ((AppContext.User?.HasRole(ClaimValueRole.Administrator) ?? false) == true
                        || (AppContext.User?.HasRole(ClaimValueRole.Publicator) ?? false) == true)
                    {
                        settings.Filters.Add(QueryFilter.State, "-1");
                    }
                    else
                    {
                        settings.Filters.Add(QueryFilter.State, "1");
                    }
                    if (settings == null || settings.Filters == null)
                    {
                        _Log.LogCritical("Not enough memory to allocate a new JsonListSettings!");
                        return null;
                    }
                }
                if (defaultFilters != null)
                {
                    foreach (KeyValuePair<string, string> kp in defaultFilters)
                    {
                        settings.Filters.Add(kp.Key, kp.Value);
                    }
                }
                // By default: Always add childs items of the categories...
                if (settings.Filters.ContainsKey(QueryFilter.ShowChildsCategoriesPosts) == false)
                {
                    settings.Filters.Add(QueryFilter.ShowChildsCategoriesPosts, "true");
                }
                // Get posts based on filter...
                PostProvider provider = new PostProvider(AppContext);
                Dictionary<string, object> filters = provider?.ConvertFilter(settings.Filters);
                IEnumerable<Post> posts = await provider?.Get(filters, settings.Skip, settings.Take);
                JsonListPost list = new JsonListPost(settings, posts?.Select(p => new JsonPost(AppContext, p)));
                if (settings.Skip == 0 && settings.Count == 0
                    && list != null)
                {
                    // Get posts total count...
                    list.Settings.Count = await provider?.Count(filters);
                }
                // Clean settings...
                if (defaultFilters != null)
                {
                    foreach (KeyValuePair<string, string> kp in defaultFilters)
                    {
                        settings.Filters.Remove(kp.Key);
                    }
                }
                return list;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception getting posts - HttpPost:/api/post/list: {0}", e.Message);
                return null;
            }
        }

        // GET api/post/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<JsonPost> Get(int id)
        {
            try
            {
                JsonPost jsnPost = null;
                // Get and return the post...
                if (id == 0)
                {
                    // Call for adding a new post...
                    jsnPost = new JsonPost(AppContext, null, true);
                    jsnPost.Title =  "Votre titre ici...";
                    jsnPost.TextContain = "Votre texte la...";
                }
                else
                {
                    PostProvider provider = new PostProvider(AppContext);
                    Post post = await provider?.Get(id);
                    jsnPost = new JsonPost(AppContext, post, true);
                }
                return jsnPost;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception getting post - HttpGet:/api/post/{0}: {1}", id, e.Message);
                return null;
            }
        }

        // POST api/post
        [HttpPost]
        public async Task<JsonPost> Post(int type, [FromBody]JsonPost post)
        {
            try
            {
                if (post != null)
                {
                    Post postEdited = null;
                    PostProvider provider = new PostProvider(AppContext);

                    // Update the post...
                    if (type == 1)
                        // Update post title and text...
                        postEdited = await provider?.Update(post.Id, post.Title, post.TextContain);
                    else if (type == 2)
                        // Update post start\end date and registration...
                        postEdited = await provider?.Update(post.Id, post.StartDateString, post.EndDateString, 
                            post.ClaimRegistration, post.RegistrationWebConfirmation, post.RegistrationEmailConfirmation,
                            post.RegistrationFields/*RegistrationFieldsToPartialClaims()*/);
                    else if (type == 3)
                        // Update post settings...
                        postEdited = await provider?.Update(post.Id, post.State, 
                            post.PostRegions.ToIdArray(), post.PostCategorys.ToIdArray(),
                            post.PostTags.ToIdArray(), post.PostGroups.ToIdArray());

                    // The result...
                    post = (postEdited == null)
                        ? new JsonPost(provider?.LastError)
                        : new JsonPost(AppContext, postEdited, true);
                }
                //Thread.Sleep(5 * 1000);
                return post;
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception updating post - HttpPost:/api/post(type={0}, post={1}): {2}", type, post?.Id ?? 0, e.Message);
                return new JsonPost(UserMessage.UnexpectedError);
            }
        }

        // POST api/post/inscription
        [AllowAnonymous]
        [HttpPost("Inscription/{id}")]
        public async Task<string> Inscription(int id, [FromBody]List<JsonPostRegistrationField> registrationFields)
        {
            try
            {
                PostProvider provider = new PostProvider(AppContext, _emailSender);
                return await provider?.Registration(id, registrationFields);
            }
            catch (Exception e)
            {
                AppContext?.Log?.LogError("Exception during post registration - HttpGet:/api/post/inscription/{0}: {1}", id, e.Message);
                return null;
            }
        }

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            return true;
        }

        // DELETE api/post/5/{id}/{fileId}
        [HttpDelete("{id}/{fileId}")]
        public async Task<bool> Delete(int id, int fileId)
        {
            try
            {
                PostProvider provider = new PostProvider(AppContext);
                return (provider == null)
                    ? false
                    : await provider.DeleteFile(id, fileId);
            }
            catch
            {
                return false;
            }
        }

        //[AllowAnonymous]
        //[HttpGet("Configuration")]
        //public PostsConfiguration Configuration(int id)
        //{
        //    //Attention_Prob_De_Cache;
        //    //TODO: Activé l'authentification - retirer la decoration AllowAnonymous.
        //    return null;
        //}

        //// POST api/upload
        //[HttpPost]
        //public async Task<JsonPost> upload()
        //{
        //    try
        //    {
        //        return null;
        //    }
        //    catch
        //    {
        //        return new JsonPost(UserMessage.UnexpectedError);
        //    }
        //}

        public class FileDescriptionShort
        {
            public int Id { get; set; }

            public string Description { get; set; }

            public string Name { get; set; }

            public ICollection<IFormFile> File { get; set; }
        }

        ////[Route("upload")]
        //[HttpPost]
        ////[ServiceFilter(typeof(ValidateMimeMultipartContentFilter))]
        //public async Task<bool> UploadFiles(FileDescriptionShort fileDescriptionShort)
        //{
        //    var names = new List<string>();
        //    var contentTypes = new List<string>();
        //    if (ModelState.IsValid)
        //    {
        //        // http://www.mikesdotnetting.com/article/288/asp-net-5-uploading-files-with-asp-net-mvc-6
        //        // http://dotnetthoughts.net/file-upload-in-asp-net-5-and-mvc-6/
        //        foreach (var file in fileDescriptionShort.File)
        //        {
        //            if (file.Length > 0)
        //            {
        //                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
        //                contentTypes.Add(file.ContentType);
        //                names.Add(fileName);

        //                // Extension method update RC2 has removed this 
        //                //await file.SaveAsAsync(Path.Combine(_optionsApplicationConfiguration.Value.ServerUploadFolder, fileName));
        //            }
        //        }
        //    }
        //    //var files = new FileResult
        //    //{
        //    //    FileNames = names,
        //    //    ContentTypes = contentTypes,
        //    //    Description = fileDescriptionShort.Description,
        //    //    CreatedTimestamp = DateTime.UtcNow,
        //    //    UpdatedTimestamp = DateTime.UtcNow,
        //    //};
        //    //_fileRepository.AddFileDescriptions(files);
        //    //return RedirectToAction("ViewAllFiles", "FileClient");
        //    return false;
        //}

        // https://github.com/aspnet/Mvc/issues/2938
        // http://www.mikesdotnetting.com/article/288/uploading-files-with-asp-net-core-1-0-mvc
        // 

        //[HttpPost("UploadFiles2")]
        //public async Task<IActionResult> UploadFiles2(ICollection<IFormFile> files)
        //{
        //    var uploads = Path.Combine(_environment.WebRootPath, "uploads");
        //    foreach (var file in files)
        //    {
        //        if (file.Length > 0)
        //        {
        //            using (var fileStream = new FileStream(Path.Combine(uploads, file.FileName), FileMode.Create))
        //            {
        //                await file.CopyToAsync(fileStream);
        //            }
        //        }
        //    }
        //    return View();
        //}

        //public async Task<IActionResult> HandleFiles()
        //{
        //    var files = (await Request.GetFormAsync()).Files;
        //    return null;
        //}
    }
}
