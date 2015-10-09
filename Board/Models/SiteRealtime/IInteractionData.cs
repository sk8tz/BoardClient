namespace Board.Models.SiteRealtime
{
    public interface IInteractionData
    {
        string Campaign { get; set; }
        string CampaignName { get; set; }
        string Media { get; set; }
        string MediaName { get; set; }
        string Placement { get; set; }
        string PlacementName { get; set; } 
    }
}