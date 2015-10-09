using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Board.Controls;
using Board.Enums;

using C1.WPF.FlexGrid;

namespace Board.Common
{
    public class FlexGridCellFactory : CellFactory
    {
        private static readonly Thickness _thicknessEmpty = new Thickness(0);

        public override void CreateCellContent(C1FlexGrid grid, Border bdr, CellRange range)
        {
            try
            {
                var row = grid.Rows[range.Row];
                var column = grid.Columns[range.Column];
                var propertyInfo = column.PropertyInfo;
                if(propertyInfo != null && propertyInfo.Name.Contains("Difference"))
                {
                    // create stock ticker cell
                    TrendTicker ticker = new TrendTicker();
                    bdr.Child = ticker;
                    bdr.Padding = _thicknessEmpty;
                    bdr.HorizontalAlignment = HorizontalAlignment.Stretch;

                    // traditional binding
                    var binding = new Binding(propertyInfo.Name);
                    binding.Source = row.DataItem;
                    binding.Mode = BindingMode.OneWay;
                    ticker.SetBinding(TrendTicker.ValueProperty, binding);
                }
                else
                {
                    // use default implementation
                    base.CreateCellContent(grid, bdr, range);
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("创建单元格内容出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to CreateCellContent", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "CreateCellContent", null);
                }
            }
        }

        public override void CreateColumnHeaderContent(C1FlexGrid grid, Border bdr, CellRange range)
        {
            base.CreateColumnHeaderContent(grid, bdr, range);

            //var row = grid.Rows[range.Row];
            //var column = grid.Columns[range.Column];
            //var propertyInfo = column.PropertyInfo;

            //CustomColumnHeader header =new CustomColumnHeader();


            //Binding nameBinding = new Binding();
            //nameBinding.Path = new PropertyPath("HeaderName");
            //header.SetBinding(CustomColumnHeader.HeaderNameProperty, nameBinding);

            //Binding tooltipBinding = new Binding();
            //tooltipBinding.Path = new PropertyPath("HeaderName");
            //header.SetBinding(CustomColumnHeader.HeaderTooltipProperty, tooltipBinding);


            //bdr.Child = header;
            //bdr.Padding = _thicknessEmpty;
            //bdr.HorizontalAlignment = HorizontalAlignment.Stretch;

            //header.HeaderName = propertyInfo.Name;
            //header.HeaderToolTip = propertyInfo.Name;

            //// traditional binding
            //var binding = new Binding(propertyInfo.Name);
            //binding.Source = grid.ColumnHeaders;
            //binding.Mode = BindingMode.OneWay;
            //header.SetBinding(CustomColumnHeader.HeaderNameProperty, binding);
        }
    }
}
