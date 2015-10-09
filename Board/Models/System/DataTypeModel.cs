using Board.Models.Control;

namespace Board.Models.System
{
    public class DataTypeModel : IItemTypeModel
    {
        public int Id { get; set; }
        public string EnName { get; set; }
        public string CnName { get; set; }
        public string EnHeader { get; set; }
        public string CnHeader { get; set; }
        public int? Interval { get; set; }
        public int SystemTypeId { get; set; }
        public string CnDescription { get; set; }
        public string EnDescription { get; set; }
        public int GroupIndex { get; set; }
        public string Color { get; set; }
        public string Format { get; set; }
        public int LookLevel { get; set; }


        public bool IsChecked { get; set; }
    }
}
