using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;

using Board.Common;
using Board.Controls;
using Board.Enums;
using Board.Models.Control;
using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Models.Users;
using Board.Services.System;
using Board.Services.TargetAudience;
using Board.SystemData.Base;
using Board.SystemData.TrackAnalysisReport;
using Board.Views;

using Caliburn.Micro;

using Newtonsoft.Json;

using RestSharp;

namespace Board.ViewModels
{
    public class WidgetViewModel : Screen
    {
        #region

        private const string _trackRealtimeDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed";
        private const string _trackAnalysisReportOtherWithTimeDimensionDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed";
        private const string _trackAnalysisReportOtherWithoutTimeDimensionDisplayType1 = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut";
        private const string _trackAnalysisReportOtherWithoutTimeDimensionDisplayType2 = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut,XyPlot,Radar,RadarFilled";
        private const string _trackAnalysisReportOtherWithoutTimeDimensionDisplayType3 = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut,Bubble,Radar,RadarFilled";
        private const string _trackAnalysisReportOtherWithoutTimeDimensionDisplayType4And5 = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut,Radar,RadarFilled";
        private const string _trackAnalysisReportOtherWithoutTimeDimensionDisplayTypeMore5 = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut";
        private const string _trackAnalysisReportIgrpDisplayType = "BarStacked100Pc,ColumnStacked100Pc,LineStacked100Pc,LineSymbolsStacked100Pc,AreaStacked100Pc";
        private const string _trackAnalysisReportPanelsDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Pie,PieDoughnut,Radar,RadarFilled";
        private const string _trackAnalysisReportFreqsWithMetricDisplayType1 = "Grid,Pie,PieDoughnut";
        private const string _trackAnalysisReportFreqsWithMetricDisplayType2 = "Bar,Column,Line,LineSmoothed,LineSymbols,LineSymolsSmoothed,Area,AreaSmoothed,Radar,RadarFilled";
        private const string _trackAnalysisReportFreqsWithMetricFrequencyDisplayType = "BarStacked100Pc,ColumnStacked100Pc,LineStacked100Pc,LineSymbolsStacked100Pc,AreaStacked100Pc";
        private const string _siteRealtimeSummaryDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,Area,AreaSmoothed,XyPlot";
        private const string _siteRealtimeEventDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,Area,AreaSmoothed,Pie,PieDoughnut";
        private const string _siteRealtimeOtherDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,Area,AreaSmoothed,XyPlot,Pie,PieDoughnut";
        private const string _siteRealtimeTransformSummaryDisplayType = "Grid,Bar,Column,Line,LineSmoothed,LineSymbols,Area,AreaSmoothed";

        #endregion

        #region 变量

        private string _timeDimensionString;
        private string _dimensionString;
        private string _metricString;
        private string _dataOrderString;
        private List<TimeDimensionTypeModel> _selectedTimeDimensions;
        private List<DimensionTypeModel> _selectedDimensions;
        private List<MetricTypeModel> _selectedMetrics;
        private int _dataTypeCount;
        private string _dataType;

        /// <summary>
        ///排序的数据源
        /// </summary>
        private ObservableCollection<ResultValue> _orderDatasX;

        /// <summary>
        ///排序规则
        /// </summary>
        private ObservableCollection<ResultValue> _orderRulesX;

        #endregion

        #region 属性

        /// <summary>
        /// 选中的城市
        /// </summary>
        public RegionTypeModel SelectedCity { get; set; }

        /// <summary>
        /// 选中的站点
        /// </summary>
        public SiteInfo SelectedSite { get; set; }

        /// <summary>
        /// 城市列表
        /// </summary>
        public ObservableCollection<RegionTypeModel> CityList { get; set; }

        /// <summary>
        /// 站点列表
        /// </summary>
        public ObservableCollection<SiteInfo> SiteList { get; set; }

        /// <summary>
        /// 数据类型选中Index
        /// </summary>
        public int DataTypeTabSelectedIndex { get; set; }

        /// <summary>
        /// 维度选中Index
        /// </summary>
        public int DimensionTabSelectedIndex { get; set; }

        /// <summary>
        /// 指标选中Index
        /// </summary>
        public int MetricTabSelectedIndex { get; set; }

        /// <summary>
        /// 缓存时间维度
        /// </summary>
        public List<TimeDimensionTypeModel> TimeDimensionsCache { get; set; }

        /// <summary>
        /// 缓存维度
        /// </summary>
        public List<DimensionTypeModel> DimensionsCache { get; set; }

        /// <summary>
        /// 数据类型缓存
        /// </summary>
        public List<DataTypeModel> DataTypeCache { get; set; }

        /// <summary>
        /// 缓存指标
        /// </summary>
        public List<MetricTypeModel> MetricsCache { get; set; }


        /// <summary>
        /// 节目数据源
        /// </summary>
        public ObservableCollection<Node> ProgramItems { get; set; }

        /// <summary>
        /// 选中节目字符串
        /// </summary>
        public string ProgramString { get; set; }

        /// <summary>
        /// 当前客户Id
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Tab选中序号
        /// </summary>
        public int TabSelectedIndex { get; set; }

        /// <summary>
        /// 当前日期
        /// </summary>
        public DateTime CurrentDate { get; set; }

        /// <summary>
        /// 传输WidgetModel
        /// </summary>
        public WidgetModel WidgetModelEntity { get; set; }

        /// <summary>
        /// 系统类别
        /// </summary>
        public SystemTypeModel SystemType { get; set; }

        /// <summary>
        /// 开始日期是否可用
        /// </summary>
        public bool StartDateIsEnabled { get; set; }

        /// <summary>
        /// 结束日期是否可用
        /// </summary>
        public bool EndDateIsEnabled { get; set; }

        /// <summary>
        /// 排序项
        /// </summary>
        public DataOrderItem DataOrderItem { get; set; }

        /// <summary>
        /// 排序顺序集合
        /// </summary>
        public ObservableCollection<DataOrderItem> DataOrderItems { get; set; }

        /// <summary>
        /// 数据个数
        /// </summary>
        public int DataCount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 系统类型集合
        /// </summary>
        public ObservableCollection<SystemTypeModel> SystemTypeList { get; set; }

        /// <summary>
        /// 图表类型
        /// </summary>
        public DisplayTypeModel DisplayType { get; set; }

        /// <summary>
        /// 图表类型集合
        /// </summary>
        public ObservableCollection<DisplayTypeModel> DisplayTypeList { get; set; }

        /// <summary>
        /// 指标集合
        /// </summary>
        public ObservableCollection<CheckBoxTab> DimensionItemsSource { get; set; }

        /// <summary>
        /// 维度集合
        /// </summary>
        public ObservableCollection<CheckBoxTab> MetricItemsSource { get; set; }

        /// <summary>
        /// 数据类型集合
        /// </summary>
        public ObservableCollection<CheckBoxTab> DataTypeItemsSource { get; set; }

        /// <summary>
        /// 时间粒度集合
        /// </summary>
        public ObservableCollection<TimeDimensionTypeModel> TimeDimensionTypeList { get; set; }

        /// <summary>
        /// 日期类型集合
        /// </summary>
        public ObservableCollection<DateTypeModel> DateTypeList { get; set; }

        /// <summary>
        /// 时间粒度可见性
        /// </summary>
        public Visibility TimeDimensionVisibility { get; set; }

        /// <summary>
        /// 时间可见性
        /// </summary>
        public Visibility TimeVisibility { get; set; }

        /// <summary>
        /// iGRP城市可见性
        /// </summary>
        public Visibility IgrpCityVisibility { get; set; }

        /// <summary>
        /// 打通站点可见性
        /// </summary>
        public Visibility SiteVisibility { get; set; }

        /// <summary>
        /// 数据类型可见性
        /// </summary>
        public Visibility DataTypeVisibility { get; set; }

        /// <summary>
        /// 维度可见性
        /// </summary>
        public Visibility DimensionVisibility { get; set; }

        /// <summary>
        /// 指标可见性
        /// </summary>
        public Visibility MetricVisibility { get; set; }


        #endregion

        #region TA

        public bool ActivateTa { get; set; }
        public bool AllAgeSelected { get; set; }

        private int minAge = 20;
        private int maxAge = 30;

        public int MinAge
        {
            get
            {
                return this.minAge;
            }

            set
            {
                this.minAge = value;
                if (this.minAge > this.maxAge)
                {
                    this.MaxAge = this.minAge + 1;
                }
            }
        }

        public int MaxAge
        {
            get
            {
                return this.maxAge;
            }

            set
            {
                this.maxAge = value;
                if (this.maxAge < this.minAge)
                {
                    this.MinAge = this.maxAge - 1;
                }
            }
        }

        public ObservableCollection<Node> GenderItems { get; set; }
        public string SelectedGender { get; set; }

        public ObservableCollection<Node> ZoneItems { get; set; }
        public string SelectedZone { get; set; }

        public ObservableCollection<Node> EducationItems { get; set; }
        public string SelectedEducation { get; set; }

        public ObservableCollection<Node> MarriageItems { get; set; }
        public string SelectedMarriage { get; set; }

        public ObservableCollection<Node> IncomeItems { get; set; }
        public string SelectedIncome { get; set; }

        public bool ShowSampleCount { get; set; }

        public string SampleInfo { get; set; }

        #endregion

        public WidgetViewModel()
        {
            _timeDimensionString = string.Empty;
            _dimensionString = string.Empty;
            _metricString = string.Empty;
            _dataOrderString = string.Empty;
            _selectedTimeDimensions = new List<TimeDimensionTypeModel>();
            _selectedDimensions = new List<DimensionTypeModel>();
            _selectedMetrics = new List<MetricTypeModel>();
            DataOrderItems = new ObservableCollection<DataOrderItem>();

            LoadBaseData();
        }

        #region 初始数据

        /// <summary>ProgramItems
        /// 加载基础数据
        /// </summary>
        private async void LoadBaseData()
        {
            try
            {
                var baseDataResult = await WidgetService.GetBaseData();

                if (((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var baseDataDic = baseDataResult.Data.ToDictionary(d => d.Key, d => d.Value);

                    var systemTypeListString = baseDataDic["SystemTypeList"];
                    var displayTypeListString = baseDataDic["DisplayTypeList"];
                    InitSystemTypeList(systemTypeListString);
                    InitDisplayType(displayTypeListString);

                    if (WidgetModelEntity == null || WidgetModelEntity.Id == 0)
                    {
                        SystemType = SystemTypeList.FirstOrDefault();
                    }
                    else
                    {
                        SystemType = SystemTypeList.FirstOrDefault(s => s.Id == WidgetModelEntity.SystemTypeId);
                    }
                }
                else if (((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)baseDataResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ShowMessage.Show("访问500错误");
                }
                else
                {
                    ShowMessage.Show("未知错误");
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to LoadBaseData", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "LoadBaseData", null);
                }
            }
        }

        /// <summary>
        /// 初始化系统类型列表
        /// </summary>
        /// <param name="systemTypeListString"></param>
        private void InitSystemTypeList(string systemTypeListString)
        {
            try
            {
                var systemTypeList = JsonConvert.DeserializeObject<List<SystemTypeModel>>(systemTypeListString);
                var systemTypeSource = new ObservableCollection<SystemTypeModel>();
                foreach (var systemTypex in systemTypeList)
                {
                    if (systemTypex.Id != 7)
                    {
                        var systemType = new SystemTypeModel
                        {
                            Id = systemTypex.Id,
                            EnName = systemTypex.EnName,
                            CnName = systemTypex.CnName
                        };
                        systemTypeSource.Add(systemType);
                    }
                }
                SystemTypeList = systemTypeSource;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化系统类型列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitSystemTypeList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitSystemTypeList", null);
                }
            }
        }

        /// <summary>
        /// 初始化时间类型列表
        /// </summary>
        /// <param name="dateTypeListString"></param>
        private void InitDateTypeList(string dateTypeListString)
        {
            try
            {
                var dateTypeList = JsonConvert.DeserializeObject<List<DateTypeModel>>(dateTypeListString);
                var dateTypeSource = new ObservableCollection<DateTypeModel>();
                foreach (var dateTypex in dateTypeList)
                {
                    dateTypeSource.Add(new DateTypeModel
                    {
                        Id = dateTypex.Id,
                        EnName = dateTypex.EnName,
                        CnName = dateTypex.CnName
                    });
                }
                DateTypeList = dateTypeSource;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化时间类型列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDateTypeList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitSystemTypeList", null);
                }
            }
        }

        /// <summary>
        /// 初始化时间维度
        /// </summary>
        /// <param name="timeDimensionString"></param>
        private void InitTimeDimension(string timeDimensionString)
        {
            try
            {
                var timeDimensionTypeList = JsonConvert.DeserializeObject<List<TimeDimensionTypeModel>>(timeDimensionString);
                var dimensionItemsSource = new ObservableCollection<TimeDimensionTypeModel>();
                foreach (var timeDimensionx in timeDimensionTypeList)
                {
                    var timeDimension = new TimeDimensionTypeModel
                    {
                        Id = timeDimensionx.Id,
                        EnName = timeDimensionx.EnName,
                        CnName = timeDimensionx.CnName,
                        SystemTypeId = timeDimensionx.SystemTypeId,
                        IsEnabled = true
                    };
                    dimensionItemsSource.Add(timeDimension);
                }
                TimeDimensionTypeList = dimensionItemsSource;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化时间维度出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitTimeDimension", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitTimeDimension", null);
                }
            }
        }

        /// <summary>
        /// 初始化维度
        /// </summary>
        /// <param name="dimensionHeaderString"></param>
        /// <param name="dimensionString"></param>
        private void InitDimension(string dimensionHeaderString, string dimensionString)
        {
            try
            {
                var dimensionHeaderList = JsonConvert.DeserializeObject<List<HeaderKeyValue>>(dimensionHeaderString);
                var dimensionTypeList = JsonConvert.DeserializeObject<List<DimensionTypeModel>>(dimensionString);
                var dimensionItemsSource = new ObservableCollection<CheckBoxTab>();
                foreach (var header in dimensionHeaderList)
                {
                    var checkBoxTab = new CheckBoxTab
                    {
                        Header = header,
                        Items = new ObservableCollection<IItemTypeModel>()
                    };

                    foreach (var dimensionType in dimensionTypeList)
                    {
                        if (dimensionType.CnHeader == header.CnHeader)
                        {
                            dimensionType.IsEnabled = true;
                            checkBoxTab.Items.Add(dimensionType);
                        }
                    }

                    dimensionItemsSource.Add(checkBoxTab);
                }

                DimensionItemsSource = dimensionItemsSource;

                WidgetView view = GetView() as WidgetView;
                if (view != null)
                {
                    view.DimensionTabControl.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化维度出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDimension", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDimension", null);
                }
            }
        }

        /// <summary>
        /// 初始化指标
        /// </summary>
        /// <param name="metricHeaderListString"></param>
        /// <param name="metricTypeListString"></param>
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
                            if (SystemType.Id == (int)SystemTypeEnum.TrackRealtime && (metricType.EnName == "uimp" || metricType.EnName == "uclk"))
                            {
                                metricType.IsEnabled = false;
                            }
                            else
                            {
                                metricType.IsEnabled = true;
                            }
                            checkBoxTab.Items.Add(metricType);
                        }
                    }

                    metricItemsSource.Add(checkBoxTab);
                }

                MetricItemsSource = metricItemsSource;

                WidgetView view = GetView() as WidgetView;
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

        private void InitDataType(string dataTypeHeaderListString, string dataTypeListString)
        {
            try
            {
                var dataTypeHeaderList = JsonConvert.DeserializeObject<List<HeaderKeyValue>>(dataTypeHeaderListString);
                var dataTypeList = JsonConvert.DeserializeObject<List<DataTypeModel>>(dataTypeListString);
                var dataItemsSource = new ObservableCollection<CheckBoxTab>();
                foreach (var header in dataTypeHeaderList)
                {
                    var checkBoxTab = new CheckBoxTab
                    {
                        Header = header,
                        Items = new ObservableCollection<IItemTypeModel>()
                    };

                    foreach (var dataType in dataTypeList)
                    {
                        if (dataType.EnHeader.Contains(header.EnHeader))
                        {
                            checkBoxTab.Items.Add(dataType);
                        }
                    }

                    dataItemsSource.Add(checkBoxTab);
                }

                DataTypeItemsSource = dataItemsSource;

                WidgetView view = GetView() as WidgetView;
                if (view != null)
                {
                    view.DataTypeTabControl.SelectedIndex = 0;
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

        /// <summary>
        /// 初始化显示类型
        /// </summary>
        /// <param name="displayTypeListString"></param>
        private void InitDisplayType(string displayTypeListString)
        {
            try
            {
                var displayTypeList = JsonConvert.DeserializeObject<List<DisplayTypeModel>>(displayTypeListString);
                var displayItemsSource = new ObservableCollection<DisplayTypeModel>();
                foreach (var displayType in displayTypeList)
                {
                    var displayTypex = new DisplayTypeModel
                    {
                        Id = displayType.Id,
                        TypeName = displayType.TypeName,
                        TypeIndex = displayType.TypeIndex,
                        Data = displayType.Data,
                        Type = displayType.Type,
                        Description = displayType.Description
                    };
                    displayItemsSource.Add(displayTypex);
                }
                DisplayTypeList = displayItemsSource;
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

        #endregion

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        public void OkClick()
        {
            GetParameters();

            try
            {
                if (VerifyParameters())
                {
                    WidgetModelEntity.SystemTypeId = SystemType.Id;
                    WidgetModelEntity.TableName = ProgramString;
                    WidgetModelEntity.DisplayType = DisplayType.Type;
                    WidgetModelEntity.DisplayTypeIndex = DisplayType.TypeIndex;
                    WidgetModelEntity.DataOrderBy = _dataOrderString;
                    WidgetModelEntity.DataCount = DataCount;
                    WidgetModelEntity.EnHeader = GetHeader();

                    if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                    {
                        WidgetModelEntity.DataType = _dataType;
                    }
                    else
                    {
                        if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.TrackAnalysisReport)
                        {
                            switch (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader)
                            {
                                case "igrp":
                                    WidgetModelEntity.CityCode = SelectedCity.RegionId.ToString();
                                    break;
                                case "integrate":
                                    WidgetModelEntity.SiteCode = SelectedSite.Id;
                                    break;
                            }
                        }

                        WidgetModelEntity.TimeDimensions = _timeDimensionString;
                        WidgetModelEntity.Dimensions = _dimensionString;
                        WidgetModelEntity.Metrics = _metricString;
                        WidgetModelEntity.StartDate = StartDate;
                        WidgetModelEntity.EndDate = EndDate;

                        //Collect Ta
                        if (this.ActivateTa)
                        {
                            var taInfo = this.GetTaInfo();
                            WidgetModelEntity.Filter = JsonConvert.SerializeObject(taInfo);
                        }
                    }

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

        private string GetHeader()
        {
            var header = string.Empty;
            WidgetView view = GetView() as WidgetView;
            if (view != null)
            {
                if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                {
                    var selectedItem = view.DataTypeTabControl.SelectedItem as CheckBoxTab;
                    if (selectedItem != null)
                    {
                        header = selectedItem.Header.EnHeader;
                    }
                }
                else
                {
                    var selectedItem = view.DimensionTabControl.SelectedItem as CheckBoxTab;
                    if (selectedItem != null)
                    {
                        header = selectedItem.Header.EnHeader;
                    }
                }
            }
            return header;
        }

        /// <summary>
        /// 得到参数
        /// </summary>
        private void GetParameters()
        {
            try
            {
                if (SystemType.Id != (int)SystemTypeEnum.SiteRealtime)
                {
                    _timeDimensionString = string.Empty;
                    foreach (var timeDimension in TimeDimensionTypeList)
                    {
                        if (timeDimension.IsChecked)
                        {
                            if (string.IsNullOrEmpty(_timeDimensionString))
                            {
                                _timeDimensionString = timeDimension.EnName;
                            }
                            else
                            {
                                _timeDimensionString += "," + timeDimension.EnName;
                            }
                        }
                    }

                    _dimensionString = string.Empty;
                    foreach (var dimensionItem in DimensionItemsSource)
                    {
                        foreach (var dimension in dimensionItem.Items)
                        {
                            if (dimension.IsChecked)
                            {
                                if (string.IsNullOrEmpty(_dimensionString))
                                {
                                    _dimensionString = dimension.EnName;
                                }
                                else
                                {
                                    _dimensionString += "," + dimension.EnName;
                                }
                            }
                        }
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

                    _dataOrderString = string.Empty;
                    foreach (var dataOrderItem in DataOrderItems)
                    {
                        if (dataOrderItem.OrderName == null
                            || (string.IsNullOrEmpty(dataOrderItem.OrderName) && string.IsNullOrEmpty(dataOrderItem.OrderRule)))
                        {
                            continue;
                        }

                        _dataOrderString += dataOrderItem.OrderName + "*" + dataOrderItem.OrderRule + ",";
                    }
                    _dataOrderString = _dataOrderString.TrimEnd(',');
                }
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

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <returns></returns>
        private bool VerifyParameters()
        {
            try
            {
                if (string.IsNullOrEmpty(WidgetModelEntity.Title))
                {
                    ShowMessage.Show("标题不能为空");
                    return false;
                }

                if (string.IsNullOrEmpty(ProgramString))
                {
                    ShowMessage.Show("数据源不能为空");
                    return false;
                }

                if (DisplayType == null)
                {
                    ShowMessage.Show("显示类型不能为空");
                    return false;
                }

                if (SystemType.Id != (int)SystemTypeEnum.Sponsor && ProgramString.Contains(','))
                {
                    ShowMessage.Show("不支持多选项目或站点");
                    return false;
                }

                if (SystemType.Id != (int)SystemTypeEnum.SiteRealtime)
                {
                    if (SystemType.Id == (int)SystemTypeEnum.TrackAnalysisReport)
                    {
                        if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader != "igrp")
                        {
                            if (_selectedDimensions.Count == 0)
                            {
                                ShowMessage.Show("至少选一个维度");
                                return false;
                            }
                        }

                        if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "target")
                        {
                            if (!(_selectedDimensions.Any(d => d.EnName == "campaignsName") || _selectedDimensions.Any(d => d.EnName == "mediaName") || _selectedDimensions.Any(d => d.EnName == "placementName")))
                            {
                                ShowMessage.Show("项目,媒体,广告位至少要选中一个");
                                return false;
                            }
                        }
                    }

                    if (_selectedMetrics.Count == 0)
                    {
                        ShowMessage.Show("至少选一个指标");
                        return false;
                    }
                }
                else
                {
                    if (_dataTypeCount == 0)
                    {
                        ShowMessage.Show("数据类型不能为空");
                        return false;
                    }
                }

                foreach (var dataOrderItem in DataOrderItems)
                {
                    if (string.IsNullOrEmpty(dataOrderItem.OrderName) && !string.IsNullOrEmpty(dataOrderItem.OrderRule)
                        || !string.IsNullOrEmpty(dataOrderItem.OrderName) && string.IsNullOrEmpty(dataOrderItem.OrderRule))
                    {
                        ShowMessage.Show("排序项不能有空值");
                        return false;
                    }
                }

                switch (SystemType.Id)
                {
                    case (int)SystemTypeEnum.SiteAnalysisReport:
                    case (int)SystemTypeEnum.SiteDataBank:
                    case (int)SystemTypeEnum.TrackDataBank:
                    case (int)SystemTypeEnum.SiteRealtime:
                        return true;
                    case (int)SystemTypeEnum.TrackAnalysisReport:
                        return VerifyTrackAnalysisReport();
                    case (int)SystemTypeEnum.Sponsor:
                        return VerifySponsor();
                    case (int)SystemTypeEnum.TrackRealtime:
                        return VerifyTrackRealtime();
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

        private bool VerifyTrackAnalysisReport()
        {
            if (SystemType.Id == (int)SystemTypeEnum.TrackAnalysisReport && DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "igrp"
                && _selectedTimeDimensions.Count == 0 && _selectedDimensions.Count == 0)
            {
                ShowMessage.Show("时间粒度和维度至少选一个");
                return false;
            }

            return true;
        }

        private bool VerifyTrackRealtime()
        {
            if (_selectedTimeDimensions.Count > 1)
            {
                ShowMessage.Show("时间粒度只能选一个");
                return false;
            }

            if (_selectedTimeDimensions.Count == 0)
            {
                ShowMessage.Show("需要选一个时间粒度");
                return false;
            }

            if (_selectedDimensions.Count == 0)
            {
                ShowMessage.Show("需要选一个维度");
                return false;
            }

            return true;
        }

        private bool VerifySponsor()
        {
            if (_selectedTimeDimensions.Count == 0 && _selectedDimensions.Count == 0)
            {
                ShowMessage.Show("时间粒度,维度至少要选一个");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        public void CancelClick()
        {
            TryClose();
        }

        /// <summary>
        /// 系统类型改变事件
        /// </summary>
        public async void SystemTypeChanged()
        {
            try
            {
                #region 控制显示性

                switch (SystemType.Id)
                {
                    case (int)SystemTypeEnum.SiteAnalysisReport:
                        break;
                    case (int)SystemTypeEnum.SiteDataBank:
                        break;
                    case (int)SystemTypeEnum.SiteRealtime:
                        TimeDimensionVisibility = Visibility.Collapsed;
                        TimeVisibility = Visibility.Collapsed;
                        IgrpCityVisibility = Visibility.Collapsed;
                        SiteVisibility = Visibility.Collapsed;
                        DimensionVisibility = Visibility.Collapsed;
                        MetricVisibility = Visibility.Collapsed;
                        DataTypeVisibility = Visibility.Visible;
                        break;
                    case (int)SystemTypeEnum.Sponsor:
                        TimeDimensionVisibility = Visibility.Visible;
                        TimeVisibility = Visibility.Visible;
                        IgrpCityVisibility = Visibility.Collapsed;
                        SiteVisibility = Visibility.Collapsed;
                        DimensionVisibility = Visibility.Visible;
                        MetricVisibility = Visibility.Visible;
                        DataTypeVisibility = Visibility.Collapsed;
                        break;
                    case (int)SystemTypeEnum.TrackAnalysisReport:
                        TimeDimensionVisibility = Visibility.Visible;
                        TimeVisibility = Visibility.Visible;
                        IgrpCityVisibility = Visibility.Collapsed;
                        SiteVisibility = Visibility.Collapsed;
                        DimensionVisibility = Visibility.Visible;
                        MetricVisibility = Visibility.Visible;
                        DataTypeVisibility = Visibility.Collapsed;
                        break;
                    case (int)SystemTypeEnum.TrackDataBank:
                        break;
                    case (int)SystemTypeEnum.TrackRealtime:
                        TimeDimensionVisibility = Visibility.Visible;
                        TimeVisibility = Visibility.Hidden;
                        IgrpCityVisibility = Visibility.Collapsed;
                        SiteVisibility = Visibility.Collapsed;
                        DimensionVisibility = Visibility.Visible;
                        MetricVisibility = Visibility.Visible;
                        DataTypeVisibility = Visibility.Collapsed;
                        break;
                }

                #endregion

                #region 清空数据

                ProgramItems = new ObservableCollection<Node>();
                ProgramString = string.Empty;
                TimeDimensionTypeList = new ObservableCollection<TimeDimensionTypeModel>();
                DimensionItemsSource = new ObservableCollection<CheckBoxTab>();
                DataTypeItemsSource = new ObservableCollection<CheckBoxTab>();
                MetricItemsSource = new ObservableCollection<CheckBoxTab>();
                _selectedTimeDimensions = new List<TimeDimensionTypeModel>();
                _selectedDimensions = new List<DimensionTypeModel>();
                _selectedMetrics = new List<MetricTypeModel>();
                DisplayType = DisplayTypeList.FirstOrDefault();
                foreach (var displayTypeModel in DisplayTypeList)
                {
                    displayTypeModel.IsEnabled = true;
                }

                DataOrderItems.Clear();

                #endregion

                #region 查询数据表

                var system = SystemDataBuilder.SystemBuilder(SystemType.Id);
                var parameterDictionary = new Dictionary<string, object>();
                parameterDictionary.Add("SystemTypeId", SystemType.Id);
                parameterDictionary.Add("ClientId", ClientId);
                parameterDictionary.Add("UserId", User.UserInfo.Id);
                var items = await system.GetTableSource(parameterDictionary);
                ProgramItems = items;

                #endregion

                #region 查询时间粒度，维度，指标

                var result = await WidgetService.GetDataBySystemType(SystemType.Id);
                if (result != null)
                {
                    var baseDataDic = result.Data.ToDictionary(d => d.Key, d => d.Value);

                    if (SystemType.Id == (int)SystemTypeEnum.SiteRealtime)
                    {
                        var dataTypeHeaderString = baseDataDic["DataTypeHeaderList"];
                        var dataTypeListString = baseDataDic["DataTypeList"];
                        InitDataType(dataTypeHeaderString, dataTypeListString);
                    }
                    else
                    {
                        switch (SystemType.Id)
                        {
                            case (int)SystemTypeEnum.TrackRealtime:
                                SetDisplayTypeEnabled(_trackRealtimeDisplayType);
                                break;
                            case (int)SystemTypeEnum.Sponsor:
                            case (int)SystemTypeEnum.TrackAnalysisReport:
                                var dateTypeListString = baseDataDic["DateTypeList"];
                                InitDateTypeList(dateTypeListString);
                                break;
                        }

                        var timeDimensionTypeListString = baseDataDic["TimeDimensionTypeList"];
                        var dimensionHeaderString = baseDataDic["DimensionHeaderList"];
                        var dimensionTypeListString = baseDataDic["DimensionTypeList"];
                        var metricHeaderList = baseDataDic["MetricHeaderList"];
                        var metricTypeListString = baseDataDic["MetricTypeList"];
                        InitTimeDimension(timeDimensionTypeListString);
                        InitDimension(dimensionHeaderString, dimensionTypeListString);
                        InitMetric(metricHeaderList, metricTypeListString);
                    }

                    #region 赋值

                    if (WidgetModelEntity != null && WidgetModelEntity.Id != 0 && WidgetModelEntity.SystemTypeId == SystemType.Id)
                    {
                        ProgramString = WidgetModelEntity.TableName;
                        SetDisplayType();

                        if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                        {
                            #region 设置数据类型

                            switch (WidgetModelEntity.EnHeader)
                            {
                                case "online":
                                    DataTypeTabSelectedIndex = 0;
                                    break;
                                case "statistics":
                                    DataTypeTabSelectedIndex = 1;
                                    break;
                                case "transform":
                                    DataTypeTabSelectedIndex = 2;
                                    break;
                            }
                            _dataType = WidgetModelEntity.DataType;
                            var selectedDataType = DataTypeCache.Find(d => d.EnName == _dataType && d.SystemTypeId == WidgetModelEntity.SystemTypeId);
                            if (selectedDataType != null)
                            {
                                foreach (var dataType in DataTypeItemsSource[DataTypeTabSelectedIndex].Items)
                                {
                                    if (_dataType == dataType.EnName)
                                    {
                                        dataType.IsChecked = true;
                                        break;
                                    }
                                }
                            }

                            _dataTypeCount = 1;

                            #endregion
                        }
                        else
                        {
                            #region 设置时间维度

                            var timeDimensionsx = WidgetModelEntity.TimeDimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var timeDimensionx in timeDimensionsx)
                            {
                                var selectedTimeDimensionx = TimeDimensionsCache.Find(t => t.EnName == timeDimensionx && t.SystemTypeId == WidgetModelEntity.SystemTypeId);
                                if (selectedTimeDimensionx != null)
                                {
                                    _selectedTimeDimensions.Add(selectedTimeDimensionx);
                                }
                            }

                            var timeDimensionList = TimeDimensionTypeList.Where(t => WidgetModelEntity.TimeDimensions.Contains(t.EnName));
                            foreach (var timeDimension in timeDimensionList)
                            {
                                timeDimension.IsChecked = true;
                            }

                            #endregion

                            if (WidgetModelEntity.SystemTypeId != (int)SystemTypeEnum.Sponsor)
                            {
                                for (int i = 0; i < DimensionItemsSource.Count; i++)
                                {
                                    if (DimensionItemsSource[i].Header.EnHeader == WidgetModelEntity.EnHeader)
                                    {
                                        DimensionTabSelectedIndex = MetricTabSelectedIndex = i;
                                    }
                                }
                            }

                            #region 设置维度

                            _dimensionString = WidgetModelEntity.Dimensions;
                            var dimensionsx = WidgetModelEntity.Dimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var dimensionx in dimensionsx)
                            {
                                var selectedDimensionx = DimensionsCache.Find(d => d.EnName == dimensionx && d.SystemTypeId == WidgetModelEntity.SystemTypeId && (d.EnHeader == WidgetModelEntity.EnHeader || d.EnHeader == "ALL"));
                                if (selectedDimensionx != null)
                                {
                                    _selectedDimensions.Add(selectedDimensionx);
                                }
                            }

                            var dimensionXX = WidgetModelEntity.Dimensions.Split(',');
                            foreach (var dimensionItems in DimensionItemsSource)
                            {
                                if (dimensionItems.Header.EnHeader == WidgetModelEntity.EnHeader || dimensionItems.Header.EnHeader == "ALL")
                                {
                                    foreach (var dimension in dimensionItems.Items)
                                    {
                                        if (dimensionXX.Contains(dimension.EnName))
                                        {
                                            dimension.IsChecked = true;
                                        }
                                    }
                                }
                            }

                            #endregion

                            #region 设置指标

                            _metricString = WidgetModelEntity.Metrics;
                            var metricsx = WidgetModelEntity.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.Sponsor)
                            {
                                foreach (var metricx in metricsx)
                                {
                                    var selectedMetricx = MetricsCache.Find(m => m.EnName == metricx && m.SystemTypeId == WidgetModelEntity.SystemTypeId);
                                    if (selectedMetricx != null)
                                    {
                                        _selectedMetrics.Add(selectedMetricx);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var metricx in metricsx)
                                {
                                    var selectedMetricx = MetricsCache.Find(m => m.EnName == metricx && m.SystemTypeId == WidgetModelEntity.SystemTypeId && (m.EnHeader == WidgetModelEntity.EnHeader || m.EnHeader == "ALL"));
                                    if (selectedMetricx != null)
                                    {
                                        _selectedMetrics.Add(selectedMetricx);
                                    }
                                }
                            }

                            var metricsXX = WidgetModelEntity.Metrics.Split(',');
                            if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.Sponsor)
                            {
                                foreach (var metricItems in MetricItemsSource)
                                {
                                    foreach (var metric in metricItems.Items)
                                    {
                                        if (metricsXX.Contains(metric.EnName))
                                        {
                                            metric.IsChecked = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var metricItems in MetricItemsSource)
                                {
                                    if (metricItems.Header.EnHeader == WidgetModelEntity.EnHeader || metricItems.Header.EnHeader == "ALL")
                                    {
                                        foreach (var metric in metricItems.Items)
                                        {
                                            if (metricsXX.Contains(metric.EnName))
                                            {
                                                metric.IsChecked = true;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion

                            if (WidgetModelEntity.SystemTypeId != (int)SystemTypeEnum.TrackRealtime)
                            {
                                StartDate = WidgetModelEntity.StartDate;
                                EndDate = WidgetModelEntity.EndDate;
                            }
                            else if (WidgetModelEntity.SystemTypeId == (int)SystemTypeEnum.TrackAnalysisReport)
                            {
                                switch (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader)
                                {
                                    case "igrp":
                                        IgrpCityVisibility = Visibility.Visible;
                                        SiteVisibility = Visibility.Collapsed;
                                        break;
                                    case "integrate":
                                        IgrpCityVisibility = Visibility.Collapsed;
                                        SiteVisibility = Visibility.Visible;
                                        break;
                                    default:
                                        IgrpCityVisibility = Visibility.Collapsed;
                                        SiteVisibility = Visibility.Collapsed;
                                        break;
                                }
                            }
                        }

                        SetDisplayType();

                        #region 设置显示类型

                        var displayType = DisplayTypeList.FirstOrDefault(d => d.Type == WidgetModelEntity.DisplayType && d.TypeIndex == WidgetModelEntity.DisplayTypeIndex);
                        DisplayType = displayType;

                        #endregion

                        DataCount = WidgetModelEntity.DataCount;

                        #region 排序

                        var dataOrderBys = WidgetModelEntity.DataOrderBy == null ? null : WidgetModelEntity.DataOrderBy.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        var orderNames = new ObservableCollection<ResultValue>();

                        if (!string.IsNullOrEmpty(WidgetModelEntity.TimeDimensions))
                        {
                            var timeDimensions = WidgetModelEntity.TimeDimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < timeDimensions.Count(); i++)
                            {
                                var timeDimension = TimeDimensionsCache.Find(t => t.EnName == timeDimensions[i]);
                                if (timeDimension != null)
                                {
                                    orderNames.Add(new ResultValue
                                    {
                                        Key = timeDimensions[i],
                                        Value = timeDimension.CnName
                                    });
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(WidgetModelEntity.Dimensions))
                        {
                            var dimensions = WidgetModelEntity.Dimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < dimensions.Count(); i++)
                            {
                                var dimension = DimensionsCache.Find(d => d.EnName == dimensions[i]);
                                if (dimension != null)
                                {
                                    orderNames.Add(new ResultValue
                                    {
                                        Key = dimensions[i],
                                        Value = dimension.CnName
                                    });
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(WidgetModelEntity.Metrics))
                        {
                            var metrics = WidgetModelEntity.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            for (var i = 0; i < metrics.Count(); i++)
                            {
                                var metric = MetricsCache.Find(m => m.EnName == metrics[i]);
                                if (metric != null)
                                {
                                    orderNames.Add(new ResultValue
                                    {
                                        Key = metrics[i],
                                        Value = metric.CnName
                                    });
                                }
                            }
                        }

                        _orderRulesX = new ObservableCollection<ResultValue>();
                        _orderRulesX.Add(new ResultValue { Key = "0", Value = "升序" });
                        _orderRulesX.Add(new ResultValue { Key = "1", Value = "降序" });

                        

                        if (dataOrderBys != null)
                        {
                            foreach (var dataOrderBy in dataOrderBys)
                            {
                                var dataOrders = dataOrderBy.Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                                var dataOrderItem = new DataOrderItem();
                                dataOrderItem.OrderName = dataOrders[0];
                                dataOrderItem.OrderNames = orderNames;
                                dataOrderItem.OrderRule = dataOrders[1];
                                dataOrderItem.OrderRules = _orderRulesX;
                                DataOrderItems.Add(dataOrderItem);
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }
            catch (Exception ex)
            {
                ShowMessage.Show("系统类型改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to SystemTypeChanged", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "SystemTypeChanged", null);
                }
            }
        }

        private void SetDisplayTypeEnabled(string displayTypeString)
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

        /// <summary>
        /// 主Tab选中改变事件
        /// </summary>
        public void MainTabChanged()
        {
            try
            {
                if (TabSelectedIndex == 1)
                {
                    InitDefaultOrderItem();

                    if (DataOrderItems.Count == 0)
                    {
                        DataOrderItems.Add(DataOrderItem);
                    }

                    foreach (var dataOrderItem in DataOrderItems)
                    {
                        dataOrderItem.OrderNames = _orderDatasX;
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

        private void InitDefaultOrderItem()
        {
            InitOrderItems();

            _orderRulesX = new ObservableCollection<ResultValue>();
            _orderRulesX.Add(new ResultValue { Key = "0", Value = "升序" });
            _orderRulesX.Add(new ResultValue { Key = "1", Value = "降序" });

            DataOrderItem = new DataOrderItem { OrderNames = _orderDatasX, OrderRules = _orderRulesX };
        }

        private void InitOrderItems()
        {
            _orderDatasX = new ObservableCollection<ResultValue>();
            foreach (var timeDimension in TimeDimensionTypeList)
            {
                if (timeDimension.IsChecked)
                {
                    _orderDatasX.Add(new ResultValue { Key = timeDimension.EnName, Value = timeDimension.CnName });
                }
            }

            for (var i = 0; i < DimensionItemsSource.Count; i++)
            {
                foreach (var dimension in DimensionItemsSource[i].Items)
                {
                    if (dimension.IsChecked)
                    {
                        _orderDatasX.Add(new ResultValue { Key = dimension.EnName, Value = dimension.CnName });
                    }
                }
            }

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

        private void RemoveUnUsedOrderItem()
        {
            InitOrderItems();

            var dataOrderItemsX = new ObservableCollection<DataOrderItem>();
            foreach (var dataOrderItemX in DataOrderItems)
            {
                if (_orderDatasX.Select(o => o.Key).ToList().Contains(dataOrderItemX.OrderName))
                {
                    dataOrderItemsX.Add(dataOrderItemX);
                }
            }

            DataOrderItems = dataOrderItemsX;
        }

        /// <summary>
        /// 维度Tab选中改变事件
        /// </summary>
        public async void DimensionTabChanged()
        {
            #region TrackAnalysisReport

            if (SystemType.Id == (int)SystemTypeEnum.TrackAnalysisReport)
            {
                DimensionTabSelectedIndex = DimensionTabSelectedIndex == -1 ? 0 : DimensionTabSelectedIndex;
                MetricTabSelectedIndex = DimensionTabSelectedIndex;

                WidgetModelEntity.DateTypeId = 1;
                if (DimensionItemsSource.Count == 0)
                {
                    IgrpCityVisibility = Visibility.Collapsed;
                    SiteVisibility = Visibility.Collapsed;
                }
                else
                {
                    _selectedTimeDimensions.Clear();
                    switch (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader)
                    {
                        case "basics":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Collapsed;
                            TimeDimensionVisibility = Visibility.Visible;
                            foreach (var timeDimension in TimeDimensionTypeList)
                            {
                                timeDimension.IsChecked = false;
                                timeDimension.IsEnabled = true;
                            }

                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                        case "complete":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Collapsed;
                            foreach (var timeDimension in TimeDimensionTypeList)
                            {
                                timeDimension.IsChecked = false;
                                if (timeDimension.EnName != "date")
                                {
                                    timeDimension.IsEnabled = false;
                                }
                            }

                            TimeDimensionVisibility = Visibility.Visible;
                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                        case "freqs":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Collapsed;
                            TimeDimensionVisibility = Visibility.Hidden;
                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                        case "igrp":
                            IgrpCityVisibility = Visibility.Visible;
                            SiteVisibility = Visibility.Collapsed;
                            CityList = await TrackAnalysisReportExtension.GetCityList(ProgramString);
                            foreach (var timeDimension in TimeDimensionTypeList)
                            {
                                if (timeDimension.EnName != "date")
                                {
                                    timeDimension.IsChecked = false;
                                    timeDimension.IsEnabled = false;
                                }
                            }

                            TimeDimensionVisibility = Visibility.Visible;
                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                        case "panels":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Collapsed;
                            TimeDimensionVisibility = Visibility.Collapsed;
                            TimeVisibility = Visibility.Collapsed;
                            break;
                        case "integrate":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Visible;
                            SiteList = await TrackAnalysisReportExtension.GetSiteList(ProgramString);
                            TimeDimensionVisibility = Visibility.Hidden;
                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                        case "target":
                            IgrpCityVisibility = Visibility.Collapsed;
                            SiteVisibility = Visibility.Collapsed;
                            TimeDimensionVisibility = Visibility.Hidden;
                            TimeVisibility = Visibility.Visible;
                            StartDateIsEnabled = true;
                            EndDateIsEnabled = true;
                            break;
                    }

                    foreach (var checkBoxTab in DimensionItemsSource)
                    {
                        foreach (var dimension in checkBoxTab.Items)
                        {
                            (dimension as DimensionTypeModel).IsEnabled = true;
                        }
                    }
                }
            }

            #endregion

            #region 非Sponsor

            if (SystemType.Id != (int)SystemTypeEnum.Sponsor)
            {
                _selectedDimensions.Clear();
                _selectedMetrics.Clear();

                for (int i = 0; i < DimensionItemsSource.Count; i++)
                {
                    if (i != DimensionTabSelectedIndex)
                    {
                        foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                        {
                            dimension.IsChecked = false;
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// 指标Tab切换
        /// </summary>
        public void MetricTabChanged()
        {
            if (SystemType.Id != (int)SystemTypeEnum.Sponsor)
            {
                for (int i = 0; i < MetricItemsSource.Count; i++)
                {
                    if (i != MetricTabSelectedIndex)
                    {
                        foreach (var metric in MetricItemsSource[DimensionTabSelectedIndex].Items)
                        {
                            metric.IsChecked = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加过滤
        /// </summary>
        public void AddDataFilter()
        {
            try
            {
                DataOrderItems.Add(new DataOrderItem { OrderNames = _orderDatasX, OrderRules = _orderRulesX });
            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加过滤出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddDataFilter", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddDataFilter", null);
                }
            }
        }

        /// <summary>
        /// 删除过滤
        /// </summary>
        /// <param name="dataOrderItem"></param>
        public void DeleteDataFilter(DataOrderItem dataOrderItem)
        {
            try
            {
                DataOrderItems.Remove(dataOrderItem);
                if (DataOrderItems.Count == 0)
                {
                    DataOrderItems.Add(new DataOrderItem { OrderNames = _orderDatasX, OrderRules = _orderRulesX });
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("删除过滤出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DeleteDataFilter", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DeleteDataFilter", null);
                }
            }
        }

        /// <summary>
        /// 日期类型改变
        /// </summary>
        public void DateTypeChanged()
        {
            try
            {
                switch (WidgetModelEntity.DateTypeId)
                {
                    case 0: //受全局控制
                        StartDateIsEnabled = false;
                        EndDateIsEnabled = false;
                        StartDate = WidgetModelEntity.StartDate;
                        EndDate = WidgetModelEntity.EndDate;
                        break;
                    case 1: //自定义
                        StartDateIsEnabled = true;
                        EndDateIsEnabled = true;
                        break;
                    case 2: //昨天
                        StartDateIsEnabled = false;
                        EndDateIsEnabled = false;
                        StartDate = CurrentDate.AddDays(-1);
                        EndDate = CurrentDate;
                        break;
                    case 3: //上周
                        StartDateIsEnabled = false;
                        EndDateIsEnabled = false;
                        StartDate = CurrentDate.AddDays(-7);
                        EndDate = CurrentDate;
                        break;
                    case 4: //上月
                        StartDateIsEnabled = false;
                        EndDateIsEnabled = false;
                        StartDate = CurrentDate.AddMonths(-1);
                        EndDate = CurrentDate;
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("日期类型改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DateTypeChanged", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DateTypeChanged", null);
                }
            }
        }

        /// <summary>
        /// 时间粒度点击事件
        /// </summary>
        public void TimeDimensionClick(TimeDimensionTypeModel timeDimension)
        {
            try
            {
                var timeDimensionx = TimeDimensionsCache.Find(t => t.EnName == timeDimension.EnName && t.SystemTypeId == SystemType.Id);


                if (timeDimension.IsChecked)
                {
                    _selectedTimeDimensions.Add(timeDimensionx);
                }
                else
                {
                    _selectedTimeDimensions.Remove(timeDimensionx);
                }

                SetDisplayType();

                RemoveUnUsedOrderItem();
            }
            catch (Exception ex)
            {
                ShowMessage.Show("时间粒度选择出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to TimeDimensionClick", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "TimeDimensionClick", null);
                }
            }
        }

        /// <summary>
        /// 维度点击事件
        /// </summary>
        public void DimensionClick(DimensionTypeModel dimension)
        {
            try
            {
                var dimensionx = DimensionsCache.Find(d => d.EnName == dimension.EnName && d.SystemTypeId == SystemType.Id);

                if (dimension.IsChecked)
                {
                    _selectedDimensions.Add(dimensionx);
                }
                else
                {
                    _selectedDimensions.Remove(dimensionx);
                }

                SetDisplayType();

                RemoveUnUsedOrderItem();
            }
            catch (Exception ex)
            {
                ShowMessage.Show("维度选择出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DimensionClick", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DimensionClick", null);
                }
            }
        }

        /// <summary>
        /// 数据类型点击事件
        /// </summary>
        /// <param name="dataType"></param>
        public void DataTypeClick(DataTypeModel dataType)
        {
            _dataTypeCount = 1;
            _dataType = dataType.EnName;

            SetDisplayType();
        }

        /// <summary>
        /// 指标点击事件
        /// </summary>
        public void MetricClick(MetricTypeModel metric)
        {
            try
            {
                var metricx = MetricsCache.Find(m => m.EnName == metric.EnName && m.SystemTypeId == SystemType.Id);
                if (metric.IsChecked)
                {
                    _selectedMetrics.Add(metricx);
                }
                else
                {
                    _selectedMetrics.Remove(metricx);
                }

                SetDisplayType();
                RemoveUnUsedOrderItem();
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

        private void SetDisplayType()
        {
            #region TrackRealtime

            if (SystemType.Id == (int)SystemTypeEnum.TrackRealtime)
            {
                if (_selectedDimensions.Any(d => d.EnName == "placementName") && _selectedDimensions.Any(d => d.EnName == "creativeName"))
                {
                    foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                    {
                        if (metric.EnName == "uimp" || metric.EnName == "uclk")
                        {
                            (metric as MetricTypeModel).IsEnabled = true;
                        }
                    }
                }
                else
                {
                    foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                    {
                        if (metric.EnName == "uimp" || metric.EnName == "uclk")
                        {
                            (metric as MetricTypeModel).IsChecked = false;
                            (metric as MetricTypeModel).IsEnabled = false;
                        }
                    }
                }

                if (_selectedDimensions.Count > 1)
                {
                    SetDisplayTypeEnabled("Grid");
                }
                else if (_selectedDimensions.Any(d => d.EnName == "campaignsName"))
                {
                    if (_selectedDimensions.Count == 1)
                    {
                        SetDisplayTypeEnabled(_trackRealtimeDisplayType);
                    }

                    if (_selectedDimensions.Count > 1 && _selectedMetrics.Count > 1)
                    {
                        SetDisplayTypeEnabled("Grid");
                    }
                }
                else if (_selectedDimensions.Any(d => d.EnName == "mediaName") || _selectedDimensions.Any(d => d.EnName == "placementName") || _selectedDimensions.Any(d => d.EnName == "creativeName"))
                {
                    if (_selectedMetrics.Count == 1)
                    {
                        SetDisplayTypeEnabled(_trackRealtimeDisplayType);
                    }
                    else if (_selectedMetrics.Count > 1)
                    {
                        SetDisplayTypeEnabled("Grid");
                    }
                }
            }

            #endregion

            #region TrackAnalysisReport

            if (SystemType.Id == (int)SystemTypeEnum.TrackAnalysisReport)
            {
                if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "basics"
                   || DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "complete"
                   || DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "igrp"
                   || DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "integrate")
                {
                    if (_selectedTimeDimensions.Count == 1)
                    {
                        if (_selectedDimensions.Count == 0 && _selectedMetrics.Count > 0)
                        {
                            SetDisplayTypeEnabled(_trackAnalysisReportOtherWithTimeDimensionDisplayType);
                        }
                        else if (_selectedDimensions.Any(d => d.EnName == "campaignsName"))
                        {
                            SetDisplayTypeEnabled(_trackAnalysisReportOtherWithTimeDimensionDisplayType);
                        }
                        else if (_selectedDimensions.Count == 1
                                 && (_selectedDimensions.Any(d => d.EnName == "mediaName")
                                     || _selectedDimensions.Any(d => d.EnName == "placementName")
                                     || _selectedDimensions.Any(d => d.EnName == "creativeName")))
                        {
                            if (_selectedMetrics.Count > 1)
                            {
                                SetDisplayTypeEnabled("Grid");
                            }
                            else
                            {
                                SetDisplayTypeEnabled(_trackAnalysisReportOtherWithTimeDimensionDisplayType);
                            }
                        }
                        else if (_selectedDimensions.Count > 1)
                        {
                            SetDisplayTypeEnabled("Grid");
                        }
                        else
                        {
                        }
                    }
                    else if (_selectedTimeDimensions.Count == 0)
                    {
                        if (_selectedDimensions.Count == 1)
                        {
                            if (_selectedDimensions.Any(d => d.EnName == "mediaName")
                                || _selectedDimensions.Any(d => d.EnName == "placementName")
                                || _selectedDimensions.Any(d => d.EnName == "creativeName")
                                || _selectedDimensions.Any(d => d.EnName == "keyword")
                                || _selectedDimensions.Any(d => d.EnName == "provinceName")
                                || _selectedDimensions.Any(d => d.EnName == "cityName")
                                || _selectedDimensions.Any(d => d.EnName == "stepIgrp"))
                            {
                                switch (_selectedMetrics.Count)
                                {
                                    case 1:
                                        SetDisplayTypeEnabled(_trackAnalysisReportOtherWithoutTimeDimensionDisplayType1);
                                        break;
                                    case 2:
                                        SetDisplayTypeEnabled(_trackAnalysisReportOtherWithoutTimeDimensionDisplayType2);
                                        break;
                                    case 3:
                                        SetDisplayTypeEnabled(_trackAnalysisReportOtherWithoutTimeDimensionDisplayType3);
                                        break;
                                    case 4:
                                    case 5:
                                        SetDisplayTypeEnabled(_trackAnalysisReportOtherWithoutTimeDimensionDisplayType4And5);
                                        break;
                                    default:
                                        SetDisplayTypeEnabled(_trackAnalysisReportOtherWithoutTimeDimensionDisplayTypeMore5);
                                        break;
                                }
                            }
                            else
                            {

                            }
                        }
                        else if (_selectedDimensions.Count > 1 && _selectedMetrics.Count > 1)
                        {
                            SetDisplayTypeEnabled("Grid");
                        }
                    }

                    if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "basics")
                    {
                        if (_selectedDimensions.Any(d => d.EnName == "creativeName"))
                        {
                            foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                            {
                                if (dimension.EnName == "keyword"
                                    || dimension.EnName == "provinceName"
                                    || dimension.EnName == "cityName")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                        else if (_selectedDimensions.Any(d => d.EnName == "keyword"))
                        {
                            foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                            {
                                if (dimension.EnName == "creativeName"
                                    || dimension.EnName == "provinceName"
                                    || dimension.EnName == "cityName")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                        else if (_selectedDimensions.Any(d => d.EnName == "provinceName"))
                        {
                            foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                            {
                                if (dimension.EnName == "keyword"
                                    || dimension.EnName == "creativeName"
                                    || dimension.EnName == "cityName")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                        else if (_selectedDimensions.Any(d => d.EnName == "cityName"))
                        {
                            foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                            {
                                if (dimension.EnName == "keyword"
                                    || dimension.EnName == "provinceName"
                                    || dimension.EnName == "creativeName")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                        else
                        {
                            foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                            {
                                (dimension as DimensionTypeModel).IsEnabled = true;
                            }
                        }
                    }
                }
                else if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "panels")
                {
                    if (_selectedDimensions.Any(d => d.EnName == "provinceName"))
                    {
                        foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                        {
                            if (dimension.EnName == "cityName")
                            {
                                (dimension as DimensionTypeModel).IsEnabled = false;
                            }
                        }
                    }
                    else if (_selectedDimensions.Any(d => d.EnName == "cityName"))
                    {
                        foreach (var dimension in DimensionItemsSource[DimensionTabSelectedIndex].Items)
                        {
                            if (dimension.EnName == "provinceName")
                            {
                                (dimension as DimensionTypeModel).IsEnabled = false;
                            }
                        }
                    }
                }

                #region Igrp

                if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "igrp")
                {
                    if (_selectedDimensions.Count == 1 && _selectedMetrics.Count == 1 && _selectedMetrics.Any(m => m.EnName == "reach"))
                    {
                        SetDisplayTypeEnabled(_trackAnalysisReportIgrpDisplayType);
                    }

                    if (_selectedTimeDimensions.Count == 0 && _selectedDimensions.Any(d => d.EnName == "campaignsName"))
                    {
                        foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                        {
                            var list = new List<string> { "taImp", "taUimp", "stableImpRate", "ppl", "netizen", "taNetizen", "generalTaRate", "actualTaRate" };
                            if (!list.Contains(metric.EnName))
                            {
                                (metric as MetricTypeModel).IsEnabled = false;
                            }
                        }
                    }

                    if (_selectedTimeDimensions.Any(t => t.EnName == "date"))
                    {
                        foreach (var dimensionCheckBoxTab in DimensionItemsSource)
                        {
                            foreach (var dimension in dimensionCheckBoxTab.Items)
                            {
                                if (dimension.EnName == "stepIgrp")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                    }

                    if (_selectedDimensions.Any(t => t.EnName == "stepIgrp"))
                    {
                        foreach (var timeDimension in TimeDimensionTypeList)
                        {
                            if (timeDimension.EnName == "date")
                            {
                                (timeDimension as TimeDimensionTypeModel).IsEnabled = false;
                            }
                        }
                    }

                    if (_selectedDimensions.Count == 1 && _selectedDimensions.Any(m => m.EnName == "mediaName"))
                    {
                        foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                        {
                            if (metric.EnName != "sumImp")
                            {
                                (metric as MetricTypeModel).IsEnabled = false;
                            }
                        }
                    }
                }

                #endregion

                #region 人口属性

                if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "panels")
                {
                    if (_selectedDimensions.Count == 1)
                    {
                        if (_selectedDimensions.Any(d => d.EnName == "mediaName")
                            || _selectedDimensions.Any(d => d.EnName == "provinceName")
                            || _selectedDimensions.Any(d => d.EnName == "cityName")
                            || _selectedDimensions.Any(d => d.EnName == "gender")
                            || _selectedDimensions.Any(d => d.EnName == "fiveAges"))
                        {
                            SetDisplayTypeEnabled(_trackAnalysisReportPanelsDisplayType);
                        }
                    }
                    else if (_selectedDimensions.Count > 1)
                    {
                        SetDisplayTypeEnabled("Grid");
                    }
                }

                #endregion

                #region 增量频次

                if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "freqs")
                {
                    StartDateIsEnabled = false;

                    if (_selectedDimensions.Count == 1
                        && (_selectedDimensions.Any(d => d.EnName == "campaigns")
                            || _selectedDimensions.Any(d => d.EnName == "mediaName")
                            || _selectedDimensions.Any(d => d.EnName == "placementName")
                            || _selectedDimensions.Any(d => d.EnName == "provinceName")
                            || _selectedDimensions.Any(d => d.EnName == "cityName")))
                    {
                        if (_selectedMetrics.Any(m => m.EnName.Contains("imp"))
                            || _selectedMetrics.Any(m => m.EnName.Contains("uimp"))
                            || _selectedMetrics.Any(m => m.EnName.Contains("clk"))
                            || _selectedMetrics.Any(m => m.EnName.Contains("uclk")))
                        {
                            if (_selectedDimensions.Count == 1)
                            {
                                SetDisplayTypeEnabled(_trackAnalysisReportFreqsWithMetricDisplayType1);
                            }
                            else if (_selectedDimensions.Count > 1)
                            {
                                SetDisplayTypeEnabled(_trackAnalysisReportFreqsWithMetricDisplayType2);
                            }
                        }
                        //少2个判断
                    }

                    if (!_selectedDimensions.Any(d => d.EnName == "campaigns"))
                    {
                        if (_selectedDimensions.Count == 2)
                        {
                            foreach (var checkbox in DimensionItemsSource)
                            {
                                foreach (var dimension in checkbox.Items)
                                {
                                    if (!_selectedDimensions.Contains(dimension))
                                    {
                                        (dimension as DimensionTypeModel).IsEnabled = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var checkbox in DimensionItemsSource)
                            {
                                foreach (var dimension in checkbox.Items)
                                {
                                    if (!_selectedDimensions.Contains(dimension))
                                    {
                                        (dimension as DimensionTypeModel).IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region 定投类型

                if (DimensionItemsSource[DimensionTabSelectedIndex].Header.EnHeader == "target")
                {
                    SetDisplayTypeEnabled("Grid");

                    if (_selectedDimensions.Any(d => d.EnName == "placementName"))
                    {
                        foreach (var checkbox in DimensionItemsSource)
                        {
                            foreach (var dimension in checkbox.Items)
                            {
                                if (dimension.EnName == "targetUrl")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }
                    }

                    if (_selectedDimensions.Any(d => d.EnName == "targetUrl"))
                    {
                        foreach (var checkbox in DimensionItemsSource)
                        {
                            foreach (var dimension in checkbox.Items)
                            {
                                if (dimension.EnName == "placementName")
                                {
                                    (dimension as DimensionTypeModel).IsEnabled = false;
                                }
                            }
                        }

                        var list = new List<string> { "result", "title", "breadcrumb", "identifier", "targetRate" };
                        foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                        {
                            if (!list.Contains(metric.EnName))
                            {
                                (metric as MetricTypeModel).IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        var list = new List<string> { "result", "title", "breadcrumb", "identifier", "targetRate" };
                        foreach (var metric in MetricItemsSource[MetricTabSelectedIndex].Items)
                        {
                            if (!list.Contains(metric.EnName))
                            {
                                (metric as MetricTypeModel).IsEnabled = false;
                            }
                        }
                    }
                }

                #endregion
            }

            #endregion

            #region SiteRealtime

            if (SystemType.Id == (int)SystemTypeEnum.SiteRealtime)
            {
                if (DataTypeItemsSource[DataTypeTabSelectedIndex].Header.EnHeader == "online" ||
                    DataTypeItemsSource[DataTypeTabSelectedIndex].Header.EnHeader == "statistics")
                {
                    switch (_dataType)
                    {
                        case "summary":
                        case "detail":
                            SetDisplayTypeEnabled(_siteRealtimeSummaryDisplayType);
                            break;
                        case "city":
                        case "province":
                        case "keyword":
                        case "ads":
                        case "event":
                            SetDisplayTypeEnabled(_siteRealtimeEventDisplayType);
                            break;
                        case "source":
                        case "organic":
                            SetDisplayTypeEnabled(_siteRealtimeOtherDisplayType);
                            break;
                        case "page":
                            SetDisplayTypeEnabled("Grid");
                            break;
                    }
                }
                else
                {
                    switch (_dataType)
                    {
                        case "summary":
                        case "hour":
                            SetDisplayTypeEnabled(_siteRealtimeTransformSummaryDisplayType);
                            break;
                        case "event":
                        case "detail":
                            SetDisplayTypeEnabled(_siteRealtimeEventDisplayType);
                            break;
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// 显示类型改变后改变数据数量
        /// </summary>
        public void DisplayTypeSelectiuonChanged()
        {
            switch (DisplayType.Type)
            {
                case "Grid":
                    DataCount = 100;
                    break;
                case "Chart":
                    DataCount = 10;
                    break;
                case "Gauge":
                    DataCount = 1;
                    break;
                case "Map":
                    DataCount = 10;
                    break;
            }
        }

        #region TA

        public void OnTaActivated()
        {
            if (this.GenderItems == null)
            {
                this.GenderItems = this.GetGender();
            }

            if (this.ZoneItems == null)
            {
                this.ZoneItems = this.GetZone();
            }

            if (this.EducationItems == null)
            {
                this.EducationItems = this.GetEduction();
            }

            if (this.MarriageItems == null)
            {
                this.MarriageItems = this.GetMarriage();
            }

            if (this.IncomeItems == null)
            {
                this.IncomeItems = this.GetIncome();
            }
        }

        private ObservableCollection<Node> GetGender()
        {
            var genderItems = new ObservableCollection<Node>();
            var man = new Node { Id = "1", Title = "男", IsSelected = false };
            genderItems.Add(man);

            var woman = new Node { Id = "2", Title = "女", IsSelected = false };
            genderItems.Add(woman);
            return genderItems;
        }

        private ObservableCollection<Node> GetZone()
        {
            var zoneItems = new ObservableCollection<Node>();
            var beijing = new Node { Id = "1", Title = "北京", IsSelected = false };
            zoneItems.Add(beijing);
            var shanghai = new Node { Id = "2", Title = "上海", IsSelected = false };
            zoneItems.Add(shanghai);
            var chengdu = new Node { Id = "3", Title = "成都", IsSelected = false };
            zoneItems.Add(chengdu);
            var tianjin = new Node { Id = "4", Title = "天津", IsSelected = false };
            zoneItems.Add(tianjin);
            var guangzhou = new Node { Id = "5", Title = "广州", IsSelected = false };
            zoneItems.Add(guangzhou);
            var shenzhen = new Node { Id = "6", Title = "深圳", IsSelected = false };
            zoneItems.Add(shenzhen);
            var nanjing = new Node { Id = "7", Title = "南京", IsSelected = false };
            zoneItems.Add(nanjing);
            var hangzhou = new Node { Id = "8", Title = "杭州", IsSelected = false };
            zoneItems.Add(hangzhou);
            var wuhan = new Node { Id = "9", Title = "武汉", IsSelected = false };
            zoneItems.Add(wuhan);
            var shenyang = new Node { Id = "10", Title = "沈阳", IsSelected = false };
            zoneItems.Add(shenyang);

            return zoneItems;
        }

        private ObservableCollection<Node> GetEduction()
        {
            var eductionItems = new ObservableCollection<Node>();
            var none = new Node { Id = "1", Title = "未受过正式教育", IsSelected = false };
            eductionItems.Add(none);
            var primary = new Node { Id = "2", Title = "小学", IsSelected = false };
            eductionItems.Add(primary);
            var junior = new Node { Id = "3", Title = "初中", IsSelected = false };
            eductionItems.Add(junior);
            var senior = new Node { Id = "4", Title = "高中", IsSelected = false };
            eductionItems.Add(senior);
            var technical = new Node { Id = "5", Title = "中专", IsSelected = false };
            eductionItems.Add(technical);
            var juniorCollege = new Node { Id = "6", Title = "专科", IsSelected = false };
            eductionItems.Add(juniorCollege);
            var college = new Node { Id = "7", Title = "本科", IsSelected = false };
            eductionItems.Add(college);
            var master = new Node { Id = "8", Title = "硕士", IsSelected = false };
            eductionItems.Add(master);
            var doctor = new Node { Id = "9", Title = "博士及其以上", IsSelected = false };
            eductionItems.Add(doctor);
            var refusal = new Node { Id = "10", Title = "拒绝回答", IsSelected = false };
            eductionItems.Add(refusal);

            return eductionItems;
        }

        private ObservableCollection<Node> GetMarriage()
        {
            var marriageItems = new ObservableCollection<Node>();
            var none = new Node { Id = "1", Title = "未婚", IsSelected = false };
            marriageItems.Add(none);
            var noChild = new Node { Id = "2", Title = "已婚无小孩", IsSelected = false };
            marriageItems.Add(noChild);
            var hasChild = new Node { Id = "3", Title = "已婚有小孩", IsSelected = false };
            marriageItems.Add(hasChild);
            var divorce = new Node { Id = "4", Title = "离异", IsSelected = false };
            marriageItems.Add(divorce);
            var other = new Node { Id = "5", Title = "其他", IsSelected = false };
            marriageItems.Add(other);

            return marriageItems;
        }

        private ObservableCollection<Node> GetIncome()
        {
            var incomeItems = new ObservableCollection<Node>();
            var none = new Node { Id = "1", Title = "没有收入", IsSelected = false };
            incomeItems.Add(none);
            var lowest = new Node { Id = "2", Title = "人民币1000元以下", IsSelected = false };
            incomeItems.Add(lowest);

            for (int index = 3; index <= 12; index++)
            {
                var id = index.ToString();
                var minIncome = (index - 1) * 500;
                var maxIncome = index * 500 - 1;
                var title = "人民币" + minIncome + "-" + maxIncome + "元";
                var lower = new Node { Id = id, Title = title, IsSelected = false };
                incomeItems.Add(lower);
            }

            for (int index = 13; index <= 16; index++)
            {
                var id = index.ToString();
                var minIncome = (index - 7) * 1000;
                var maxIncome = (index - 6) * 1000 - 1;
                var title = "人民币" + minIncome + "-" + maxIncome + "元";
                var higher = new Node { Id = id, Title = title, IsSelected = false };
                incomeItems.Add(higher);
            }

            var higher1 = new Node { Id = "17", Title = "人民币10000-11999元", IsSelected = false };
            incomeItems.Add(higher1);

            var higher2 = new Node { Id = "18", Title = "人民币12000-14999元", IsSelected = false };
            incomeItems.Add(higher2);

            var higher3 = new Node { Id = "19", Title = "人民币15000-19999元", IsSelected = false };
            incomeItems.Add(higher3);

            var highest = new Node { Id = "20", Title = "人民币20000元以上", IsSelected = false };
            incomeItems.Add(highest);

            return incomeItems;
        }

        public void OnGenderClosed()
        {
            if (!this.ShowSampleCount)
            {
                return;
            }

            var count = this.GetSamplesCount();
            this.SampleInfo = string.Format("样本总量为：{0}", count);
        }

        public void OnZoneClosed()
        {
            if (!this.ShowSampleCount)
            {
                return;
            }

            var count = this.GetSamplesCount();
            this.SampleInfo = string.Format("样本总量为：{0}", count);
        }

        public void OnEducationClosed()
        {
            if (!this.ShowSampleCount)
            {
                return;
            }

            var count = this.GetSamplesCount();
            this.SampleInfo = string.Format("样本总量为：{0}", count);
        }

        public void OnMarriageClosed()
        {
            if (!this.ShowSampleCount)
            {
                return;
            }

            var count = this.GetSamplesCount();
            this.SampleInfo = string.Format("样本总量为：{0}", count);
        }

        public void OnIncomeClosed()
        {
            if (!this.ShowSampleCount)
            {
                return;
            }

            var count = this.GetSamplesCount();
            this.SampleInfo = string.Format("样本总量为：{0}", count);
        }

        public void OnSampleActivated()
        {
            if (!this.ShowSampleCount)
            {
                this.SampleInfo = string.Empty;
            }
            else
            {
                this.SampleInfo = "show";
            }
        }

        private int GetSamplesCount()
        {
            var taInfo = this.GetTaInfo();

            var count = TargetAudienceService.GetTaCount(taInfo);
            return 0;
        }

        private TargetAudienceInfo GetTaInfo()
        {
            var taInfo = new TargetAudienceInfo
            {
                Age = this.MinAge + "," + this.MaxAge,
                Gender = this.SelectedGender,
                Zone = this.SelectedZone,
                Education = this.SelectedEducation,
                Marriage = this.SelectedMarriage,
                Income = this.SelectedIncome
            };

            //            taInfo.Gender = taInfo.ToList(this.SelectedGender);
            //            taInfo.Zone = taInfo.ToList(this.SelectedZone);
            //            taInfo.Education = taInfo.ToList(this.SelectedEducation);
            //            taInfo.Marriage = taInfo.ToList(this.SelectedMarriage);
            //            taInfo.Income = taInfo.ToList(this.SelectedIncome);

            return taInfo;
        }

        #endregion
    }
}