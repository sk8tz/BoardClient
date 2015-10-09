using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeSummaryData : ISystemData
    {
        public string PageViews { get; set; }
        public string Visitors { get; set; }
    }
}