using System.ComponentModel.DataAnnotations;

namespace WPass.Core.Model
{
    public class Setting
    {
        [Key]
        public string Key { get; set; } = "Default";
        public string Value { get; set; } = "Default";
    }
}
