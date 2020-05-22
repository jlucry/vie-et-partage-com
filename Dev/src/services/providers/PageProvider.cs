// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE
#define INCLUDE_ON

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Page provider.
    /// </summary>
    public class PageProvider : BaseProvider
    {
        /// <summary>
        /// Page provider constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public PageProvider(WcmsAppContext appContext)
            : base(appContext)
        {
        }

        /// <summary>
        /// Get pages.
        /// </summary>
        /// <param name="onlyInMenu"></param>
        /// <param name="parent"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Page>> Get(bool onlyInMenu, int? parent, bool recursive = false)
        {
            try
            {
                // Checking...
                if ((AppContext?.IsValid() ?? false) == false)
                {
                    _Log?.LogError("Failed to get pages: Invalid contexts.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PageProvider::Gets::Invalid contexts");
                    return null;
                }
                // Return pages...
                IEnumerable<Page> pages = await PageAuthorizationHandler.Get(AppContext, onlyInMenu, parent);
                if (recursive == true
                    && pages != null && pages.Count() != 0)
                {
                    foreach (Page page in pages)
                    {
                        if (page != null)
                        {
                            page.Childs = await Get(onlyInMenu, page.Id, true);
                        }
                    }
                }
                // Trace performance and exit...
                AppContext?.AddPerfLog("PageProvider::Gets");
                return pages;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get pages: {0}.", e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("PageProvider::Gets::Exception"/* + e.Message + " | " + e.StackTrace*/);
                return null;
            }
        }

        /// <summary>
        /// Get a page from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Page> Get(string id)
        {
            int idValue = 0;
            // Checking...
            if (string.IsNullOrEmpty(id) == true
                || int.TryParse(id, out idValue) == false)
            {
                return null;
            }
            return await Get(idValue);
        }

        /// <summary>
        /// Get a page from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Page> Get(int id)
        {
            try
            {
                // Checking...
                if (AppContext == null
                    || AppContext.AppDbContext == null
                    || AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to get page: Invalid contexts.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("PageProvider::Get::Invalid contexts");
                    return null;
                }
                // Query the DB to get the specified page...
                Page page = await AppContext.AppDbContext.Pages
#                   if INCLUDE_ON
                    .Include(p => p.PageClaims)
#                   endif
                    .Include(p => p.Creator)
                    //.Include(p => p.Parent)
                    .Include(p => p.Site)
                    .SingleOrDefaultAsync(s => s.Id == id);
                if (page != null)
                {
                    // Provide the site on which we make the request...
                    page.RequestSite = AppContext.Site;
                    // Retrieve claims...
#                   if !DENORMALIZE
                    page.PageGroups = await AppContext.AppDbContext.PageGroups.Where(st => st.PageId == page.Id)?.ToListAsync();
                    page.PageRegions = await AppContext.AppDbContext.PageRegions.Where(st => st.PageId == page.Id)?.ToListAsync();
                    page.PageCategorys = await AppContext.AppDbContext.PageCategorys.Where(st => st.PageId == page.Id)?.ToListAsync();
                    page.PageTags = await AppContext.AppDbContext.PageTags.Where(st => st.PageId == page.Id)?.ToListAsync();
#                   endif
#                   if !INCLUDE_ON
                    page.PageClaims = await AppContext.AppDbContext.PageClaims.Where(st => st.PageId == page.Id)?.ToListAsync();
#                   endif
                    // Check for authorization...
                    if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                        page, new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read }))?.Succeeded ?? false) == false)
                    {
                        _Log?.LogWarning("Failed to get page {0}: Access denied.", id);
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("PageProvider::Get::Access denied");
                        return null;
                    }
                }
                // Trace performance and exit...
                AppContext?.AddPerfLog("PageProvider::Get");
                return page;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get page {0}: {1}.", id, e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("PageProvider::Get::Exception");
                return null;
            }
        }

        /// <summary>
        /// Set page claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inputClaims"></param>
        /// <returns></returns>
        public async Task<bool> SetClaims(int id, IEnumerable<PageClaim> inputClaims)
        {
            try
            {
                Page page = null;
                // Checking...
                if (inputClaims == null)
                {
                    _Log?.LogError("Failed to set page claims: Invalid claims.");
                    return false;
                }
                else if (AppContext == null
                    || AppContext.AppDbContext == null
                    || AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to set page claims: Invalid contexts.");
                    return false;
                }
                // Retrieve the page...
                else if ((page = await Get(id)) == null)
                {
                    return false;
                }
                // Check for update authorization...
                else if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    page, new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Update }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to set page {0}({1}) claims: Access denied.", id, page.Title);
                    return false;
                }

                // Add\update claims...
                foreach (PageClaim inputClaim in inputClaims)
                {
                    if (inputClaim.Id == 0)
                    {
                        // Add as new claim...
                        inputClaim.Page = page;
                        AppContext.AppDbContext.PageClaims.Add(inputClaim);
                    }
                    else
                    {
                        // Update an existing claim...
                        PageClaim pageClaim =
                            page.GetClaim(inputClaim.Id);   //No db connection!
                                                            //AppContext.AppDbContext.PageClaims.FirstOrDefault(stc => stc.Id == inputClaim.Id);  //This cause a DB connection!
                        if (pageClaim != null)
                        {
                            if (inputClaim.Value != null || inputClaim.StringValue != null)
                            {
                                // Update the claim value...
                                pageClaim.Value = inputClaim.Value;
                                pageClaim.StringValue = inputClaim.StringValue;
                            }
                            else
                            {
                                // Delete the claim...
                                AppContext.AppDbContext.PageClaims.Remove(pageClaim);
                            }
                        }
                    }
                }

                // Commit changes...
                return (await AppContext.AppDbContext.SaveChangesAsync() > 0/*== inputClaims.Count()*/) ? true : false;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to set page {0} claims: {1}.", id, e.Message);
                return false;
            }
        }
    }
}
