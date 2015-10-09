using Board.Models.System;

namespace Board.Models.Site
{
    public class SiteData : ISystemData
    {
        public string Campain { get; set; }
        public string Medium { get; set; }
        public string Visits { get; set; }
        public string PageViews { get; set; }
        public string NewVisits { get; set; }
        public string Bounces { get; set; }
        public string Entrances { get; set; }
        public string UniquePageViews { get; set; }
        public string VisitDuration { get; set; }
        public string Exits { get; set; }
        public string PageLoadTime { get; set; }
        public string PageLoadSample { get; set; }
        public string DailyVisitors { get; set; }
    }
}