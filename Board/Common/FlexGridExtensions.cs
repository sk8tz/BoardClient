using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Board.Enums;
using Board.Models.Control;

using C1.WPF.FlexGrid;

namespace Board.Common
{
    public static class FlexGridExtensions
    {
        public static ObservableCollection<ColumnConfig> GetBindingColumns(DependencyObject obj)
        {
            return (ObservableCollection<ColumnConfig>)obj.GetValue(BindingColumnsProperty);
        }

        public static void SetBindingColumns(DependencyObject obj, ObservableCollection<ColumnConfig> value)
        {
            obj.SetValue(BindingColumnsProperty, value);
        }

        private static void BindingColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var bindingColumns = e.NewValue as ObservableCollection<ColumnConfig>;
                if(bindingColumns != null)
                {
                    var c1FlexGrid = d as C1FlexGrid;
                    if(c1FlexGrid != null)
                    {
                        //List<string> list = new List<string>
                        //{
                        //    "Viewership",
                        //    "Sentiment",
                        //    "AssociationAvg",
                        //    "PreAwareness",
                        //    "PostAwarenessAvg"
                        //};

                        C1FlexGridFilter filter = new C1FlexGridFilter();

                        c1FlexGrid.Columns.Clear();
                        foreach(var bindingColumn in bindingColumns)
                        {
                            var column = new Column
                            {
                                Header = bindingColumn.HeaderName,
                                Binding = new Binding(bindingColumn.ColumnName),
                                Width = new GridLength(bindingColumn.Width),
                                Format = bindingColumn.FormatString,
                                ToolTip = bindingColumn.HeaderDescription,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                HeaderBackground =
                                    new SolidColorBrush(ColorConverter.ConvertFromString(bindingColumn.HeaderColorString) is Color ? (Color)ColorConverter.ConvertFromString(bindingColumn.HeaderColorString) : Colors.White)
                            };
                            c1FlexGrid.Columns.Add(column);
                            //if(list.Contains(bindingColumn.ColumnName))
                            //{
                            //    filter.GetColumnFilter(column);
                            //    var cf13 = filter.GetColumnFilter(column);
                            //    cf13.FilterType = FilterType.Value;

                            //    filter.Apply();
                            //}
                        }

                        #region 设置双列头

                        //var ch = c1FlexGrid.ColumnHeaders;
                        //ch.Rows.Add(new Row());

                        //ch[0, 0] = "省份城市";
                        //ch[1, 0] = "省名";

                        //ch[0, 1] = "省份城市";
                        //ch[1, 1] = "城市";

                        //// allow merging the first fixed row
                        //ch.Rows[0].AllowMerging = true;

                        #endregion

                        //c1FlexGrid.AutoSizeColumns(0, c1FlexGrid.Columns.Count - 1, 0);

                        c1FlexGrid.CellFactory = new FlexGridCellFactory();

                        filter.Owner = c1FlexGrid;
                    }
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("列绑定出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to BindingColumnsChanged", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "BindingColumnsChanged", null);
                }
            }
        }

        public static int GetBindingFrozenColumns(DependencyObject obj)
        {
            return (int)obj.GetValue(BindingFrozenColumnsProperty);
        }

        public static void SetBindingFrozenColumns(DependencyObject obj, int value)
        {
            obj.SetValue(BindingFrozenColumnsProperty, value);
        }

        private static void BindingFrozenColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue != null)
            {
                var c1FlexGrid = d as C1FlexGrid;
                if(c1FlexGrid != null)
                {
                    c1FlexGrid.FrozenColumns = Convert.ToInt32(e.NewValue);
                }
            }
        }

        // Using a DependencyProperty as the backing store for BindingColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindingColumnsProperty =
            DependencyProperty.RegisterAttached("BindingColumns", typeof(ObservableCollection<ColumnConfig>),
                typeof(FlexGridExtensions), new PropertyMetadata(null, BindingColumnsChanged));

        // Using a DependencyProperty as the backing store for BindingFrozenColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindingFrozenColumnsProperty =
            DependencyProperty.RegisterAttached("BindingFrozenColumns", typeof(int), typeof(FlexGridExtensions),
                new PropertyMetadata(0, BindingFrozenColumnsChanged));
    }
}