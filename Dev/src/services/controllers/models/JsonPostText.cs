using Models;
using System.Net;

namespace Services
{
    /// <summary>
    /// Post text.
    /// </summary>
    public class JsonPostText
    {
        public JsonPostText(PostText text)
        {
            // Set...
            if (text != null)
            {
                Id = text.Id;
                Type = text.Type;
                Title = text.Title;
                Number = text.Number;
                Revision = text.Revision;
                //TODO: When inserting a new post, don't encode the text,
                //      not usefull since we're decoding it before send to the client.
                //      Vérifier que les données ne sont pas deja encodé quand elle sont reçut du client d'edition.
                Value = WebUtility.HtmlDecode(text.Value);
                PostId = text.Post?.Id ?? 0;
            }
        }

        /// <summary>
        /// Post text id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Post text type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Post text title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Post text number.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Post text revision.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Post text value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Post of the association.
        /// </summary>
        public  int PostId { get; set; }
    }
}