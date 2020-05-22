// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Models;
using Remotion.Linq.Parsing.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Core;
using Xunit;
using Xunit.Abstractions;

namespace Services.UnitTests
{
    /// <summary>
    /// WI 372: Lister les posts.
    /// WI 370: Ajouter un post.
    /// </summary>
    public class UPostProvider : UBaseProvider
    {
        /// <summary>
        /// WI 372: Lister les posts.
        /// WI 370: Ajouter un post
        /// </summary>
        public UPostProvider(ITestOutputHelper output)
            : base(output)
        {
        }

#       if !DENORMALIZE
        [Fact]
        public async Task JoinTest()
        {
            LogMaxLevel = 2;
            onlyUtestLog = false;

            using (AppDbContext dbctx = CreateDbContext())
            {
                WcmsAppContext ctx = await CreateAndInitContext(dbctx, "", "/", null, null, false);
                if (ctx != null)
                {
                    var query = dbctx.Posts.Join(dbctx.PostGroups, p => p.Id, g => g.PostId, (p, g) => new { p, g })
                        .Where(pg => pg.g.GroupId == 1 || pg.g.GroupId == 2)
                        .Select(pg => pg.p);
                    List<Post> posts = await query.ToListAsync();
                    _Log(1, ctx, "======================================");
                    foreach (Post post in posts)
                    {
                        _Log(1, ctx, $"Post: {post.Id} - {post.Title}.");
                    }

                    List<int> userGroupIds = new List<int> { 1, 2 };
                    var query2 = dbctx.Posts.GroupJoin(dbctx.PostGroups/*.Where(g => g.GroupId == 1 || g.GroupId == 2)*/, p => p.Id, g => g.PostId, (p, g) => new { p, g })
                        .Where(pg => pg.g.Count(g => userGroupIds.Contains(g.GroupId)/*g.GroupId == 1*/) != 0)
                        .Select(pg => pg.p);
                    List<Post> posts2 = await query2.ToListAsync();
                    _Log(1, ctx, "======================================");
                    foreach (Post post in posts2)
                    {
                        _Log(1, ctx, $"Post2: {post.Id} - {post.Title}.");
                    }
                }
            }
        }
#       endif

        //[Fact]
        //public async Task QueryTest()
        //{
        //    LogMaxLevel = 2;
        //    onlyUtestLog = false;

        //    using (AppDbContext dbctx = CreateAndCheckDbContext())
        //    {
        //        WcmsAppContext ctx = await CreateAndInitAppContext(dbctx, "", "/", null, null, false);
        //        if (ctx != null)
        //        {
        //            int pId = 0;
        //            var query = dbctx.Posts.Where(p => p.Id == pId);

        //            // https://github.com/aspnet/EntityFramework/issues/4417
        //            //using (var db = new BloggingContext())
        //            //{
        //            //    var serviceProvider = ((IInfrastructure<IServiceProvider>)db).Instance;
        //            //    var database = (IDatabase)serviceProvider.GetService(typeof(IDatabase));
        //            //    var queryContextFactory = (IQueryContextFactory)serviceProvider.GetService(typeof(IQueryContextFactory));
        //            //    var query = new EnumerableQuery<Blog>(new List<Blog>())
        //            //        .Where(x => x.Url != "")
        //            //        .SelectMany(x => x.Posts);
        //            //    var queryModel = QueryParser.CreateDefault().GetParsedQuery(query.Expression);
        //            //    var queryProvider = (IAsyncQueryProvider)serviceProvider.GetService(typeof(IAsyncQueryProvider));
        //            //    var fromExpression = Expression.Constant(new EntityQueryable<Blog>(queryProvider));
        //            //    queryModel.MainFromClause.FromExpression = fromExpression;
        //            //    var compileQueryFn = database.CompileQuery<Post>(@queryModel);
        //            //    var result = compileQueryFn(queryContextFactory.Create()).ToList();
        //            //}
        //            var queryModel = QueryParser.CreateDefault().GetParsedQuery(query.Expression);
        //            //////////
        //            pId = 1;
        //            var res1 = query?.FirstOrDefault();
        //            pId = 2;
        //            var res2 = query?.FirstOrDefault()
        //        }
        //    }
        //}

        /// <summary>
        /// WI 372: Lister les posts.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UGet_Params()
        {
            PostProvider provider = null;
            // Invalid application context...
            {
                provider = new PostProvider(null);
                Assert_Equal<IEnumerable<Post>>(null, null, await provider.Get(new Dictionary<string, object>()), "Invalid context test failed (1).");
                provider = new PostProvider(new WcmsAppContext(null, null, null, null, null, null, null));
                Assert_Equal<IEnumerable<Post>>(null, null, await provider.Get(new Dictionary<string, object>()), "Invalid context test failed (2).");
            }
            // Invalid query take...
            {
                SiteData siteDt = await DbUtil.GetSiteData(1);
                // Search for admin with two groups...
                ApplicationUser admin = null;
                foreach (ApplicationUser user in siteDt.users)
                {
                    if (user.HigherRole() == ClaimValueRole.Administrator
                        && (user.GroupsId()?.Count ?? 0) >= 2
                        && user.SiteId == siteDt.site.Id)
                    {
                        admin = user;
                    }
                }
                Assert.NotEqual(null, admin);
                WcmsAppContext ctx = await CreateAndInitAppContext(null, siteDt.site.Domain, "/",
                    "All", admin, false);
                provider = new PostProvider(ctx);
                IEnumerable<Post> post = await provider.Get(null, 0, 201);
                Assert_NotEqual<IEnumerable<Post>>(ctx, null, post, "Take test failed (1).");
                Assert_Equal<int>(ctx, 200, post.Count(), "Take test failed(2).");
            }
        }

        /// <summary>
        /// WI 372: Lister les posts.
        /// Test get pages for a specified region and for an anonymous user.
        /// Test filters:
        ///     * region
        ///     * onlyInMenu
        ///     * parent
        /// </summary>
        /// <param name="regionName"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task UGet(int siteId)
        {
            LogMaxLevel = 1;
            onlyUtestLog = true;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;

            int totalTest = 0;
            int totalTestQuery = 0;
            SiteData siteDt = await DbUtil.GetSiteData(siteId, true);
            foreach (SiteClaim region in siteDt.regions)
            {
                foreach (ApplicationUser user in siteDt.users)
                {
                    List<int> groupFilters = DbUtil.GetUserGroupsFilter(user);
                    WcmsAppContext ctx = await CreateAndInitAppContext(null, siteDt.site.Domain, "/", region.StringValue, user, false);
                    if (ctx != null)
                    {
                        PostProvider provider = new PostProvider(ctx);
                        foreach (int groupFilter in groupFilters)
                        {
                            foreach (SiteClaim category in (ctx?.Site?.GetCategories() ?? siteDt.nullSiteCats))
                            {
                                foreach (SiteClaim tag in (ctx?.Site?.GetTags() ?? siteDt.nullSiteTags))
                                {
                                    foreach (State state in Enum.GetValues(typeof(State)))
                                    {
                                        foreach (bool? highlight in new List<bool?> { null, false, true })
                                        {
                                            foreach (bool mine in new List<bool> { false, true })
                                            {
                                                int skip = 0;
                                                int take = 200;
                                                int count = 0;
                                                string testDesc1 = $"site={siteId}, region={region.StringValue}, user={user?.UserName ?? string.Empty}, group={groupFilter}, category={category.StringValue}, tag={tag.StringValue}, state={state}, highlight={highlight} |";
                                                _Log(1, ctx, $">>UGet: {++totalTestQuery}: {testDesc1}");
                                                while (true)
                                                {
                                                    string testDesc2 = $"from {skip * take}({++totalTestQuery})";
                                                    _Log(2, ctx, $"  >Posts {testDesc2}...");
                                                    if (totalTestQuery == 5690)
                                                    {
                                                        int brk = 0;
                                                    }
                                                    IEnumerable<Post> posts = await provider?.Get(new Dictionary<string, object>
                                                    {
                                                        { QueryFilter.Mine, mine },
                                                        { QueryFilter.Highlight, highlight },
                                                        { QueryFilter.State, state },
                                                        { QueryFilter.CategorieSingle, category.Id },
                                                        { QueryFilter.TagSingle, tag.Id },
                                                        { (groupFilter == -123) ? "NullGroup" : QueryFilter.Group, groupFilter },
                                                        //{ QueryFilter.Title, title },
                                                        //{ QueryFilter.StartDate, startDate },
                                                        //{ QueryFilter.EndDate, endDate },
                                                    }, skip, take, true);
                                                    if (user != null && DbUtil.IsSiteUser(siteId, user) == false)
                                                    {
                                                        // User extern to the site: cannot view posts.
                                                        Assert_Equal<IEnumerable<Post>>(ctx, null, posts, $"Check extern to the site failed: {testDesc2}: {testDesc1}");
                                                        break;
                                                    }
                                                    else if ((user == null || user.HasRoles() == false || user.HasRole(ClaimValueRole.Reader) == true
                                                        || user.HasRole(ClaimValueRole.Contributor) == true) && state != State.Valided)
                                                    {
                                                        // Guest and reader: cannot view non validated posts.
                                                        Assert_Equal<int>(ctx, 0, (posts?.Count() ?? 0), $"Check guest, reader and contributeur failed: {testDesc2}: {testDesc1}");
                                                        break;
                                                    }
                                                    else if (mine == true && (user == null || user.HasRoles() == false || user.HasRole(ClaimValueRole.Reader) == true))
                                                    {
                                                        // Guest and reader: cannot view mine post.
                                                        Assert_Equal<int>(ctx, 0, (posts?.Count() ?? 0), $"Check mine for guest, reader failed: {testDesc2}: {testDesc1}");
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        Assert_NotEqual<IEnumerable<Post>>(ctx, null, posts, $"No posts: {testDesc2}: {testDesc1}");
                                                        count += posts.Count();
                                                        foreach (Post post in posts)
                                                        {
                                                            string testDesc3 = $"post {post?.Id ?? 0}({totalTestQuery}-{++totalTest})({post?.Title ?? "null"})";
                                                            string testDescT = $"{testDesc3} {testDesc2}: {testDesc1}";
                                                            Assert_NotEqual<Post>(ctx, null, post, $"Null post: {testDescT}");
                                                            _Log(2, ctx, $"  >Checking {testDesc3}...");
                                                            post.RequestSite = ctx.Site;
                                                            if (totalTest == 10166)
                                                            {
                                                                int brk = 0;
                                                            }

                                                            _Log(4, ctx, "     Check authorization...");
                                                            Assert_Equal<bool>(ctx, true,
                                                                ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false),
                                                                $"Check authorization failed: {testDescT}");

                                                            _Log(4, ctx, "     Check site ID...");
                                                            Assert_Equal<int>(ctx, 1, post.SiteId, $"Check site ID failed: {testDescT}");
                                                            _Log(4, ctx, "     Check filter on region...");
                                                            Assert_Equal<bool>(ctx, true, (post.Region1 == 0 || post.Region1 == ctx.Region.Id)
                                                                || (post.Region2 == 0 || post.Region2 == ctx.Region.Id)
                                                                || (post.Region3 == 0 || post.Region3 == ctx.Region.Id)
                                                                || (post.Region4 == 0 || post.Region4 == ctx.Region.Id)
                                                                || (post.Region5 == 0 || post.Region5 == ctx.Region.Id)
                                                                || (post.Region6 == 0 || post.Region6 == ctx.Region.Id)
                                                                || (post.Region7 == 0 || post.Region7 == ctx.Region.Id)
                                                                || (post.Region8 == 0 || post.Region8 == ctx.Region.Id)
                                                                || (post.Region9 == 0 || post.Region9 == ctx.Region.Id)
                                                                || (post.Region10 == 0 || post.Region10 == ctx.Region.Id), $"Check filter on region failed: {testDescT}");
                                                            _Log(4, ctx, "     Check filter on category...");
                                                            Assert_Equal<bool>(ctx, true, (post.Category1 == category.Id)
                                                                || (post.Category2 == category.Id)
                                                                || (post.Category3 == category.Id)
                                                                || (post.Category4 == category.Id)
                                                                || (post.Category5 == category.Id)
                                                                || (post.Category6 == category.Id)
                                                                || (post.Category7 == category.Id)
                                                                || (post.Category8 == category.Id)
                                                                || (post.Category9 == category.Id)
                                                                || (post.Category10 == category.Id), $"Check filter on category failed: {testDescT}");
                                                            _Log(4, ctx, "     Check filter on tag...");
                                                            Assert_Equal<bool>(ctx, true, (post.Tag1 == tag.Id)
                                                                || (post.Tag2 == tag.Id)
                                                                || (post.Tag3 == tag.Id)
                                                                || (post.Tag4 == tag.Id)
                                                                || (post.Tag5 == tag.Id)
                                                                || (post.Tag6 == tag.Id)
                                                                || (post.Tag7 == tag.Id)
                                                                || (post.Tag8 == tag.Id)
                                                                || (post.Tag9 == tag.Id)
                                                                || (post.Tag10 == tag.Id), $"Check filter on tag failed: {testDescT}");
                                                            if (groupFilter != -123)
                                                            {
                                                                _Log(4, ctx, "     Check filter on group...");
                                                                Assert_Equal<bool>(ctx, true, (post.Group1 == groupFilter)
                                                                    || (post.Group2 == groupFilter)
                                                                    || (post.Group3 == groupFilter)
                                                                    || (post.Group4 == groupFilter)
                                                                    || (post.Group5 == groupFilter)
                                                                    || (post.Group6 == groupFilter)
                                                                    || (post.Group7 == groupFilter)
                                                                    || (post.Group8 == groupFilter)
                                                                    || (post.Group9 == groupFilter)
                                                                    || (post.Group10 == groupFilter), $"Check filter on group failed: {testDescT}");
                                                            }
                                                            _Log(4, ctx, "     Check filter on state (depens on user group and roles)...");
                                                            Assert_Equal<State>(ctx, state, post.State, $"Check filter on state failed: {testDescT}");
                                                            if (highlight != null)
                                                            {
                                                                _Log(4, ctx, "     Check filter on higlight...");
                                                                Assert_Equal<bool>(ctx, highlight.Value, post.Highlight, $"Check filter on higlight failed: {testDescT}");
                                                            }
                                                            if (mine == true)
                                                            {
                                                                _Log(4, ctx, "     Check filter on mine...");
                                                                Assert_Equal<string>(ctx, user.Id, post.CreatorId, $"Check filter on mine failed: {testDescT}");
                                                            }
                                                        }
                                                        if (posts.Count() == 0)
                                                        {
                                                            break;
                                                        }
                                                        skip += 1;
                                                    }
                                                }
                                                _Log(3, ctx, $"<< {count} post.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// WI 372: Lister les posts.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task UAuthorisations(int siteId)
        {
            LogMaxLevel = 2;
            onlyUtestLog = false;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;

            int totalTestquery = 0;
            int totalTestNullCheck = 0;
            int totalTest = 0;
            SiteData siteDt = await DbUtil.GetSiteData(siteId, true);
            using (AppDbContext dbctx = CreateAndCheckDbContext())
            {
                int totalPost = await dbctx.Posts.CountAsync();
                foreach (SiteClaim region in siteDt.regions) // Not usefull - get the first region.
                {
                    foreach (ApplicationUser user in siteDt.users)
                    {
                        string role = user.HigherRole();
                        WcmsAppContext ctx = await CreateAndInitAppContext(null/*dbctx*/, siteDt.site.Domain, "/", region.StringValue, user, false);
                        if (ctx != null)
                        {
                            int skip = 0;
                            int take = 200;
                            int count = 0;
                            string testDesc1 = $"region={region.StringValue}, user={user?.UserName} ({role}) ({++totalTestquery})";
                            _Log(1, ctx, $">>UAuthorisations: {totalTestquery}: Checking authz for {testDesc1}...");
                            while (true)
                            {
                                string testDesc2 = $"from {skip * take}({++totalTestquery})";
                                _Log(2, ctx, $"  >Posts {testDesc2}...");
                                List<Post> posts = await dbctx.Posts.Skip(skip * take).Take(take)
#                                   if !DENORMALIZE
                                    .Include(p => p.PostGroups)
#                                   endif
                                    .Include(p => p.Site)
                                    .AsNoTracking()
                                    .ToListAsync();
                                if (posts == null || posts.Count == 0)
                                {
                                    // Will assert if we don't test all the existing pages.
                                    Assert_Equal<int>(ctx, totalPost, count, $"Total failed: {testDesc2}: {testDesc1}");
                                    break;
                                }
                                count += posts.Count;

                                foreach (Post post in posts)
                                {
                                    bool authorized = false;
                                    bool addAuthorized = false;
                                    bool updateAuthorized = false;
                                    bool publishAuthorized = false;
                                    Assert_NotEqual<Post>(ctx, null, post, $"Null post: {totalTestquery}-{++totalTestNullCheck}: {testDesc2}: {testDesc1}");
                                    for (int i = 0; i < 4; i += 1)
                                    {
                                        string testDesc3 = $"post {post?.Id ?? 0}({totalTestquery}-{totalTestNullCheck}-{++totalTest})({post?.Title ?? "null"})";
                                        string testDescT = $"{testDesc3} {testDesc2}: {testDesc1}";
                                        _Log(3, ctx, $"    Checking({i}) {testDesc3}...");
                                        if (totalTest == 34562)
                                        {
                                            int brk = 0;
                                        }
                                        // Play with the RequestSite to invole the site membership checks in the authorization module.
                                        switch (i)
                                        {
                                            case 0:
                                                // Simulate case where the user don't have access to the site.
                                                post.RequestSite = null;
                                                break;
                                            case 1:
                                                // Can be like if a user was able to log into a site on which he's not register.
                                                post.RequestSite = siteDt.site;
                                                break;
                                            case 2:
                                                // Force the request site to be the site of the post.
                                                post.RequestSite = post.Site;
                                                break;
                                        }

                                        // Compute authorization...
                                        {
                                            if (post.Site == null || post.RequestSite == null)
                                            {
                                                // We need to have the post site and the site requesting the post.
                                                authorized = false;
                                            }
                                            else if (post.CreatorId == null)
                                            {
                                                // A post must have a creator.
                                                authorized = false;
                                            }
                                            else if (post.SiteId != post.RequestSite.Id)
                                            {
                                                // A site cannot request posts of another site.
                                                authorized = false;
                                            }
                                            else if (post.RequestSite.Private == true && post.Private == false)
                                            {
                                                // No public post in a private site.
                                                authorized = false;
                                            }
                                            else if (user == null)
                                            {
                                                // Published and public posts are granted to anonymous.
                                                if (post.State == State.Valided && post.Private == false)
                                                {
                                                    authorized = true;
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                            }
                                            else if (DbUtil.IsSiteUser(post.SiteId, user) == false)
                                            {
                                                // Post can be see only by user of the same site.
                                                authorized = false;
                                            }
                                            // Authenticated users can see only public post and post of theirs groups.
#                                           if !DENORMALIZE
                                            else if (post.Private == true && DbUtil.MemberOf(user, post?.PostGroups) == false)
#                                           else
                                            else if (post.Private == true && user.MemberOf(post) == false)
#                                           endif
                                            {
                                                authorized = false;
                                            }
                                            else if (role == ClaimValueRole.Administrator || role == ClaimValueRole.Publicator)
                                            {
                                                // Administrator and publicator have all rights.
                                                authorized = true;
                                                if (user.MemberOfAll(post) == true)
                                                {
                                                    addAuthorized = true;
                                                    updateAuthorized = true;
                                                    publishAuthorized = true;
                                                }
                                            }
                                            else if (role == ClaimValueRole.Contributor)
                                            {
                                                // Contributeur can read their post and published posts
                                                if (post.State == State.Valided)
                                                {
                                                    authorized = true;
                                                }
                                                else if (post.CreatorId == user.Id)
                                                {
                                                    authorized = true;
                                                    updateAuthorized = true;
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                                if (user.MemberOfAll(post) == true)
                                                {
                                                    addAuthorized = true;
                                                }
                                            }
                                            else if (role == ClaimValueRole.Reader)
                                            {
                                                // Reader can only read published post.
                                                if (post.State == State.Valided)
                                                {
                                                    authorized = true;
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                            }
                                            else
                                            {
                                                // User without role can only read published post.
                                                if (post.State == State.Valided)
                                                {
                                                    authorized = true;
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                            }
                                        }

                                        // Get provider authorization...
                                        bool authorizedMod = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false);
                                        bool authorizedModAdd = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Add } }))?.Succeeded ?? false);
                                        bool authorizedModUpdate = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Update } }))?.Succeeded ?? false);
                                        bool authorizedModPublish = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, post, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Publish } }))?.Succeeded ?? false);

                                        Assert_Equal<bool>(ctx, authorized, authorizedMod, $"Read authz failure: {testDescT}");
                                        Assert_Equal<bool>(ctx, addAuthorized, authorizedModAdd, $"Add authz failure: {testDescT}");
                                        Assert_Equal<bool>(ctx, updateAuthorized, authorizedModUpdate, $"Update authz failure: {testDescT}");
                                        Assert_Equal<bool>(ctx, publishAuthorized, authorizedModPublish, $"Publish authz failure: {testDescT}");
                                    }
                                }

                                skip += 1;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// WI 370: Ajouter un post.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(1)]
        public async Task UUpdateAuthorisation(int siteId)
        {
            LogMaxLevel = 2;
            onlyUtestLog = true;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;

            string textPrefix = "UUpdate_";
            // Remove all post starting with "UUpdate_"...
            using (AppDbContext dbctxClean = CreateAndCheckDbContext())
            {
                List<Post> postsClean = await dbctxClean.Posts
#                   if !DENORMALIZE
                    .Include(p => p.PostGroups)
                    .Include(p => p.PostRegions)
                    .Include(p => p.PostCategorys)
                    .Include(p => p.PostTags)
#                   endif
                    .Include(p => p.PostClaims)
                    .Include(p => p.PostFiles)
                    .Include(p => p.PostTexts)
                    .Where(p => p.Title.StartsWith(textPrefix) == true)
                    .AsNoTracking()
                    .ToListAsync();
                if (postsClean != null && postsClean.Count > 0)
                {
                    dbctxClean.Posts.RemoveRange(postsClean);
                    Assert.NotEqual(0, await dbctxClean.SaveChangesAsync());
                }
            }

            // Add test...
            int postAddedCount = 0;
            using (AppDbContext dbctx = CreateAndCheckDbContext())
            {
                int iDx = 0;
                SiteData siteDt = await DbUtil.GetSiteData(siteId, true, dbctx);
                foreach (ApplicationUser user in siteDt.users)
                {
                    string role = user.HigherRole();
                    WcmsAppContext ctx = await CreateAndInitAppContext(dbctx, siteDt.site.Domain, "/", siteDt.regions[0].StringValue, user, false);
                    if (ctx != null)
                    {
                        _Log(1, ctx, $">>Checking add authorization for user={user?.UserName} ({role})...");

                        PostProvider provider = new PostProvider(ctx);
                        Assert.NotEqual(null, provider);

                        // Add a new post...
                        string postTitle = $"{textPrefix}Add_Title_{iDx++}_By_{user?.UserName}";
                        string postText = $"{textPrefix}Add_Text_{iDx++}_By_{user?.UserName}";
                        Post post = await provider.Update(0, postTitle, postText);
                        // Verify add authorization...
                        if (user == null)
                        {
                            // Anonymous user cannot add post.
                            Assert.Equal(null, post);
                        }
                        else if (DbUtil.IsSiteUser(siteId, user) == false)
                        {
                            // Post can be added only by user of the same site.
                            Assert.Equal(null, post);
                        }
                        else if (role == ClaimValueRole.Administrator || role == ClaimValueRole.Publicator)
                        {
                            // Administrator and publicator have all rights.
                            Assert.NotEqual(null, post);
                        }
                        else if (role == ClaimValueRole.Contributor)
                        {
                            // Contributeur can add posts.
                            Assert.NotEqual(null, post);
                        }
                        else
                        {
                            // Reader and other users cannot add post.
                            Assert.Equal(null, post);
                        }
                        // Check added data...
                        if (post != null)
                        {
                            postAddedCount += 1;
                            Assert.NotEqual(0, post.Id);
                            using (AppDbContext dbctx2 = CreateAndCheckDbContext())
                            {
                                Post post2 = await dbctx2.Posts
#                                   if !DENORMALIZE
                                    .Include(p => p.PostRegions)
#                                   endif
                                    //.Include(p => p.Creator)
                                    //.Include(p => p.Site)
                                    .FirstOrDefaultAsync(p => p.Id == post.Id);
                                Assert.NotEqual(null, post2);
                                Assert.Equal(ctx.Site.Private, post2.Private);
                                Assert.Equal(user.Id, post2.CreatorId);
                                Assert.Equal(0, (int)post2.CreationDate.Subtract(DateTime.Now).TotalDays);
                                Assert.Equal(ctx.Site.Id, post2.SiteId);
                                Assert.Equal(postTitle, post2.Title);
                                Assert.NotEqual(null, post2.ModifiedDate);
                                Assert.Equal(0, (int)post2.ModifiedDate.Value.Subtract(DateTime.Now).TotalDays);
                                Assert.Equal(State.NotValided, post2.State);
                                Assert.Equal(null, post2.ValidationDate);
                                Assert.Equal(null, post2.StartDate);
                                Assert.Equal(null, post2.EndDate);
#                               if !DENORMALIZE
                                Assert.NotEqual(null, post2.PostRegions);
                                Assert.Equal(1, post2.PostRegions.Count);
                                Assert.Equal(siteDt.regions[0].Id, post2.PostRegions.FirstOrDefault().RegionId);
#                               else
                                Assert.Equal(-1, post2.Group1);
                                Assert.Equal(-1, post2.Group2);
                                Assert.Equal(-1, post2.Group3);
                                Assert.Equal(-1, post2.Group4);
                                Assert.Equal(-1, post2.Group5);
                                Assert.Equal(-1, post2.Group6);
                                Assert.Equal(-1, post2.Group7);
                                Assert.Equal(-1, post2.Group8);
                                Assert.Equal(-1, post2.Group9);
                                Assert.Equal(-1, post2.Group10);
                                Assert.Equal(siteDt.regions[0].Id, post2.Region1);
                                Assert.Equal(-1, post2.Region2);
                                Assert.Equal(-1, post2.Region3);
                                Assert.Equal(-1, post2.Region4);
                                Assert.Equal(-1, post2.Region5);
                                Assert.Equal(-1, post2.Region6);
                                Assert.Equal(-1, post2.Region7);
                                Assert.Equal(-1, post2.Region8);
                                Assert.Equal(-1, post2.Region9);
                                Assert.Equal(-1, post2.Region10);
                                Assert.Equal(-1, post2.Category1);
                                Assert.Equal(-1, post2.Category2);
                                Assert.Equal(-1, post2.Category3);
                                Assert.Equal(-1, post2.Category4);
                                Assert.Equal(-1, post2.Category5);
                                Assert.Equal(-1, post2.Category6);
                                Assert.Equal(-1, post2.Category7);
                                Assert.Equal(-1, post2.Category8);
                                Assert.Equal(-1, post2.Category9);
                                Assert.Equal(-1, post2.Category10);
                                Assert.Equal(-1, post2.Tag1);
                                Assert.Equal(-1, post2.Tag2);
                                Assert.Equal(-1, post2.Tag3);
                                Assert.Equal(-1, post2.Tag4);
                                Assert.Equal(-1, post2.Tag5);
                                Assert.Equal(-1, post2.Tag6);
                                Assert.Equal(-1, post2.Tag7);
                                Assert.Equal(-1, post2.Tag8);
                                Assert.Equal(-1, post2.Tag9);
                                Assert.Equal(-1, post2.Tag10);
#                               endif

                                PostText[] postText2 = await dbctx2.PostTexts
                                    .Where(pt => pt.PostId == post.Id)
                                    .ToArrayAsync();
                                Assert.NotEqual(null, postText2);
                                Assert.Equal(1, postText2.Length);
                                Assert.Equal(PostTextType.Contain, postText2[0].Type);
                                Assert.Equal(1, postText2[0].Number);
                                Assert.Equal(1, postText2[0].Revision);
                                Assert.Equal(postText, postText2[0].Value);
                            }
                        }
                    }
                }
            }

            // Update test...
            int updateTestCount = 0;
            using (AppDbContext dbctxAdd = CreateAndCheckDbContext())
            {
                List<Post> postsAdded = await dbctxAdd.Posts
                    .Where(p => p.Title.StartsWith(textPrefix) == true)
                    //.Include(p => p.Creator)
                    //.Include(p => p.Site)
                    .AsNoTracking()
                    .ToListAsync();
                Assert.NotEqual(null, postsAdded);
                Assert.Equal(postAddedCount, postsAdded.Count);
                foreach (Post postAdded in postsAdded)
                {
                    using (AppDbContext dbctx = CreateAndCheckDbContext())
                    {
                        SiteData siteDt = await DbUtil.GetSiteData(siteId, true, dbctx);
                        foreach (ApplicationUser user in siteDt.users)
                        {
                            string role = user.HigherRole();
                            WcmsAppContext ctx = await CreateAndInitAppContext(dbctx, siteDt.site.Domain, "/", siteDt.regions[0].StringValue, user, false);
                            if (ctx != null)
                            {
                                _Log(1, ctx, $">>{++updateTestCount}: Checking update authorization of post {postAdded.Id} for user={user?.UserName} ({role})...");

                                PostProvider provider = new PostProvider(ctx);
                                Assert.NotEqual(null, provider);

                                // Update the post...
                                string postTitleUpdate = $"{textPrefix}Update_Title_{postAdded.Id}_By_{user?.UserName}";
                                string postTextUpdate = $"{textPrefix}Update_Text_{postAdded.Id}_By_{user?.UserName}";
                                string startDateUpdate = "02/01/2016 04:05";
                                string endDateUpdate = "02/01/2017 04:05";
                                for (int iProc = 1; iProc <= 3; iProc += 1)
                                {
                                    Post post = null;
                                    if (updateTestCount == 13)
                                    {
                                        int brk = 0;
                                    }
                                    if (iProc == 1)
                                    {
                                        _Log(2, ctx, $"  {++updateTestCount}: Checking title and text update...");
                                        post = await provider.Update(postAdded.Id, postTitleUpdate, postTextUpdate);
                                    }
                                    else if (iProc == 2)
                                    {
                                        _Log(2, ctx, $"  {++updateTestCount}: Checking dates and registration update...");
                                        post = await provider.Update(postAdded.Id, startDateUpdate, endDateUpdate, true);
                                    }
                                    else if (iProc == 3)
                                    {
                                        _Log(2, ctx, $"  {++updateTestCount}: Checking dates and registration update (removal)...");
                                        post = await provider.Update(postAdded.Id, null, null, true);
                                    }
                                    else
                                        Assert.Equal(0, 1);
                                    // Verify add authorization...
                                    if (user == null)
                                    {
                                        // Anonymous user cannot update post.
                                        Assert.Equal(null, post);
                                    }
                                    else if (DbUtil.IsSiteUser(siteId, user) == false)
                                    {
                                        // Post can be updated only by user of the same site.
                                        Assert.Equal(null, post);
                                    }
                                    else if (role == ClaimValueRole.Administrator || role == ClaimValueRole.Publicator)
                                    {
                                        // Administrator and publicator have all rights.
                                        Assert.NotEqual(null, post);
                                    }
                                    else if (role == ClaimValueRole.Contributor)
                                    {
                                        // Contributeur can only update their posts.
                                        if (postAdded.CreatorId == user.Id)
                                        {
                                            Assert.NotEqual(null, post);
                                        }
                                        else
                                        {
                                            Assert.Equal(null, post);
                                        }
                                    }
                                    else
                                    {
                                        // Reader and other users cannot update post.
                                        Assert.Equal(null, post);
                                    }
                                    // Check update data...
                                    if (post != null)
                                    {
                                        Assert.NotEqual(0, post.Id);
                                        using (AppDbContext dbctx2 = CreateAndCheckDbContext())
                                        {
                                            Post post2 = await dbctx2.Posts
#                                               if !DENORMALIZE
                                                .Include(p => p.PostRegions)
#                                               endif
                                                //.Include(p => p.Creator)
                                                //.Include(p => p.Site)
                                                .FirstOrDefaultAsync(p => p.Id == post.Id);
                                            Assert.NotEqual(null, post2);
                                            Assert.Equal(postAdded.Private, post2.Private);
                                            Assert.Equal(postAdded.CreatorId, post2.CreatorId);
                                            Assert.Equal(postAdded.CreationDate, post2.CreationDate);
                                            Assert.Equal(postAdded.SiteId, post2.SiteId);
                                            Assert.Equal(postTitleUpdate, post2.Title);
                                            Assert.NotEqual(null, post2.ModifiedDate);
                                            Assert.Equal(0, (int)post2.ModifiedDate.Value.Subtract(DateTime.Now).TotalDays);
                                            if (ctx.User.HasRole(ClaimValueRole.Administrator) == true || ctx.User.HasRole(ClaimValueRole.Publicator) == true)
                                            {
                                                Assert.Equal(postAdded.State, post2.State);
                                                Assert.Equal(postAdded.ValidationDate, post2.ValidationDate);
                                            }
                                            else
                                            {
                                                Assert.Equal(State.NotValided, post2.State);
                                                Assert.Equal(null, post2.ValidationDate);
                                            }
                                            if (iProc == 2)
                                            {
                                                Assert.NotEqual(null, post2.StartDate);
                                                Assert.NotEqual(null, post2.EndDate);
                                            }
                                            else
                                            {
                                                Assert.Equal(null, post2.StartDate);
                                                Assert.Equal(null, post2.EndDate);
                                            }
#                                           if !DENORMALIZE
                                            Assert.NotEqual(null, post2.PostRegions);
                                            Assert.Equal(1, post2.PostRegions.Count);
                                            Assert.Equal(siteDt.regions[0].Id, post2.PostRegions.FirstOrDefault().RegionId);
#                                           else
                                            Assert.Equal(-1, post2.Group1);
                                            Assert.Equal(-1, post2.Group2);
                                            Assert.Equal(-1, post2.Group3);
                                            Assert.Equal(-1, post2.Group4);
                                            Assert.Equal(-1, post2.Group5);
                                            Assert.Equal(-1, post2.Group6);
                                            Assert.Equal(-1, post2.Group7);
                                            Assert.Equal(-1, post2.Group8);
                                            Assert.Equal(-1, post2.Group9);
                                            Assert.Equal(-1, post2.Group10);
                                            Assert.Equal(siteDt.regions[0].Id, post2.Region1);
                                            Assert.Equal(-1, post2.Region2);
                                            Assert.Equal(-1, post2.Region3);
                                            Assert.Equal(-1, post2.Region4);
                                            Assert.Equal(-1, post2.Region5);
                                            Assert.Equal(-1, post2.Region6);
                                            Assert.Equal(-1, post2.Region7);
                                            Assert.Equal(-1, post2.Region8);
                                            Assert.Equal(-1, post2.Region9);
                                            Assert.Equal(-1, post2.Region10);
                                            Assert.Equal(-1, post2.Category1);
                                            Assert.Equal(-1, post2.Category2);
                                            Assert.Equal(-1, post2.Category3);
                                            Assert.Equal(-1, post2.Category4);
                                            Assert.Equal(-1, post2.Category5);
                                            Assert.Equal(-1, post2.Category6);
                                            Assert.Equal(-1, post2.Category7);
                                            Assert.Equal(-1, post2.Category8);
                                            Assert.Equal(-1, post2.Category9);
                                            Assert.Equal(-1, post2.Category10);
                                            Assert.Equal(-1, post2.Tag1);
                                            Assert.Equal(-1, post2.Tag2);
                                            Assert.Equal(-1, post2.Tag3);
                                            Assert.Equal(-1, post2.Tag4);
                                            Assert.Equal(-1, post2.Tag5);
                                            Assert.Equal(-1, post2.Tag6);
                                            Assert.Equal(-1, post2.Tag7);
                                            Assert.Equal(-1, post2.Tag8);
                                            Assert.Equal(-1, post2.Tag9);
                                            Assert.Equal(-1, post2.Tag10);
#                                           endif

                                            PostText[] postText2 = await dbctx2.PostTexts
                                                .Where(pt => pt.PostId == post.Id)
                                                .ToArrayAsync();
                                            Assert.NotEqual(null, postText2);
                                            Assert.Equal(1, postText2.Length);
                                            Assert.Equal(PostTextType.Contain, postText2[0].Type);
                                            Assert.Equal(1, postText2[0].Number);
                                            Assert.Equal(1, postText2[0].Revision);
                                            Assert.Equal(postTextUpdate, postText2[0].Value);
                                            
                                            PostClaim[] postClaims2 = await dbctx2.PostClaims
                                                .Where(pt => pt.PostId == post.Id && pt.Type == PostClaimType.Registration)
                                                .ToArrayAsync();
                                            if (iProc == 2)
                                            {
                                                Assert.NotEqual(null, postClaims2);
                                                Assert.Equal(1, postClaims2.Length);
                                                Assert.Equal("yes", postClaims2[0].StringValue);
                                            }
                                            else
                                            {
                                                Assert.NotEqual(null, postClaims2);
                                                Assert.Equal(0, postClaims2.Length);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Update date and registration test...

            // Delete test...
        }
    }
}
