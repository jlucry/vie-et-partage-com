// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
#define INCLUDE_ON

using Contracts;
using Framework;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Post provider.
    /// </summary>
    public class PostProvider : BaseProvider
    {
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Post provider constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public PostProvider(WcmsAppContext appContext, IEmailSender emailSender = null)
            : base(appContext)
        {
            _emailSender = emailSender;
            // TODO: Attention, les cat audio et film on été inversé lors dans la procedure de migration!!!
        }

        /// <summary>
        /// Count posts.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public async Task<int> Count(Dictionary<string, object> filters)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid() ?? false) == false)
                {
                    _Log?.LogError("Failed to get posts count: Invalid contexts.");
                    LastError = UserMessage.InternalError;
                    return 0;
                }
                // Return pages...
                return await PostAuthorizationHandler.Count(AppContext, filters);
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get posts count: {0}.", e.Message);
                LastError = UserMessage.UnexpectedError;
                return 0;
            }
        }

        /// <summary>
        /// Get posts.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="allFields"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Post>> Get(Dictionary<string, object> filters, int skip = 0, int take = 20, bool allFields = false)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid() ?? false) == false)
                {
                    _Log?.LogError("Failed to get posts: Invalid contexts.");
                    LastError = UserMessage.InternalError;
                    return null;
                }
                // Return pages...
                return await PostAuthorizationHandler.Get(AppContext, filters, skip, take, allFields);
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get posts: {0}.", e.Message);
                LastError = UserMessage.UnexpectedError;
                return null;
            }
        }

        /// <summary>
        /// Get a post from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Post> Get(string id)
        {
            int idValue = 0;
            // Checking...
            if (string.IsNullOrEmpty(id) == true
                || int.TryParse(id, out idValue) == false)
            {
                LastError = UserMessage.InvalidInputs;
                return null;
            }
            return await Get(idValue);
        }

        /// <summary>
        /// Get a post from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Post> Get(int id)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to get post: Invalid context.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Get::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to get post: Invalid authz provider.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Get::Invalid authz provider");
                    return null;
                }

                // Query the DB to get the specified post...
                Post post = await AppContext.AppDbContext.Posts
#                   if INCLUDE_ON
                    .Include(p => p.PostTexts)
                    .Include(p => p.PostFiles)
                    .Include(p => p.PostClaims)
#                   endif
                    .Include(p => p.Creator)
                    .Include(p => p.Site)
                    .SingleOrDefaultAsync(s => s.Id == id);
                if (post != null)
                {
                    // Provide the site on which we make the request...
                    post.RequestSite = AppContext.Site;
                    // Retrieve claims...
#                   if !DENORMALIZE
                    post.PostGroups = await AppContext.AppDbContext.PostGroups.Where(st => st.PostId == post.Id)?.ToListAsync();
                    post.PostRegions = await AppContext.AppDbContext.PostRegions.Where(st => st.PostId == post.Id)?.ToListAsync();
                    post.PostCategorys = await AppContext.AppDbContext.PostCategorys.Where(st => st.PostId == post.Id)?.ToListAsync();
                    post.PostTags = await AppContext.AppDbContext.PostTags.Where(st => st.PostId == post.Id)?.ToListAsync();
#                   endif 
#                   if !INCLUDE_ON
                    post.PostTexts = await AppContext.AppDbContext.PostTexts.Where(st => st.PostId == post.Id)?.ToListAsync();
                    post.PostFiles = await AppContext.AppDbContext.PostFiles.Where(st => st.PostId == post.Id)?.ToListAsync();
                    post.PostClaims = await AppContext.AppDbContext.PostClaims.Where(st => st.PostId == post.Id)?.ToListAsync();
#                   endif
                    // Check for authorization...
                    if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                        post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false) == false)
                    {
                        _Log?.LogWarning("Failed to get post {0}: Access denied.", id);
                        LastError = UserMessage.AccessDenied;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PostProvider::Get::Access denied");
                        return null;
                    }
                }
                return post;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get post {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::Get::Exception");
                return null;
            }
        }

        /// <summary>
        /// Update the post.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<Post> Update(int id, string title, string text)
        {
            bool add = (id == 0) ? true : false;

            try
            {
                Post post = null;
                PostText postText = null;
                ApplicationUser user = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to update post {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to update post {0}: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::Invalid authz provider");
                    return null;
                }
                else if (string.IsNullOrEmpty(title) == true
                    || string.IsNullOrEmpty(text) == true)
                {
                    _Log?.LogError("Failed to update post {0}: Invalid inputs.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::Invalid inputs");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to update post {0}: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::Invalid user");
                    return null;
                }

                // Add or update the post...
                if (id == 0)
                {
                    post = new Post()
                    {
                        Private = AppContext.Site.Private,
                        Creator = user,
                        CreationDate = DateTime.Now,
                        SiteId = AppContext.Site.Id
                    };
                    postText = null;
#                   if DENORMALIZE
                    // Init group, region, cat and tag...
                    post.Group1 = -1;
                    post.Group2 = -1;
                    post.Group3 = -1;
                    post.Group4 = -1;
                    post.Group5 = -1;
                    post.Group6 = -1;
                    post.Group7 = -1;
                    post.Group8 = -1;
                    post.Group9 = -1;
                    post.Group10 = -1;
                    post.Region1 = (AppContext.Region?.Id ?? 0);
                    post.Region2 = -1;
                    post.Region3 = -1;
                    post.Region4 = -1;
                    post.Region5 = -1;
                    post.Region6 = -1;
                    post.Region7 = -1;
                    post.Region8 = -1;
                    post.Region9 = -1;
                    post.Region10 = -1;
                    post.Category1 = -1;
                    post.Category2 = -1;
                    post.Category3 = -1;
                    post.Category4 = -1;
                    post.Category5 = -1;
                    post.Category6 = -1;
                    post.Category7 = -1;
                    post.Category8 = -1;
                    post.Category9 = -1;
                    post.Category10 = -1;
                    post.Tag1 = -1;
                    post.Tag2 = -1;
                    post.Tag3 = -1;
                    post.Tag4 = -1;
                    post.Tag5 = -1;
                    post.Tag6 = -1;
                    post.Tag7 = -1;
                    post.Tag8 = -1;
                    post.Tag9 = -1;
                    post.Tag10 = -1;
#                   endif
                }
                else
                {
                    // Retrieve the specified post...
                    if ((post = await Get(id)) == null)
                    {
                        _Log?.LogError("Failed to update post {0}: Invalid post id.", id);
                        LastError = UserMessage.InvalidInputs;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PostProvider::Update::Invalid post id");
                        return null;
                    }
                    if (post.PostTexts != null)
                    {
                        foreach (PostText ptxt in post.PostTexts)
                        {
                            if (ptxt.Type == PostTextType.Contain)
                            {
                                postText = ptxt;
                                break;
                            }
                        }
                    }
                }
                if (postText == null)
                {
                    postText = new PostText()
                    {
                        Type = PostTextType.Contain,
                        Number = 1,
                        Revision = 1,
                        SiteId = AppContext.Site.Id,
                    };
                }

                // Check for authorization...
                post.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    post,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = (add == true) ? AuthorizationRequirement.Add : AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to update post {0}: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::Access denied");
                    return null;
                }

                // Apply change to the post...
                post.Title = title;
                post.ModifiedDate = DateTime.Now;
                // Change the state on add if the user don't have publication right...
                if (add == true 
                    || !(AppContext.User.HasRole(ClaimValueRole.Administrator) == true || AppContext.User.HasRole(ClaimValueRole.Publicator) == true))
                {
                    post.State = State.NotValided;
                    post.ValidationDate = null;
                }
                // Apply change to db context...
                if (post.Id == 0)
                {
                    AppContext.AppDbContext.Posts.Add(post);
                }
                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError("Failed to update post {0}: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::DB access failure");
                    return null;
                }

#               if !DENORMALIZE
                // Add post region...
                // !!!Because of issues with Join in EF core denormalize region table!!!
                if (add == true)
                {
                    PostRegion postRegion = new PostRegion
                    {
                        Post = post,
                        RegionId = (AppContext.Region?.Id ?? 0)
                    };
                    if (postRegion.RegionId == -1)
                        postRegion.RegionId = 0;
                    // Apply change to db context...
                    AppContext.AppDbContext.PostRegions.Add(postRegion);
                }
#               endif

                // Add\update post text...
                postText.Value = text;
                postText.Post = post;
                // Apply change to db context...
                if (postText.Id == 0)
                {
                    AppContext.AppDbContext.PostTexts.Add(postText);
                }

                // Update history...
                if (add == false)
                {
                    _UpdateHistory(id, post, user, SiteActionType.Modification);
                }

                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    // Something failed...
                    if (add == true)
                    {
                        // Delete the added post...
                        AppContext.AppDbContext.Posts.Remove(post);
                        await AppContext.AppDbContext.SaveChangesAsync();
                    }
                    _Log?.LogError("Failed to update post {0} region, text and history: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Update::DB access failure");
                    return null;
                }
                // Update post root path...
                _UpdatePostRootPath(AppContext, post);

                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::Update");
                // Return the updated post...
                return post;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to update post {0}: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::Update::Exception");
                return null;
            }
        }

        /// <summary>
        /// Update post dates and registration.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="registration"></param>
        /// <param name="registrationWebConfirmation"></param>
        /// <param name="registrationEmailConfirmation"></param>
        /// <param name="registrationField"></param>
        /// <returns></returns>
        public async Task<Post> Update(int id, string startDate, string endDate, 
            bool registration,
            string registrationWebConfirmation = null, string registrationEmailConfirmation = null,
            ICollection<PostRegistrationField> registrationField = null)
        {
            try
            {
                Post post = null;
                ApplicationUser user = null;
                PostClaim registrationClaim = null;
                PostClaim registrationWebClaim = null;
                PostClaim registrationEmailClaim = null;
                PostClaim registrationFieldClaim = null;
                string registrationStringValue = (registration == true) ? "yes" : "no";
                string registrationFieldStringValue = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::Invalid authz provider");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::Invalid user");
                    return null;
                }
                // Serialize registration fields...
                if (registrationField != null)
                {
                    string srlzErr = null;
                    try
                    {
                        registrationFieldStringValue = JsonConvert.SerializeObject(registrationField);
                    }
                    catch(Exception se)
                    {
                        srlzErr = se.Message;
                        registrationFieldStringValue = null;
                    }
                    if (registrationFieldStringValue == null)
                    {
                        _Log?.LogError("Failed to update post {0} dates and registration: Json serialization failed ({1}).", id, srlzErr);
                        LastError = UserMessage.InternalError;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PostProvider::UpdateDate::Serialization failed");
                        return null;
                    }
                    //Bug: EF Migrations, Insert value for Text Data Type always has (Size=255
                    //https://github.com/SapientGuardian/SapientGuardian.EntityFrameworkCore.MySql/issues/50
                    //Bug: String fields without max length constraint default to varchar(255)
                    //https://github.com/SapientGuardian/SapientGuardian.EntityFrameworkCore.MySql/issues/47
                }

                // Retrieve the specified post...
                if ((post = await Get(id)) == null)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: Invalid post id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::Invalid post id");
                    return null;
                }
                if (post.PostClaims != null)
                {
                    foreach (PostClaim claim in post.PostClaims)
                    {
                        if (claim.Type == PostClaimType.Registration)
                        {
                            registrationClaim = claim;
                        }
                        else if (claim.Type == PostClaimType.RegistrationWebConfirmation)
                        {
                            registrationWebClaim = claim;
                        }
                        else if (claim.Type == PostClaimType.RegistrationEmailConfirmation)
                        {
                            registrationEmailClaim = claim;
                        }
                        else if (claim.Type == PostClaimType.RegistrationField)
                        {
                            if (registrationFieldClaim != null)
                            {
                                // Should never happens, but added to fix an issue on the first deployed version
                                // where an entry was added each time the registration failed was saved.
                                AppContext.AppDbContext.PostClaims.Remove(registrationFieldClaim);
                            }
                            registrationFieldClaim = claim;
                        }
                        if (registrationClaim != null
                            && registrationWebClaim != null && registrationEmailClaim != null
                            && registrationFieldClaim != null)
                            break;
                    }
                }

                // Check for authorization...
                post.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    post,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: Access denied.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::Access denied");
                    return null;
                }

                // Apply change to the post...
                if (string.IsNullOrEmpty(startDate) == true)
                {
                    // Delete the dates informations...
                    post.StartDate = null;
                    post.EndDate = null;
                    // Remove the registration claim...
                    if (registrationClaim != null)
                    {
                        AppContext.AppDbContext.PostClaims.Remove(registrationClaim);
                    }
                    if (registrationWebClaim != null)
                    {
                        AppContext.AppDbContext.PostClaims.Remove(registrationWebClaim);
                    }
                    if (registrationEmailClaim != null)
                    {
                        AppContext.AppDbContext.PostClaims.Remove(registrationEmailClaim);
                    }
                    if (registrationFieldClaim != null)
                    {
                        AppContext.AppDbContext.PostClaims.Remove(registrationFieldClaim);
                    }
                }
                else
                {
                    // Update the dates...
                    try
                    {
                        post.StartDate = DateTime.ParseExact(startDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                        post.EndDate = DateTime.ParseExact(endDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                    }
                    catch(FormatException)
                    {
                        _Log?.LogError("Failed to update post {0} dates and registration: Invalid dates {1}, {2}.", id, startDate, endDate);
                        LastError = UserMessage.InvalidInputs;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PostProvider::UpdateDate::Invalid dates");
                        return null;
                    }
                    // Add\update the registration claim...
                    if (registrationClaim != null)
                    {
                        registrationClaim.StringValue = registrationStringValue;
                    }
                    else
                    {
                        AppContext.AppDbContext.PostClaims.Add(new PostClaim
                        {
                            Type = PostClaimType.Registration,
                            StringValue = registrationStringValue,
                            Post = post,
                            SiteId = AppContext.Site.Id
                        });
                    }
                    // Add\update the registration web confirmation...
                    if (string.IsNullOrWhiteSpace(registrationWebConfirmation) == true && registrationWebClaim != null)
                    {
                        // Delete registration web confirmation...
                        AppContext.AppDbContext.PostClaims.Remove(registrationWebClaim);
                    }
                    else if (string.IsNullOrWhiteSpace(registrationWebConfirmation) != true)
                    {
                        if (registrationWebClaim != null)
                        {
                            registrationWebClaim.StringValue = registrationWebConfirmation;
                        }
                        else
                        {
                            AppContext.AppDbContext.PostClaims.Add(new PostClaim
                            {
                                Type = PostClaimType.RegistrationWebConfirmation,
                                StringValue = registrationWebConfirmation,
                                Post = post,
                                SiteId = AppContext.Site.Id
                            });
                        }
                    }
                    // Add\update the registration email confirmation...
                    if (string.IsNullOrWhiteSpace(registrationEmailConfirmation) == true && registrationEmailClaim != null)
                    {
                        // Delete registration email confirmation...
                        AppContext.AppDbContext.PostClaims.Remove(registrationEmailClaim);
                    }
                    else if (string.IsNullOrWhiteSpace(registrationEmailConfirmation) != true)
                    {
                        if (registrationEmailClaim != null)
                        {
                            registrationEmailClaim.StringValue = registrationEmailConfirmation;
                        }
                        else
                        {
                            AppContext.AppDbContext.PostClaims.Add(new PostClaim
                            {
                                Type = PostClaimType.RegistrationEmailConfirmation,
                                StringValue = registrationEmailConfirmation,
                                Post = post,
                                SiteId = AppContext.Site.Id
                            });
                        }
                    }
                    // Add\update the registration fields claim...
                    if (registrationFieldStringValue == null && registrationFieldClaim != null)
                    {
                        // Delete registration fields...
                        AppContext.AppDbContext.PostClaims.Remove(registrationFieldClaim);
                    }
                    else if (registrationFieldStringValue != null)
                    {
                        if (registrationFieldClaim != null)
                        {
                            registrationFieldClaim.StringValue = registrationFieldStringValue;
                        }
                        else
                        {
                            AppContext.AppDbContext.PostClaims.Add(new PostClaim
                            {
                                Type = PostClaimType.RegistrationField,
                                StringValue = registrationFieldStringValue,
                                Post = post,
                                SiteId = AppContext.Site.Id
                            });
                        }
                    }
                }
                post.ModifiedDate = DateTime.Now;
                // Change the state if the user don't have publication right...
                if (!(AppContext.User.HasRole(ClaimValueRole.Administrator) == true || AppContext.User.HasRole(ClaimValueRole.Publicator) == true))
                {
                    post.State = State.NotValided;
                    post.ValidationDate = null;
                }

                // Update history...
                _UpdateHistory(id, post, user, SiteActionType.Modification);

                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError("Failed to update post {0} dates and registration: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateDate::DB access failure");
                    return null;
                }
                // Update post root path...
                _UpdatePostRootPath(AppContext, post);

                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::UpdateDate");
                // Return the updated post...
                return post;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to update post {0} dates and registration: {1}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::UpdateDate::Exception");
                return null;
            }
        }

        /// <summary>
        /// Update post settings.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="regions"></param>
        /// <param name="categorys"></param>
        /// <param name="tags"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public async Task<Post> Update(int id, State state, IEnumerable<int> regions, IEnumerable<int> categorys, IEnumerable<int> tags, IEnumerable<int> groups)
        {
            try
            {
                Post post = null;
                bool publish = false;
                ApplicationUser user = null;
                IEnumerable<int> userGroups = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to update post {0} settings: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateSettings::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to update post {0} settings: Invalid authz provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateSettings::Invalid authz provider");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError("Failed to update post {0} settings: Invalid user.", id);
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateSettings::Invalid user");
                    return null;
                }

                // Retrieve the specified post...
                if ((post = await Get(id)) == null)
                {
                    _Log?.LogError("Failed to update post {0} settings: Invalid post id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateSettings::Invalid post id");
                    return null;
                }

                // Check for authorization...
                post.RequestSite = AppContext.Site;
                // Check for publich authorization...
                if ((publish = ((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    post,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Publish
                        }
                    }))?.Succeeded ?? false)) == false)
                {
                    // Check for update authorization...     
                    if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                        post,
                        new List<OperationAuthorizationRequirement>()
                        {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                        }))?.Succeeded ?? false) == false)
                    {
                        _Log?.LogError("Failed to update post {0} settings: Access denied.", id);
                        LastError = UserMessage.AccessDenied;
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PostProvider::UpdateSettings::Access denied");
                        return null;
                    }
                }

                // Change the post state...
                if (publish == true)
                {
                    post.State = state;
                    if (post.State == State.Valided)
                    {
                        post.ValidationDate = DateTime.Now;
                    }
                }
                else
                {
                    // Don't change the state of the post when a contributor change regions,
                    // cats, tags and groups.
                }
#               if !DENORMALIZE
                throw new Exception("Not implemented!!!");
#               else
                // Change the post regions...
                post.PostRegions(regions);
                // Change the post categories...
                post.PostCategorys(categorys);
                // Change the post tags...
                post.PostTags(tags);
                // Change the post group and private information only if the user is member of all post groups...
                userGroups = AppContext.User.Filter(groups);
                if ((userGroups != null && userGroups.Count() == groups.Count())
                    || AppContext.User.MemberOfAllGroup() == true)
                {
                    // Change the post group...
                    post.PostGroups(userGroups);
                    // Change post private information...
                    post.Private = (userGroups.Count() > 0) ? true : false;
                }
#               endif
                post.ModifiedDate = DateTime.Now;

                // Update history...
                _UpdateHistory(id, post, user, SiteActionType.Modification);

                // Commit DB changes...
                if (await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError("Failed to update post {0} settings: DB access failure.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::UpdateSettings::DB access failure");
                    return null;
                }
                // Update post root path...
                _UpdatePostRootPath(AppContext, post);

                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::UpdateSettings");
                // Return the updated post...
                return post;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to update post {0} settings.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::UpdateSettings::Exception");
                return null;
            }
        }

        /// <summary>
        /// Add a file to the post.
        /// Always send the cover before the cropped cover.
        /// </summary>
        /// <param name="id">Post Id</param>
        /// <param name="fileName"></param>
        /// <param name="fileType"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> AddFile(int id, string fileName, int fileType, Stream file)
        {
            try
            {
                Post post = null;
                bool updateDb = false;
                ApplicationUser user = null;
                string ext = string.Empty;
                string type = PostFileType.File;
                PostFile dbFile = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Invalid context.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Invalid context");
                    return null;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Invalid authz provider.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Invalid authz provider");
                    return null;
                }
                else if (string.IsNullOrEmpty(fileName) == true || file == null)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Invalid inputs.");
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Invalid inputs");
                    return null;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Invalid user.");
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Invalid user");
                    return null;
                }                
                // Retrieve the specified post...
                else if ((post = await Get(id)) == null)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Invalid post id.");
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Invalid post id");
                    return null;
                }

                // Check for authorization...
                post.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    post,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Access denied.");
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Access denied");
                    return null;
                }
                // File cannot be added when the post is validated...
                if (post.State == State.Valided
                    && !(AppContext.User.HasRole(ClaimValueRole.Administrator) == true || AppContext.User.HasRole(ClaimValueRole.Publicator) == true))
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: Access denied (post already validated).");
                    LastError = UserMessage.AccessDeniedAlreadyValidated;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::Access denied, already validated");
                    return null;
                }

                // Get file extension...
                fileName = fileName.ToLower();
                ext = Path.GetExtension(fileName);

                // Add file to post...
                if (fileType == 1 || fileType == 2)
                {
                    // Delete previous cover bassed on the previous ext...
                    if (fileType == 1)
                    {
                        string oldCoverPath = post.GetFilePath(AppContext, "cover" + post.Cover);
                        string oldCroppedPath = post.GetFilePath(AppContext, "cover.crop" + post.Cover);
                        if (File.Exists(oldCoverPath) == true) File.Delete(oldCoverPath);
                        if (File.Exists(oldCroppedPath) == true) File.Delete(oldCroppedPath);
                    }
                    // Update cover extension...
                    if (fileType == 1 && post.Cover != ext)
                    {
                        post.Cover = ext;
                        updateDb = true;
                    }
                }
                else
                {
                    // Get file type...
                    if (PostFileType.Map.ContainsKey(ext) == true)
                    {
                        string[] typAndMime = PostFileType.Map[ext];
                        if ((typAndMime?.Count() ?? 0) == 2)
                        {
                            type = typAndMime[0];
                        }
                    }
                    // Add file to DB...
                    AppContext.AppDbContext.PostFiles.Add((dbFile = new PostFile
                    {
                        Type = type,
                        Title = fileName,
                        Creator = user,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Post = post,
                        SiteId = AppContext.Site.Id
                    }));
                    updateDb = true;
                }

                // Commit DB changes...
                if (updateDb == true && await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: DB access failure.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::DB access failure");
                    return null;
                }

                // Add file to FS...
                if (fileType == 1 || fileType == 2)
                {
                    fileName = ((fileType == 1) ? "cover" : "cover.crop") + ext;
                }
                else
                {
                    fileName = dbFile.Id.ToString() + ext;
                }
                string outPutFile = post.GetFilePath(AppContext, fileName);
                try
                {
                    post.InitDirectory(AppContext);
                    using (FileStream fileStream = System.IO.File.Create(outPutFile))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile");
                    // Return file url...
                    return  post.GetFileUrl(fileName);
                }
                catch(Exception edb)
                {
                    _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: {edb.Message}");
                    // Delete the db entry...
                    if (dbFile != null && dbFile.Id != 0)
                    {
                        await DeleteFile(post.Id, dbFile.Id);
                    }
                    LastError = UserMessage.UnexpectedError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::AddFile::ExceptionDb");
                    return null;
                }
            }
            catch (Exception e)
            {
                _Log?.LogError($"Failed to add file {fileName} of type {fileType} to post {id}: {e.Message}");
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::AddFile::Exception");
                return null;
            }
        }

        /// <summary>
        /// Delete a file from a post.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(int id, int fileId)
        {
            try
            {
                Post post = null;
                bool updateDb = false;
                string ext = null;
                ApplicationUser user = null;
                PostFile dbFile = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Invalid context.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }
                else if (AppContext.AuthzProvider == null)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Invalid authz provider.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }
                else if ((user = await AppContext.GetUser()) == null)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Invalid user.");
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }
                // Retrieve the specified post...
                else if ((post = await Get(id)) == null)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Invalid post id.");
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }
                else if (fileId != 0 && (dbFile = await AppContext.AppDbContext.PostFiles.FirstAsync(f => f.Id == fileId)) == null)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Invalid file id.");
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }

                // Check for authorization...
                post.RequestSite = AppContext.Site;
                if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    post,
                    new List<OperationAuthorizationRequirement>()
                    {
                        new OperationAuthorizationRequirement
                        {
                            Name = AuthorizationRequirement.Update
                        }
                    }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Access denied.");
                    LastError = UserMessage.AccessDenied;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                    return false;
                }
                // File cannot be delete when the post is validated...
                if (post.State == State.Valided
                    && !(AppContext.User.HasRole(ClaimValueRole.Administrator) == true || AppContext.User.HasRole(ClaimValueRole.Publicator) == true))
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: Access denied (post already validated).");
                    LastError = UserMessage.AccessDeniedAlreadyValidated;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::Access denied, already validated");
                    return false;
                }

                // Delete file from the DB...
                if (fileId == 0)
                {
                    // Remove cover...
                    ext = post.Cover;
                    if (post.Cover != null)
                    {
                        post.Cover = null;
                        updateDb = true;
                    }
                }
                else
                {
                    AppContext.AppDbContext.PostFiles.Remove(dbFile);
                    updateDb = true;
                }
                // Commit DB changes...
                if (updateDb == true && await AppContext.AppDbContext.SaveChangesAsync() == 0)
                {
                    _Log?.LogError($"Failed to delete file {fileId} from post {id}: : DB access failure.");
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::DeleteFile::DB access failure");
                    return false;
                }

                // Delete file from the FS...
                string fileName1 = null;
                string fileName2 = null;
                if (fileId == 0)
                {
                    // Remove cover from the FS...
                    if (ext != null)
                    {
                        fileName1 = "cover" + ext;
                        fileName2 = "cover.crop" + ext;
                    }
                }
                else if (dbFile.Url == null)
                {
                    // Remove the file from the FS...
                    fileName1 = dbFile.Id.ToString() + Path.GetExtension(dbFile.Title);
                }
                string outPutFile1 = (fileName1 == null) ? null : post.GetFilePath(AppContext, fileName1);
                string outPutFile2 = (fileName2 == null) ? null : post.GetFilePath(AppContext, fileName2);
                if (outPutFile1 != null && File.Exists(outPutFile1) == true) File.Delete(outPutFile1);
                if (outPutFile2 != null && File.Exists(outPutFile2) == true) File.Delete(outPutFile2);
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::AddFile");
                // Return...
                return true;
            }
            catch (Exception e)
            {
                _Log?.LogError($"Failed to delete file {fileId} from post {id}: {e.Message}.");
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::DeleteFile::Exception");
                return false;
            }
        }

        /// <summary>
        /// Convert filter got from a request.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public Dictionary<string, object> ConvertFilter(Dictionary<string, string> filters)
        {
            return PostAuthorizationHandler.ConvertFilter(AppContext, filters);
        }

        /// <summary>
        /// Process registration to post.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="registrationFields"></param>
        /// <returns></returns>
        public async Task<string> Registration(Post post, List<JsonPostRegistrationField> registrationFields)
        {
            return await Registration(post?.Id ?? 0, registrationFields);
        }

        /// <summary>
        /// Process registration to post.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="registrationFields"></param>
        /// <returns></returns>
        public async Task<string> Registration(int id, List<JsonPostRegistrationField> registrationFields)
        {
            try
            {
                Post post = null;
                string regMsg = string.Empty;
                string registrationMail = null;

                // Checking...
                if ((AppContext?.IsValid(false) ?? false) == false)
                {
                    _Log?.LogError("Failed to process registration to post {0}: Invalid context.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Registration::Invalid context");
                    return null;
                }
                else if (_emailSender == null)
                {
                    _Log?.LogError("Failed to process registration to post {0}: Invalid email provider.", id);
                    LastError = UserMessage.InternalError;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Registration::Invalid email provider");
                    return null;
                }
                // Retrieve the specified post...
                else if ((post = await Get(id)) == null)
                {
                    _Log?.LogError("Failed to process registration to post {0}: Invalid post id.", id);
                    LastError = UserMessage.InvalidInputs;
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PostProvider::Registration::Invalid post id");
                    return null;
                }
                string title = HtmlString.TitleReSize(post.Title, -1);
                string date = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                // Get registration web and email confirmation...
                List<PostClaim> mailConfirmation = post.GetClaims(PostClaimType.RegistrationEmailConfirmation)?.ToList();
                List<PostClaim> webConfirmation = post.GetClaims(PostClaimType.RegistrationWebConfirmation)?.ToList();

                // Process the registration...
                regMsg = $"Inscription à <a href=\"{AppContext.Site.GetRootUrl()}{post.GetUrl(AppContext)}\">{post.Title}</a>:<br/><br/>";
                foreach (JsonPostRegistrationField reg in registrationFields)
                {
                    regMsg += string.Format("<b>{0}</b>: {1}", reg.Title, reg.Value);
                    if (reg.Type == 3 || reg.Type == 5)
                    {
                        regMsg += string.Format(" - {0}", reg.Value2);
                    }
                    regMsg += "<br/>";
                    // Extract the email from the registration values...
                    if (reg.Type == 1 && Email.IsValidEmail_T(reg.Value) == true)
                    {
                        registrationMail = reg.Value;
                    }
                }

                // Send the registration information...
                await _emailSender.SendEmailAsync(await _GetRegistrationTo(post.PostRegions(AppContext)?.Select(pr => pr.Id)?.ToList()),
                    $"Nouvelle inscription - {title} - {date}",
                    regMsg);

                // Send the confirmation email...
                if (registrationMail != null && (mailConfirmation?.Count ?? 0) > 0
                    && string.IsNullOrEmpty(mailConfirmation[0].StringValue) == false)
                {
                    await _emailSender.SendEmailAsync(registrationMail,
                        $"Votre inscription - {title} - {date}",
                        mailConfirmation[0].StringValue);
                }

                // Return the web confirmation...
                if ((webConfirmation?.Count ?? 0) > 0
                    && string.IsNullOrEmpty(webConfirmation[0].StringValue) == false)
                {
                    regMsg = webConfirmation[0].StringValue + "<br/><br/><hr/><br/>" + regMsg;
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::Registration");
                // Return the updated post...
                return (string.IsNullOrWhiteSpace(regMsg) == true) ? null : regMsg;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to process registration to post {0}.", id, e.Message);
                LastError = UserMessage.UnexpectedError;
                // Trace performance and exit...
                AppContext?.AddPerfLog("PostProvider::Registration::Exception");
                return null;
            }
        }

        /// <summary>
        /// Update the post rooth path.
        /// </summary>
        /// <param name="appctx"></param>
        /// <param name="post"></param>
        private bool _UpdatePostRootPath(WcmsAppContext appctx, Post post)
        {
            // Checking...
            if (post == null || post.Site == null)
            {
                return false;
            }

            // Get the expected root path...
            string expectedRtPath = post.GetRootPath(appctx);
            string pvtRootPath = appctx?.HostingEnvironment?.WebRootFileProvider.GetFileInfo($"{post.Site.Id}{CRoute.RouteStaticFile_Post}")?.PhysicalPath;
            string pubRootPath = appctx?.HostingEnvironment?.WebRootFileProvider.GetFileInfo($"{post.Site.Id}{CRoute.RouteStaticFile_PostPub}")?.PhysicalPath;
            _Log?.LogDebug("_UpdatePostRootPath: expectedRtPath={0}.", expectedRtPath);
            _Log?.LogDebug("_UpdatePostRootPath: pvtRootPath={0}.", pvtRootPath);
            _Log?.LogDebug("_UpdatePostRootPath: pubRootPath={0}.", pubRootPath);
            if (expectedRtPath == null || pvtRootPath == null || pubRootPath == null)
            {
                return false;
            }
            else if (Directory.Exists(expectedRtPath) == true)
            {
                // The post path is as expected...
                return true;
            }
            else
            {
                // The post path is not as expected...
                bool expectedRtPathIsPub = expectedRtPath.Contains(pubRootPath);
                // Get current folder (opposite of the eexpected path)...
                string currentRtPath = (expectedRtPathIsPub == true)
                    ? expectedRtPath.Replace(pubRootPath, pvtRootPath)
                    : expectedRtPath.Replace(pvtRootPath, pubRootPath);
                _Log?.LogDebug("_UpdatePostRootPath: currentRtPath={0}.", currentRtPath);
                // Update depending on current\expected...
                if (Directory.Exists(currentRtPath) == false)
                {
                    // No file in post...
                    return true;
                }
                try
                {
                    // First create parent directory if needed...
                    if (expectedRtPath[expectedRtPath.Length - 1] == '\\' || expectedRtPath[expectedRtPath.Length - 1] == '/')
                    {
                        expectedRtPath = expectedRtPath.Remove(expectedRtPath.Length - 1, 1);
                        _Log?.LogDebug("_UpdatePostRootPath: expectedRtPath(2)={0}.", expectedRtPath);
                    }
                    DirectoryInfo expectedParentDir = Directory.GetParent(expectedRtPath);
                    _Log?.LogDebug("_UpdatePostRootPath: expectedParentDir={0}.", expectedParentDir);
                    if (Directory.Exists(expectedParentDir.FullName) == false)
                    {
                        Directory.CreateDirectory(expectedParentDir.FullName);
                    }
                    // Move the directory...
                    Directory.Move(currentRtPath, expectedRtPath);
                    return true;
                }
                catch(Exception e)
                {
                    _Log?.LogError("Failed to update post root folder. Exception: {0}.", e);
                    return false;
                }
            }
        }

        /// <summary>
        /// Update post history.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="post"></param>
        /// <param name="user"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private void _UpdateHistory(int id, Post post, ApplicationUser user, string action)
        {
            AppContext.AppDbContext.SiteActions.Add(new SiteAction()
            {
                Table = "Post",
                Element = post.Id.ToString(),
                Type = action,
                Actor = user,
                ActionDate = DateTime.Now,
                SiteId = AppContext.Site.Id
            });
        }

        /// <summary>
        /// TODO: Move to User provider!!!
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        private async Task<List<string>> _GetRegistrationTo(List<int> regions)
        {
            List<string> registrationTos = new List<string>();
            // Get user which have role to received inscription to post.
            var registrationDest = await AppContext?.AppDbContext?.UserClaims.Where(uc
                => uc.ClaimType == UserClaimType.Role && uc.ClaimValue == ClaimValueRole.PostRegistrationTo)
                .Select(uc => uc.UserId).ToListAsync();
            if (registrationDest != null && registrationDest.Count != 0)
            {
                var tos = await AppContext?.AppDbContext?.Users?.Where(u
                    => registrationDest.Contains(u.Id) && u.Enabled == true && u.EmailConfirmed == true)?.ToListAsync();
                if ((tos?.Count ?? 0) > 0) {
                    // Get user that match all region of the post...
                    foreach(ApplicationUser to in tos)
                    {
                        //if (to.HasRegions(regions) == true)
                        {
                            registrationTos?.Add(to.Email);
                        }
                    }
                }
            }
            // Send to domain admin and default registration mail too...
            string inscripion_default_from = AppContext.Configuration[$"{AppContext.Module.Name}:inscripion_default_from"];
            if (string.IsNullOrEmpty(inscripion_default_from) == false)
            {
                registrationTos?.Add(inscripion_default_from);
            }
            string email_from_address = AppContext.Configuration[$"{AppContext.Module.Name}:email_from_address"];
            if (string.IsNullOrEmpty(email_from_address) == false)
            {
                registrationTos?.Add(email_from_address);
            }

            return registrationTos;
        }
    }
}
