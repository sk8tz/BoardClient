using System.Collections.ObjectModel;

namespace Board.Models.Control
{
    public class MyReport
    {
        public ObservableCollection<C1Data> Data { get; set; }
        public string Label { get; set; }
    }
}