using PropertyChanged;

namespace Board.Models.System
{
    [ImplementPropertyChanged]
    public class DisplayTypeModel
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string Type { get; set; }
        public int TypeIndex { get; set; }
        public int IsEnable { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }


        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; }
    }
}