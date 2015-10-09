using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeOrganicData : ISystemData
    {
        public string Domain { get; set; } 
        public string PageViews { get; set; }
        public string Visitors { get; set; }
        
    }
}