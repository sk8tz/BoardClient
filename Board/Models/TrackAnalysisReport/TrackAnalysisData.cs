using Board.Models.System;

namespace Board.Models.TrackAnalysisReport
{
    public class TrackAnalysisData: ISystemData
    {
        #region dimensions
        public string Date { get; set; }

        public string Hour { get; set; }

        public string Week { get; set; }

        public string Campaigns { get; set; }

        public string CampaignsName { get; set; }

        public long Media { get; set; }

        public string MediaName { get; set; }

        public long Placement { get; set; }

        public string PlacementName { get; set; }

        public long Creative { get; set; }

        public string CreativeName { get; set; }

        public string Keyword { get; set; }

        public long Province { get; set; }

        public string ProvinceName { get; set; }

        public long City { get; set; }

        public string CityName { get; set; }

        public string TargetUrl { get; set; }

        public string StepIgrp { get; set; }

        public string Gender { get; set; }

        public string ActualChannel { get; set; }

        public string ActualPlay { get; set; }

        public string FiveAges { get; set; }

        #endregion

        #region metrics
        public double Uimp { get; set; }

        public double Imp { get; set; }

        public double Clk { get; set; }

        public double Uclk { get; set; }

        public double CtRate { get; set; }



        public double ClkCompleteRate { get; set; }

        public double EstimateClk { get; set; }

        public double ImpCompleteRate { get; set; }

        public double EstimateImp { get; set; }




        public double Clk1a { get; set; }

        public double Clk1 { get; set; }

        public double Clk2 { get; set; }

        public double Clk3 { get; set; }

        public double Clk4 { get; set; }

        public double Clk5 { get; set; }

        public double Clk6a { get; set; }

        public double Imp1a { get; set; }

        public double Imp1 { get; set; }

        public double Imp2 { get; set; }

        public double Imp3 { get; set; }

        public double Imp4 { get; set; }

        public double Imp5 { get; set; }

        public double Imp6a { get; set; }

        public double Uclk1a { get; set; }

        public double Uclk1 { get; set; }

        public double Uclk2 { get; set; }

        public double Uclk3 { get; set; }

        public double Uclk4 { get; set; }

        public double Uclk5 { get; set; }

        public double Uclk6a { get; set; }

        public double Uimp1a { get; set; }

        public double Uimp1 { get; set; }

        public double Uimp2 { get; set; }

        public double Uimp3 { get; set; }

        public double Uimp4 { get; set; }

        public double Uimp5 { get; set; }

        public double Uimp6a { get; set; }



        public double BounceRate { get; set; }

        public double Bounces { get; set; }

        public double DailyVisitors { get; set; }

        public double Entrances { get; set; }

        public double Exits { get; set; }

        public double LandingRate { get; set; }

        public double NewVisits { get; set; }

        public double PageLoadTime { get; set; }

        public double Pageviews { get; set; }

        public double UniquePageviews { get; set; }

        public double VisitDuration { get; set; }

        public double Visits { get; set; }



        public double MatchedRate { get; set; }

        public double MatchedResolvableImp { get; set; }

        public double MismatchedRate { get; set; }

        public double MismatchedResolvableImp { get; set; }

        public double ResolvableImp { get; set; }
        
        public double Percent { get; set; }

        public double Reach1Rate { get; set; }

        public double Reach2Rate { get; set; }

        public double Reach3Rate { get; set; }

        public double Reach4Rate { get; set; }

        public double Reach5Rate { get; set; }


        public double TaImp { get; set; }
        #endregion 
    }
}