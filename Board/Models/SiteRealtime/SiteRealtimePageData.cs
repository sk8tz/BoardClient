using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimePageData : ISystemData
    {
        public string PageUrl { get; set; }
        public string PageTitle { get; set; }  
        public string PageViews { get; set; }
        public string Visitors { get; set; }   
    }
}