using System.ComponentModel.DataAnnotations;

namespace WPass.Core.Model
{
    public class Website
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Url { get; set; } = string.Empty;

        public string? EntryId { get; set; }
        public Entry? Entry { get; set; }
    }
}
