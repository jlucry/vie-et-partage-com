using Models;
using System;
using System.IO;

namespace Services
{
    /// <summary>
    /// Post file.
    /// File can be added to post by any one that have contribution access.
    /// </summary>
    public class JsonPostFile
    {
        public JsonPostFile(PostFile file, Post post)
        {
            // Set...
            if (file != null)
            {
                Id = file.Id;
                Type = file.Type;
                Title = file.Title;
                Url = post.GetFileUrl(file);
                CreatorId = file.CreatorId;
                CreatorName = file.Creator?.UserName ?? "???";
                CreationDate = file.CreationDate;
                CreationDateAge = Convert.ToInt32(DateTime.Now.Subtract(CreationDate).TotalDays);
                ModifiedDate = file.ModifiedDate;
                PostId = file.Post?.Id ?? 0;
            }
        }

        /// <summary>
        /// Post file id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Post file type:
        ///     * cover
        ///     * url
        ///     * file
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Post file title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Post file Url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Post creator.
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Post creator.
        /// </summary>
        public string CreatorName { get; set; }
        /// <summary>
        /// Post file creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Post creation age.
        /// </summary>
        public int CreationDateAge { get; set; }
        /// <summary>
        /// Post file modified date.
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Post of the association.
        /// </summary>
        public  int PostId { get; set; }
    }
}