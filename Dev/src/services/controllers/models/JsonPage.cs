// !!!Because of issues with Join in EF core denormalize group, region, category and tag tables!!! //
#define DENORMALIZE

using Models;
using System;
using System.Collections.Generic;

namespace Services
{
    /// <summary>
    /// Json page.
    /// </summary>
    public class JsonPage
    {
        private readonly Page _page = null;
        private List<JsonPage> _childs = null;

        /// <summary>
        /// Json page contructor.
        /// </summary>
        public JsonPage(Page page)
        {
            _page = page;
        }

        /// <summary>
        /// Model id.
        /// </summary>
        public int Id { get { return _page?.Id ?? 0; } }

        /// <summary>
        /// Model title.
        /// </summary>
        public string Title { get { return _page?.Title; } }

        /// <summary>
        /// Model state.
        /// </summary>
        public State State { get { return _page?.State ?? State.NotValided; } }

        /// <summary>
        /// Private access.
        /// When true, access is granted only to users member of the post group.
        /// </summary>
        public bool Private { get { return _page?.Private ?? true; } }
        
        /// <summary>
        /// Position in the navigation tree.
        /// 0 if the page should not be inserted in the navigation tree.
        /// </summary>
        public int PositionInNavigation { get { return _page?.PositionInNavigation ?? -1; } }

        /// <summary>
        /// Model controller.
        /// </summary>
        public string Controller { get { return _page?.Controller; } }

        /// <summary>
        /// Model action.
        /// </summary>
        public string Action { get { return _page?.Action; } }

        /// <summary>
        /// Page claims:
        ///     * group
        ///     * region
        ///     * categorie
        ///     * tag
        /// </summary>
        public ICollection<PageClaim> PageClaims { get; set; }
        /// <summary>
        /// Page groups.
        /// </summary>
        public ICollection<PageGroup> PageGroups { get; set; }
        /// <summary>
        /// Page regions.
        /// </summary>
        public ICollection<PageRegion> PageRegions { get; set; }
        /// <summary>
        /// Page categories.
        /// </summary>
        public ICollection<PageCategory> PageCategorys { get; set; }
        /// <summary>
        /// Page tags.
        /// </summary>
        public ICollection<PageTag> PageTags { get; set; }

        ///// <summary>
        ///// Post creator.
        ///// </summary>
        public string CreatorName { get { return _page?.Creator?.UserName ?? "!!!"/*string.Empty*/; } }
        /// <summary>
        /// Post creation date.
        /// </summary>
        public DateTime CreationDate { get { return _page?.CreationDate ?? new DateTime(0); } }
        /// <summary>
        /// Post creation age.
        /// </summary>
        public int CreationDateAge { get { return Convert.ToInt32(DateTime.Now.Subtract(CreationDate).TotalDays); } }
        /// <summary>
        /// Post modified date.
        /// </summary>
        public DateTime? ModifiedDate { get { return _page?.ModifiedDate; } }
        
        /// <summary>
        /// Parent page.
        /// </summary>
        public int ParentId { get { return _page?.ParentId ?? 0; } }
        /// <summary>
        /// Childs pages.
        /// </summary>
        public IEnumerable<JsonPage> Childs
        {
            get
            {
                if (_childs == null
                    && _page?.Childs != null)
                {
                    _childs = new List<JsonPage>();
                    foreach (Page page in _page.Childs)
                    {
                        _childs.Add(new JsonPage(page));
                    }
                }
                return _childs;
            }
        }
    }
}
