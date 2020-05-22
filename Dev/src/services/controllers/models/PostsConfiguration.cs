using Models;

namespace Services
{
    /// <summary>
    /// Post list configuration.
    /// </summary>
    public class PostsConfiguration
    {
        public string Search { get; set; }
        public State State { get; set; }
        public int Sort { get; set; }
        public int CategoryId { get; set; }
        public int TagId { get; set; }
        public int GroupId { get; set; }
    }
}
