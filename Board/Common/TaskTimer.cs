using System.Windows.Threading;

using Board.Models.System;

namespace Board.Common
{
    public class TaskTimer : DispatcherTimer
    {
        public bool IsModelInitialized { get; set; }
        public WidgetModel WidgetModel { get; set; }
    }
}