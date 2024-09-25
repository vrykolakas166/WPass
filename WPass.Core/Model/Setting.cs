using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPass.Core.Model
{
    public class Setting
    {
        [Key]
        [StringLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string Key { get; set; } = "Default";
        
        [Column(TypeName = "TEXT")]
        public string Value { get; set; } = "Default";
    }
}
