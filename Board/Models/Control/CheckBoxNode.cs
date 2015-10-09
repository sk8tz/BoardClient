using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class CheckBoxNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}