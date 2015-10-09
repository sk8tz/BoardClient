using System;

namespace Board.Models.Track
{
    public class CampaignModel
    {
        public string DefaultTarget { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int NetworkId { get; set; }
        public int AdvertiserId { get; set; }
        public int BrandId { get; set; }
        public int IndustryId { get; set; }
        public int AgencyId { get; set; }
        public int CreatorId { get; set; }
        public string TrackType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MediaIds { get; set; }
        public string CostType { get; set; }
        public string SiteMasterLinkCookie { get; set; }
        public string SiteMasterLinkUrl { get; set; }
        public int SiteMasterId { get; set; }
        public string IsDelete { get; set; }
        public string Party_name { get; set; }
        public string Party_email { get; set; }
        public string VisibleImpStatus { get; set; }
        public string Freq_status { get; set; }
        public DateTime  Freq_startDate { get; set; }
        public DateTime Freq_endDate { get; set; }
        public string Igrp_status { get; set; }
        public DateTime Igrp_startDate { get; set; }
        public DateTime  Igrp_endDate { get; set; }
        public string Ta_gender { get; set; }
        public int Ta_minAge { get; set; }
        public int Ta_maxAge { get; set; }
        public int TargetGeos { get; set; }
        public string Contract { get; set; }
        public int Placements { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
