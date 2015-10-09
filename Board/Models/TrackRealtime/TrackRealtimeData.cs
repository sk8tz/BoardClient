using System;

using Board.Models.System;

namespace Board.Models.TrackRealtime
{
    public class TrackRealtimeData : ISystemData
    {
        public DateTime DateTime { get; set; }
        public DateTime FiveMinutes { get; set; }

        public string Second { get; set; }
        public string Campaigns { get; set; }
        public string CampaignsName { get; set; }

        #region dimension
        public int Placement { get; set; }
        public string PlacementName { get; set; }
        public int Creative { get; set; }
        public string CreativeName { get; set; }
        public int Media { get; set; }
        public string MediaName { get; set; }
        #endregion

        #region metrics
        public int Uimp { get; set; }
        public int Imp { get; set; }
        public int Clk { get; set; }
        public int Uclk { get; set; }
        public int CtRate { get; set; }
        #endregion
    }
}
