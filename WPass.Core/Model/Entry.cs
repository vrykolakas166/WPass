using System.ComponentModel.DataAnnotations;

namespace WPass.Core.Model
{
    public class Entry
    {
        public Entry()
        {
            Websites = [];
        }

        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = "Example user";
        public string EncryptedPassword { get; set; } = "Example password";
        
        public virtual List<Website> Websites { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
