using System.Windows;
using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class GaugeItemInfo : IWidgetInfo
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


        public int GaugeType { get; set; }
        public double MaximumValue { get; set; }
        public double MinimumValue { get; set; }
        public double GaugeValue { get; set; }
        public bool KnobVisibility { get; set; }
        public bool LinearVisibility { get; set; }
        public bool RadialVisibility { get; set; }
        public bool RegionKnobVisibility { get; set; }
        public bool RulerVisibility { get; set; }
        public bool SpeedometerVisibility { get; set; }
        public bool VolumeVisibility { get; set; }
    }
}