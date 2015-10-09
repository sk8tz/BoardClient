using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeDetailData : ISystemData
    {
        public string PageViews { get; set; }
        public string Visitors { get; set; }
        public string Time { get; set; }
    }
}