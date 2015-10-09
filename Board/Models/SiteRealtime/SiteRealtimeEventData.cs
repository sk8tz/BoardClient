using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeEventData : ISystemData
    {
        public string Category { get; set; }
        public string Action { get; set; }
        public string TotalEvents { get; set; } 
    }
}