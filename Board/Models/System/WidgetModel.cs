using System;

using Board.Common;

using PropertyChanged;

namespace Board.Models.System
{
    [ImplementPropertyChanged]
    public class WidgetModel
    {
        [DoNotNotify]
        public int Id { get; set; }
        public string Title { get; set; }
        [DoNotNotify]
        public int DataCabinId { get; set; }
        [DoNotNotify]
        public int GroupIndex { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int SystemTypeId { get; set; }
        public string TableName { get; set; }
        [DoNotNotify]
        public string DisplayType { get; set; }
        [DoNotNotify]
        public int DisplayTypeIndex { get; set; }
        [DoNotNotify]
        public string TimeDimensions { get; set; }
        [DoNotNotify]
        public string Dimensions { get; set; }
        [DoNotNotify]
        public string DataType { get; set; }
        [DoNotNotify]
        public string Metrics { get; set; }
        [DoNotNotify]
        public string EnHeader { get; set; }
        [DoNotNotify]
        public string Filter { get; set; }
        [DoNotNotify]
        public string DataOrderBy { get; set; }
        public int DataCount { get; set; }
        public int DateTypeId { get; set; }
        [DoNotNotify]
        public DateTime? StartDate { get; set; }
        [DoNotNotify]
        public DateTime? EndDate { get; set; }
        [DoNotNotify]
        public DateTime CreateDate { get; set; }
        [DoNotNotify]
        public TaskTimer WidgetTimer { get; set; }
        [DoNotNotify]
        public string CityCode { get; set; }
        [DoNotNotify]
        public string SiteCode { get; set; }
    }
}