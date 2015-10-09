using System.Collections.ObjectModel;

using Board.Models.System;

using PropertyChanged;

namespace Board.Models.Control
{
    [ImplementPropertyChanged]
    public class DataOrderItem
    {
        public string OrderName { get; set; }
        public ObservableCollection<ResultValue> OrderNames { get; set; }
        public string OrderRule { get; set; }
        public ObservableCollection<ResultValue> OrderRules { get; set; }
    }
}