using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeCityData : ISystemData
    {
        public string City { get; set; }
        public string PageViews { get; set; }
        public string Visitors { get; set; }  
    }
}