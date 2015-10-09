using System;

using Board.Models.System;

namespace Board.Models.Sponsor
{
    public class SponsorData : ISystemData
    {
        public int Id { get; set; }
        public int? Year { get; set; }
        public string ProgramType { get; set; }
        public string ProgramTypeEn { get; set; }
        public string Program { get; set; }
        public string TvStation { get; set; }
        public string VideoSet { get; set; }
        public string ProgramEn { get; set; }
        public int? Length { get; set; }
        public DateTime? FirstOnAir { get; set; }
        public DateTime? OnAirTime { get; set; }
        public string Sponsor { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string SponsorEn { get; set; }
        public int? Term { get; set; }
        public string SponsorLevel { get; set; }
        public string SponsorLevelEn { get; set; }
        public int? Weekly { get; set; }
        public decimal? Viewership { get; set; }
        public decimal? Sentiment { get; set; }
        //TVratings、attentionBenchmark
        public decimal? AttentionIndex { get; set; }
        public decimal? AttentionDifference { get; set; }
        //socialMention
        public int? PositivePosts1 { get; set; }
        //maxIndex1、maxWeibo1、functionResult1、weiboBenchmark1、socialIndex、searchCount
        //maxIndex2、maxSearch、functionResult2、searchBenchmark、searchIndex、
        public decimal? EngagementIndex { get; set; }
        public decimal? EngagementDifference { get; set; }
        public decimal? ProgramIndex { get; set; }
        public decimal? ProgramDifference { get; set; }
        public decimal? AssociationAvg { get; set; }
        //recallBenchmark
        public decimal? RecallIndex { get; set; }
        public decimal? RecallDifference { get; set; }
        //socialAssociation、positiveRate2
        public int? PositivePosts2 { get; set; }
        //maxIndex3、maxWeibo3、functionResult3、linkageBenchmark
        public decimal? LinkageIndex { get; set; }
        public decimal? LinkageDifference { get; set; }
        public decimal? AssociationIndex { get; set; }
        public decimal? AssociationDifference { get; set; }
        //postAwareness
        public decimal? PostAwarenessAvg { get; set; }
        //postPreference
        public decimal? PostPreferenceAvg { get; set; }
        //postPurchase
        public decimal? PostPurchaseAvg { get; set; }
        public decimal? PreAwareness { get; set; }
        public decimal? PrePreference { get; set; }
        public decimal? PrePurchase { get; set; }
        //preferenceIndex、maxIndex4、maxValue4、functionResult4、preferenceLiftBenchmark
        public decimal? PreferenceLift { get; set; }
        public decimal? PreferenceDifference { get; set; }
        //purchaseIndex、maxIndex5、maxValue5、functionResult5、purchaseLiftBenchmark
        public decimal? PurchaseLift { get; set; }
        public decimal? PurchaseDifference { get; set; }
        public decimal? CommitmentIndex { get; set; }
        public decimal? CommitmentDifference { get; set; }
        public decimal? Sei { get; set; }
        public decimal? SeiDifference { get; set; }
        public int? SponsorshipFee { get; set; }
        public decimal? Roi { get; set; }
        //sampleNum
    }
}