using WPass.Core.Model;
using WPass.DTO;

namespace WPass
{
    /// <summary>
    /// Save data in app session instead of query from db. In order to get record faster
    /// </summary>
    public static class GlobalSession
    {
        public static EntryDto? DefaultEntry { get; set; }
        public static List<EntryDto> EntryDtos { get; set; } = [];
        public static List<BrowserElement> BrowserElements { get; set; } = [];
    }
}
