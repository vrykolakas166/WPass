using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPass.Core.Model
{
    public class Website
    {
        [Key]
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Column(TypeName = "TEXT")]
        public string Url { get; set; } = string.Empty;

        public string? EntryId { get; set; }
        public Entry? Entry { get; set; }
    }
}
