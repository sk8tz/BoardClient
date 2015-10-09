using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeSourceData : ISystemData
    {
        public string SourceType { get; set; }
        public string PageViews { get; set; }
        public string Visitors { get; set; } 
    }
}