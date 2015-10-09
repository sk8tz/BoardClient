using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class ColumnConfig
    {
        public string HeaderName { get; set; }
        public string HeaderDescription { get; set; }
        public string ColumnName { get; set; }
        public double Width { get; set; }
        public string FormatString { get; set; }
        public string HeaderColorString { get; set; }
    }
}