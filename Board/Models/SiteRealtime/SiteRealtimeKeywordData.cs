using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeKeywordData : ISystemData
    {
        public string Keyword { get; set; }
        public string PageViews { get; set; }
        public string Visitors { get; set; } 
    }
}