// !!! Because of an insertion issue with the RC2 !!!
#define RC2ISSUE
// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitTests.Core;
using Xunit;

namespace Models.UnitTests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class UAppDbContext
    {
        private static int countCover = 0;

        // Site related..
        private Site _SiteVep { get; set; }
        private Site _SiteDfd { get; set; }

#if DEBUG
        /// <summary>
        /// Db creation test.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UDbCreation(bool prod)
        {
            using (var context = _CreateContext(prod, false))
            {
                if (Global.UseMySql == false)
                    IdentityResultAssert.IsSuccess(await context.Database.EnsureCreatedAsync());
                else
                    IdentityResultAssert.IsSuccess(context.Database.EnsureCreated());
            }
        }

        /// <summary>
        /// Db deletion test.
        /// </summary>
        /// <returns></returns>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UDbDeletion(bool prod)
        {
            using (var context = _CreateContext(prod, false))
            {
                if (Global.UseMySql == false)
                    IdentityResultAssert.IsSuccess(await context.Database.EnsureDeletedAsync());
                else
                    IdentityResultAssert.IsSuccess(context.Database.EnsureDeleted());
            }
        }
#endif

        /// <summary>
        /// Fill db for unit testing.
        /// </summary>
        /// <param name="initSiteOnly"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public async Task UDbFillSiteTables(bool prod, bool initSiteOnly)
        {
            // Clean the datase...
            using (var context = _CreateContext(prod, false))
            {
                if (Global.UseMySql == false)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.EnsureCreatedAsync();
                }
                else
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                }
            }

            // Init the database...
            using (var context = _CreateContext(prod, false))
            {
                // Init site tables...
                await _InitSiteTable(context);
                if (initSiteOnly == false)
                {
                    foreach (Site site in new List<Site> { _SiteVep, _SiteDfd })
                    {
                        string groupsStr = string.Empty;
                        string regionsStr = string.Empty;
                        string categoriesStr = string.Empty;
                        string tagsStr = string.Empty;
                        List<SiteClaim> groups = new List<SiteClaim>();
                        List<SiteClaim> regions = new List<SiteClaim>();
                        List<SiteClaim> categories = new List<SiteClaim>();
                        List<SiteClaim> tags = new List<SiteClaim>();
                        List<ApplicationUser> users = new List<ApplicationUser>();

                        // Init site claims table...
                        _InitSiteClaimsTable(context, site, ref groups, ref regions, ref categories, ref tags);
                        #region Get "all lists" as string...
                        // Get "all lists" as string...
                        {
                            bool first = true;
                            foreach (SiteClaim group in groups)
                            {
                                if (group.Id > 0)
                                {
                                    if (first == false)
                                    {
                                        groupsStr += ", ";
                                    }
                                    groupsStr += $"{group.Id}";
                                    first = false;
                                }
                            }
                            first = true;
                            foreach (SiteClaim region in regions)
                            {
                                if (region.Id > 0)
                                {
                                    if (first == false)
                                    {
                                        regionsStr += ", ";
                                    }
                                    regionsStr += $"{region.Id}";
                                    first = false;
                                }
                            }
                            first = true;
                            foreach (SiteClaim category in categories)
                            {
                                if (category.Id > 0)
                                {
                                    if (first == false)
                                    {
                                        categoriesStr += ", ";
                                    }
                                    categoriesStr += $"{category.Id}";
                                    first = false;
                                }
                            }
                            first = true;
                            foreach (SiteClaim tag in tags)
                            {
                                if (tag.Id > 0)
                                {
                                    if (first == false)
                                    {
                                        tagsStr += ", ";
                                    }
                                    tagsStr += $"{tag.Id}";
                                    first = false;
                                }
                            }
                        }
                        #endregion
                        // Init user tables...
                        users.AddRange(_InitUserTable(context, site, groups));
                        // Init page tables...
                        _InitPageTable(context, site, groups, regions, categories, tags,
                            groupsStr, regionsStr, categoriesStr, tagsStr,
                            users);
                        // Init post tables...
                        _InitPostTable(context, site, groups, regions, categories, tags,
                            groupsStr, regionsStr, categoriesStr, tagsStr,
                            users);
                    }
                }
            }

            // Site unit test...
            using (var contextSite = _CreateContext(prod, false))
            {
                Site siteVep = null;
                //Site siteDfd = null;
                // Read site alone...
                Assert.NotEqual(null, siteVep = await contextSite.Sites.FirstAsync(s => s.Id == _SiteVep.Id));
                Assert.Equal(null, siteVep.SiteClaims);
                // Read site with claims...
                Assert.NotEqual(null, siteVep = await contextSite.Sites.Include(s => s.SiteClaims).FirstAsync(s => s.Id == _SiteVep.Id));
                Assert.NotEqual(null, siteVep.SiteClaims);
            }

            //using (var contextA = _CreateContext(prod, false))
            //{
            //    // Read sites...
            //    /*Task<Site> site3;
            //    Assert.NotEqual(null, await (site3 = context.Sites.Include(s => s.Pages).FirstAsync(s => s.SiteId == siteId)));
            //    Assert.Equal(siteId, site3.Result.SiteId);
            //    Assert.NotEqual(null, site3.Result.Pages);
            //    Assert.Equal(siteId, site3.Result.Pages.Count);*/
            //}
        }

        /// <summary>
        /// Create the db context.
        /// </summary>
        /// <param name="ensureCreated"></param>
        /// <returns></returns>
        private AppDbContext _CreateContext(bool prod, bool ensureCreated = false)
        {
            var db = DbUtil.CreateDbContext(prod);
            if (ensureCreated)
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            return db;
        }

        /// <summary>
        /// Init Site table.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task _InitSiteTable(AppDbContext context)
        {
            Task<int> taskInt = null;
            List<Site> sites = new List<Site>
            {
                new Site
                {
                    Title = "Vep",
                    Domain = "vieetpartage.com",
                    Private = false,
                    HasRegions = true,
                    IsPublicRegistration = false,
                    LockoutUserOnFailure = true
                },
                new Site
                {
                    Title = "Dfide",
                    Domain = "dfide.com",
                    Private = true,
                    IsPublicRegistration = false,
                    LockoutUserOnFailure = true
                }
            };
            // Add sites...
            context.Sites.AddRange(sites);
            await (taskInt = context.SaveChangesAsync());
            Assert.Equal(sites.Count, taskInt.Result);
            // Save sites...
            _SiteVep = sites[0];
            _SiteDfd = sites[1];
        }

        /// <summary>
        /// Init Site claims table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="site"></param>
        /// <param name="groups"></param>
        /// <param name="regions"></param>
        /// <param name="categorys"></param>
        /// <param name="tags"></param>
        private void _InitSiteClaimsTable(AppDbContext context, Site site,
            ref List<SiteClaim> groups, ref List<SiteClaim> regions, ref List<SiteClaim> categorys, ref List<SiteClaim> tags)
        {
            List<SiteClaim> claims = new List<SiteClaim>
            {
                // Groups...
                new SiteClaim
                {
                    Type = SiteClaimType.Group,
                    StringValue = $"Group#Id#@{site.Id}",
                    SiteId = site.Id,
                },
                new SiteClaim
                {
                    Type = SiteClaimType.Group,
                    StringValue = $"Group#Id#@{site.Id}",
                    SiteId = site.Id,
                },
                // Regions...
                new SiteClaim
                {
                    Type = SiteClaimType.Region,
                    StringValue = $"Region#Id#@{site.Id}",
                    SiteId = site.Id
                },
                new SiteClaim
                {
                    Type = SiteClaimType.Region,
                    StringValue = $"Region#Id#@{site.Id}",
                    SiteId = site.Id
                },
                // Categories...
                new SiteClaim
                {
                    Type = SiteClaimType.Categorie,
                    StringValue = $"Category#Id#@{site.Id}",
                    SiteId = site.Id
                },
                new SiteClaim
                {
                    Type = SiteClaimType.Categorie,
                    StringValue = $"Category#Id#@{site.Id}",
                    SiteId = site.Id
                },
                // Tags...
                new SiteClaim
                {
                    Type = SiteClaimType.Tag,
                    StringValue = $"Tag#Id#@{site.Id}",
                    SiteId = site.Id,
                },
                new SiteClaim
                {
                    Type = SiteClaimType.Tag,
                    StringValue = $"Tag#Id#@{site.Id}",
                    SiteId = site.Id,
                },
            };
            // Add site claims...
            context.SiteClaims.AddRange(claims);
            Assert.Equal(claims.Count, context.SaveChanges());
            // Update clains based on their Ids...
            foreach (SiteClaim claim in claims)
            {
                claim.StringValue = claim.StringValue.Replace("#Id#", claim.Id.ToString());
            }
            Assert.Equal(claims.Count, context.SaveChanges());
            // Save groups, regions, categories and tags...
            groups.Add(claims[0]);
            groups.Add(claims[1]);
            groups.Add(new SiteClaim { Id = -1 });
            groups.Add(new SiteClaim { Id = -2 });
            regions.Add(new SiteClaim { Id = 0 });
            regions.Add(claims[2]);
            regions.Add(claims[3]);
            regions.Add(new SiteClaim { Id = -1 });
            categorys.Add(claims[4]);
            categorys.Add(claims[5]);
            tags.Add(claims[6]);
            tags.Add(claims[7]);
            tags.Add(new SiteClaim { Id = -1 });

            // Add childs categories...
            List<SiteClaim> childClaims = new List<SiteClaim>
            {
                new SiteClaim
                {
                    Type = SiteClaimType.Categorie,
                    StringValue = $"Sub#Id#.{claims[4].StringValue}",
                    SiteId = site.Id,
                    Parent = claims[4]
                },
                new SiteClaim
                {
                    Type = SiteClaimType.Categorie,
                    StringValue = $"Sub#Id#.{claims[4].StringValue}",
                    SiteId = site.Id,
                    Parent = claims[4]
                },
            };
            // Add childs site claims...
            context.SiteClaims.AddRange(childClaims);
            Assert.Equal(childClaims.Count, context.SaveChanges());
            // Update clains based on their Ids...
            foreach (SiteClaim childClaim in childClaims)
            {
                childClaim.StringValue = childClaim.StringValue.Replace("#Id#", childClaim.Id.ToString());
            }
            Assert.Equal(childClaims.Count, context.SaveChanges());
            // Save sub-categories...
            categorys.Add(childClaims[0]);
            categorys.Add(childClaims[1]);
            categorys.Add(new SiteClaim { Id = -1 });
        }

        /// <summary>
        /// Init User table.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="site"></param>
        /// <param name="sitegroup"></param>
        /// <returns></returns>
        private List<ApplicationUser> _InitUserTable(AppDbContext context, Site site, List<SiteClaim> sitegroup)
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            List<IdentityUserClaim<string>> userClaims = new List<IdentityUserClaim<string>>();

            // Add users to db...
            foreach (SiteClaim group in sitegroup)
            {
                List<SiteClaim> userGroups = (group.Id == -2)
                    ? null
                    : ((group.Id == -1) ? sitegroup : new List<SiteClaim> { group });
                users.Add(__GetUser(context, site, ClaimValueRole.Administrator, userGroups, ref userClaims));
                users.Add(__GetUser(context, site, ClaimValueRole.Publicator, userGroups, ref userClaims));
                users.Add(__GetUser(context, site, ClaimValueRole.Contributor, userGroups, ref userClaims));
                users.Add(__GetUser(context, site, ClaimValueRole.Reader, userGroups, ref userClaims));
                users.Add(__GetUser(context, site, null, userGroups, ref userClaims));
            }

            // Add users claims to db...
            context.UserClaims.AddRange(userClaims);
            Assert.Equal(userClaims.Count, context.SaveChanges());

            // Return users...
            return users;
        }

        /// <summary>
        /// Init Page table.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private void _InitPageTable(AppDbContext context, Site site,
            List<SiteClaim> groups, List<SiteClaim> regions, List<SiteClaim> categorys, List<SiteClaim> tags,
            string groupsStr, string regionsStr, string categoriesStr, string tagsStr,
            List<ApplicationUser> users)
        {
            int position = 0;
            Dictionary<Page, PageDatas> pageDatas = new Dictionary<Page, PageDatas>();

            // Create pages...
            foreach (SiteClaim group in groups)
            {
                foreach (SiteClaim region in regions)
                {
                    foreach (SiteClaim category in categorys)
                    {
                        foreach (SiteClaim tag in tags)
                        {
                            foreach (State state in Enum.GetValues(typeof(State)))
                            {
                                foreach (bool inMenu in new List<bool> { false, true })
                                {
                                    int pos = (inMenu == false) ? 0 : ++position;
                                    // Add the page...
                                    Page page = __GetPage(site, group, region, category, tag, state, pos,
                                        groupsStr, regionsStr, categoriesStr, tagsStr,
                                        users);
                                    // Add page datas...
                                    pageDatas.Add(page, new PageDatas
                                    {
#                                       if !DENORMALIZE
                                        // Add page group...
                                        PageGroups = (group.Id == -2)
                                            ? null // Public page
                                            : (__GetPageGroups(page, (group.Id == -1) ? groups : new List<SiteClaim> { group })),
                                        // Add page region...
                                        PageRegions = __GetPageRegions(page, (region.Id == -1)
                                            ? regions
                                            : new List<SiteClaim> { region }),
                                        // Add page category...
                                        PageCategorys = __GetPageCategories(page, (category.Id == -1)
                                            ? categorys
                                            : new List<SiteClaim> { category }),
                                        // Add page tag...
                                        PageTags = __GetPageTags(page, (tag.Id == -1)
                                            ? tags
                                            : new List<SiteClaim> { tag }),
#                                       else
                                        // Add page group...
                                        PageGroups = (group.Id == -2)
                                            ? null // Public page
                                            : ((group.Id == -1) ? groups : new List<SiteClaim> { group }),
                                        // Add page region...
                                        PageRegions = (region.Id == -1)
                                            ? regions
                                            : new List<SiteClaim> { region },
                                        // Add page category...
                                        PageCategorys = (category.Id == -1)
                                            ? categorys
                                            : new List<SiteClaim> { category },
                                        // Add page tag...
                                        PageTags = (tag.Id == -1)
                                            ? tags
                                            : new List<SiteClaim> { tag },
#                                       endif
                                        // Add pages claims...
                                        PageClaims = new List<PageClaim>
                                        {
                                            new PageClaim
                                            {
                                                Type = "ClaimType1",
                                                Page = page,
                                                SiteId = site.Id,
                                            },
                                            new PageClaim
                                            {
                                                Type = "ClaimType2",
                                                Page = page,
                                                SiteId = site.Id,
                                            }
                                        }
                                    });

                                    // Insert data to the db...
                                    if (pageDatas.Count() > 150)
                                    {
                                        // Add pages datas...
                                        __AddPagesDatas(context, pageDatas);
                                    }
                                }
                            }
                        }
                    }
                }
                //return;
            }
            // Finalize...
            __AddPagesDatas(context, pageDatas);
        }

        /// <summary>
        /// Init Post table.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private void _InitPostTable(AppDbContext context, Site site,
            List<SiteClaim> groups, List<SiteClaim> regions, List<SiteClaim> categorys, List<SiteClaim> tags,
            string groupsStr, string regionsStr, string categoriesStr, string tagsStr,
            List<ApplicationUser> users)
        {
            Dictionary<Post, PostDatas> postDatas = new Dictionary<Post, PostDatas>();

            // Create posts...
            foreach (SiteClaim group in groups)
            {
                foreach (SiteClaim region in regions)
                {
                    foreach (SiteClaim category in categorys)
                    {
                        foreach (SiteClaim tag in tags)
                        {
                            foreach (State state in Enum.GetValues(typeof(State)))
                            {
                                foreach (bool higlight in new List<bool> { false, true })
                                {
                                    // Add the post...
                                    Post post = __GetPost(site, group, region, category, tag, state, higlight,
                                        groupsStr, regionsStr, categoriesStr, tagsStr,
                                        users);
                                    // Add post datas...
                                    postDatas.Add(post, new PostDatas
                                    {
#                                       if !DENORMALIZE
                                        // Add post group...
                                        PostGroups = (group.Id == -2)
                                            ? null
                                            : (__GetPostGroups(post, (group.Id == -1) ? groups : new List<SiteClaim> { group })),
                                        // Add post region...
                                        PostRegions = __GetPostRegions(post, (region.Id == -1)
                                            ? regions
                                            : new List<SiteClaim> { region }),
                                        // Add post category...
                                        PostCategorys = __GetPostCategories(post, (category.Id == -1)
                                            ? categorys
                                            : new List<SiteClaim> { category }),
                                        // Add post tag...
                                        PostTags = __GetPostTags(post, (tag.Id == -1)
                                            ? tags
                                            : new List<SiteClaim> { tag }),
#                                       else
                                        // Add post group...
                                        PostGroups = (group.Id == -2)
                                            ? null
                                            : ((group.Id == -1) ? groups : new List<SiteClaim> { group }),
                                        // Add post region...
                                        PostRegions = (region.Id == -1)
                                            ? regions
                                            : new List<SiteClaim> { region },
                                        // Add post category...
                                        PostCategorys = (category.Id == -1)
                                            ? categorys
                                            : new List<SiteClaim> { category },
                                        // Add post tag...
                                        PostTags = (tag.Id == -1)
                                            ? tags
                                            : new List<SiteClaim> { tag },
#                                       endif
                                        // Add posts text...
                                        PostTexts = new List<PostText>
                                        {
                                            new PostText
                                            {
                                                Type = PostTextType.Contain,
                                                Number = 1,
                                                Revision = 1,
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                        },
                                        // Add posts file...
                                        PostFiles = new List<PostFile>
                                        {
                                            new PostFile
                                            {
                                                Type = PostFileType.File,
                                                Creator = users[0],
                                                CreationDate = DateTime.Now,
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                            new PostFile
                                            {
                                                Type = PostFileType.File,
                                                Creator = users[0],
                                                CreationDate = DateTime.Now,
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                            new PostFile
                                            {
                                                Type = PostFileType.File,
                                                Creator = users[0],
                                                CreationDate = DateTime.Now,
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                            new PostFile
                                            {
                                                Type = PostFileType.File,
                                                Creator = users[0],
                                                CreationDate = DateTime.Now,
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                        },
                                        // Add posts claims...
                                        PostClaims = new List<PostClaim>
                                        {
                                            new PostClaim
                                            {
                                                Type = "ClaimType1",
                                                Post = post,
                                                SiteId = site.Id,
                                            },
                                            new PostClaim
                                            {
                                                Type = "ClaimType2",
                                                Post = post,
                                                SiteId = site.Id,
                                            }
                                        }
                                    });

                                    // Insert data to the db...
                                    if (postDatas.Count() > 150)
                                    {
                                        // Add posts datas...
                                        __AddPostsDatas(context, postDatas);
                                    }
                                }
                            }
                        }
                    }
                }
                //return;
            }
            // Finalize...
            __AddPostsDatas(context, postDatas);

            List<Post> rootPosts = new List<Post>{ };
            // Add posts...
            context.Posts.AddRange(rootPosts);
            Assert.Equal(rootPosts.Count, context.SaveChanges());
        }

        /// <summary>
        /// Get user.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="site"></param>
        /// <param name="role"></param>
        /// <param name="groups"></param>
        /// <param name="userClaims"></param>
        /// <returns></returns>
        private ApplicationUser __GetUser(AppDbContext context, Site site, string role, List<SiteClaim> groups, ref List<IdentityUserClaim<string>> userClaims)
        {
            // Groups as a string...
            string groupsStr = string.Empty;
            if (groups != null && groups.Count() > 0)
            {
                foreach (SiteClaim group in groups)
                {
                    if (group.Id > 0)
                        groupsStr += $"-{group.Id.ToString()}";
                }
            }
            string userRole = (role == null) ? "norole" : role;

            // The user...
            ApplicationUser user = new ApplicationUser
            {
                AccessFailedCount = 0,
                ConcurrencyStamp = "4126c22b-a2fd-4e3f-bb85-1fa6f980961c",
                Email = $"{userRole.ToLower()}{groupsStr}@domain.com",
                EmailConfirmed = false,
                LockoutEnabled = true,
                LockoutEnd = null,
                NormalizedEmail = $"{userRole.ToUpper()}{groupsStr}@DOMAIN.COM",
                NormalizedUserName = $"{userRole.ToUpper()}{groupsStr}@DOMAIN.COM@{site.Id}",
                // Password@0
                PasswordHash = "AQAAAAEAACcQAAAAEGIFKhLhBBEvAx6BvjRCw4abCcXvpOLi7i81d4M7koRTbocwyXWNwPNDijyGSyeUaA==",
                PhoneNumber = null,
                PhoneNumberConfirmed = false,
                SecurityStamp = "ad4ab184-20e8-4eb9-a670-a5ecc8d0a4e0",
                TwoFactorEnabled = false,
                UserName = $"{userRole.ToLower()}{groupsStr}@domain.com@{site.Id}",
                Group1 = (groups != null && groups.Count() > 1) ? groups[0].Id : -1,
                Group2 = (groups != null && groups.Count() > 2) ? groups[1].Id : -1,
                Group3 = (groups != null && groups.Count() > 3) ? groups[2].Id : -1,
                Group4 = (groups != null && groups.Count() > 4) ? groups[3].Id : -1,
                Group5 = (groups != null && groups.Count() > 5) ? groups[4].Id : -1,
                Group6 = (groups != null && groups.Count() > 6) ? groups[5].Id : -1,
                Group7 = (groups != null && groups.Count() > 7) ? groups[6].Id : -1,
                Group8 = (groups != null && groups.Count() > 8) ? groups[7].Id : -1,
                Group9 = (groups != null && groups.Count() > 9) ? groups[8].Id : -1,
                Group10 = (groups != null && groups.Count() > 10) ? groups[9].Id : -1,
                Region1 = -1,//(AppContext.Region?.Id ?? -1),
                Region2 = -1,
                Region3 = -1,
                Region4 = -1,
                Region5 = -1,
                Region6 = -1,
                Region7 = -1,
                Region8 = -1,
                Region9 = -1,
                Region10 = -1,
                Enabled = true,
                Cover = null,
                SiteId = site.Id
            };
            context.Users.Add(user);
            Assert.Equal(1, context.SaveChanges());

            // The user claims...
            if (userClaims != null)
            {
                // Role...
                if (role != null)
                {
                    userClaims.Add(new IdentityUserClaim<string>
                    {
                        ClaimType = UserClaimType.Role,
                        ClaimValue = role,
                        UserId = user.Id,
                    });
                }
            }

            return user;
        }

        /// <summary>
        /// Add pages data.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pageDatas"></param>
        private void __AddPagesDatas(AppDbContext context, Dictionary<Page, PageDatas> pageDatas)
        {
            if (pageDatas != null && pageDatas.Count() != 0)
            {
                // Add pages...
                {
                    foreach (KeyValuePair<Page, PageDatas> pageData in pageDatas)
                    {
                        // Add page group...
                        pageData.Key.Group1 = -1;
                        pageData.Key.Group2 = -1;
                        pageData.Key.Group3 = -1;
                        pageData.Key.Group4 = -1;
                        pageData.Key.Group5 = -1;
                        pageData.Key.Group6 = -1;
                        pageData.Key.Group7 = -1;
                        pageData.Key.Group8 = -1;
                        pageData.Key.Group9 = -1;
                        pageData.Key.Group10 = -1;
                        if (pageData.Value.PageGroups != null && pageData.Value.PageGroups.Count() > 0)
                        {
                            int iGroupDx = 1;
                            foreach (SiteClaim group in pageData.Value.PageGroups)
                            {
                                if (iGroupDx == 1) pageData.Key.Group1 = group.Id;
                                else if (iGroupDx == 2) pageData.Key.Group2 = group.Id;
                                else if (iGroupDx == 3) pageData.Key.Group3 = group.Id;
                                else if (iGroupDx == 4) pageData.Key.Group4 = group.Id;
                                else if (iGroupDx == 5) pageData.Key.Group5 = group.Id;
                                else if (iGroupDx == 6) pageData.Key.Group6 = group.Id;
                                else if (iGroupDx == 7) pageData.Key.Group7 = group.Id;
                                else if (iGroupDx == 8) pageData.Key.Group8 = group.Id;
                                else if (iGroupDx == 9) pageData.Key.Group9 = group.Id;
                                else if (iGroupDx == 10) pageData.Key.Group10 = group.Id;
                                iGroupDx += 1;
                                if (iGroupDx > 10) { break; }
                            }
                        }
                        // Add page region...
                        pageData.Key.Region1 = -1;
                        pageData.Key.Region2 = -1;
                        pageData.Key.Region3 = -1;
                        pageData.Key.Region4 = -1;
                        pageData.Key.Region5 = -1;
                        pageData.Key.Region6 = -1;
                        pageData.Key.Region7 = -1;
                        pageData.Key.Region8 = -1;
                        pageData.Key.Region9 = -1;
                        pageData.Key.Region10 = -1;
                        if (pageData.Value.PageRegions != null && pageData.Value.PageRegions.Count() > 0)
                        {
                            int iRegionDx = 1;
                            foreach (SiteClaim region in pageData.Value.PageRegions)
                            {
                                if (iRegionDx == 1) pageData.Key.Region1 = region.Id;
                                else if (iRegionDx == 2) pageData.Key.Region2 = region.Id;
                                else if (iRegionDx == 3) pageData.Key.Region3 = region.Id;
                                else if (iRegionDx == 4) pageData.Key.Region4 = region.Id;
                                else if (iRegionDx == 5) pageData.Key.Region5 = region.Id;
                                else if (iRegionDx == 6) pageData.Key.Region6 = region.Id;
                                else if (iRegionDx == 7) pageData.Key.Region7 = region.Id;
                                else if (iRegionDx == 8) pageData.Key.Region8 = region.Id;
                                else if (iRegionDx == 9) pageData.Key.Region9 = region.Id;
                                else if (iRegionDx == 10) pageData.Key.Region10 = region.Id;
                                iRegionDx += 1;
                                if (iRegionDx > 10) { break; }
                            }
                        }
                        // Add page category...
                        pageData.Key.Category1 = -1;
                        pageData.Key.Category2 = -1;
                        pageData.Key.Category3 = -1;
                        pageData.Key.Category4 = -1;
                        pageData.Key.Category5 = -1;
                        pageData.Key.Category6 = -1;
                        pageData.Key.Category7 = -1;
                        pageData.Key.Category8 = -1;
                        pageData.Key.Category9 = -1;
                        pageData.Key.Category10 = -1;
                        if (pageData.Value.PageCategorys != null && pageData.Value.PageCategorys.Count() > 0)
                        {
                            int iCategoryDx = 1;
                            foreach (SiteClaim category in pageData.Value.PageCategorys)
                            {
                                if (iCategoryDx == 1) pageData.Key.Category1 = category.Id;
                                else if (iCategoryDx == 2) pageData.Key.Category2 = category.Id;
                                else if (iCategoryDx == 3) pageData.Key.Category3 = category.Id;
                                else if (iCategoryDx == 4) pageData.Key.Category4 = category.Id;
                                else if (iCategoryDx == 5) pageData.Key.Category5 = category.Id;
                                else if (iCategoryDx == 6) pageData.Key.Category6 = category.Id;
                                else if (iCategoryDx == 7) pageData.Key.Category7 = category.Id;
                                else if (iCategoryDx == 8) pageData.Key.Category8 = category.Id;
                                else if (iCategoryDx == 9) pageData.Key.Category9 = category.Id;
                                else if (iCategoryDx == 10) pageData.Key.Category10 = category.Id;
                                iCategoryDx += 1;
                                if (iCategoryDx > 10) { break; }
                            }
                        }
                        // Add page tag...
                        pageData.Key.Tag1 = -1;
                        pageData.Key.Tag2 = -1;
                        pageData.Key.Tag3 = -1;
                        pageData.Key.Tag4 = -1;
                        pageData.Key.Tag5 = -1;
                        pageData.Key.Tag6 = -1;
                        pageData.Key.Tag7 = -1;
                        pageData.Key.Tag8 = -1;
                        pageData.Key.Tag9 = -1;
                        pageData.Key.Tag10 = -1;
                        if (pageData.Value.PageTags != null && pageData.Value.PageTags.Count() > 0)
                        {
                            int iTagDx = 1;
                            foreach (SiteClaim tag in pageData.Value.PageTags)
                            {
                                if (iTagDx == 1) pageData.Key.Tag1 = tag.Id;
                                else if (iTagDx == 2) pageData.Key.Tag2 = tag.Id;
                                else if (iTagDx == 3) pageData.Key.Tag3 = tag.Id;
                                else if (iTagDx == 4) pageData.Key.Tag4 = tag.Id;
                                else if (iTagDx == 5) pageData.Key.Tag5 = tag.Id;
                                else if (iTagDx == 6) pageData.Key.Tag6 = tag.Id;
                                else if (iTagDx == 7) pageData.Key.Tag7 = tag.Id;
                                else if (iTagDx == 8) pageData.Key.Tag8 = tag.Id;
                                else if (iTagDx == 9) pageData.Key.Tag9 = tag.Id;
                                else if (iTagDx == 10) pageData.Key.Tag10 = tag.Id;
                                iTagDx += 1;
                                if (iTagDx > 10) { break; }
                            }
                        }
                        context.Pages.Add(pageData.Key);
                    }
                    Assert.Equal(pageDatas.Count(), context.SaveChanges());
                }
                // Add pages datas...
                int nbChanges = 0;
                foreach (KeyValuePair<Page, PageDatas> pageData in pageDatas)
                {
                    // Add pages claims...
                    if (pageData.Value.PageClaims != null && pageData.Value.PageClaims.Count() > 0)
                    {
                        // Update pages claims...
                        {
                            foreach (PageClaim pgclaim in pageData.Value.PageClaims)
                            {
                                pgclaim.StringValue = $"{pgclaim.Type}-{pgclaim.PageId}";
                            }
                        }
                        context.PageClaims.AddRange(pageData.Value.PageClaims);
                        nbChanges += pageData.Value.PageClaims.Count();
                    }
                }
                Assert.Equal(nbChanges, context.SaveChanges());
                pageDatas.Clear();
            }
        }

        /// <summary>
        /// Get page.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="region"></param>
        /// <param name="category"></param>
        /// <param name="tag"></param>
        /// <param name="state"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Page __GetPage(Site site, SiteClaim group, SiteClaim region, SiteClaim category, SiteClaim tag, State state, int pos,
            string allgroupsstr, string allregionsstr, string allcategoriesstr, string alltagsstr,
            List<ApplicationUser> users)
        {
            if (group == null
                || region == null
                || category == null
                || tag == null)
            {
                return null;
            }
            string grouptitle = (group.Id == -2) ? "public" : ((group.Id == -1) ? $"{allgroupsstr}" : $"{group.Id}");
            string regiontitle = (region.Id == -1) ? $"{allregionsstr}" : $"{region.Id}";
            string categorytitle = (category.Id == -1) ? $"{allcategoriesstr}" : $"{category.Id}";
            string tagtitle = (tag.Id == -1) ? $"{alltagsstr}" : $"{tag.Id}";
            // Add a page...
            return new Page
            {
                //Id = 0,
                Title = $"| site={site.Id} | group={grouptitle} | region={regiontitle} | category={categorytitle} | tag={tagtitle} | state={state} | pos={pos} |",
                State = state,
                Private = (group.Id == -2) ? false : true,
                PositionInNavigation = pos,
                Controller = "controller",
                Action = "action",
                SiteId = site.Id,
                Creator = users[0],
            };
        }

        /// <summary>
        /// Get page groups.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        private List<PageGroup> __GetPageGroups(Page page, List<SiteClaim> groups)
        {
            if (groups != null && groups.Count() != 0)
            {
                List<PageGroup> pageGroups = new List<PageGroup>();
                foreach (SiteClaim group in groups)
                {
                    if (group.Id > 0)
                    {
                        pageGroups.Add(new PageGroup
                        {
                            Page = page,
                            GroupId = group.Id,
                        });
                    }
                }
                return pageGroups;
            }
            return null;
        }
        
        /// <summary>
        /// Get page regions.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        private List<PageRegion> __GetPageRegions(Page page, List<SiteClaim> regions)
        {
            if (regions != null && regions.Count() != 0)
            {
                List<PageRegion> pageRegions = new List<PageRegion>();
                foreach (SiteClaim region in regions)
                {
                    if (region.Id > 0)
                    {
                        pageRegions.Add(new PageRegion
                        {
                            Page = page,
                            RegionId = region.Id,
                        });
                    }
                }
                return pageRegions;
            }
            return null;
        }

        /// <summary>
        /// Get page categories.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        private List<PageCategory> __GetPageCategories(Page page, List<SiteClaim> categories)
        {
            if (categories != null && categories.Count() != 0)
            {
                List<PageCategory> pageCategorys = new List<PageCategory>();
                foreach (SiteClaim category in categories)
                {
                    if (category.Id > 0)
                    {
                        pageCategorys.Add(new PageCategory
                        {
                            Page = page,
                            CategoryId = category.Id,
                        });
                    }
                }
                return pageCategorys;
            }
            return null;
        }

        /// <summary>
        /// Get page tags.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private List<PageTag> __GetPageTags(Page page, List<SiteClaim> tags)
        {
            if (tags != null && tags.Count() != 0)
            {
                List<PageTag> pageTags = new List<PageTag>();
                foreach (SiteClaim tag in tags)
                {
                    if (tag.Id > 0)
                    {
                        pageTags.Add(new PageTag
                        {
                            Page = page,
                            TagId = tag.Id,
                        });
                    }
                }
                return pageTags;
            }
            return null;
        }

        /// <summary>
        /// Add posts data.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="postDatas"></param>
        private void __AddPostsDatas(AppDbContext context, Dictionary<Post, PostDatas> postDatas)
        {
            if (postDatas != null && postDatas.Count() != 0)
            {
                // Add posts...
                {
                    foreach (KeyValuePair<Post, PostDatas> postData in postDatas)
                    {
                        // Add post group...
                        postData.Key.Group1 = -1;
                        postData.Key.Group2 = -1;
                        postData.Key.Group3 = -1;
                        postData.Key.Group4 = -1;
                        postData.Key.Group5 = -1;
                        postData.Key.Group6 = -1;
                        postData.Key.Group7 = -1;
                        postData.Key.Group8 = -1;
                        postData.Key.Group9 = -1;
                        postData.Key.Group10 = -1;
                        if (postData.Value.PostGroups != null && postData.Value.PostGroups.Count() > 0)
                        {
                            int iGroupDx = 1;
                            foreach(SiteClaim group in postData.Value.PostGroups)
                            {
                                if (iGroupDx == 1) postData.Key.Group1 = group.Id;
                                else if (iGroupDx == 2) postData.Key.Group2 = group.Id;
                                else if (iGroupDx == 3) postData.Key.Group3 = group.Id;
                                else if (iGroupDx == 4) postData.Key.Group4 = group.Id;
                                else if (iGroupDx == 5) postData.Key.Group5 = group.Id;
                                else if (iGroupDx == 6) postData.Key.Group6 = group.Id;
                                else if (iGroupDx == 7) postData.Key.Group7 = group.Id;
                                else if (iGroupDx == 8) postData.Key.Group8 = group.Id;
                                else if (iGroupDx == 9) postData.Key.Group9 = group.Id;
                                else if (iGroupDx == 10) postData.Key.Group10 = group.Id;
                                iGroupDx += 1;
                                if (iGroupDx > 10) { break; }
                            }
                        }
                        // Add post region...
                        postData.Key.Region1 = -1;
                        postData.Key.Region2 = -1;
                        postData.Key.Region3 = -1;
                        postData.Key.Region4 = -1;
                        postData.Key.Region5 = -1;
                        postData.Key.Region6 = -1;
                        postData.Key.Region7 = -1;
                        postData.Key.Region8 = -1;
                        postData.Key.Region9 = -1;
                        postData.Key.Region10 = -1;
                        if (postData.Value.PostRegions != null && postData.Value.PostRegions.Count() > 0)
                        {
                            int iRegionDx = 1;
                            foreach (SiteClaim region in postData.Value.PostRegions)
                            {
                                if (iRegionDx == 1) postData.Key.Region1 = region.Id;
                                else if (iRegionDx == 2) postData.Key.Region2 = region.Id;
                                else if (iRegionDx == 3) postData.Key.Region3 = region.Id;
                                else if (iRegionDx == 4) postData.Key.Region4 = region.Id;
                                else if (iRegionDx == 5) postData.Key.Region5 = region.Id;
                                else if (iRegionDx == 6) postData.Key.Region6 = region.Id;
                                else if (iRegionDx == 7) postData.Key.Region7 = region.Id;
                                else if (iRegionDx == 8) postData.Key.Region8 = region.Id;
                                else if (iRegionDx == 9) postData.Key.Region9 = region.Id;
                                else if (iRegionDx == 10) postData.Key.Region10 = region.Id;
                                iRegionDx += 1;
                                if (iRegionDx > 10) { break; }
                            }
                        }
                        // Add post category...
                        postData.Key.Category1 = -1;
                        postData.Key.Category2 = -1;
                        postData.Key.Category3 = -1;
                        postData.Key.Category4 = -1;
                        postData.Key.Category5 = -1;
                        postData.Key.Category6 = -1;
                        postData.Key.Category7 = -1;
                        postData.Key.Category8 = -1;
                        postData.Key.Category9 = -1;
                        postData.Key.Category10 = -1;
                        if (postData.Value.PostCategorys != null && postData.Value.PostCategorys.Count() > 0)
                        {
                            int iCategoryDx = 1;
                            foreach (SiteClaim category in postData.Value.PostCategorys)
                            {
                                if (iCategoryDx == 1) postData.Key.Category1 = category.Id;
                                else if (iCategoryDx == 2) postData.Key.Category2 = category.Id;
                                else if (iCategoryDx == 3) postData.Key.Category3 = category.Id;
                                else if (iCategoryDx == 4) postData.Key.Category4 = category.Id;
                                else if (iCategoryDx == 5) postData.Key.Category5 = category.Id;
                                else if (iCategoryDx == 6) postData.Key.Category6 = category.Id;
                                else if (iCategoryDx == 7) postData.Key.Category7 = category.Id;
                                else if (iCategoryDx == 8) postData.Key.Category8 = category.Id;
                                else if (iCategoryDx == 9) postData.Key.Category9 = category.Id;
                                else if (iCategoryDx == 10) postData.Key.Category10 = category.Id;
                                iCategoryDx += 1;
                                if (iCategoryDx > 10) { break; }
                            }
                        }
                        // Add post tag...
                        postData.Key.Tag1 = -1;
                        postData.Key.Tag2 = -1;
                        postData.Key.Tag3 = -1;
                        postData.Key.Tag4 = -1;
                        postData.Key.Tag5 = -1;
                        postData.Key.Tag6 = -1;
                        postData.Key.Tag7 = -1;
                        postData.Key.Tag8 = -1;
                        postData.Key.Tag9 = -1;
                        postData.Key.Tag10 = -1;
                        if (postData.Value.PostTags != null && postData.Value.PostTags.Count() > 0)
                        {
                            int iTagDx = 1;
                            foreach (SiteClaim tag in postData.Value.PostTags)
                            {
                                if (iTagDx == 1) postData.Key.Tag1 = tag.Id;
                                else if (iTagDx == 2) postData.Key.Tag2 = tag.Id;
                                else if (iTagDx == 3) postData.Key.Tag3 = tag.Id;
                                else if (iTagDx == 4) postData.Key.Tag4 = tag.Id;
                                else if (iTagDx == 5) postData.Key.Tag5 = tag.Id;
                                else if (iTagDx == 6) postData.Key.Tag6 = tag.Id;
                                else if (iTagDx == 7) postData.Key.Tag7 = tag.Id;
                                else if (iTagDx == 8) postData.Key.Tag8 = tag.Id;
                                else if (iTagDx == 9) postData.Key.Tag9 = tag.Id;
                                else if (iTagDx == 10) postData.Key.Tag10 = tag.Id;
                                iTagDx += 1;
                                if (iTagDx > 10) { break; }
                            }
                        }
                        context.Posts.Add(postData.Key);
                    }
                    Assert.Equal(postDatas.Count(), context.SaveChanges());
                }
                // Add posts datas...
                int nbChanges = 0;
                foreach (KeyValuePair<Post, PostDatas> postData in postDatas)
                {
                    // Add posts text...
                    if (postData.Value.PostTexts != null && postData.Value.PostTexts.Count() > 0)
                    {
                        // Update post text...
                        {
                            foreach (PostText psttext in postData.Value.PostTexts)
                            {
                                psttext.Title = $"Title-{psttext.PostId}";
                                psttext.Value = $"Value-{psttext.PostId}";
                            }
                        }
                        context.PostTexts.AddRange(postData.Value.PostTexts);
                        nbChanges += postData.Value.PostTexts.Count();
                    }
                    // Add posts file...
                    if (postData.Value.PostFiles != null && postData.Value.PostFiles.Count() > 0)
                    {
                        // Update post text...
                        {
                            int iDx = 0;
                            foreach (PostFile pstfile in postData.Value.PostFiles)
                            {
                                // Audio...
                                if (iDx == 0)
                                {
                                    pstfile.Type = PostFileType.Audio;
                                    pstfile.Title = $"Title-Audio-{pstfile.PostId}";
                                    pstfile.Url = null;
                                }
                                // Files...
                                else if (iDx == 1)
                                {
                                    pstfile.Type = PostFileType.File;
                                    pstfile.Title = $"Title-File-{pstfile.PostId}";
                                    pstfile.Url = null;
                                }
                                // Photos...
                                else if (iDx == 2)
                                {
                                    pstfile.Type = PostFileType.Photo;
                                    pstfile.Title = $"Title-Photo-{pstfile.PostId}";
                                    pstfile.Url = null;
                                }
                                // Videos...
                                else if (iDx == 3)
                                {
                                    pstfile.Type = PostFileType.Video;
                                    pstfile.Title = $"Title-Video-{pstfile.PostId}";
                                    pstfile.Url = null;
                                }
                                iDx += 1;
                            }
                        }
                        context.PostFiles.AddRange(postData.Value.PostFiles);
                        nbChanges += postData.Value.PostFiles.Count();
                    }
                    // Add posts claims...
                    if (postData.Value.PostClaims != null && postData.Value.PostClaims.Count() > 0)
                    {
                        // Update post claims...
                        {
                            foreach (PostClaim pstclaim in postData.Value.PostClaims)
                            {
                                pstclaim.StringValue = $"{pstclaim.Type}-{pstclaim.PostId}";
                            }
                        }
                        context.PostClaims.AddRange(postData.Value.PostClaims);
                        nbChanges += postData.Value.PostClaims.Count();
                    }
#                   if RC2ISSUE
                    Assert.Equal(nbChanges, context.SaveChanges());
                    nbChanges = 0;
#                   endif
                }
#               if !RC2ISSUE
                Assert.Equal(nbChanges, context.SaveChanges());
#               endif
                postDatas.Clear();
            }
        }

        /// <summary>
        /// Get post.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="region"></param>
        /// <param name="category"></param>
        /// <param name="tag"></param>
        /// <param name="state"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Post __GetPost(Site site, SiteClaim group, SiteClaim region, SiteClaim category, SiteClaim tag, State state, bool highlight,
            string allgroupsstr, string allregionsstr, string allcategoriesstr, string alltagsstr,
            List<ApplicationUser> users)
        {
            if (group == null
                || region == null
                || category == null
                || tag == null)
            {
                return null;
            }
            string grouptitle = (group.Id == -2) ? "public" : ((group.Id == -1) ? $"{allgroupsstr}" : $"{group.Id}");
            string regiontitle = (region.Id == -1) ? $"{allregionsstr}" : $"{region.Id}";
            string categorytitle = (category.Id == -1) ? $"{allcategoriesstr}" : $"{category.Id}";
            string tagtitle = (tag.Id == -1) ? $"{alltagsstr}" : $"{tag.Id}";
            string title = $"| site={site.Id} | group={grouptitle} | region={regiontitle} | category={categorytitle} | tag={tagtitle} | state={state} | highlight={highlight} |";
            // Add a post...
            Post post = new Post
            {
                Title = title,
                //Text = title,
                State = state,
                Private = (group.Id == -2) ? false : true,
                Highlight = highlight,
                SiteId = site.Id,
                Creator = users[0],
                CreationDate = DateTime.Now,
                Cover = (countCover++ % 2 == 0) ? null : "cover",
            };
            if (state != State.NotValided)
            {
                post.ModifiedDate = DateTime.Now;
                post.ValidationDate = DateTime.Now;
            }
            return post;
        }

        /// <summary>
        /// Get post groups.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        private List<PostGroup> __GetPostGroups(Post post, List<SiteClaim> groups)
        {
            if (groups != null && groups.Count() != 0)
            {
                List<PostGroup> postGroups = new List<PostGroup>();
                foreach (SiteClaim group in groups)
                {
                    if (group.Id > 0)
                    {
                        postGroups.Add(new PostGroup
                        {
                            Post = post,
                            GroupId = group.Id,
                        });
                    }
                }
                return postGroups;
            }
            return null;
        }

        /// <summary>
        /// Get post regions.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="regions"></param>
        /// <returns></returns>
        private List<PostRegion> __GetPostRegions(Post post, List<SiteClaim> regions)
        {
            if (regions != null && regions.Count() != 0)
            {
                List<PostRegion> postRegions = new List<PostRegion>();
                foreach (SiteClaim region in regions)
                {
                    if (region.Id > 0)
                    {
                        postRegions.Add(new PostRegion
                        {
                            Post = post,
                            RegionId = region.Id,
                        });
                    }
                }
                return postRegions;
            }
            return null;
        }

        /// <summary>
        /// Get post categories.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        private List<PostCategory> __GetPostCategories(Post post, List<SiteClaim> categories)
        {
            if (categories != null && categories.Count() != 0)
            {
                List<PostCategory> postCategorys = new List<PostCategory>();
                foreach (SiteClaim category in categories)
                {
                    if (category.Id > 0)
                    {
                        postCategorys.Add(new PostCategory
                        {
                            Post = post,
                            CategoryId = category.Id,
                        });
                    }
                }
                return postCategorys;
            }
            return null;
        }

        /// <summary>
        /// Get post tags.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private List<PostTag> __GetPostTags(Post post, List<SiteClaim> tags)
        {
            if (tags != null && tags.Count() != 0)
            {
                List<PostTag> postTags = new List<PostTag>();
                foreach (SiteClaim tag in tags)
                {
                    if (tag.Id > 0)
                    {
                        postTags.Add(new PostTag
                        {
                            Post = post,
                            TagId = tag.Id,
                        });
                    }
                }
                return postTags;
            }
            return null;
        }
    }

    class PageDatas
    {
        public List<SiteClaim> PageGroups { get; set; }
        public List<SiteClaim> PageRegions { get; set; }
        public List<SiteClaim> PageCategorys { get; set; }
        public List<SiteClaim> PageTags { get; set; }
        public List<PageClaim> PageClaims { get; set; }
    }
    
    class PostDatas
    {
        public List<SiteClaim> PostGroups { get; set; }
        public List<SiteClaim> PostRegions { get; set; }
        public List<SiteClaim> PostCategorys { get; set; }
        public List<SiteClaim> PostTags { get; set; }
        public List<PostText> PostTexts { get; set; }
        public List<PostFile> PostFiles { get; set; }
        public List<PostClaim> PostClaims { get; set; }
    }
}
