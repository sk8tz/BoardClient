using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeProvinceData : ISystemData
    {
        public string Province { get; set; }
        public string PageViews { get; set; }
        public string Visitors { get; set; } 
    }
}