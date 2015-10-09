using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeInteractionHourData : ISystemData
    {
        public string TotalEvents { get; set; }
        public string Hour { get; set; } 
    }
}