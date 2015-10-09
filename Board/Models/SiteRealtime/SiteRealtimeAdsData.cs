using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    

    public class SiteRealtimeAdsData : ISystemData
    {
        public string Placement { get; set; }
        public string PageViews { get; set; }
        public string Visitors { get; set; }   
    }
}