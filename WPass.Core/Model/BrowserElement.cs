using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WPass.Core.Model
{
    /// <summary>
    /// The Name property of the AutomationElement corresponding to an input element would be first derived from the aria-label attribute.
    /// If the aria-label is not present, the Name property would fall back to the text content of the<label> element associated with the input element via the for attribute.
    /// The placeholder attribute would be considered only if neither aria-label nor <label> elements are available.
    /// ----------------
    /// Ordered
    /// 1. aria-label
    /// 2. associated label
    /// 3. placeholder
    /// </summary>
    public class BrowserElement
    {
        [Key]
        [Column(TypeName = "TEXT")]
        public string Name { get; set; } = "Username";
    }
}
