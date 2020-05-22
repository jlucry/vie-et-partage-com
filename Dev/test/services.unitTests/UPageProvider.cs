// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Core;
using Xunit;
using Xunit.Abstractions;

namespace Services.UnitTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class UPageProvider : UBaseProvider
    {
        /// <summary>
        /// Sample: https://github.com/aspnet/MusicStore/blob/dev/test/MusicStore.Test/HomeControllerTest.cs
        /// </summary>
        public UPageProvider(ITestOutputHelper output)
            : base(output)
        {
        }

        [Theory]
        [InlineData(1)]
        public async Task UGet(int siteId)
        {
            LogMaxLevel = 1;
            onlyUtestLog = true;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;
            int countTest = 0;
            int countQueryTest = 0;

            SiteData siteDt = await DbUtil.GetSiteData(siteId, true);
            foreach (SiteClaim region in siteDt.regions)
            {
                foreach (ApplicationUser user in siteDt.users)
                {
                    string role = user.HigherRole();
                    WcmsAppContext ctx = await CreateAndInitAppContext(null, siteDt.site.Domain, "/", region.StringValue, user, false);
                    if (ctx != null)
                    {
                        string testDesc1 = $"region={region.StringValue}, user={user?.UserName} ({role})";
                        _Log(1, ctx, $">>{++countQueryTest}: Checking page get for {testDesc1}...");

                        PageProvider provider = new PageProvider(ctx);
                        IEnumerable<Page> pages = await provider?.Get(false, -1);
                        if (user != null && DbUtil.IsSiteUser(siteId, user) == false)
                        {
                            // User extern to the site: cannot view pages.
                            Assert_Equal<IEnumerable<Page>>(ctx, null, pages, $"Check extern to the site failed: {countQueryTest}: {testDesc1}");
                        }
                        else
                        {
                            Assert.NotEqual(null, pages);
                            Assert.NotEqual(0, pages.Count());
                            foreach (Page page in pages)
                            {
                                string testDesc2 = $"page {page?.Id ?? 0}({countQueryTest}-{++countTest})({page?.Title ?? "null"})";
                                string testDescT = $"{testDesc2}: {testDesc1}";
                                _Log(2, ctx, $"  >Checking {testDesc2}...");
                                if (countTest == 2611)
                                {
                                    int brk = 0;
                                }
                                Assert_NotEqual<Page>(ctx, null, page, $"Null page: {testDescT}");
                                page.RequestSite = ctx.Site;

                                _Log(4, ctx, "     Check site ID...");
                                Assert_Equal<int>(ctx, 1, page.SiteId, $"Check site failed: {testDescT}");
                                _Log(4, ctx, "     Check region...");
                                Assert_Equal<bool>(ctx, true, (page.Region1 == 0 || page.Region1 == ctx.Region.Id)
                                    || (page.Region2 == 0 || page.Region2 == ctx.Region.Id)
                                    || (page.Region3 == 0 || page.Region3 == ctx.Region.Id)
                                    || (page.Region4 == 0 || page.Region4 == ctx.Region.Id)
                                    || (page.Region5 == 0 || page.Region5 == ctx.Region.Id)
                                    || (page.Region6 == 0 || page.Region6 == ctx.Region.Id)
                                    || (page.Region7 == 0 || page.Region7 == ctx.Region.Id)
                                    || (page.Region8 == 0 || page.Region8 == ctx.Region.Id)
                                    || (page.Region9 == 0 || page.Region9 == ctx.Region.Id)
                                    || (page.Region10 == 0 || page.Region10 == ctx.Region.Id), $"Check region failed: {testDescT}");
                                _Log(4, ctx, "     Check for authorization...");
                                Assert_Equal<bool>(ctx, true, ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, page, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false),
                                    $"Check authorization failed: {testDescT}");
                            }
                        }
                    }
                }
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task UGetRecursive(int siteId)
        {
            LogMaxLevel = 1;
            onlyUtestLog = true;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;

            SiteData siteDt = await DbUtil.GetSiteData(siteId);
            foreach (SiteClaim region in siteDt.regions)
            {
                // Create and init the context...
                WcmsAppContext ctx = await CreateAndInitAppContext(null, siteDt.site.Domain, "/", region.StringValue, null, false);
                if (ctx != null)
                {
                    _Log(1, ctx, $">>Checking page recursive get for region={region.StringValue}...");

                    PageProvider provider = new PageProvider(ctx);
                    IEnumerable<Page> pages = await provider?.Get(false, null, true);
                    Assert.NotEqual(null, pages);
                    //TODO: Add a check to validate a get with recusion enabled.
                    
                }
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task UAuthorisations(int siteId)
        {
            LogMaxLevel = 1;
            onlyUtestLog = true;
            SiteAuthorizationHandler.LogDisabled = true;
            PageAuthorizationHandler.LogDisabled = true;
            PostAuthorizationHandler.LogDisabled = true;

            SiteData siteDt = await DbUtil.GetSiteData(siteId, true);
            using (AppDbContext dbctx = CreateAndCheckDbContext())
            {
                int totalPage = await dbctx.Pages.CountAsync();
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
                            _Log(1, ctx, $">>Checking page authorization for region={region.StringValue} and user={user?.UserName} ({role})...");
                            while (true)
                            {
                                _Log(2, ctx, $"  >Pages from {skip * take}...");
                                List<Page> pages = await dbctx.Pages.Skip(skip * take).Take(take)
#                                   if !DENORMALIZE
                                    .Include(p => p.PageGroups)
#                                   endif
                                    .Include(p => p.Site)
                                    .AsNoTracking()
                                    .ToListAsync();
                                if (pages == null || pages.Count == 0)
                                {
                                    // Will assert if we don't test all the existing pages.
                                    Assert.Equal(totalPage, count);
                                    break;
                                }
                                count += pages.Count;

                                foreach (Page page in pages)
                                {
                                    Assert.NotEqual(null, page);
                                    _Log(3, ctx, $"    Checking {page?.Id ?? 0}: {page?.Title ?? "null"}...");
                                    for (int i = 0; i < 4; i += 1)
                                    {
                                        bool authorized = false;
                                        bool addAuthorized = false;
                                        bool updateAuthorized = false;
                                        bool publishAuthorized = false;

                                        // Play with the RequestSite to invole the site membership checks in the authorization module.
                                        switch (i)
                                        {
                                            case 0:
                                                // Simulate case where the user don't have access to the site.
                                                page.RequestSite = null;
                                                break;
                                            case 1:
                                                // Can be like if a user was able to log into a site on which he's not register.
                                                page.RequestSite = siteDt.site;
                                                break;
                                            case 2:
                                                // Force the request site to be the site of the page.
                                                page.RequestSite = page.Site;
                                                break;
                                        }

                                        // Compute authorization...
                                        {
                                            if (page.Site == null || page.RequestSite == null)
                                            {
                                                // We need to have the page site and the site requesting the page.
                                                authorized = false;
                                            }
                                            else if (page.SiteId != page.RequestSite.Id)
                                            {
                                                // A site cannot request pages of another site.
                                                authorized = false;
                                            }
                                            else if (page.Site.Private == true && page.Private == false)
                                            {
                                                // No public page in a private site.
                                                authorized = false;
                                            }
                                            else if (user == null)
                                            {
                                                // Published and public pages are granted to anonymous.
                                                if (page.State == State.Valided && page.Private == false)
                                                {
                                                    authorized = true; 
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                            }
                                            else if (DbUtil.IsSiteUser(page.SiteId, user) == false)
                                            {
                                                // Page can be see only by user of the same site.
                                                authorized = false;
                                            }
                                            // Authenticated users can see only public page and page of theirs groups.
#                                           if !DENORMALIZE
                                            else if (page.Private == true && DbUtil.MemberOf(user, page?.PageGroups) == false)
#                                           else
                                            else if (page.Private == true && user.MemberOf(page) == false)
#                                           endif
                                            {
                                                authorized = false;
                                            }
                                            else if (role == ClaimValueRole.Administrator || role == ClaimValueRole.Publicator)
                                            {
                                                // Administrator and publicator have all rights.
                                                authorized = true;
                                                if (user.MemberOfAll(page) == true)
                                                {
                                                    addAuthorized = true;
                                                    updateAuthorized = true;
                                                }
                                            }
                                            else if (role == ClaimValueRole.Contributor || role == ClaimValueRole.Reader)
                                            {
                                                // Contributeur and reader can only read published page.
                                                if (page.State == State.Valided)
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
                                                // User without group can only read published page.
                                                if (page.State == State.Valided)
                                                {
                                                    authorized = true;
                                                }
                                                else
                                                {
                                                    authorized = false;
                                                }
                                            }
                                        }

                                        // Get provider authorizations...
                                        bool authorizedMod = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, page, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Read } }))?.Succeeded ?? false);
                                        bool authorizedModAdd = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, page, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Add } }))?.Succeeded ?? false);
                                        bool authorizedModUpdate = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, page, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Update } }))?.Succeeded ?? false);
                                        bool authorizedModPublish = ((await ctx.AuthzProvider.AuthorizeAsync(ctx.UserPrincipal, page, new List<OperationAuthorizationRequirement>() { new OperationAuthorizationRequirement { Name = AuthorizationRequirement.Publish } }))?.Succeeded ?? false);
                                        
                                        // Check provider authorization...
                                        if (authorized != authorizedMod
                                            || addAuthorized != authorizedModAdd
                                            || updateAuthorized != authorizedModUpdate
                                            || publishAuthorized != authorizedModPublish)
                                        {
                                            _Log(1, ctx, $"  Failure detected for region={region.StringValue}, user={user?.UserName} ({role}), page={page?.Id ?? 0}: {page?.Title ?? "null"}: autz={authorized} vs {authorizedMod}, autzAdd={addAuthorized} vs {authorizedModAdd}, autzUpdate={updateAuthorized} vs {authorizedModUpdate}, autzPublish={publishAuthorized} vs {authorizedModPublish}, ");
                                        }
                                        Assert.Equal(authorized, authorizedMod);
                                        Assert.Equal(addAuthorized, authorizedModAdd);
                                        Assert.Equal(updateAuthorized, authorizedModUpdate);
                                        Assert.Equal(publishAuthorized, authorizedModPublish);
                                    }
                                }

                                skip += 1;
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async Task SenEmail()
        {
            try
            {
                string email_from_name = "www@vieetpartage.com";
                string email_from_address = "www@vieetpartage.com";
                string email_srv_address = "smtp.oxito.com";
                string email_srv_port = "587";
                string email_srv_ssl = "true";
                string email_srv_login = "www@vieetpartage.com";
                string email_srv_password = "Vep@972";

                // https://github.com/jstedfast/MailKit
                var mimeMsg = new MimeMessage();
                mimeMsg.From.Add(new MailboxAddress(email_from_name, email_from_address));
                mimeMsg.To.Add(new MailboxAddress("david.fidelin@live.com"));
                mimeMsg.Subject = $"[TEST]Email de test - {DateTime.Now.ToString()}";
                mimeMsg.Body = new TextPart(TextFormat.Html)
                {
                    Text = $"[TEST]Email de test - {DateTime.Now.ToString()}"
                };

                using (var pop = new ImapClient(new ProtocolLogger("d:\\imap.log")))
                {
                    // Accept all SSL certificates (in case the server supports STARTTLS)...
                    pop.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    //pop.Connect("mail.oxito.com", 143, MailKit.Security.SecureSocketOptions.StartTls/*true*/);
                    //pop.Authenticate(email_srv_login, email_srv_password);

                    using (var client = new SmtpClient(new ProtocolLogger("d:\\smtp.log")))
                    {
                        // Accept all SSL certificates (in case the server supports STARTTLS)...
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;


                        client.Connect(email_srv_address, int.Parse(email_srv_port), MailKit.Security.SecureSocketOptions.StartTls/*(email_srv_ssl == "true")*/);

                        // Note: since we don't have an OAuth2 token, disable
                        // the XOAUTH2 authentication mechanism.
                        client.AuthenticationMechanisms.Remove("XOAUTH2");

                        // Note: only needed if the SMTP server requires authentication
                        client.Authenticate(email_srv_login, email_srv_password);

                        client.Send(mimeMsg);
                        client.Disconnect(true);
                    }

                    //pop.Disconnect(true);
                }
            }
            catch(Exception e)
            {
                e = e;
            }
        }
    }
}
