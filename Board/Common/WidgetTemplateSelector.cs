using System.Windows;
using System.Windows.Controls;

using Board.Models.Control;

namespace Board.Common
{
    public class WidgetTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GridDataTemplate { get; set; }
        public DataTemplate ChartDataTemplate { get; set; }
        public DataTemplate GaugeDataTemplate { get; set; }
        public DataTemplate MapDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dataTemplate = new DataTemplate();
            if ((item as GridItemInfo) != null)
            {
                dataTemplate = GridDataTemplate;
            }
            if ((item as ChartItemInfo) != null)
            {
                dataTemplate = ChartDataTemplate;
            }
            if ((item as GaugeItemInfo) != null)
            {
                dataTemplate = GaugeDataTemplate;
            }
            if ((item as MapItemInfo) != null)
            {
                dataTemplate = MapDataTemplate;
            }
            return dataTemplate;
        }
    }
}