using System.Collections.ObjectModel;

using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class CheckBoxTab
    {
        public HeaderKeyValue Header { get; set; }
        public ObservableCollection<IItemTypeModel> Items { get; set; }
    }
}