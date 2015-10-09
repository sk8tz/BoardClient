using Board.Models.Control;

using PropertyChanged;

namespace Board.Models.System
{
    [ImplementPropertyChanged]
    public class MetricTypeModel : IItemTypeModel
    {
        public int Id { get; set; }
        public string EnName { get; set; }
        public string DisEnName { get; set; }
        public string CnName { get; set; }
        public string EnHeader { get; set; }
        public string CnHeader { get; set; }
        public int SystemTypeId { get; set; }
        public string CnDescription { get; set; }
        public string EnDescription { get; set; }
        public int GroupIndex { get; set; }
        public string Color { get; set; }
        public string Format { get; set; }
        public int LookLevel { get; set; }


        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; }
    }
}