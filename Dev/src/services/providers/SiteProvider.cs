// Don't use the Include:
// After getting the site (with _Get) and update claims a next call to _Get will not reflects the changes apply to the data base...
#define GETCLAIMSWITHINCLUDE

using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Site provider.
    /// </summary>
    public class SiteProvider : BaseProvider
    {
        private static Dictionary<int, Site> _SiteIdCache = new Dictionary<int, Site>();
        private static Dictionary<string, int> _SiteDomainCache = new Dictionary<string, int>();

        ////var customer = new Customer { Id = 42 };
        ////var customerEntry = context.Entry(customer).GetService();
        //public static bool DontUseLinQMethod = false;

        /// <summary>
        /// Site provider constructor.
        /// </summary>
        /// <param name="appContext"></param>
        public SiteProvider(WcmsAppContext appContext)
            : base(appContext)
        {
        }

        ///// <summary>
        ///// Init the site provider.
        ///// </summary>
        //public static void Init()
        //{
        //    using (AppDbContext ctx = new AppDbContext())
        //    {
        //        List<Site> sites = ctx?.Sites.ToList();
        //        if (sites != null)
        //        {
        //            foreach(Site site in sites)
        //            {
        //                if (site != null)
        //                {
        //                    _SiteIdCache[site.Id] = site;
        //                    _SiteDomainCache[site.Domain] = site.Id;
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Get sites.
        ///// </summary>
        ///// <returns></returns>
        //public async Task<IEnumerable<Site>> Get()
        //{
        //    try
        //    {
        //        List<string> userGroupId = null;
        //        // Checking...
        //        if (AppContext == null 
        //            || AppContext.AppDbContext == null)
        //        {
        //            _Log?.LogError("Failed to get sites: Invalid contexts.");
        //            return null;
        //        }
        //        else if (AppContext.User != null
        //            && AppContext.User.HasRole(ClaimValueRole.Administrator) == true)
        //        {
        //            // Get user group ids...
        //            if ((userGroupId = AppContext.User.GetGroupsId()) == null)
        //            {
        //                _Log?.LogError("Failed to get sites: No user groups.");
        //                return null;
        //            }
        //            // Return site associated to the administrator group...
        //            else if (DontUseLinQMethod == true)
        //            {
        //                return await (from site in AppContext.AppDbContext.Sites
        //                                join siteClaim in AppContext.AppDbContext.SiteClaims on site.Id equals siteClaim.SiteId
        //                                where siteClaim.Type == UserClaimType.Group && userGroupId.Contains(siteClaim.Value.ToString()) == true
        //                                select site).ToListAsync();
        //            }
        //            else
        //            {
        //                return await AppContext.AppDbContext.Sites.Include(s => s.SiteClaims)
        //                    .Join(AppContext.AppDbContext.SiteClaims, s => s.Id, c => c.SiteId, (s, c) => new { s, c })
        //                    .Where(sc => sc.c.Type == UserClaimType.Group && userGroupId.Contains(sc.c.Value.ToString()) == true)
        //                    .Select(sc => sc.s).ToListAsync();
        //            }
        //        }
        //        else
        //        {
        //            // Not allowaed...
        //            return null;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _Log?.LogError($"Failed to get sites.", e);
        //        return null;
        //    }
        //}

        /// <summary>
        /// Provide if the last asked site exist:
        /// -1: Last get failed.
        /// 0: Last asked site doesn't exist.
        /// 1: Last asked site exist.
        /// </summary>
        public int Exist { get; set; }

        /// <summary>
        /// Get a site from domain.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task<Site> Get(string domain)
        {
            return await _Get(domain, null);
        }

        /// <summary>
        /// Get site from domain and context.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<Site> Get(string domain, HttpContext context)
        {
            if (context == null || context.Request == null || string.IsNullOrWhiteSpace(context.Request.Path) == true)
            {
                _Log?.LogError("Failed to get site: Invalid http context.");
                return null;
            }
            return await _Get(domain, null, $"{AuthorizationRequirement.Read}{context.Request.Path}");
        }

        /// <summary>
        /// Get a site from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        public async Task<Site> Get(int id)
        {
            return await _Get(null, id);
        }

        /// <summary>
        /// Get site claims.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SiteClaim>> GetClaims(string domain, string type = null)
        {
            try
            {
                IEnumerable<SiteClaim> claims = (await Get(domain))?.GetClaims(type);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::GetClaimsDomain");
                return claims;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get site {0} claims: {1}.", domain, e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::GetClaimsDomain::Exception");
                return null;
            }
        }

        /// <summary>
        /// Get site claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<IEnumerable<SiteClaim>> GetClaims(int id, string type = null)
        {
            try
            {
                IEnumerable<SiteClaim> claims = (await Get(id))?.GetClaims(type);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::GetClaimsId");
                return claims;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get site {0} claims: {1}.", id, e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::GetClaimsId::Exception");
                return null;
            }
        }

        /// <summary>
        /// Set site claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inputClaims"></param>
        /// <returns></returns>
        public async Task<bool> SetClaims(int id, IEnumerable<SiteClaim> inputClaims)
        {
            try
            {
                Site site = null;
                // Checking...
                if (inputClaims == null)
                {
                    _Log?.LogError("Failed to set site claims: Invalid claims.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::SetClaims::Invalid claims");
                    return false;
                }
                else if (AppContext == null
                    || AppContext.AppDbContext == null
                    || AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to set site claims: Invalid contexts.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::SetClaims::Invalid contexts");
                    return false;
                }
                // Retrieve the site...
                else if ((site = await Get(id)) == null)
                {
                    return false;
                }
                // Check for update authorization...
                else if (((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    site, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Update } }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to set site {0}({1}) claims: Access denied.", id, site?.Title);
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::SetClaims::Access denied");
                    return false;
                }

                // Add\update claims...
                foreach (SiteClaim inputClaim in inputClaims)
                {
                    if (inputClaim.Id == 0)
                    {
                        // Add as new claim...
                        inputClaim.Site = site;
                        AppContext.AppDbContext.SiteClaims.Add(inputClaim);
                    }
                    else
                    {
                        // Update an existing claim...
                        SiteClaim siteClaim =
                            site.GetClaim(inputClaim.Id);   //No db connection!
                            //AppContext.AppDbContext.SiteClaims.FirstOrDefault(stc => stc.Id == inputClaim.Id);  //This cause a DB connection!
                        if (siteClaim != null)
                        {
                            if (inputClaim.Value != null || inputClaim.StringValue != null)
                            {
                                // Update the claim value...
                                siteClaim.Value = inputClaim.Value;
                                siteClaim.StringValue = inputClaim.StringValue;
                            }
                            else
                            {
                                // Delete the claim...
                                AppContext.AppDbContext.SiteClaims.Remove(siteClaim);
                            }
                        }
                    }
                }

                // Commit changes...
                bool res = (await AppContext.AppDbContext.SaveChangesAsync() > 0/*== inputClaims.Count()*/) ? true : false;
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::SetClaims");
                return res;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to set site {0} claims: {1}.", id, e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::SetClaims::Exception");
                return false;
            }
        }

        /// <summary>
        /// Get a site from its domain or its id.
        /// </summary>
        /// <param name="exist"></param>
        /// <param name="domain"></param>
        /// <param name="id"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        private async Task<Site> _Get(string domain, int? id, string requirement = AuthorizationRequirement.Read)
        {
            try
            {
                Site site = null;
                // Init...
                Exist = -1;
                // Checking...
                if (AppContext == null
                    || AppContext.AppDbContext == null
                    || AppContext.AuthzProvider == null)
                {
                    _Log?.LogError("Failed to get site: Invalid contexts.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::_Get::Invalid contexts1");
                    return null;
                }
                else if (string.IsNullOrWhiteSpace(domain) == true && id == null)
                {
                    _Log?.LogError("Failed to get site: Invalid domain or id.");
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::_Get::Invalid domain or id");
                    return null;
                }
                Exist = 0;

                // Get the site from cache...
                if (domain != null && _SiteDomainCache.ContainsKey(domain) == true)
                {
                    int siteId = _SiteDomainCache[domain];
                    if (_SiteIdCache.ContainsKey(siteId) == true)
                    {
                        // Trace performance and exit...
                        AppContext?.AddPerfLog("SiteProvider::_Get::Cache");
                        site = _SiteIdCache[siteId];
                    }
                }
                else if (id != null && _SiteIdCache.ContainsKey(id.Value) == true)
                {
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::_Get::Cache");
                    site = _SiteIdCache[id.Value];
                }

                // If not in cache, query the DB to get the specified site...
                if (site == null)
                {
#                   if GETCLAIMSWITHINCLUDE
                    site = await ((id == null)
                        ? AppContext.AppDbContext.Sites.Include(s => s.SiteClaims).SingleOrDefaultAsync(s => s.Domain == domain)
                        : AppContext.AppDbContext.Sites.Include(s => s.SiteClaims).SingleOrDefaultAsync(s => s.Id == id.Value));
#                   else
                    site = await ((id == null)
                        ? AppContext.AppDbContext.Sites.SingleOrDefaultAsync(s => s.Domain == domain)
                        : AppContext.AppDbContext.Sites.SingleOrDefaultAsync(s => s.Id == id.Value));
                    if (site != null)
                    {
                        // Retrieve claims...
                        site.SiteClaims = await AppContext.AppDbContext.SiteClaims.Where(st => st.SiteId == site.Id)?.ToListAsync();
                    }
#                   endif

                    // Add to cache...
                    if (site != null)
                    {
                        if (domain != null)
                        {
                            _SiteDomainCache[domain] = site.Id;
                            _SiteIdCache[site.Id] = site;
                        }
                        else if (id != null)
                        {
                            _SiteIdCache[id.Value] = site;
                        }
                    }
                }
                // Save that the site exist...
                if (site != null)
                    Exist = 1;

                // Check authorizations...
                if (site != null && ((await AppContext.AuthzProvider.AuthorizeAsync(AppContext.UserPrincipal,
                    site, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = requirement } }))?.Succeeded ?? false) == false)
                {
                    _Log?.LogWarning("Failed to get site {0}|{1}: Access denied.", domain, id);
                    // Trace performance and exit...
                    AppContext?.AddPerfLog("SiteProvider::_Get::5");
                    return null;
                }

                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::_Get");
                return site;
            }
            catch (Exception e)
            {
                _Log?.LogError("Failed to get site {0}|{1}: {2}."/*, e*/, domain, id, e.Message);
                // Trace performance and exit...
                AppContext?.AddPerfLog("SiteProvider::_Get::Exception");
                return null;
            }
        }

        ///// <summary>
        ///// Get site claims.
        ///// </summary>
        ///// <param name="id"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //public async Task<IEnumerable<SiteClaim>> _GetClaims(int siteId, string type)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(type) == true)
        //        {
        //            return await AppContext.AppDbContext.SiteClaims.Where(
        //                st => st.SiteId == siteId).ToListAsync();
        //        }
        //        return await AppContext.AppDbContext.SiteClaims.Where(
        //            st => st.SiteId == siteId && st.Type == type).ToListAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        _Log?.LogError($"Failed to get site {siteId} claims.", e);
        //        return null;
        //    }
        //}
    }
}
