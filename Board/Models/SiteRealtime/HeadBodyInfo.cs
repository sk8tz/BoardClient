using System.Collections.Generic;

namespace Board.Models.SiteRealtime
{
    public class HeadBodyInfo
    {
        public Dictionary<string, string> Header { get; set; }
        public List<Dictionary<string, string>> Body { get; set; }  
    }
}