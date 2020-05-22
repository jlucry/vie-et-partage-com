// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Services
{
    /// <summary>
    /// Represents a post.
    /// </summary>
    public class JsonPost
    {
        /// <summary>
        /// Json post contructor.
        /// </summary>
        /// <param name="error"></param>
        public JsonPost(string error)
        {
            Error = (string.IsNullOrEmpty(error) == true)
                ? UserMessage.UnexpectedError
                : error;
        }

        /// <summary>
        /// Json post contructor.
        /// </summary>
        /// <param name="AppContext"></param>
        /// <param name="post"></param>
        /// <param name="forEdition"></param>
        [JsonConstructor]
        public JsonPost(Services.WcmsAppContext AppContext, Post post = null, bool forEdition = false)
        {
            ICollection<PostGroup> postGroups = null;
            ICollection<PostRegion> postRegions = null;
            ICollection<PostCategory> postCategorys = null;
            ICollection<PostTag> postTags = null;

            // Set...
            if (post != null)
            {
                Id = post.Id;
                Title = post.Title;
                State = post.State;
                Private = post.Private;
                Cover = post.GetCoverUrl();
                CoverCrop = post.GetCoverUrl().Replace("cover", "cover.crop");
                Text = post.Text;
                CreatorId = post.CreatorId;
                CreatorName = post.Creator?.UserName ?? "???";
                CreationDate = post.CreationDate;
                ModifiedDate = post.ModifiedDate;
                ValidationDate = post.ValidationDate;
                // Age...
                {
                    TimeSpan ts = (ModifiedDate != null)
                        ? DateTime.Now.Subtract(ModifiedDate.Value)
                        : DateTime.Now.Subtract(CreationDate);
                    if (ts.TotalDays < 30)
                    {
                        Age = Convert.ToInt32(ts.TotalDays);
                        AgeUnit = (Age <= 1) ? "jour" : "jours";
                    }
                    else if (ts.TotalDays < 365)
                    {
                        Age = Convert.ToInt32(ts.TotalDays) / 30;
                        AgeUnit = "mois";
                    }
                    else
                    {
                        Age = Convert.ToInt32(ts.TotalDays) / 365;
                        AgeUnit = "ans";
                    }
                }
                Highlight = post.Highlight;
                StartDate = post.StartDate;
                StartDateString = post.StartDate?.ToString("dd/MM/yyyy HH:mm");
                EndDate = post.EndDate;
                EndDateString = post.EndDate?.ToString("dd/MM/yyyy HH:mm");
#               if !DENORMALIZE
                if ((postGroups = post.PostGroups)) != null)
#               else
                if ((postGroups = post.PostGroups(AppContext)) != null)
#               endif
                {
                    PostGroups = new List<JsonPostGroup>();
                    if (forEdition == false)
                    {
                        foreach (PostGroup group in postGroups)
                        {
                            PostGroups.Add(new JsonPostGroup(group));
                        }
                    }
                    else
                    {
                        ICollection<SiteClaim> userGroups = AppContext.Site.GetGroups(AppContext?.User?.GroupsId());
                        if (userGroups != null)
                        {
                            foreach (SiteClaim stGrp in userGroups)
                            {
                                JsonPostGroup pstGrp = new JsonPostGroup(post, stGrp);
                                foreach (PostGroup group in postGroups)
                                {
                                    if (group.GroupId == stGrp.Id)
                                    {
                                        pstGrp.Checked = true;
                                        break;
                                    }
                                }
                                PostGroups.Add(pstGrp);
                            }
                        }
                    }
                }
#               if !DENORMALIZE
                if ((postRegions = post.PostRegions) != null)
#               else
                if ((postRegions = post.PostRegions(AppContext)) != null)
#               endif
                {
                    PostRegions = new List<JsonPostRegion>();
                    if (forEdition == false)
                    {
                        foreach (PostRegion region in postRegions)
                        {
                            PostRegions.Add(new JsonPostRegion(region));
                        }
                    }
                    else {
                        foreach (SiteClaim stReg in AppContext.Site.GetRegions())
                        {
                            JsonPostRegion pstReg = new JsonPostRegion(post, stReg);
                            foreach (PostRegion region in postRegions)
                            {
                                if (region.RegionId == 0 || region.RegionId == stReg.Id)
                                {
                                    pstReg.Checked = true;
                                    break;
                                }
                            }
                            PostRegions.Add(pstReg);
                        }
                    }
                }
#               if !DENORMALIZE
                if ((postCategorys = post.PostCategorys) != null)
#               else
                if ((postCategorys = post.PostCategorys(AppContext)) != null)
#               endif
                {
                    PostCategorys = new List<JsonPostCategory>();
                    if (forEdition == false)
                    {
                        foreach (PostCategory cat in postCategorys)
                        {
                            PostCategorys.Add(new JsonPostCategory(cat));
                        }
                    }
                    else {
                        List<JsonSiteClaim> flatCat = JsonSiteClaim.ToFlatList(AppContext?.Site?.GetCategories(null, true)?.Select(cat => new JsonSiteClaim(cat))?.ToList(), new List<JsonSiteClaim>());
                        if (flatCat != null)
                        {
                            foreach (JsonSiteClaim stCat in flatCat)
                            {
                                JsonPostCategory pstCat = new JsonPostCategory(post, stCat);
                                foreach (PostCategory cat in postCategorys)
                                {
                                    if (cat.CategoryId == stCat.Id)
                                    {
                                        pstCat.Checked = true;
                                        break;
                                    }
                                }
                                PostCategorys.Add(pstCat);
                            }
                        }
                    }
                }
#               if !DENORMALIZE
                if ((postTags = post.PostTags) != null)
#               else
                if ((postTags = post.PostTags(AppContext)) != null)
#               endif
                {
                    PostTags = new List<JsonPostTag>();
                    if (forEdition == false)
                    {
                        foreach (PostTag tag in postTags)
                        {
                            PostTags.Add(new JsonPostTag(tag));
                        }
                    }
                    else
                    {
                        foreach (SiteClaim stTag in AppContext.Site.GetTags())
                        {
                            JsonPostTag pstTag = new JsonPostTag(post, stTag);
                            foreach (PostTag tag in postTags)
                            {
                                if (tag.TagId == stTag.Id)
                                {
                                    pstTag.Checked = true;
                                    break;
                                }
                            }
                            PostTags.Add(pstTag);
                        }
                    }
                }
                if (post.PostTexts != null)
                {
                    //PostTexts = new List<JsonPostText>();
                    foreach (PostText tag in post.PostTexts)
                    {
                        //PostTexts.Add(new JsonPostText(tag));
                        if (tag.Type == PostTextType.Contain)
                            TextContain = WebUtility.HtmlDecode(tag.Value);
                        //else if (tag.Type == PostTextType.Address)
                        //    TextAddress = WebUtility.HtmlDecode(tag.Value);
                    }
                }
                if (post.PostFiles != null)
                {
                    foreach (PostFile file in post.PostFiles)
                    {
                        if (file.Type == PostFileType.Photo)
                        {
                            if (PostImg == null)
                            {
                                PostImg = new List<JsonPostFile>();
                            }
                            PostImg.Add(new JsonPostFile(file, post));
                            //// TODO: Seulement pour test...
                            //if (PostMediaVideo == null)
                            //{
                            //    PostMediaVideo = new List<JsonPostFile>();
                            //}
                            //PostMediaVideo.Add(new JsonPostFile(file));
                            //if (PostMedia == null)
                            //{
                            //    PostMedia = new List<JsonPostFile>();
                            //}
                            //PostMedia.Add(new JsonPostFile(file));
                            //if (PostFiles == null)
                            //{
                            //    PostFiles = new List<JsonPostFile>();
                            //}
                            //PostFiles.Add(new JsonPostFile(file));
                        }
                        else if (file.Type == PostFileType.Video)
                        {
                            //TODO: Les vidéos you tube ne sont pas lues...
                            if (PostMediaVideo == null)
                            {
                                PostMediaVideo = new List<JsonPostFile>();
                            }
                            PostMediaVideo.Add(new JsonPostFile(file, post));
                        }
                        else if (file.Type == PostFileType.Audio)
                        {
                            if (PostMedia == null)
                            {
                                PostMedia = new List<JsonPostFile>();
                            }
                            PostMedia.Add(new JsonPostFile(file, post));
                        }
                        else if (file.Type != PostFileType.Cover)
                        {
                            if (PostFiles == null)
                            {
                                PostFiles = new List<JsonPostFile>();
                            }
                            PostFiles.Add(new JsonPostFile(file, post));
                        }
                    }
                }
                if (post.PostClaims != null)
                {
                    //PostClaims = new List<JsonPostClaim>();
                    foreach (PostClaim claim in post.PostClaims)
                    {
                        //PostClaims.Add(new JsonPostClaim(claim));
                        if (claim.Type == PostClaimType.Registration
                            && claim.StringValue.ToLower() == "yes")
                        {
                            ClaimRegistration = true;
                        }
                        else if (claim.Type == PostClaimType.RegistrationWebConfirmation)
                        {
                            RegistrationWebConfirmation = claim.StringValue;
                        }
                        else if (claim.Type == PostClaimType.RegistrationEmailConfirmation)
                        {
                            RegistrationEmailConfirmation = claim.StringValue;
                        }
                        else if (claim.Type == PostClaimType.RegistrationField
                            && string.IsNullOrEmpty(claim.StringValue) == false)
                        {
                            //Bug: EF Migrations, Insert value for Text Data Type always has (Size=255
                            //https://github.com/SapientGuardian/SapientGuardian.EntityFrameworkCore.MySql/issues/50
                            //Bug: String fields without max length constraint default to varchar(255)
                            //https://github.com/SapientGuardian/SapientGuardian.EntityFrameworkCore.MySql/issues/47
                            RegistrationFields = JsonConvert.DeserializeObject<ICollection<PostRegistrationField>>(claim.StringValue);
                        }
                    }
                }
            }

            // Set default post registration fields...
            DefaultRegistrationFields = PostRegistrationField.DefaultRegistrationFields;
        }

        /// <summary>
        /// Model id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Model title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Model state.
        /// </summary>
        public State State { get; set; }

        /// <summary>
        /// Private access.
        /// When true, access is granted only to users member of the post group.
        /// </summary>
        public bool Private { get; set; }

        /// <summary>
        /// Post cover.
        /// </summary>
        public string Cover { get; set; }
        /// <summary>
        /// Post cover.
        /// </summary>
        public string CoverCrop { get; set; }

        /// <summary>
        /// Post text.
        /// </summary>
        public string Text  { get; set; }

        /// <summary>
        /// Post creator.
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Post creator.
        /// </summary>
        public string CreatorName { get; set; }
        /// <summary>
        /// Post creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Post creation age.
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// Age unit.
        /// </summary>
        public string AgeUnit { get; set; }
        /// <summary>
        /// Post modified date.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
        /// <summary>
        /// Post validation date.
        /// </summary>
        public DateTime? ValidationDate { get; set; }

        /// <summary>
        /// Post highlighted.
        /// </summary>
        public bool Highlight { get; set; }
        /// <summary>
        /// Post start date.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// Post start date.
        /// </summary>
        public string StartDateString { get; set; }
        /// <summary>
        /// Post end date.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Post end date.
        /// </summary>
        public string EndDateString { get; set; }

        /// <summary>
        /// Post groups.
        /// </summary>
        public ICollection<JsonPostGroup> PostGroups { get; set; }
        /// <summary>
        /// Post regions.
        /// </summary>
        public ICollection<JsonPostRegion> PostRegions { get; set; }
        /// <summary>
        /// Post categories.
        /// </summary>
        public ICollection<JsonPostCategory> PostCategorys { get; set; }
        /// <summary>
        /// Post tags.
        /// </summary>
        public ICollection<JsonPostTag> PostTags { get; set; }

        ///// <summary>
        ///// Post texts.
        ///// </summary>
        //public ICollection<JsonPostText> PostTexts { get; set; }
        /// <summary>
        /// Post contain.
        /// </summary>
        public string TextContain { get; set; }
        ///// <summary>
        ///// Post address.
        ///// </summary>
        //public string TextAddress { get; set; }

        /// <summary>
        /// Post img.
        /// </summary>
        public ICollection<JsonPostFile> PostImg { get; set; }
        /// <summary>
        /// Post video.
        /// </summary>
        public ICollection<JsonPostFile> PostMediaVideo { get; set; }
        /// <summary>
        /// Post audio.
        /// </summary>
        public ICollection<JsonPostFile> PostMedia { get; set; }
        /// <summary>
        /// Post files.
        /// </summary>
        public ICollection<JsonPostFile> PostFiles { get; set; }

        ///// <summary>
        ///// Post claims
        ///// </summary>
        //public ICollection<JsonPostClaim> PostClaims { get; set; }

        /// <summary>
        /// Post registration.
        /// </summary>
        public bool ClaimRegistration { get; set; }
        /// <summary>
        /// Registration web confirmation.
        /// </summary>
        public string RegistrationWebConfirmation { get; set; }
        /// <summary>
        /// Registration email confirmation.
        /// </summary>
        public string RegistrationEmailConfirmation { get; set; }
        /// <summary>
        /// Post default registration fields.
        /// </summary>
        public ICollection<PostRegistrationField> DefaultRegistrationFields { get; set; }
        /// <summary>
        /// Post registration fields.
        /// </summary>
        public ICollection<PostRegistrationField> RegistrationFields { get; set; }

        /// <summary>
        /// Error message.
        /// </summary>
        public string Error { get; set; }

        ///// <summary>
        ///// Registrations fields To partial claims (no post and site id).
        ///// </summary>
        ///// <param name="registrationFields"></param>
        ///// <returns></returns>
        //public ICollection<PostClaim> RegistrationFieldsToPartialClaims()
        //{
        //    // Checking...
        //    if (Id == 0 || RegistrationFields == null || RegistrationFields.Count() == 0)
        //    {
        //        return null;
        //    }
        //    ICollection<PostClaim> claims = new List<PostClaim>();
        //    foreach(JsonPostRegistrationField registrationField in RegistrationFields)
        //    {
        //        claims.Add(new PostClaim
        //        {
        //            Type = PostClaimType.RegistrationField,
        //            StringValue = JsonConvert.SerializeObject(registrationField)
        //            //PostId = ...
        //            //SiteId = ... 
        //        });
        //    }
        //    return null;
        //}
    }
}