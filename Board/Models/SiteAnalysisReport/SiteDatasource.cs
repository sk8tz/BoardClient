namespace Board.Models.Site
{
    public class SiteDatasource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Founder { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string HomePage { get; set; }
        public string Adv { get; set; }
        public string Role { get; set; }
        public long TotalPageviews { get; set; }
        public int YesterdayPageviews { get; set; }
        public string Status { get; set; }
    }
}