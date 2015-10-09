using System.Windows;
using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class MapItemInfo : IWidgetInfo
    {
        public int Id { get; set; }
        public string ItemTitle { get; set; }
        [DoNotNotify]
        public double ItemWidth { get; set; }
        [DoNotNotify]
        public double ItemHeight { get; set; }
        public int GroupIndex { get; set; }
        public bool CanSetting { get; set; }
        public bool CanCopy { get; set; }
        public bool CanExportExcel { get; set; }
        public bool CanExportPicture { get; set; }
        public bool CanLookOriginalData { get; set; }
        public bool CanDelete { get; set; }
        public bool IsLoading { get; set; }
        public Visibility IsNoAuthorization { get; set; }
    }
}