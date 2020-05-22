// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Services
{
    /// <summary>
    /// Represents a user.
    /// </summary>
    public class JsonUser
    {
        /// <summary>
        /// Json user contructor.
        /// </summary>
        /// <param name="error"></param>
        public JsonUser(string error)
        {
            Error = (string.IsNullOrEmpty(error) == true)
                ? UserMessage.UnexpectedError
                : error;
        }

        /// <summary>
        /// Json user contructor.
        /// </summary>
        /// <param name="AppContext"></param>
        /// <param name="user"></param>
        /// <param name="forEdition"></param>
        [JsonConstructor]
        public JsonUser(Services.WcmsAppContext AppContext, ApplicationUser user = null, bool forEdition = false)
        {
            ICollection<UserGroup> dataGroups = null;
            ICollection<UserRegion> dataRegions = null;

            // Set...
            if (user != null)
            {
                Id = user.Id;
                Email = user.Email;
                Cover = user.GetCoverUrl();
                CoverCrop = user.GetCoverUrl().Replace("cover", "cover.crop");
                Enabled = user.Enabled;
                //Claims = user.Claims;
                if (user.Claims != null)
                {
                    foreach (IdentityUserClaim<string> claim in user.Claims)
                    {
                        if (claim != null)
                        {
                            if (claim.ClaimType == UserClaimType.FirstName)
                            {
                                FName = claim.ClaimValue;
                            }
                            else if (claim.ClaimType == UserClaimType.LastName)
                            {
                                LName = claim.ClaimValue;
                            }
                            else if (claim.ClaimType == UserClaimType.Phone)
                            {
                                Phone = claim.ClaimValue;
                            }
                            else if (claim.ClaimType == UserClaimType.Zip)
                            {
                                Zip = claim.ClaimValue;
                            }
                        }
                    }
                }
                if ((dataGroups = user.UserGroups(AppContext)) != null)
                {
                    UserGroups = new List<JsonUserGroup>();
                    if (forEdition == false)
                    {
                        foreach (UserGroup group in dataGroups)
                        {
                            UserGroups.Add(new JsonUserGroup(group));
                        }
                    }
                    else
                    {
                        ICollection<SiteClaim> userGroups = AppContext.Site.GetGroups(AppContext?.User?.GroupsId());
                        if (userGroups != null)
                        {
                            foreach (SiteClaim stGrp in userGroups)
                            {
                                JsonUserGroup pstGrp = new JsonUserGroup(user, stGrp);
                                foreach (UserGroup group in dataGroups)
                                {
                                    if (group.GroupId == stGrp.Id)
                                    {
                                        pstGrp.Checked = true;
                                        break;
                                    }
                                }
                                UserGroups.Add(pstGrp);
                            }
                        }
                    }
                }
                if ((dataRegions = user.UserRegions(AppContext)) != null)
                {
                    UserRegions = new List<JsonUserRegion>();
                    if (forEdition == false)
                    {
                        foreach (UserRegion region in dataRegions)
                        {
                            UserRegions.Add(new JsonUserRegion(region));
                        }
                    }
                    else {
                        foreach (SiteClaim stReg in AppContext.Site.GetRegions())
                        {
                            JsonUserRegion pstReg = new JsonUserRegion(user, stReg);
                            foreach (UserRegion region in dataRegions)
                            {
                                if (region.RegionId == 0 || region.RegionId == stReg.Id)
                                {
                                    pstReg.Checked = true;
                                    break;
                                }
                            }
                            UserRegions.Add(pstReg);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Model id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Model FName.
        /// </summary>
        public string FName { get; set; }

        /// <summary>
        /// Model LName.
        /// </summary>
        public string LName { get; set; }

        /// <summary>
        /// Model Email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Model Phone.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Model Zip.
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// User cover.
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// User cover.
        /// </summary>
        public string CoverCrop { get; set; }
        
        /// <summary>
        /// User claims.
        /// </summary>
        //public IdentityUserClaim<string> Claims { get; set; }

        /// <summary>
        /// User groups.
        /// </summary>
        public ICollection<JsonUserGroup> UserGroups { get; set; }
        /// <summary>
        /// User regions.
        /// </summary>
        public ICollection<JsonUserRegion> UserRegions { get; set; }

        public bool Enabled { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Error { get; set; }
    }
}