using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;

using Board.Common;
using Board.Controls;
using Board.Enums;
using Board.Models.Control;
using Board.Models.System;
using Board.SystemData.Benchmark;
using Board.Views;

using C1.WPF;
using Caliburn.Micro;

using Newtonsoft.Json;

namespace Board.ViewModels
{
    public class BenchmarkViewModel : Screen, IBaseSystemViewModel
    {
        private List<MetricTypeModel> _selectedMetrics;
        private string _metricString;
        private string _dataOrderString;
        private ObservableCollection<ResultValue> _orderDatasX;
        private ObservableCollection<ResultValue> _orderRulesX;

        #region 属性

        public int ClientId { get; set; }
        public int DataCabinId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SystemTypeEnum SystemType { get; set; }
        public WidgetModel WidgetModelEntity { get; set; }
        public List<MetricTypeModel> MetricsCache { get; set; }


        public int TabSelectedIndex { get; set; }
        public Visibility MultiSelectCategoryVisibility { get; set; }
        public Visibility BrandCategoryVisibility { get; set; }


        public string WidgetTitle { get; set; }
        public int DateTypeId { get; set; }
        public int DataTypeId { get; set; }
        public string SelectedCategory { get; set; }
        public string SelectedCategorys1 { get; set; }
        public string SelectedCategorys2 { get; set; }
        public DisplayTypeModel DisplayType { get; set; }


        public ObservableCollection<DateTypeModel> DateTypeList { get; set; }
        public ObservableCollection<BenchmarkType> BenchmarkTypeList { get; set; }
        public ObservableCollection<Node> MultiSelectCategoryList { get; set; }
        public ObservableCollection<Node> Categorys1List { get; set; }
        public ObservableCollection<Node> Categorys2List { get; set; }
        public ObservableCollection<CheckBoxTab> MetricItemsSource { get; set; }
        public ObservableCollection<DisplayTypeModel> DisplayTypeList { get; set; }

        #endregion

        public BenchmarkViewModel()
        {
            MultiSelectCategoryVisibility = Visibility.Collapsed;
            BrandCategoryVisibility = Visibility.Collapsed;

            SystemType = SystemTypeEnum.Benchmark;
            _selectedMetrics = new List<MetricTypeModel>();
            MultiSelectCategoryList = new ObservableCollection<Node>();
            Categorys1List = new ObservableCollection<Node>();
            Categorys2List = new ObservableCollection<Node>();
            WidgetTitle = "BENCHMARK";

            LoadBaseData();
        }

        private void AssignmentWidgetInfo()
        {
            if (WidgetModelEntity == null)
            {
                DateTypeId = (int)DateTypeEnum.CurrentPeriod;
                DataTypeId = (int)BenchmarkTypeEnum.AllProgram;
                DisplayType = DisplayTypeList.FirstOrDefault();
            }
            else
            {
                WidgetTitle = WidgetModelEntity.Title;
                DateTypeId = WidgetModelEntity.DateTypeId;

                #region 设置指标

                _metricString = WidgetModelEntity.Metrics;
                var metricsx = WidgetModelEntity.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var metricx in metricsx)
                {
                    var selectedMetricx = MetricsCache.Find(m => m.EnName == metricx && m.SystemTypeId == WidgetModelEntity.SystemTypeId);
                    if (selectedMetricx != null)
                    {
                        _selectedMetrics.Add(selectedMetricx);
                    }
                }

                foreach (var metricItems in MetricItemsSource)
                {
                    foreach (var metric in metricItems.Items)
                    {
                        if (metricsx.Contains(metric.EnName))
                        {
                            metric.IsChecked = true;
                        }
                    }
                }

                #endregion

                #region 设置显示类型

                //SetDisplayType();
                var displayType = DisplayTypeList.FirstOrDefault(d => d.Type == WidgetModelEntity.DisplayType && d.TypeIndex == WidgetModelEntity.DisplayTypeIndex);
                DisplayType = displayType;

                #endregion
            }
        }

        private void AssignmentCategory(int index)
        {
            if (DataTypeId == Convert.ToInt32(WidgetModelEntity.DataType))
            {
                var strings = WidgetModelEntity.Filter.Split(new[] {"*"}, StringSplitOptions.RemoveEmptyEntries);
                if (index == 0)
                {
                    SelectedCategorys1 = strings[0];
                }
                else
                {
                    SelectedCategorys2 = strings[1];
                }
            }
        }

        public void SelectedTypeChanged(object sender, EventArgs e)
        {
            var c1ComboBox = sender as C1ComboBox;
            if (c1ComboBox != null)
            {
                DataTypeId = Convert.ToInt32(c1ComboBox.SelectedValue);
                SetTypeData(DataTypeId);
            }
        }

        private void SetTypeData(int typeIndex)
        {
            switch (typeIndex)
            {
                case (int)BenchmarkTypeEnum.AllProgram:
                    MultiSelectCategoryVisibility = Visibility.Collapsed;
                    AssignmentWidgetInfo();
                    break;
                case (int)BenchmarkTypeEnum.DesignatedProgram:
                    MultiSelectCategoryVisibility = Visibility.Visible;
                    BrandCategoryVisibility = Visibility.Collapsed;
                    InitMultiSelectCategory(typeIndex);
                    break;
                case (int)BenchmarkTypeEnum.ProgramType:
                    MultiSelectCategoryVisibility = Visibility.Visible;
                    BrandCategoryVisibility = Visibility.Collapsed;
                    InitMultiSelectCategory(typeIndex);
                    break;
                case (int)BenchmarkTypeEnum.SponsorLevel:
                    MultiSelectCategoryVisibility = Visibility.Visible;
                    BrandCategoryVisibility = Visibility.Collapsed;
                    InitMultiSelectCategory(typeIndex);
                    break;
                case (int)BenchmarkTypeEnum.TvStation:
                    MultiSelectCategoryVisibility = Visibility.Visible;
                    BrandCategoryVisibility = Visibility.Collapsed;
                    InitMultiSelectCategory(typeIndex);
                    break;
                case (int)BenchmarkTypeEnum.VideoSite:
                    MultiSelectCategoryVisibility = Visibility.Visible;
                    BrandCategoryVisibility = Visibility.Collapsed;
                    InitMultiSelectCategory(typeIndex);
                    break;
                case (int)BenchmarkTypeEnum.BrandType:
                    MultiSelectCategoryVisibility = Visibility.Collapsed;
                    BrandCategoryVisibility = Visibility.Visible;
                    InitMultiSelectCategory(typeIndex);
                    break;
            }
        }

        private async void InitMultiSelectCategory(int typeIndex)
        {
            var result = await BenchmarkDataService.GetDataByType(ClientId.ToString(), typeIndex);
            if (result.Value != null)
            {
                var resultString = result.Value;
                switch (typeIndex)
                {
                    case (int)BenchmarkTypeEnum.DesignatedProgram:
                        var programModels = JsonConvert.DeserializeObject<List<ProgramModel>>(resultString);
                        var programList = new ObservableCollection<Node>();
                        foreach (var programModel in programModels)
                        {
                            programList.Add(new Node { Id = programModel.ProgramName, Title = programModel.ProgramName });
                        }

                        MultiSelectCategoryList = programList;
                        if (WidgetModelEntity != null)
                        {
                            SelectedCategory = WidgetModelEntity.Filter;
                        }
                        break;
                    case (int)BenchmarkTypeEnum.ProgramType:
                    case (int)BenchmarkTypeEnum.SponsorLevel:
                    case (int)BenchmarkTypeEnum.TvStation:
                    case (int)BenchmarkTypeEnum.VideoSite:
                        var types = JsonConvert.DeserializeObject<List<string>>(resultString);
                        var typeList = new ObservableCollection<Node>();
                        foreach (var type in types)
                        {
                            typeList.Add(new Node { Id = type, Title = type });
                        }

                        MultiSelectCategoryList = typeList;
                        if (WidgetModelEntity != null)
                        {
                            SelectedCategory = WidgetModelEntity.Filter;
                        }
                        break;
                    case (int)BenchmarkTypeEnum.BrandType:
                        var brandTypes = JsonConvert.DeserializeObject<List<string>>(resultString);
                        var brandTypeList = new ObservableCollection<Node>();
                        foreach (var brandType in brandTypes)
                        {
                            brandTypeList.Add(new Node { Id = brandType, Title = brandType });
                        }

                        Categorys1List = brandTypeList;
                        if (WidgetModelEntity != null)
                        {
                            AssignmentCategory(0);
                        }
                        break;
                }
            }
        }

        public async void SelectedCategory1Changed(object sender, EventArgs e)
        {
            var c1ComboBox = sender as C1ComboBox;
            if (c1ComboBox != null)
            {
                var result = await BenchmarkDataService.GetDataByCategorys1(c1ComboBox.SelectedValue.ToString());
                if (result.Value != null)
                {
                    var resultString = result.Value;
                    var category2s = JsonConvert.DeserializeObject<List<string>>(resultString);
                    var category2List = new ObservableCollection<Node>();
                    foreach (var category2 in category2s)
                    {
                        category2List.Add(new Node { Id = category2, Title = category2 });
                    }

                    Categorys2List = category2List;
                    if (WidgetModelEntity != null)
                    {
                        AssignmentCategory(1);
                    }
                }
            }
        }

        #region IBaseSystemViewModel 成员

        public async void LoadBaseData()
        {
            var dataDictionary = await BenchmarkDataService.GetBaseData();
            var dateTypeList = dataDictionary["DateTypeList"];
            var typeList = dataDictionary["BenchmarkTypeList"];
            var metricHeaderList = dataDictionary["MetricHeaderList"];
            var metricTypeList = dataDictionary["MetricTypeList"];
            var displayTypeList = dataDictionary["DisplayTypeList"];

            InitDateType(dateTypeList);
            InitMetric(metricHeaderList, metricTypeList);
            InitDisplayType(displayTypeList);
            InitBenchmarkType(typeList);

            AssignmentWidgetInfo();
        }

        private void InitDateType(string dateTypeListString)
        {
            try
            {
                var dateTypeList = JsonConvert.DeserializeObject<List<DateTypeModel>>(dateTypeListString);
                var dateTypeItemsSource = new ObservableCollection<DateTypeModel>();
                foreach (var dateType in dateTypeList)
                {
                    dateTypeItemsSource.Add(dateType);
                }

                DateTypeList = dateTypeItemsSource;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化时间类型出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDateType", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDateType", null);
                }
            }
        }

        private void InitBenchmarkType(string typeListString)
        {
            try
            {
                var benchmarkTypeList = JsonConvert.DeserializeObject<List<BenchmarkType>>(typeListString);
                var benchmarkTypeItemsSource = new ObservableCollection<BenchmarkType>();
                foreach (var benchmarkType in benchmarkTypeList)
                {
                    benchmarkTypeItemsSource.Add(benchmarkType);
                }

                BenchmarkTypeList = benchmarkTypeItemsSource;
                if (WidgetModelEntity != null)
                {
                    DataTypeId = Convert.ToInt32(WidgetModelEntity.DataType);
                }
                else
                {
                    DateTypeId = DateTypeList.FirstOrDefault().Id;
                    DataTypeId = BenchmarkTypeList.FirstOrDefault().Id;
                    DisplayType = DisplayTypeList.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化Benchmark类型出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDateType", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDateType", null);
                }
            }
        }

        private void InitMetric(string metricHeaderListString, string metricTypeListString)
        {
            try
            {
                var metricHeaderList = JsonConvert.DeserializeObject<List<HeaderKeyValue>>(metricHeaderListString);
                var metricTypeList = JsonConvert.DeserializeObject<List<MetricTypeModel>>(metricTypeListString);
                var metricItemsSource = new ObservableCollection<CheckBoxTab>();
                foreach (var header in metricHeaderList)
                {
                    var checkBoxTab = new CheckBoxTab
                    {
                        Header = header,
                        Items = new ObservableCollection<IItemTypeModel>()
                    };

                    foreach (var metricType in metricTypeList)
                    {
                        if (metricType.CnHeader.Contains(header.CnHeader))
                        {
                            metricType.IsEnabled = true;
                            checkBoxTab.Items.Add(metricType);
                        }
                    }

                    metricItemsSource.Add(checkBoxTab);
                }

                MetricItemsSource = metricItemsSource;

                BenchmarkView view = GetView() as BenchmarkView;
                if (view != null)
                {
                    view.MetricTabControl.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化指标出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitMetric", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitMetric", null);
                }
            }
        }

        private void InitDisplayType(string displayTypeListString)
        {
            try
            {
                var displayTypeList = JsonConvert.DeserializeObject<List<DisplayTypeModel>>(displayTypeListString);
                var displayItemsSource = new ObservableCollection<DisplayTypeModel>();
                foreach (var displayType in displayTypeList)
                {
                    displayItemsSource.Add(displayType);
                }

                DisplayTypeList = displayItemsSource;

                SetDisplayTypeEnabled("Grid,Bar,Column");
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化显示类型出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDisplayType", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDisplayType", null);
                }
            }
        }

        public void MetricClick(MetricTypeModel metric)
        {
            try
            {
                if (metric.IsChecked)
                {
                    _selectedMetrics.Add(metric);
                }
                else
                {
                    _selectedMetrics.Remove(metric);
                }

                //SetDisplayType();
            }
            catch (Exception ex)
            {
                ShowMessage.Show("指标选择出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to MetricClick", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "MetricClick", null);
                }
            }
        }

        //private void SetDisplayType()
        //{
        //    if(_selectedMetrics.Count > 0)
        //    {
        //        SetDisplayTypeEnabled("Grid,Bar,Column");
        //    }
        //}

        public void SetDisplayTypeEnabled(string displayTypeString)
        {
            var displayTypeList = displayTypeString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            DisplayType = DisplayTypeList.FirstOrDefault(d => d.TypeName == displayTypeList[0]);

            foreach (var displayType in DisplayTypeList)
            {
                if (displayTypeList.Contains(displayType.TypeName))
                {
                    displayType.IsEnabled = true;
                }
                else
                {
                    displayType.IsEnabled = false;
                }
            }
        }

        public void MainTabChanged()
        {
            try
            {
                if (TabSelectedIndex == 1)
                {
                    _orderDatasX = new ObservableCollection<ResultValue>();

                    for (var i = 0; i < MetricItemsSource.Count; i++)
                    {
                        foreach (var metric in MetricItemsSource[i].Items)
                        {
                            if (metric.IsChecked)
                            {
                                _orderDatasX.Add(new ResultValue { Key = metric.EnName, Value = metric.CnName });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("标签改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to TabChanged", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "TabChanged", null);
                }
            }
        }

        public void SetDisplayTypeBySelectedConditions()
        {
            //SetDisplayTypeEnabled("ALL");
        }

        public string GetHeader()
        {
            return "ALL";
        }

        public void GetParameters()
        {
            try
            {
                if (WidgetModelEntity == null)
                {
                    WidgetModelEntity = new WidgetModel();
                }

                WidgetModelEntity.Title = WidgetTitle;
                WidgetModelEntity.DataCabinId = DataCabinId;
                WidgetModelEntity.SystemTypeId = (int)SystemTypeEnum.Benchmark;
                WidgetModelEntity.DisplayType = DisplayType.Type;
                WidgetModelEntity.DisplayTypeIndex = DisplayType.TypeIndex;
                WidgetModelEntity.DateTypeId = DateTypeId;
                WidgetModelEntity.DataType = DataTypeId.ToString();
                switch (DataTypeId)
                {
                    case 1:
                        WidgetModelEntity.Dimensions = "program";
                        break;
                    case 2:
                        WidgetModelEntity.Dimensions = "program";
                        break;
                    case 3:
                        WidgetModelEntity.Dimensions = "programType";
                        break;
                    case 4:
                        WidgetModelEntity.Dimensions = "sponsorLevel";
                        break;
                    case 5:
                        WidgetModelEntity.Dimensions = "tvStation";
                        break;
                    case 6:
                        WidgetModelEntity.Dimensions = "videoSet";
                        break;
                    case 7:
                        WidgetModelEntity.Dimensions = "category1";
                        break;
                }
                WidgetModelEntity.StartDate = StartDate;
                WidgetModelEntity.EndDate = EndDate;
                WidgetModelEntity.EnHeader = GetHeader();
                switch (DataTypeId)
                {
                    case (int)BenchmarkTypeEnum.AllProgram:
                        break;
                    case (int)BenchmarkTypeEnum.DesignatedProgram:
                    case (int)BenchmarkTypeEnum.ProgramType:
                    case (int)BenchmarkTypeEnum.SponsorLevel:
                    case (int)BenchmarkTypeEnum.TvStation:
                    case (int)BenchmarkTypeEnum.VideoSite:
                        WidgetModelEntity.Filter = SelectedCategory;
                        break;
                    case (int)BenchmarkTypeEnum.BrandType:
                        WidgetModelEntity.Filter = SelectedCategorys1 + "*" + SelectedCategorys2;
                        break;
                }

                _metricString = string.Empty;
                foreach (var metricItem in MetricItemsSource)
                {
                    foreach (var metric in metricItem.Items)
                    {
                        if (metric.IsChecked)
                        {
                            if (string.IsNullOrEmpty(_metricString))
                            {
                                _metricString = metric.EnName;
                            }
                            else
                            {
                                _metricString += "," + metric.EnName;
                            }
                        }
                    }
                }

                WidgetModelEntity.Metrics = _metricString;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("整理参数出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetParameters", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetParameters", null);
                }
            }
        }

        public bool VerifyParameters()
        {
            try
            {
                if (string.IsNullOrEmpty(WidgetModelEntity.Title))
                {
                    ShowMessage.Show("标题不能为空");
                    return false;
                }

                var typeIndex = Convert.ToInt32(WidgetModelEntity.DataType);
                switch (typeIndex)
                {
                    case (int)BenchmarkTypeEnum.AllProgram:
                        break;
                    case (int)BenchmarkTypeEnum.DesignatedProgram:
                    case (int)BenchmarkTypeEnum.ProgramType:
                    case (int)BenchmarkTypeEnum.SponsorLevel:
                    case (int)BenchmarkTypeEnum.TvStation:
                    case (int)BenchmarkTypeEnum.VideoSite:
                        if (string.IsNullOrEmpty(SelectedCategory))
                        {
                            ShowMessage.Show("类型不能为空");
                            return false;
                        }

                        break;
                    case (int)BenchmarkTypeEnum.BrandType:
                        if (string.IsNullOrEmpty(SelectedCategorys1))
                        {
                            ShowMessage.Show("类型1不能为空");
                            return false;
                        }

                        if (string.IsNullOrEmpty(SelectedCategorys2))
                        {
                            ShowMessage.Show("类型2不能为空");
                            return false;
                        }

                        break;
                }

                if (string.IsNullOrEmpty(WidgetModelEntity.Metrics))
                {
                    ShowMessage.Show("至少选一个指标");
                    return false;
                }

                if (DisplayType == null)
                {
                    ShowMessage.Show("显示类型不能为空");
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("验证参数出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to VerifyParameters", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "VerifyParameters", null);
                }
            }

            return true;
        }

        public void OkClick()
        {
            GetParameters();

            try
            {
                if (VerifyParameters())
                {
                    var eventX = IoC.Get<IEventAggregator>();
                    eventX.PublishOnUIThread(WidgetModelEntity);

                    TryClose();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("点击确定按钮出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to OkClick", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "OkClick", null);
                }
            }
        }

        public void CancleClick()
        {
            TryClose();
        }

        #endregion
    }
}
