using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPass.Core.Model
{
    public class Entry
    {
        public Entry()
        {
            Websites = [];
        }

        [Key]
        [StringLength(36)]
        [Column(TypeName = "CHAR")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [StringLength(50)]
        [Column(TypeName = "NVARCHAR")]
        public string Username { get; set; } = "Example user";

        [Column(TypeName = "TEXT")]
        public string EncryptedPassword { get; set; } = "Example password";

        [Column(TypeName = "BIT")]
        public bool IsDefault { get; set; } = false;

        public virtual List<Website> Websites { get; set; }

        [Column(TypeName = "DATETIME")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
