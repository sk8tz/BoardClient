using Board.Models.System;

namespace Board.Models.SiteRealtime
{
    public class SiteRealtimeInteractionEventData : ISystemData, IInteractionData
    {
        public string Campaign { get; set; }
        public string CampaignName { get; set; }
        public string Media { get; set; }
        public string MediaName { get; set; }
        public string Placement { get; set; }
        public string PlacementName { get; set; }
        public string Keyword { get; set; }
        public string Category { get; set; }
        public string Action { get; set; }
        public string TotalValue { get; set; }
        public string Total { get; set; }
    }
}