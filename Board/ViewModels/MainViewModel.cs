using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Board.Common;
using Board.Common.ExcelExport;
using Board.Enums;
using Board.Models.Control;
using Board.Models.Sponsor;
using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Models.TrackRealtime;
using Board.Models.Users;
using Board.Services.System;
using Board.SystemData.Base;
using Board.SystemData.Benchmark;
using Board.SystemData.SiteRealtime;

using C1.WPF.C1Chart;
using C1.WPF.C1Chart.Extended;
using C1.WPF.Excel;
using C1.WPF.FlexGrid;
using Caliburn.Micro;

using MahApps.Metro.Controls;

using Newtonsoft.Json;

using PropertyChanged;

using RestSharp;

using FileFormat = C1.WPF.Excel.FileFormat;
using Screen = Caliburn.Micro.Screen;

namespace Board.ViewModels
{
    [ImplementPropertyChanged]
    public class MainViewModel : Screen, IShell, IHandle<WidgetModel>
    {
        #region 变量

        private Dictionary<int, string> _trackRealtimeDic;

        private TaskFactory _taskFactory;

        private CancellationToken _cancellationToken;

        private List<CancellationTokenSource> cancellationTokenSources = new List<CancellationTokenSource>();

        private List<WidgetModel> _timerList;

        private List<int> _systemList;

        /// <summary>
        /// 缓存时间维度
        /// </summary>
        private List<TimeDimensionTypeModel> _timeDimensionsCache;

        /// <summary>
        /// 缓存维度
        /// </summary>
        private List<DimensionTypeModel> _dimensionsCache;

        /// <summary>
        /// 缓存数据类型
        /// </summary>
        private List<DataTypeModel> _dataTypesCache;

        /// <summary>
        /// 缓存指标
        /// </summary>
        private List<MetricTypeModel> _metricsCache;

        /// <summary>
        /// 当前日期
        /// </summary>
        private DateTime _currentDateTime;

        /// <summary>
        /// 上次的DateType，防止切换DataCabin切换后，DateType相同，不查询Widget
        /// </summary>
        private int _lastDateTypeId;

        private SelectedTypeEnum _selectedTypeEnum;

        #endregion

        #region 属性

        /// <summary>
        /// 显示时间最大值
        /// </summary>
        public DateTime DisplayDateEnd { get; set; }

        /// <summary>
        /// 自定义时间
        /// </summary>
        public bool IsCustomChecked { get; set; }

        /// <summary>
        /// 昨天时间
        /// </summary>

        public bool IsYesterdayChecked { get; set; }

        /// <summary>
        /// 上周时间
        /// </summary>
        public bool IsLastWeekChecked { get; set; }

        /// <summary>
        /// 上月时间
        /// </summary>
        public bool IsLastMonthChecked { get; set; }

        /// <summary>
        /// 时间可用
        /// </summary>
        public bool DateIsEnabled { get; set; }

        /// <summary>
        /// 是否可添加
        /// </summary>
        public bool IsCanAdd { get; set; }

        public bool IsCanAddBenchmark { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Widget区域宽度
        /// </summary>
        public double WidgetAreaWidth { get; set; }

        /// <summary>
        /// Widget区域高度
        /// </summary>
        public double WidgetAreaHeight { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Widget列表
        /// </summary>
        public ObservableCollection<IWidgetInfo> WidgetList { get; set; }

        /// <summary>
        /// DataCabin列表
        /// </summary>
        public ObservableCollection<DataCabinModel> DataCabinList { get; set; }

        /// <summary>
        /// 原始数据
        /// </summary>
        public ObservableCollection<OriginalDataModel> OriginalDataList { get; set; }

        /// <summary>
        /// 客户列表
        /// </summary>
        public ObservableCollection<ClientModel> ClientList { get; set; }

        /// <summary>
        /// 添加DataCabin名称
        /// </summary>
        public string AddDataCabinName { get; set; }

        /// <summary>
        /// 选中DataCabin
        /// </summary>
        public DataCabinModel SelectedDataCabin { get; set; }

        /// <summary>
        /// 显示区可见性
        /// </summary>
        public bool WidgetAreaVisibility { get; set; }

        /// <summary>
        /// 显示区可见性
        /// </summary>
        public bool OriginalDataAreaVisibility { get; set; }

        /// <summary>
        /// 选中客户
        /// </summary>
        public ClientModel ClientSelectedItem { get; set; }

        #endregion

        #region 构造

        /// <summary>
        /// 构造
        /// </summary>
        public MainViewModel()
        {
            var eventX = IoC.Get<IEventAggregator>();
            eventX.Subscribe(this);

            _cancellationToken = new CancellationToken();
            _taskFactory = new TaskFactory(_cancellationToken);
            _timerList = new List<WidgetModel>();
            _trackRealtimeDic = new Dictionary<int, string>();

            LoadConfigData();
        }

        #endregion

        #region 初始化数据

        /// <summary>
        /// 加载配置数据
        /// </summary>
        private async void LoadConfigData()
        {
            try
            {
                var configDictionaryResult = await MainService.GetConfigData(User.UserInfo.UserName);

                if (((RestResponseBase)(configDictionaryResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var configDictionary = configDictionaryResult.Data.ToDictionary(d => d.Key, d => d.Value);

                    string currentDateTimeString = configDictionary["CurrentDate"];
                    string userString = configDictionary["UserInfo"];
                    string clientListString = configDictionary["ClientList"];
                    string originalDataListString = configDictionary["OriginalDataList"];
                    string timeDimensionListString = configDictionary["TimeDimensionList"];
                    string dimensionListString = configDictionary["DimensionList"];
                    string dataTypeListString = configDictionary["DataTypeList"];
                    string metricListString = configDictionary["MetricList"];
                    string systemListString = configDictionary["SystemList"];

                    InitDateTime(currentDateTimeString);
                    InitUser(userString);
                    InitClientList(clientListString);
                    var originalDataList = JsonConvert.DeserializeObject<ObservableCollection<OriginalDataModel>>(originalDataListString);
                    InitOriginalDataList(originalDataList);

                    _systemList = JsonConvert.DeserializeObject<List<int>>(systemListString);
                    _timeDimensionsCache = JsonConvert.DeserializeObject<List<TimeDimensionTypeModel>>(timeDimensionListString);
                    _dimensionsCache = JsonConvert.DeserializeObject<List<DimensionTypeModel>>(dimensionListString);
                    _dataTypesCache = JsonConvert.DeserializeObject<List<DataTypeModel>>(dataTypeListString);
                    _metricsCache = JsonConvert.DeserializeObject<List<MetricTypeModel>>(metricListString);
                }
                else if (((RestResponseBase)(configDictionaryResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)configDictionaryResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(configDictionaryResult)).StatusCode == HttpStatusCode.InternalServerError)//500
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
                ShowMessage.Show("初始化数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to LoadConfigData", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "LoadConfigData", null);
                }
            }
        }

        /// <summary>
        /// 初始化日期
        /// </summary>
        /// <param name="currentDateTimeString"></param>
        private void InitDateTime(string currentDateTimeString)
        {
            try
            {
                var currentDateTime = Convert.ToDateTime(currentDateTimeString);
                _currentDateTime = currentDateTime;

                DisplayDateEnd = currentDateTime;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化日期出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDateTime", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDateTime", null);
                }
            }
        }

        /// <summary>
        /// 初始化用户
        /// </summary>
        /// <param name="userString"></param>
        private void InitUser(string userString)
        {
            try
            {
                var user = JsonConvert.DeserializeObject<UserModel>(userString);

                var userInfo = new UserInfo()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    AuthorizationId = user.AuthorizationId,
                    CreteDate = user.CreateDate
                };
                User.UserInfo = userInfo;
                UserName = user.UserName;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化用户出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitUser", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitUser", null);
                }
            }
        }

        /// <summary>
        /// 初始化客户列表
        /// </summary>
        /// <param name="clientListString"></param>
        private void InitClientList(string clientListString)
        {
            try
            {
                var clientList = JsonConvert.DeserializeObject<List<ClientModel>>(clientListString);
                var clientSource = new ObservableCollection<ClientModel>();
                foreach (var client in clientList)
                {
                    var clienInfo = new ClientModel()
                    {
                        Id = client.Id,
                        ClientName = client.ClientName,
                        CreateDate = client.CreateDate
                    };
                    clientSource.Add(clienInfo);
                }

                ClientList = clientSource;
                ClientSelectedItem = clientSource[0];
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化客户列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitClientList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitClientList", null);
                }
            }
        }

        /// <summary>
        /// 初始化DataCabin列表
        /// </summary>
        /// <param name="dataCabinList"></param>
        private void InitDataCabinList(List<DataCabinModel> dataCabinList)
        {
            try
            {
                var dataCabinSource = new ObservableCollection<DataCabinModel>();
                foreach (var dataCabin in dataCabinList)
                {
                    var dataCabinInfo = new DataCabinModel();
                    dataCabinInfo.Id = dataCabin.Id;
                    dataCabinInfo.DataCabinName = dataCabin.DataCabinName;
                    dataCabinInfo.DataCabinTypeId = dataCabin.DataCabinTypeId;
                    dataCabinInfo.ClientId = dataCabin.ClientId;
                    dataCabinInfo.UserId = dataCabin.UserId;
                    dataCabinInfo.DateTypeId = dataCabin.DateTypeId;
                    dataCabinInfo.StartDate = dataCabin.StartDate;
                    dataCabinInfo.EndDate = dataCabin.EndDate;
                    dataCabinInfo.CreateDate = dataCabin.CreateDate;
                    dataCabinInfo.UpdateDate = dataCabin.UpdateDate;
                    if (dataCabin.DataCabinTypeId == 1)
                    {
                        dataCabinInfo.CanChange = false;
                        dataCabinInfo.CanDelete = User.UserInfo.AuthorizationId != 3;
                    }
                    else
                    {
                        dataCabinInfo.CanChange = User.UserInfo.AuthorizationId != 3;
                        dataCabinInfo.CanDelete = true;
                    }

                    dataCabinSource.Add(dataCabinInfo);
                }

                DataCabinList = dataCabinSource;

                if (dataCabinSource.Count == 0)
                {
                    SelectedDataCabin = null;
                    IsLastWeekChecked = true;
                    StartDate = _currentDateTime.AddDays(-7);
                    EndDate = _currentDateTime;
                    WidgetList = new ObservableCollection<IWidgetInfo>();
                    SetWhetherCanAdd();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化数据舱列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitDataCabinList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitDataCabinList", null);
                }
            }
        }

        /// <summary>
        /// 初始化原始数据列表
        /// </summary>
        /// <param name="originalDataList"></param>
        private void InitOriginalDataList(ObservableCollection<OriginalDataModel> originalDataList)
        {
            try
            {
                OriginalDataList = originalDataList;
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化原始数据列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitOriginalDataList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitOriginalDataList", null);
                }
            }
        }

        /// <summary>
        /// 初始化Widget列表
        /// </summary>
        /// <param name="widgetModelList"></param>
        private void InitWidgetList(List<WidgetModel> widgetModelList)
        {
            try
            {
                CleanTimerList();

                foreach (var cancellationTokenSource in cancellationTokenSources)
                {
                    cancellationTokenSource.Cancel();
                }

                cancellationTokenSources.Clear();
                WidgetList = new ObservableCollection<IWidgetInfo>();

                foreach (var widgetModel in widgetModelList)
                {
                    switch (widgetModel.DisplayType)
                    {
                        case "Grid":
                            AddGridItem(widgetModel);
                            break;
                        case "Chart":
                            AddChartItem(widgetModel);
                            break;
                        case "Gauge":
                            AddGaugeItem(widgetModel);
                            break;
                        case "Map":
                            AddMapItem(widgetModel);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("初始化Widget列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to InitWidgetList", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "InitWidgetList", null);
                }
            }
        }

        /// <summary>
        /// 设置日期选中(DataCabin选择改变时触发)
        /// </summary>
        private void SetDateChecked()
        {
            try
            {
                var dateTypeId = SelectedDataCabin == null ? 3 : SelectedDataCabin.DateTypeId;

                switch (dateTypeId)
                {
                    case 1:
                        IsCustomChecked = true;
                        if (SelectedDataCabin != null)
                        {
                            StartDate = SelectedDataCabin.StartDate;
                            EndDate = SelectedDataCabin.EndDate;
                        }

                        break;
                    case 2:
                        IsYesterdayChecked = true;
                        StartDate = _currentDateTime.AddDays(-1);
                        EndDate = _currentDateTime;
                        break;
                    case 3:
                        IsLastWeekChecked = true;
                        StartDate = _currentDateTime.AddDays(-7);
                        EndDate = _currentDateTime;
                        break;
                    case 4:
                        IsLastMonthChecked = true;
                        StartDate = _currentDateTime.AddMonths(-1);
                        EndDate = _currentDateTime;
                        break;
                }

                if (_lastDateTypeId == dateTypeId)
                {
                    DateSwitchChecked();
                }
                else
                {
                    _lastDateTypeId = dateTypeId;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("设置日期选中出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to SetDateChecked", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "SetDateChecked", null);
                }
            }
        }

        /// <summary>
        /// 设置是否可添加
        /// </summary>
        private void SetWhetherCanAdd()
        {
            try
            {
                if (SelectedDataCabin == null)
                {
                    IsCanAdd = false;
                    IsCanAddBenchmark = false;
                }
                else if (SelectedDataCabin.DataCabinTypeId == 1)
                {
                    IsCanAdd = User.UserInfo.AuthorizationId != 3;
                    if (User.UserInfo.AuthorizationId != 3 && _systemList.Contains((int)SystemTypeEnum.Benchmark))
                    {
                        IsCanAddBenchmark = true;
                    }
                    else
                    {
                        IsCanAddBenchmark = false;
                    }
                }
                else
                {
                    IsCanAdd = true;
                    if (_systemList.Contains((int)SystemTypeEnum.Benchmark))
                    {
                        IsCanAddBenchmark = true;
                    }
                    else
                    {
                        IsCanAddBenchmark = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("设置添加按钮状态出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to SetWhetherCanAdd", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "SetWhetherCanAdd", null);
                }
            }
        }

        #endregion

        #region 查询数据

        /// <summary>
        /// 根据时间查询数据
        /// </summary>
        /// <param name="widgetModel"></param>
        private async void QueryData(WidgetModel widgetModel)
        {
            try
            {
                switch (widgetModel.DateTypeId)
                {
                    case 0: //受全局控制
                        widgetModel.StartDate = StartDate;
                        widgetModel.EndDate = EndDate;
                        break;
                    case 1: //自定义
                        break;
                    case 2: //昨天
                        widgetModel.StartDate = _currentDateTime.AddDays(-1);
                        widgetModel.EndDate = _currentDateTime;
                        break;
                    case 3: //上周
                        widgetModel.StartDate = _currentDateTime.AddDays(-7);
                        widgetModel.EndDate = _currentDateTime;
                        break;
                    case 4: //上月
                        widgetModel.StartDate = _currentDateTime.AddMonths(-1);
                        widgetModel.EndDate = _currentDateTime;
                        break;
                }

                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.Benchmark)
                {
                    var parameterDictionary = new Dictionary<string, object>();
                    parameterDictionary.Add("DateType", widgetModel.DateTypeId);
                    parameterDictionary.Add("BrandType", widgetModel.DataType);
                    parameterDictionary.Add("Category", widgetModel.Filter);
                    parameterDictionary.Add("Dimensions", widgetModel.Dimensions);
                    parameterDictionary.Add("Metrics", widgetModel.Metrics);
                    var data = await BenchmarkDataService.GetSystemData(parameterDictionary);

                    switch (widgetModel.DisplayType)
                    {
                        case "Grid":
                            #region

                            var bindingColumns = BenchmarkDataService.ConvertToGridData(widgetModel, _dimensionsCache, _metricsCache);
                            var gridItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (gridItem is GridItemInfo)
                            {
                                var count = 0;
                                switch (widgetModel.DataType)
                                {
                                    case "1":
                                        break;
                                    case "2":
                                    case "3":
                                    case "4":
                                    case "5":
                                    case "6":
                                        count = 1;
                                        break;
                                    case "7":
                                        count = 2;
                                        break;
                                }

                                Execute.OnUIThreadAsync(() =>
                                {
                                    var gridItemx = gridItem as GridItemInfo;
                                    gridItemx.BindingFrozenColumns = 1;
                                    gridItemx.BindingColumnsConfig = bindingColumns;
                                    var items = new ObservableCollection<ISystemData>();
                                    var systemData = JsonConvert.DeserializeObject<SponsorData>(data.Value);
                                    items.Add(systemData);
                                    gridItemx.GridItemSource = items;
                                    gridItemx.IsLoading = false;
                                });
                            }

                            #endregion
                            break;
                        case "Chart":

                            #region
                            var list = BenchmarkDataService.ConvertToChartData(widgetModel, data, _metricsCache);
                            var datas = (ObservableCollection<string>)list[0];
                            var series = (ObservableCollection<MyReport>)list[1];
                            var axisDic = (Dictionary<string, Tuple<string, string>>)list[2];
                            var values = (List<decimal>)list[3];

                            var chartItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (chartItem is ChartItemInfo)
                            {
                                Execute.OnUIThreadAsync(() =>
                                {
                                    #region 设置X,Y轴

                                    var axisList = new ObservableCollection<Axis>();
                                    var maxValue = values.Max();
                                    foreach (var axis in axisDic)
                                    {
                                        var axisType = AxisType.Y;
                                        var position = AxisPosition.Inner;

                                        if (axisList.Count == 0)
                                        {
                                            axisList.Add(new Axis
                                            {
                                                Name = axis.Value.Item1,
                                                AxisType = AxisType.Y,
                                                Position = AxisPosition.Inner,
                                                Min = 0,
                                                Max = (double)maxValue == 0 ? 1 : (double)maxValue,
                                                MajorGridStrokeThickness = 0
                                            });
                                        }
                                    }

                                    var axisX = axisList.FirstOrDefault(a => a.AxisType == AxisType.X);
                                    if (axisX != null)
                                    {
                                        axisList.Remove(axisX);
                                    }

                                    var axisY = axisList.FirstOrDefault(a => a.AxisType == AxisType.Y);
                                    if (axisY != null)
                                    {
                                        axisList.Remove(axisY);
                                    }

                                    #endregion

                                    var chartItemx = chartItem as ChartItemInfo;
                                    chartItemx.Axis = axisList;
                                    chartItemx.Datas = datas;
                                    chartItemx.Series = series;
                                    chartItemx.IsLoading = false;
                                });
                            }

                            #endregion
                            break;
                    }
                }
                else
                {
                    var startDate = widgetModel.StartDate == null ? StartDate.ToString("yyyy-MM-dd HH:mm:ss") : widgetModel.StartDate.ToString().Replace('/', '-');
                    var endDate = widgetModel.EndDate == null ? EndDate.ToString("yyyy-MM-dd HH:mm:ss") : widgetModel.EndDate.ToString().Replace('/', '-');

                    var system = SystemDataBuilder.SystemBuilder(widgetModel.SystemTypeId);
                    int? interval = new int?();

                    if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                    {
                        _timerList.Add(widgetModel);

                        widgetModel.WidgetTimer.WidgetModel = widgetModel;

                        widgetModel.WidgetTimer.IsModelInitialized = true;
                        if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                        {
                            interval = _dataTypesCache.Find(d => d.EnName == widgetModel.DataType).Interval;
                        }
                    }

                    if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                    {
                        startDate = _currentDateTime.ToShortDateString().Replace('/', '-') + " 00:00:00";
                        endDate = _currentDateTime.AddDays(1).ToShortDateString().Replace('/', '-') + " 00:00:00";
                    }

                    if (widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                    {
                        if (widgetModel.Dimensions == "campaignsName")
                        {
                            startDate = _currentDateTime.ToShortDateString().Replace('/', '-') + " 00:00:00";
                            endDate = _currentDateTime.AddDays(1).ToShortDateString().Replace('/', '-') + " 00:00:00";
                        }
                        else
                        {
                            if (!_trackRealtimeDic.Keys.Contains(widgetModel.Id))
                            {
                                var datetime = DateTime.Now;
                                if (widgetModel.TimeDimensions == "fiveMinutes")
                                {
                                    startDate = datetime.AddMinutes(-45).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                    endDate = datetime.AddMinutes(-40).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                }
                                else
                                {
                                    startDate = datetime.AddMinutes(-45).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                    endDate = datetime.AddMinutes(-45).AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                }
                            }
                            else
                            {
                                var datetime = Convert.ToDateTime(_trackRealtimeDic[widgetModel.Id]).AddSeconds(1);
                                if (widgetModel.TimeDimensions == "fiveMinutes")
                                {
                                    startDate = datetime.ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                    endDate = datetime.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                }
                                else
                                {
                                    startDate = datetime.ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                    endDate = datetime.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                                }
                            }

                            if (!_trackRealtimeDic.Keys.Contains(widgetModel.Id))
                            {
                                _trackRealtimeDic.Add(widgetModel.Id, endDate);
                            }
                        }
                    }

                    var parameterDictionary = new Dictionary<string, object>();
                    parameterDictionary.Add("WidgetModel", widgetModel);
                    parameterDictionary.Add("Interval", interval);
                    parameterDictionary.Add("StartDate", startDate);
                    parameterDictionary.Add("EndDate", endDate);
                    parameterDictionary.Add("ClientId", ClientSelectedItem.Id);
                    parameterDictionary.Add("TimeDimensions", _timeDimensionsCache);
                    parameterDictionary.Add("Dimensions", _dimensionsCache);
                    parameterDictionary.Add("Metrics", _metricsCache);
                    var data = await system.GetSystemData(parameterDictionary);
                    ConvertDataByDisplayType(widgetModel, data);
                }
            }
            catch (Exception ex)
            {
                //ShowMessage.Show("查询数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to QueryData", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "QueryData", null);
                }
            }
        }

        private TaskTimer InitialTimer(string timeDimension)
        {
            var timer = new TaskTimer();
            if (timeDimension == "fiveMinutes")
            {
                timer.Interval = new TimeSpan(0, 5, 0);
            }
            else
            {
                timer.Interval = new TimeSpan(0, 0, 10);
            }

            timer.Tick += TimerTick;
            return timer;
        }

        private async void TimerTick(object sender, EventArgs e)
        {
            var timer = sender as TaskTimer;
            if (timer == null)
            {
                return;
            }

            if (!timer.IsModelInitialized)
            {
                return;
            }

            var widgetModel = timer.WidgetModel;
            var system = SystemDataBuilder.SystemBuilder(widgetModel.SystemTypeId);
            int? interval = new int?();

            if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
            {
                interval = _dataTypesCache.Find(d => d.EnName == widgetModel.DataType).Interval;
            }

            var startDate = widgetModel.StartDate == null ? StartDate.ToString("yyyy-MM-dd HH:mm:ss") : widgetModel.StartDate.ToString().Replace('/', '-');
            var endDate = widgetModel.EndDate == null ? EndDate.ToString("yyyy-MM-dd HH:mm:ss") : widgetModel.EndDate.ToString().Replace('/', '-');

            if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
            {
                startDate = _currentDateTime.ToShortDateString().Replace('/', '-') + " 00:00:00";
                endDate = _currentDateTime.AddDays(1).ToShortDateString().Replace('/', '-') + " 00:00:00";
            }

            if (widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
            {
                if (widgetModel.Dimensions == "campaignsName")
                {
                    startDate = _currentDateTime.ToShortDateString().Replace('/', '-') + " 00:00:00";
                    endDate = _currentDateTime.AddDays(1).ToShortDateString().Replace('/', '-') + " 00:00:00";
                }
                else
                {
                    if (!_trackRealtimeDic.Keys.Contains(widgetModel.Id))
                    {
                        var datetime = DateTime.Now;
                        if (widgetModel.TimeDimensions == "fiveMinutes")
                        {
                            startDate = datetime.AddMinutes(-45).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                            endDate = datetime.AddMinutes(-40).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                        }
                        else
                        {
                            startDate = datetime.AddMinutes(-45).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                            endDate = datetime.AddMinutes(-45).AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                        }
                    }
                    else
                    {
                        var datetime = Convert.ToDateTime(_trackRealtimeDic[widgetModel.Id]).AddSeconds(1);
                        if (widgetModel.TimeDimensions == "fiveMinutes")
                        {
                            startDate = datetime.ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                            endDate = datetime.AddMinutes(5).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                        }
                        else
                        {
                            startDate = datetime.ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                            endDate = datetime.AddSeconds(10).ToString("yyyy-MM-dd HH:mm:ss").Replace('/', '-');
                        }
                    }

                    _trackRealtimeDic[widgetModel.Id] = endDate;
                }
            }

            var parameterDictionary = new Dictionary<string, object>();
            parameterDictionary.Add("WidgetModel", widgetModel);
            parameterDictionary.Add("Interval", interval);
            parameterDictionary.Add("StartDate", startDate);
            parameterDictionary.Add("EndDate", endDate);
            parameterDictionary.Add("TimeDimensions", _timeDimensionsCache);
            parameterDictionary.Add("Dimensions", _dimensionsCache);
            parameterDictionary.Add("Metrics", _metricsCache);
            var data = await system.GetSystemData(parameterDictionary);
            if (data != null)
            {
                ConvertDataByDisplayType(widgetModel, data);
            }
        }

        /// <summary>
        /// 根据展示方式转换数据
        /// </summary>
        /// <param name="widgetModel"></param>
        /// <param name="resultValue"></param>
        private void ConvertDataByDisplayType(WidgetModel widgetModel, ResultValue resultValue)
        {
            try
            {
                var timeDimensions = widgetModel.TimeDimensions == null ? new string[0] : widgetModel.TimeDimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var dimensions = widgetModel.Dimensions == null ? new string[0] : widgetModel.Dimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var metrics = widgetModel.Metrics == null ? new string[0] : widgetModel.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                SystemDataBuilder.BuildSystemData(widgetModel.SystemTypeId, widgetModel.DataType, resultValue, widgetModel.EnHeader);

                var systemData = SystemDataBuilder.SystemData;
                var systemDataList = SystemDataBuilder.SystemDataList;

                if (systemDataList != null)
                {
                    switch (widgetModel.DisplayType)
                    {
                        case "Grid":
                            #region

                            foreach (var systemDatax in systemDataList)
                            {
                                systemData.Add(systemDatax);
                            }

                            var bindingColumns = new ObservableCollection<ColumnConfig>();

                            if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                            {
                                var type = SiteRealtimeType.GetSiteRealtimeDataType(widgetModel.EnHeader, widgetModel.DataType);
                                if (type != null)
                                {
                                    var properties = type.GetProperties();

                                    foreach (var propertyx in properties)
                                    {
                                        var propertye = propertyx.Name;
                                        var isDimension = _dimensionsCache.Any(d => d.SystemTypeId == widgetModel.SystemTypeId && d.EnName.Equals(propertye, StringComparison.OrdinalIgnoreCase));
                                        if (isDimension)
                                        {
                                            var dimension = _dimensionsCache.Find(d => d.SystemTypeId == widgetModel.SystemTypeId && d.EnName.Equals(propertye, StringComparison.OrdinalIgnoreCase));
                                            if (dimension != null)
                                            {
                                                bindingColumns.Add(new ColumnConfig
                                                {
                                                    ColumnName = dimension.EnName.Substring(0, 1).ToUpperInvariant() + dimension.EnName.Substring(1),
                                                    HeaderName = dimension.CnName,
                                                    HeaderDescription = dimension.CnDescription,
                                                    Width = dimension.CnName.Length * 18,
                                                    FormatString = dimension.Format,
                                                    HeaderColorString = dimension.Color
                                                });
                                            }
                                        }
                                        else
                                        {
                                            var metric = _metricsCache.Find(d => d.SystemTypeId == widgetModel.SystemTypeId && d.EnName.Equals(propertye, StringComparison.OrdinalIgnoreCase));
                                            if (metric != null)
                                            {
                                                bindingColumns.Add(new ColumnConfig
                                                {
                                                    ColumnName = metric.EnName.Substring(0, 1).ToUpperInvariant() + metric.EnName.Substring(1),
                                                    HeaderName = metric.CnName,
                                                    HeaderDescription = metric.CnDescription,
                                                    Width = metric.CnName.Length * 19,
                                                    FormatString = metric.Format,
                                                    HeaderColorString = metric.Color
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor)
                                {
                                    #region 设置“年”列

                                    if (timeDimensions.Contains("year"))
                                    {
                                        var year = _timeDimensionsCache.Find(t => t.EnName == "year" && t.SystemTypeId == widgetModel.SystemTypeId);
                                        if (year != null)
                                        {
                                            bindingColumns.Add(new ColumnConfig
                                            {
                                                ColumnName = "Year",
                                                HeaderName = year.CnName,
                                                Width = 40
                                            });
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {
                                    foreach (var timeDimension in timeDimensions)
                                    {
                                        var timeDimensionX = _timeDimensionsCache.Find(t => t.EnName == timeDimension && t.SystemTypeId == widgetModel.SystemTypeId);
                                        if (timeDimensionX != null)
                                        {
                                            var t = timeDimension.Substring(0, 1).ToUpperInvariant() + timeDimension.Substring(1, timeDimension.Length - 1);
                                            bindingColumns.Add(new ColumnConfig
                                            {
                                                ColumnName = t,
                                                HeaderName = timeDimensionX.CnName,
                                                HeaderDescription = timeDimensionX.CnDescription,
                                                Width = timeDimensionX.CnName.Length * 20 < 50 ? 50 : timeDimensionX.CnName.Length * 20,
                                                FormatString = timeDimensionX.Format
                                            });
                                        }
                                    }
                                }

                                #region 设置维度列

                                foreach (var dimensionx in dimensions)
                                {
                                    var dimension = _dimensionsCache.Find(d => d.EnName == dimensionx && d.SystemTypeId == widgetModel.SystemTypeId);
                                    if (dimension != null)
                                    {
                                        var d = dimensionx.Substring(0, 1).ToUpperInvariant() + dimensionx.Substring(1, dimensionx.Length - 1);
                                        bindingColumns.Add(new ColumnConfig
                                        {
                                            ColumnName = d,
                                            HeaderName = dimension.CnName,
                                            HeaderDescription = dimension.CnDescription,
                                            Width = dimension.CnName.Length * 20 < 50 ? 50 : dimension.CnName.Length * 20,
                                            FormatString = dimension.Format
                                        });
                                    }
                                }

                                #endregion

                                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor)
                                {
                                    #region 设置“周”列

                                    if (timeDimensions.Contains("weekly"))
                                    {
                                        var week = _timeDimensionsCache.Find(t => t.EnName == "weekly" && t.SystemTypeId == widgetModel.SystemTypeId);
                                        if (week != null)
                                        {
                                            bindingColumns.Add(new ColumnConfig
                                            {
                                                ColumnName = "Weekly",
                                                HeaderName = week.CnName,
                                                Width = 40
                                            });
                                        }
                                    }

                                    #endregion
                                }

                                #region 设置指标列

                                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackAnalysisReport)
                                {
                                    foreach (var metric in metrics)
                                    {
                                        var metricx = _metricsCache.Find(m => m.EnName == metric && m.SystemTypeId == widgetModel.SystemTypeId);
                                        if (widgetModel.EnHeader == "freqs")
                                        {
                                            if (metric == "imp" || metric == "uimp" || metric == "clk" || metric == "uclk")
                                            {
                                                var tempFreqsList = new List<string> { "1", "2", "3", "4", "5", "6a" };
                                                foreach (var i in tempFreqsList)
                                                {
                                                    var mm = char.ToUpper(metric[0]) + metric.Remove(0, 1) + i;
                                                    var column = new ColumnConfig
                                                    {
                                                        ColumnName = mm,
                                                        HeaderName = metricx.CnName + i,
                                                        HeaderDescription = metricx.CnDescription,
                                                        Width = metricx.CnName.Length * 20,
                                                        FormatString = metricx.Format,
                                                        HeaderColorString = metricx.Color
                                                    };

                                                    bindingColumns.Add(column);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var mm = metric.Substring(0, 1).ToUpperInvariant() + metric.Substring(1, metric.Length - 1);
                                            bindingColumns.Add(new ColumnConfig
                                            {
                                                ColumnName = mm,
                                                HeaderName = metricx.CnName,
                                                HeaderDescription = metricx.CnDescription,
                                                Width = metricx.CnName.Length * 20 < 50 ? 50 : metricx.CnName.Length * 20,
                                                FormatString = metricx.Format,
                                                HeaderColorString = metricx.Color
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var metric in metrics)
                                    {
                                        var metricx = _metricsCache.Find(m => m.EnName == metric && m.SystemTypeId == widgetModel.SystemTypeId);
                                        var mm = metric.Substring(0, 1).ToUpperInvariant() + metric.Substring(1, metric.Length - 1);
                                        bindingColumns.Add(new ColumnConfig
                                        {
                                            ColumnName = mm,
                                            HeaderName = metricx.CnName,
                                            HeaderDescription = metricx.CnDescription,
                                            Width = metricx.CnName.Length * 17 < 60 ? 60 : metricx.CnName.Length * 17,
                                            FormatString = metricx.Format,
                                            HeaderColorString = metricx.Color
                                        });
                                    }
                                }

                                #endregion
                            }

                            var gridItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (gridItem is GridItemInfo)
                            {
                                Execute.OnUIThreadAsync(() =>
                                {
                                    var gridItemx = gridItem as GridItemInfo;
                                    gridItemx.BindingFrozenColumns = dimensions.Count() + timeDimensions.Count();
                                    gridItemx.BindingColumnsConfig = bindingColumns;

                                    var items = new ObservableCollection<ISystemData>();
                                    if ((widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime) && gridItemx.GridItemSource != null)
                                    {
                                        items = gridItemx.GridItemSource;
                                    }

                                    foreach (var data in systemData)
                                    {
                                        var item = data as ISystemData;
                                        items.Add(item);
                                    }

                                    gridItemx.GridItemSource = items;
                                    gridItemx.IsLoading = false;
                                });
                            }

                            #endregion
                            break;
                        case "Chart":

                            #region

                            var axisDic = new Dictionary<string, string>();
                            var axisNameDic = new Dictionary<string, string>();
                            var datas = new ObservableCollection<string>();
                            var series = new ObservableCollection<MyReport>();
                            var valueRangeTuple = new List<Tuple<string, string, string>>();

                            if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                            {
                                #region

                                var type = SiteRealtimeType.GetSiteRealtimeDataType(widgetModel.EnHeader, widgetModel.DataType);
                                if (widgetModel.EnHeader == "transform")
                                {
                                    #region

                                    axisDic.Add("Hour", "X");
                                    axisDic.Add("TotalEvents", "Y");
                                    var metric = _metricsCache.FirstOrDefault(m => m.EnName == "totalEvents" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                    var report = new MyReport();
                                    report.Label = metric != null ? metric.CnName : "";

                                    var values = new List<decimal>();
                                    var data = new ObservableCollection<C1Data>();
                                    var propertyInfo = type.GetProperty("TotalEvents");
                                    foreach (var item in systemDataList)
                                    {
                                        var value = Convert.ToDecimal(propertyInfo.GetValue(item, null).ToString());
                                        data.Add(new C1Data { Value = value });
                                        values.Add(value);

                                        var propertyX = type.GetProperty("Hour");
                                        var valueX = propertyX.GetValue(item, null).ToString();
                                        datas.Add(valueX);
                                    }

                                    valueRangeTuple.Add(new Tuple<string, string, string>("Hour", "0", "23.5"));
                                    if (values.Count == 0)
                                    {
                                        valueRangeTuple.Add(new Tuple<string, string, string>("TotalEvents", "0", "15000"));
                                    }
                                    else
                                    {
                                        valueRangeTuple.Add(new Tuple<string, string, string>("TotalEvents", values.Min().ToString(), values.Max().ToString()));
                                    }

                                    axisNameDic.Add("Hour", "时间");
                                    axisNameDic.Add("TotalEvents", metric.CnName);

                                    report.Data = data;
                                    series.Add(report);

                                    #endregion
                                }
                                else
                                {
                                    switch (widgetModel.DataType)
                                    {
                                        case "summary":
                                        case "detail":
                                            #region

                                            axisDic.Add("Time", "X");
                                            axisDic.Add("PageViews", "Y");
                                            axisNameDic.Add("Time", "时间");
                                            valueRangeTuple.Add(new Tuple<string, string, string>("DateTime", "0", "24"));

                                            if (widgetModel.DataType == "summary")
                                            {
                                                datas.Add(DateTime.Now.ToLongTimeString());
                                            }
                                            else
                                            {
                                                var propertyInfoA1 = type.GetProperty("Time");
                                                foreach (var item in systemDataList)
                                                {
                                                    var value = propertyInfoA1.GetValue(item, null) == null ? "" : propertyInfoA1.GetValue(item, null).ToString();
                                                    datas.Add(value);
                                                }
                                            }

                                            var valuesA1 = new List<decimal>();

                                            var metricA1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportA1 = new MyReport();
                                            reportA1.Label = metricA1 != null ? metricA1.CnName : "";
                                            axisNameDic.Add("PageViews", metricA1.CnName);

                                            var dataA1 = new ObservableCollection<C1Data>();
                                            var propertyInfoA2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoA2.GetValue(item, null) == null ? "" : propertyInfoA2.GetValue(item, null).ToString());
                                                dataA1.Add(new C1Data { Value = value });
                                                valuesA1.Add(value);
                                            }

                                            if (valuesA1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesA1.Max().ToString()));
                                            }

                                            reportA1.Data = dataA1;
                                            series.Add(reportA1);

                                            var metricA2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportA2 = new MyReport();
                                            reportA2.Label = metricA2 != null ? metricA2.CnName : "";

                                            var dataA2 = new ObservableCollection<C1Data>();
                                            var propertyInfoA3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoA3.GetValue(item, null).ToString());
                                                dataA2.Add(new C1Data { Value = value });
                                                valuesA1.Add(value);
                                            }

                                            reportA2.Data = dataA2;
                                            series.Add(reportA2);

                                            #endregion
                                            break;
                                        case "city":
                                        case "province":
                                            #region

                                            var xString = widgetModel.DataType == "city" ? "City" : "Province";
                                            axisDic.Add("PageViews", "Y");
                                            var propertyInfoB1 = type.GetProperty(xString);
                                            foreach (var item in systemDataList)
                                            {
                                                var value = propertyInfoB1.GetValue(item, null) == null ? "" : propertyInfoB1.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            var valuesB1 = new List<decimal>();

                                            var metricB1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportB1 = new MyReport();
                                            reportB1.Label = metricB1 != null ? metricB1.CnName : "";
                                            axisNameDic.Add("PageViews", metricB1.CnName);

                                            var dataB1 = new ObservableCollection<C1Data>();
                                            var propertyInfoB2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoB2.GetValue(item, null).ToString());
                                                dataB1.Add(new C1Data { Value = value });
                                                valuesB1.Add(value);
                                            }

                                            if (valuesB1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesB1.Max().ToString()));
                                            }

                                            reportB1.Data = dataB1;
                                            series.Add(reportB1);

                                            var metricB2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportB2 = new MyReport();
                                            reportB2.Label = metricB2 != null ? metricB2.CnName : "";
                                            axisNameDic.Add("Visitors", metricB2.CnName);

                                            var dataB2 = new ObservableCollection<C1Data>();
                                            var propertyInfoB3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoB3.GetValue(item, null).ToString());
                                                dataB2.Add(new C1Data { Value = value });
                                                valuesB1.Add(value);
                                            }

                                            reportB2.Data = dataB2;
                                            series.Add(reportB2);

                                            #endregion
                                            break;
                                        case "source":
                                            #region

                                            axisDic.Add("PageViews", "Y");
                                            var propertyInfoC1 = type.GetProperty("SourceType");
                                            foreach (var item in systemDataList)
                                            {
                                                var enName = propertyInfoC1.GetValue(item, null).ToString();
                                                var cnName = string.Empty;
                                                if (enName == "ads")
                                                {
                                                    cnName = "推广";
                                                }
                                                else if (enName == "direct")
                                                {
                                                    cnName = "直接";
                                                }
                                                else if (enName == "referral")
                                                {
                                                    cnName = "引荐";
                                                }
                                                else if (enName == "organic")
                                                {
                                                    cnName = "搜索";
                                                }
                                                else
                                                {
                                                    cnName = "社交";
                                                }

                                                datas.Add(cnName);
                                            }

                                            var valuesC1 = new List<decimal>();

                                            var metricC1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportC1 = new MyReport();
                                            reportC1.Label = metricC1 != null ? metricC1.CnName : "";
                                            axisNameDic.Add("PageViews", metricC1.CnName);

                                            var dataC1 = new ObservableCollection<C1Data>();
                                            var propertyInfoC2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoC2.GetValue(item, null) == null ? "" : propertyInfoC2.GetValue(item, null).ToString());
                                                dataC1.Add(new C1Data { Value = value });
                                                valuesC1.Add(value);
                                            }

                                            if (valuesC1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesC1.Max().ToString()));
                                            }

                                            reportC1.Data = dataC1;
                                            series.Add(reportC1);

                                            var metricC2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportC2 = new MyReport();
                                            reportC2.Label = metricC2 != null ? metricC2.CnName : "";
                                            axisNameDic.Add("Visitors", metricC2.CnName);

                                            var dataC2 = new ObservableCollection<C1Data>();
                                            var propertyInfoC3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoC3.GetValue(item, null).ToString());
                                                dataC2.Add(new C1Data { Value = value });
                                                valuesC1.Add(value);
                                            }

                                            reportC2.Data = dataC2;
                                            series.Add(reportC2);

                                            #endregion
                                            break;
                                        case "organic":
                                            #region

                                            axisDic.Add("PageViews", "Y");
                                            var propertyInfoD1 = type.GetProperty("Domain");
                                            foreach (var item in systemDataList)
                                            {
                                                var name = propertyInfoD1.GetValue(item, null) == null ? "" : propertyInfoD1.GetValue(item, null).ToString();
                                                datas.Add(name);
                                            }

                                            var valuesD1 = new List<decimal>();

                                            var metricD1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportD1 = new MyReport();
                                            reportD1.Label = metricD1 != null ? metricD1.CnName : "";
                                            axisNameDic.Add("PageViews", metricD1.CnName);

                                            var dataD1 = new ObservableCollection<C1Data>();
                                            var propertyInfoD2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoD2.GetValue(item, null).ToString());
                                                dataD1.Add(new C1Data { Value = value });
                                                valuesD1.Add(value);
                                            }

                                            if (valuesD1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesD1.Max().ToString()));
                                            }

                                            reportD1.Data = dataD1;
                                            series.Add(reportD1);

                                            var metricD2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportD2 = new MyReport();
                                            reportD2.Label = metricD2 != null ? metricD2.CnName : "";
                                            axisNameDic.Add("Visitors", metricD2.CnName);

                                            var dataD2 = new ObservableCollection<C1Data>();
                                            var propertyInfoD3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoD3.GetValue(item, null).ToString());
                                                dataD2.Add(new C1Data { Value = value });
                                                valuesD1.Add(value);
                                            }

                                            reportD2.Data = dataD2;
                                            series.Add(reportD2);

                                            #endregion
                                            break;
                                        case "keyword":
                                            #region

                                            axisDic.Add("PageViews", "Y");
                                            var propertyInfoE1 = type.GetProperty("Keyword");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = propertyInfoE1.GetValue(item, null) == null ? "" : propertyInfoE1.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            var valuesE1 = new List<decimal>();

                                            var metricE1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportE1 = new MyReport();
                                            reportE1.Label = metricE1 != null ? metricE1.CnName : "";
                                            axisNameDic.Add("PageViews", metricE1.CnName);

                                            var dataE1 = new ObservableCollection<C1Data>();
                                            var propertyInfoE2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoE2.GetValue(item, null).ToString());
                                                dataE1.Add(new C1Data { Value = value });
                                                valuesE1.Add(value);
                                            }

                                            if (valuesE1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesE1.Max().ToString()));
                                            }

                                            reportE1.Data = dataE1;
                                            series.Add(reportE1);

                                            var metricE2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportE2 = new MyReport();
                                            reportE2.Label = metricE2 != null ? metricE2.CnName : "";
                                            axisNameDic.Add("Visitors", metricE2.CnName);

                                            var dataE2 = new ObservableCollection<C1Data>();
                                            var propertyInfoE3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoE3.GetValue(item, null).ToString());
                                                dataE2.Add(new C1Data { Value = value });
                                                valuesE1.Add(value);
                                            }

                                            reportE2.Data = dataE2;
                                            series.Add(reportE2);

                                            #endregion
                                            break;
                                        case "ads":
                                            #region

                                            axisDic.Add("PageViews", "Y");
                                            var propertyInfoF1 = type.GetProperty("Placement");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = propertyInfoF1.GetValue(item, null) == null ? "" : propertyInfoF1.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            var valuesF1 = new List<decimal>();

                                            var metricF1 = _metricsCache.FirstOrDefault(m => m.EnName == "pageViews" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportF1 = new MyReport();
                                            reportF1.Label = metricF1 != null ? metricF1.CnName : "";
                                            axisNameDic.Add("PageViews", metricF1.CnName);

                                            var dataF1 = new ObservableCollection<C1Data>();
                                            var propertyInfoF2 = type.GetProperty("PageViews");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoF2.GetValue(item, null).ToString());
                                                dataF1.Add(new C1Data { Value = value });
                                                valuesF1.Add(value);
                                            }

                                            if (valuesF1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("PageViews", "0", valuesF1.Max().ToString()));
                                            }

                                            reportF1.Data = dataF1;
                                            series.Add(reportF1);

                                            var metricF2 = _metricsCache.FirstOrDefault(m => m.EnName == "visitors" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportF2 = new MyReport();
                                            reportF2.Label = metricF2 != null ? metricF2.CnName : "";
                                            axisNameDic.Add("Visitors", metricF2.CnName);

                                            var dataF2 = new ObservableCollection<C1Data>();
                                            var propertyInfoF3 = type.GetProperty("Visitors");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoF3.GetValue(item, null).ToString()); dataF2.Add(new C1Data { Value = value });
                                                valuesF1.Add(value);
                                            }

                                            reportF2.Data = dataF2;
                                            series.Add(reportF2);

                                            #endregion
                                            break;
                                        case "event":
                                            #region

                                            axisDic.Add("TotalEvents", "Y");
                                            var propertyInfoG1 = type.GetProperty("Action");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = propertyInfoG1.GetValue(item, null) == null ? "" : propertyInfoG1.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            var valuesG1 = new List<decimal>();

                                            var metricG1 = _metricsCache.FirstOrDefault(m => m.EnName == "totalEvents" && m.SystemTypeId == (int)SystemTypeEnum.SiteRealtime);
                                            var reportG1 = new MyReport();
                                            reportG1.Label = metricG1 != null ? metricG1.CnName : "";
                                            axisNameDic.Add("TotalEvents", metricG1.CnName);

                                            var dataG1 = new ObservableCollection<C1Data>();
                                            var propertyInfoG2 = type.GetProperty("TotalEvents");
                                            foreach (var item in systemDataList)
                                            {
                                                var value = Convert.ToDecimal(propertyInfoG2.GetValue(item, null).ToString());
                                                dataG1.Add(new C1Data { Value = value });
                                                valuesG1.Add(value);
                                            }

                                            if (valuesG1.Count == 0)
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("TotalEvents", "0", "100"));
                                            }
                                            else
                                            {
                                                valueRangeTuple.Add(new Tuple<string, string, string>("TotalEvents", "0", valuesG1.Max().ToString()));
                                            }

                                            reportG1.Data = dataG1;
                                            series.Add(reportG1);

                                            #endregion
                                            break;
                                    }
                                }

                                #endregion
                            }
                            else
                            {
                                Type systemType = null;
                                switch (widgetModel.SystemTypeId)
                                {
                                    case (int)SystemTypeEnum.SiteAnalysisReport:
                                        //systemType = typeof();
                                        break;
                                    case (int)SystemTypeEnum.SiteDataBank:
                                        //systemType = typeof();
                                        break;
                                    case (int)SystemTypeEnum.Sponsor:
                                        systemType = typeof(SponsorData);
                                        break;
                                    case (int)SystemTypeEnum.TrackAnalysisReport:
                                        systemType = typeof(TrackAnalysisData);
                                        break;
                                    case (int)SystemTypeEnum.TrackDataBank:
                                        //systemType = typeof();
                                        break;
                                    case (int)SystemTypeEnum.TrackRealtime:
                                        systemType = typeof(TrackRealtimeData);
                                        break;
                                }

                                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                                {
                                    var returnDatas = new ObservableCollection<TrackRealtimeData>();
                                    var systemDatas = systemDataList as List<TrackRealtimeData>;

                                    var dateX = DateTime.Now;
                                    for (int i = 0; i < widgetModel.DataCount; i++)
                                    {
                                        var startDateM = dateX.AddMinutes(i);
                                        var startDateS = dateX.AddSeconds(i);

                                        var systemDataXs = systemDatas.FirstOrDefault(s => s.FiveMinutes == startDateM || s.Second == startDateS.ToString());
                                        if (systemDatas.Count != 0 && systemDatas.Any(s => s.FiveMinutes == startDateM || s.Second == startDateS.ToString()))
                                        {
                                            returnDatas.Add(systemDataXs);
                                        }
                                        else
                                        {
                                            var returnData = new TrackRealtimeData
                                            {
                                                Campaigns = widgetModel.TableName,
                                                CampaignsName = "",
                                                Clk = 0,
                                                Creative = 0,
                                                CreativeName = "",
                                                CtRate = 0,
                                                DateTime = _currentDateTime,
                                                FiveMinutes = startDateM,
                                                Imp = 0,
                                                Media = 0,
                                                MediaName = "",
                                                Placement = 0,
                                                PlacementName = "",
                                                Second = startDateS.ToString("yyyy-MM-dd HH:mm:ss"),
                                                Uclk = 0,
                                                Uimp = 0,
                                            };

                                            returnDatas.Add(returnData);
                                        }
                                    }

                                    PropertyInfo tPropertyInfo = systemType.GetProperty(widgetModel.TimeDimensions.Substring(0, 1).ToUpperInvariant() + widgetModel.TimeDimensions.Substring(1, widgetModel.TimeDimensions.Length - 1));

                                    if (dimensions.Any(d => d == "campaignsName"))
                                    {
                                        #region 项目

                                        axisDic.Add(metrics.FirstOrDefault(), "Y");

                                        foreach (var chartMetric in metrics)
                                        {
                                            var metricX = _metricsCache.Find(t => t.SystemTypeId == widgetModel.SystemTypeId && t.EnName == chartMetric);
                                            axisNameDic.Add(metricX.EnName, metricX.CnName);

                                            var report = new MyReport();
                                            report.Label = metricX.CnName;

                                            var values = new List<decimal>();
                                            var data = new ObservableCollection<C1Data>();
                                            PropertyInfo propertyInfo = systemType.GetProperty(chartMetric.Substring(0, 1).ToUpperInvariant() + chartMetric.Substring(1, chartMetric.Length - 1));

                                            foreach (var item in returnDatas)
                                            {
                                                decimal value;
                                                if (propertyInfo.GetValue(item, null) == null)
                                                {
                                                    value = 0;
                                                }
                                                else
                                                {
                                                    if (propertyInfo.GetValue(item, null) is int)
                                                    {
                                                        value = Convert.ToInt32(propertyInfo.GetValue(item, null));
                                                    }
                                                    else if (propertyInfo.GetValue(item, null) is double || propertyInfo.GetValue(item, null) is decimal)
                                                    {
                                                        value = Convert.ToDecimal(propertyInfo.GetValue(item, null));
                                                    }
                                                    else
                                                    {
                                                        value = 0;
                                                    }
                                                }

                                                data.Add(new C1Data { Value = value });
                                                values.Add(value);
                                            }

                                            if (valueRangeTuple.Count == 0)
                                            {
                                                if (values.Count == 0)
                                                {
                                                    valueRangeTuple.Add(new Tuple<string, string, string>(chartMetric, "", ""));
                                                }
                                                else
                                                {
                                                    valueRangeTuple.Add(new Tuple<string, string, string>(chartMetric, values.Min().ToString(CultureInfo.InvariantCulture), values.Max().ToString(CultureInfo.InvariantCulture)));
                                                }
                                            }
                                            else
                                            {
                                                if (values.Count > 0 && values.Min() < Convert.ToDecimal(valueRangeTuple[0].Item2))
                                                {
                                                    var tuple = valueRangeTuple[0];
                                                    valueRangeTuple.Clear();
                                                    valueRangeTuple.Add(new Tuple<string, string, string>(tuple.Item1, values.Min().ToString(), tuple.Item3));
                                                }

                                                if (values.Count > 0 && values.Max() > Convert.ToDecimal(valueRangeTuple[0].Item3))
                                                {
                                                    var tuple = valueRangeTuple[0];
                                                    valueRangeTuple.Clear();
                                                    valueRangeTuple.Add(new Tuple<string, string, string>(tuple.Item1, tuple.Item2, values.Max().ToString()));
                                                }
                                            }

                                            report.Data = data;
                                            series.Add(report);
                                        }

                                        #endregion
                                    }
                                    else
                                    {
                                        axisDic.Add(metrics.FirstOrDefault(), "Y");

                                        #region

                                        foreach (var chartMetric in metrics)
                                        {
                                            var metricX = _metricsCache.Find(t => t.SystemTypeId == widgetModel.SystemTypeId && t.EnName == chartMetric);
                                            axisNameDic.Add(metricX.EnName, metricX.CnName);

                                            PropertyInfo propertyInfo = systemType.GetProperty(chartMetric.Substring(0, 1).ToUpperInvariant() + chartMetric.Substring(1, chartMetric.Length - 1));
                                            //PropertyInfo propertyLabel = systemType.GetProperty(dimensions[0].Substring(0, 1).ToUpperInvariant() + dimensions[0].Substring(1, dimensions[0].Length - 1));

                                            var report = new MyReport();
                                            report.Label = metricX.CnName;

                                            var values = new List<decimal>();
                                            var data = new ObservableCollection<C1Data>();

                                            int index = 1;
                                            foreach (var item in returnDatas)
                                            {
                                                if (index > widgetModel.DataCount)
                                                {
                                                    continue;
                                                }

                                                var dataX = tPropertyInfo.GetValue(item, null);
                                                datas.Add(dataX.ToString());

                                                decimal value;
                                                if (propertyInfo.GetValue(item, null) == null)
                                                {
                                                    value = 0;
                                                }
                                                else
                                                {
                                                    if (propertyInfo.GetValue(item, null) is int)
                                                    {
                                                        value = Convert.ToInt32(propertyInfo.GetValue(item, null));
                                                    }
                                                    else if (propertyInfo.GetValue(item, null) is double || propertyInfo.GetValue(item, null) is decimal)
                                                    {
                                                        value = Convert.ToDecimal(propertyInfo.GetValue(item, null));
                                                    }
                                                    else
                                                    {
                                                        value = 0;
                                                    }
                                                }

                                                data.Add(new C1Data { Value = value });
                                                values.Add(value);

                                                if (valueRangeTuple.Count == 0)
                                                {
                                                    if (values.Count == 0)
                                                    {
                                                        valueRangeTuple.Add(new Tuple<string, string, string>(chartMetric, "", ""));
                                                    }
                                                    else
                                                    {
                                                        valueRangeTuple.Add(new Tuple<string, string, string>(chartMetric, values.Min().ToString(CultureInfo.InvariantCulture), values.Max().ToString(CultureInfo.InvariantCulture)));
                                                    }
                                                }
                                                else
                                                {
                                                    if (values.Count > 0 && values.Min() < Convert.ToDecimal(valueRangeTuple[0].Item2))
                                                    {
                                                        var tuple = valueRangeTuple[0];
                                                        valueRangeTuple.Clear();
                                                        valueRangeTuple.Add(new Tuple<string, string, string>(tuple.Item1, values.Min().ToString(), tuple.Item3));
                                                    }

                                                    if (values.Count > 0 && values.Max() > Convert.ToDecimal(valueRangeTuple[0].Item3))
                                                    {
                                                        var tuple = valueRangeTuple[0];
                                                        valueRangeTuple.Clear();
                                                        valueRangeTuple.Add(new Tuple<string, string, string>(tuple.Item1, tuple.Item2, values.Max().ToString()));
                                                    }
                                                }

                                                index++;
                                            }

                                            report.Data = data;
                                            series.Add(report);
                                        }

                                        #endregion
                                    }
                                }
                                else
                                {
                                    #region

                                    if (metrics.Count() == 1)
                                    {
                                        if (timeDimensions.Count() == 0)
                                        {
                                            PropertyInfo dPropertyInfoA1 = systemType.GetProperty(dimensions[0].Substring(0, 1).ToUpperInvariant() + dimensions[0].Substring(1, dimensions[0].Length - 1).TrimEnd());

                                            foreach (var itemA1 in systemDataList)
                                            {
                                                string value = dPropertyInfoA1.GetValue(itemA1, null) == null ? "" : dPropertyInfoA1.GetValue(itemA1, null).ToString();
                                                datas.Add(value);
                                            }

                                            axisDic.Add(dimensions[0], "X");
                                            var dimensionA1 = _dimensionsCache.FirstOrDefault(d => d.SystemTypeId == widgetModel.SystemTypeId && d.EnName == dimensions[0] && (d.EnHeader == widgetModel.EnHeader || (widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor && d.EnHeader == "ALL")));
                                            axisNameDic.Add(dimensionA1.EnName, dimensionA1.CnName);

                                            axisDic.Add(metrics[0], "Y");
                                            var metricA1 = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metrics[0] && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                            axisNameDic.Add(metricA1.EnName, metricA1.CnName);

                                            if (dimensions.Count() == 1)
                                            {
                                                var report = new MyReport();
                                                report.Label = metricA1.CnName;
                                                PropertyInfo mPropertyInfoA1 = systemType.GetProperty(metrics[0].Substring(0, 1).ToUpperInvariant() + metrics[0].Substring(1, metrics[0].Length - 1).TrimEnd());
                                                var data = new ObservableCollection<C1Data>();
                                                foreach (var itemA1 in systemDataList)
                                                {
                                                    string value = mPropertyInfoA1.GetValue(itemA1, null) == null ? "" : mPropertyInfoA1.GetValue(itemA1, null).ToString();
                                                    data.Add(new C1Data { Value = value });
                                                }

                                                report.Data = data;
                                                series.Add(report);
                                            }
                                            else
                                            {
                                                PropertyInfo dPropertyInfoB2 = systemType.GetProperty(dimensions[1].Substring(0, 1).ToUpperInvariant() + dimensions[1].Substring(1, dimensions[1].Length - 1).TrimEnd());
                                                PropertyInfo mPropertyInfoB2 = systemType.GetProperty(metrics[0].Substring(0, 1).ToUpperInvariant() + metrics[0].Substring(1, metrics[0].Length - 1).TrimEnd());

                                                var dic = new Dictionary<string, List<object>>();
                                                foreach (var itemA1 in systemDataList)
                                                {
                                                    var value = dPropertyInfoA1.GetValue(itemA1, null) == null ? "" : dPropertyInfoA1.GetValue(itemA1, null).ToString();
                                                    var key = dPropertyInfoB2.GetValue(itemA1, null) == null ? "" : dPropertyInfoB2.GetValue(itemA1, null).ToString();
                                                    var valueX = mPropertyInfoB2.GetValue(itemA1, null) == null ? "" : mPropertyInfoB2.GetValue(itemA1, null).ToString();
                                                    if (!datas.Contains(value))
                                                    {
                                                        datas.Add(value);
                                                    }

                                                    if (dic.Keys.Any(k => k == key))
                                                    {
                                                        var valueXX = dic[key];
                                                        valueXX.Add(valueX);
                                                    }
                                                    else
                                                    {
                                                        dic.Add(key, new List<object> { valueX });
                                                    }
                                                }

                                                foreach (var dicX in dic)
                                                {
                                                    var report = new MyReport();
                                                    report.Label = dicX.Key;

                                                    var data = new ObservableCollection<C1Data>();
                                                    foreach (var itemA2 in dicX.Value)
                                                    {
                                                        data.Add(new C1Data { Value = itemA2 });
                                                    }

                                                    report.Data = data;
                                                    series.Add(report);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            axisDic.Add(timeDimensions[0], "X");
                                            var timenDimensionX = _timeDimensionsCache.FirstOrDefault(t => t.SystemTypeId == widgetModel.SystemTypeId && t.EnName == timeDimensions[0]);
                                            axisNameDic.Add(timenDimensionX.EnName, timenDimensionX.CnName);

                                            axisDic.Add(metrics[0], "Y");
                                            var metricX = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metrics[0] && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                            axisNameDic.Add(metricX.EnName, metricX.CnName);

                                            PropertyInfo tPropertyInfo = systemType.GetProperty(timeDimensions[0].Substring(0, 1).ToUpperInvariant() + timeDimensions[0].Substring(1, timeDimensions[0].Length - 1).TrimEnd());
                                            PropertyInfo dPropertyInfo = systemType.GetProperty(dimensions[0].Substring(0, 1).ToUpperInvariant() + dimensions[0].Substring(1, dimensions[0].Length - 1).TrimEnd());
                                            PropertyInfo mPropertyInfo = systemType.GetProperty(metrics[0].Substring(0, 1).ToUpperInvariant() + metrics[0].Substring(1, metrics[0].Length - 1).TrimEnd());
                                            var dic = new Dictionary<string, List<object>>();
                                            foreach (var itemA1 in systemDataList)
                                            {
                                                var value = tPropertyInfo.GetValue(itemA1, null) == null ? "" : tPropertyInfo.GetValue(itemA1, null).ToString();
                                                var key = dPropertyInfo.GetValue(itemA1, null) == null ? "" : dPropertyInfo.GetValue(itemA1, null).ToString();
                                                var valueX = mPropertyInfo.GetValue(itemA1, null) == null ? "" : mPropertyInfo.GetValue(itemA1, null).ToString();
                                                if (!datas.Contains(value))
                                                    if (!datas.Contains(value))
                                                    {
                                                        datas.Add(value);
                                                    }

                                                if (dic.Keys.Any(k => k == key))
                                                {
                                                    var valueXX = dic[key];
                                                    valueXX.Add(valueX);
                                                }
                                                else
                                                {
                                                    dic.Add(key, new List<object> { valueX });
                                                }
                                            }

                                            foreach (var dicX in dic)
                                            {
                                                var report = new MyReport();
                                                report.Label = dicX.Key;

                                                var data = new ObservableCollection<C1Data>();
                                                foreach (var itemA2 in dicX.Value)
                                                {
                                                    data.Add(new C1Data { Value = itemA2 });
                                                }

                                                report.Data = data;
                                                series.Add(report);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (timeDimensions.Count() == 0)
                                        {
                                            PropertyInfo dPropertyInfo = systemType.GetProperty(dimensions[0].Substring(0, 1).ToUpperInvariant() + dimensions[0].Substring(1, dimensions[0].Length - 1).TrimEnd());
                                            foreach (var item in systemDataList)
                                            {
                                                string value = dPropertyInfo.GetValue(item, null) == null ? "" : dPropertyInfo.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            axisDic.Add(dimensions[0], "X");
                                            var dimensionX = _dimensionsCache.FirstOrDefault(d => d.SystemTypeId == widgetModel.SystemTypeId && d.EnName == dimensions[0] && (d.EnHeader == widgetModel.EnHeader || (widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor && d.EnHeader == "ALL")));
                                            axisNameDic.Add(dimensionX.EnName, dimensionX.CnName);

                                            axisDic.Add(metrics[0], "Y");
                                            var metricX = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metrics[0] && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                            axisNameDic.Add(metricX.EnName, metricX.CnName);

                                            foreach (var metric0 in metrics)
                                            {
                                                var name = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metric0 && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                                var report = new MyReport();
                                                report.Label = name.CnName;

                                                var data = new ObservableCollection<C1Data>();
                                                PropertyInfo mPropertyInfo = systemType.GetProperty(metric0.Substring(0, 1).ToUpperInvariant() + metric0.Substring(1, metric0.Length - 1).TrimEnd());
                                                foreach (var item in systemDataList)
                                                {
                                                    string value = mPropertyInfo.GetValue(item, null) == null ? "" : mPropertyInfo.GetValue(item, null).ToString();
                                                    data.Add(new C1Data { Value = value });
                                                }

                                                report.Data = data;
                                                series.Add(report);
                                            }
                                        }
                                        else
                                        {
                                            PropertyInfo tPropertyInfo = systemType.GetProperty(timeDimensions[0].Substring(0, 1).ToUpperInvariant() + timeDimensions[0].Substring(1, timeDimensions[0].Length - 1).TrimEnd());
                                            foreach (var item in systemDataList)
                                            {
                                                string value = tPropertyInfo.GetValue(item, null) == null ? "" : tPropertyInfo.GetValue(item, null).ToString();
                                                datas.Add(value);
                                            }

                                            axisDic.Add(timeDimensions[0], "X");
                                            var timeDimensionX = _timeDimensionsCache.FirstOrDefault(t => t.SystemTypeId == widgetModel.SystemTypeId && t.EnName == timeDimensions[0]);
                                            axisNameDic.Add(timeDimensionX.EnName, timeDimensionX.CnName);

                                            axisDic.Add(metrics[0], "Y");
                                            var metricX = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metrics[0] && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                            axisNameDic.Add(metricX.EnName, metricX.CnName);
                                            int iMetric = 0;
                                            foreach (var metricXX in metrics)
                                            {
                                                var name = _metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metrics[iMetric] && (m.EnHeader == widgetModel.EnHeader || widgetModel.SystemTypeId == (int)SystemTypeEnum.Sponsor));
                                                var report = new MyReport();
                                                report.Label = name.CnName;

                                                var data = new ObservableCollection<C1Data>();
                                                PropertyInfo mPropertyInfo = systemType.GetProperty(metricXX.Substring(0, 1).ToUpperInvariant() + metricXX.Substring(1, metricXX.Length - 1).TrimEnd());
                                                foreach (var item in systemDataList)
                                                {
                                                    string value = mPropertyInfo.GetValue(item, null) == null ? "" : mPropertyInfo.GetValue(item, null).ToString();
                                                    data.Add(new C1Data { Value = value });
                                                }

                                                report.Data = data;
                                                series.Add(report);
                                                iMetric++;
                                            }
                                        }
                                    }

                                    #endregion
                                }
                            }

                            var chartItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (chartItem is ChartItemInfo)
                            {
                                Execute.OnUIThreadAsync(() =>
                                {
                                    #region 设置X,Y轴

                                    var axisList = new ObservableCollection<Axis>();

                                    foreach (var axis in axisDic)
                                    {
                                        var axisType = axis.Value == "X" ? AxisType.X : AxisType.Y;
                                        var position = AxisPosition.Inner;
                                        if ((axisType == AxisType.X && axisList.Any(a => a.AxisType == AxisType.X)) ||
                                            (axisType == AxisType.Y && axisList.Any(a => a.AxisType == AxisType.Y)))
                                        {
                                            position = AxisPosition.Far;
                                        }

                                        double thickness = 0;
                                        if (widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                                        {
                                            axisList.Add(new Axis
                                            {
                                                AxisType = axisType,
                                                Position = position,
                                                AutoMin = true,
                                                AutoMax = true,
                                                MajorGridStrokeThickness = thickness
                                            });
                                        }
                                        else if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime)
                                        {
                                            var value = valueRangeTuple.Find(v => v.Item1 == axis.Key);
                                            if (axisType != AxisType.X)
                                            {
                                                if (value.Item2 == "" && value.Item3 == "")
                                                {
                                                    axisList.Add(new Axis
                                                    {
                                                        Name = axisNameDic[axis.Key],
                                                        AxisType = axisType,
                                                        Position = position,
                                                        AutoMin = true,
                                                        AutoMax = true,
                                                        MajorGridStrokeThickness = thickness
                                                    });
                                                }
                                                else
                                                {
                                                    var min = Math.Floor(Convert.ToDouble(value.Item2));
                                                    var max = Math.Ceiling(Convert.ToDouble(value.Item3));
                                                    axisList.Add(new Axis
                                                    {
                                                        Name = axisNameDic[axis.Key],
                                                        AxisType = axisType,
                                                        Position = position,
                                                        Min = min,
                                                        Max = max,
                                                        MajorGridStrokeThickness = thickness
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                axisList.Add(new Axis
                                                {
                                                    Name = axisNameDic[axis.Key],
                                                    AxisType = axisType,
                                                    Position = position,
                                                    AutoMin = true,
                                                    AutoMax = true,
                                                    MajorGridStrokeThickness = thickness
                                                });
                                            }
                                        }
                                        else
                                        {
                                            axisList.Add(new Axis
                                            {
                                                Name = axisNameDic[axis.Key],
                                                AxisType = axisType,
                                                Position = position,
                                                AutoMin = true,
                                                AutoMax = true,
                                                MajorGridStrokeThickness = thickness
                                            });
                                        }
                                    }

                                    var axisX = axisList.FirstOrDefault(a => a.AxisType == AxisType.X);
                                    if (axisX != null)
                                    {
                                        axisList.Remove(axisX);
                                    }

                                    var axisY = axisList.FirstOrDefault(a => a.AxisType == AxisType.Y);
                                    if (axisY != null)
                                    {
                                        axisList.Remove(axisY);
                                    }

                                    #endregion

                                    var chartItemx = chartItem as ChartItemInfo;
                                    chartItemx.Axis = axisList;
                                    chartItemx.Datas = datas;
                                    chartItemx.Series = series;
                                    chartItemx.IsLoading = false;
                                });
                            }

                            #endregion

                            break;
                        case "Gauge":

                            #region

                            var gaugeItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (gaugeItem is GaugeItemInfo)
                            {
                                Execute.OnUIThreadAsync(() =>
                                {
                                    var gaugeItemx = gaugeItem as GaugeItemInfo;
                                    gaugeItemx.GaugeType = widgetModel.DisplayTypeIndex;

                                    #region 设置显示切换

                                    switch (widgetModel.DisplayTypeIndex)
                                    {
                                        case 0:
                                            gaugeItemx.KnobVisibility = true;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 1:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = true;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 2:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RadialVisibility = true;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 3:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = true;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 4:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.RulerVisibility = true;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 5:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = true;
                                            gaugeItemx.VolumeVisibility = false;
                                            break;
                                        case 6:
                                            gaugeItemx.KnobVisibility = false;
                                            gaugeItemx.LinearVisibility = false;
                                            gaugeItemx.RadialVisibility = false;
                                            gaugeItemx.RegionKnobVisibility = false;
                                            gaugeItemx.RulerVisibility = false;
                                            gaugeItemx.SpeedometerVisibility = false;
                                            gaugeItemx.VolumeVisibility = true;
                                            break;
                                    }

                                    #endregion

                                    //gaugeItemx.MaximumValue = item.MaximumValue;
                                    //gaugeItemx.MinimumValue = item.MinimumValue;
                                    //gaugeItemx.GaugeValue = item.GaugeType;
                                    gaugeItemx.IsLoading = false;
                                });
                            }

                            #endregion

                            break;
                        case "Map":

                            #region

                            var mapItem = WidgetList.FirstOrDefault(w => w.Id == widgetModel.Id);
                            if (mapItem is MapItemInfo)
                            {
                                Execute.OnUIThreadAsync(() =>
                                {
                                    var mapItemx = mapItem as MapItemInfo;
                                    mapItemx.IsLoading = false;
                                });
                            }

                            #endregion

                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //ShowMessage.Show("根据显示类型转换数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ConvertDataByDisplayType", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ConvertDataByDisplayType", null);
                }
            }
        }

        /// <summary>
        /// 针对TA数据进行显示优化，标记指定行
        /// </summary>
        public void OnItemsSourceChanged(GridItemInfo gridItemInfo, C1FlexGrid c1FlexGrid)
        {
            //            var gridItemSource = gridItemInfo.GridItemSource;
            //            c1FlexGrid.Rows[10].Background = new SolidColorBrush(Colors.BurlyWood);
            //            var gridItemSource = gridItemInfo.GridItemSource;
            //            c1FlexGrid.Rows[10].Background = new SolidColorBrush(Colors.BurlyWood);
        }

        #endregion

        #region Widget

        /// <summary>
        /// 添加Widget点击事件
        /// </summary>
        public void AddWidget()
        {
            try
            {
                if (DataCabinList.Count == 0)
                {
                    return;
                }

                var widgetViewModel = IoC.Get<WidgetViewModel>();
                widgetViewModel.CurrentDate = _currentDateTime;
                widgetViewModel.ClientId = ClientSelectedItem.Id;
                widgetViewModel.TimeDimensionsCache = _timeDimensionsCache;
                widgetViewModel.DimensionsCache = _dimensionsCache;
                widgetViewModel.DataTypeCache = _dataTypesCache;
                widgetViewModel.MetricsCache = _metricsCache;
                widgetViewModel.StartDate = StartDate;
                widgetViewModel.EndDate = EndDate;

                var groupIndex = WidgetList.Count == 0 ? 1 : WidgetList.LastOrDefault().GroupIndex + 1;
                widgetViewModel.WidgetModelEntity = new WidgetModel
                {
                    DataCabinId = SelectedDataCabin.Id,
                    GroupIndex = groupIndex,
                    DateTypeId = 1
                };

                IoC.Get<IWindowManager>().ShowDialog(widgetViewModel);
            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddWidget", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddWidget", null);
                }
            }
        }

        public void AddBenchMark()
        {
            var benchmarkViewModel = IoC.Get<BenchmarkViewModel>();
            benchmarkViewModel.ClientId = ClientSelectedItem.Id;
            benchmarkViewModel.DataCabinId = SelectedDataCabin.Id;
            IoC.Get<IWindowManager>().ShowDialog(benchmarkViewModel);
        }

        /// <summary>
        /// 添加Grid
        /// </summary>
        /// <param name="widgetModel"></param>
        private void AddGridItem(WidgetModel widgetModel)
        {
            try
            {
                GridItemInfo gridItem = new GridItemInfo();
                gridItem.Id = widgetModel.Id;
                gridItem.GroupIndex = widgetModel.GroupIndex;
                gridItem.ItemTitle = widgetModel.Title;
                if (widgetModel.Width == 0 && widgetModel.Height == 0)
                {
                    gridItem.ItemHeight = WidgetAreaHeight / 2 - 18;
                    gridItem.ItemWidth = WidgetAreaWidth - 40;
                }
                else
                {
                    gridItem.ItemWidth = Math.Round(widgetModel.Width * WidgetAreaWidth, 0);
                    gridItem.ItemHeight = Math.Round(widgetModel.Height * gridItem.ItemWidth, 0);
                }

                if (User.UserInfo.AuthorizationId == 3 && SelectedDataCabin.DataCabinTypeId == 1)
                {
                    gridItem.CanSetting = false;
                    gridItem.CanCopy = false;
                    gridItem.CanExportExcel = true;
                    gridItem.CanExportPicture = true;
                    gridItem.CanLookOriginalData = false;
                    gridItem.CanDelete = false;
                }
                else
                {
                    gridItem.CanSetting = true;
                    gridItem.CanCopy = true;
                    gridItem.CanExportExcel = true;
                    gridItem.CanExportPicture = true;
                    gridItem.CanLookOriginalData = true;
                    gridItem.CanDelete = true;
                }

                gridItem.IsLoading = true;
                WidgetList.Add(gridItem);

                if (_systemList.Contains(widgetModel.SystemTypeId))
                {
                    gridItem.IsNoAuthorization = Visibility.Collapsed;
                }
                else
                {
                    gridItem.IsLoading = false;
                    gridItem.CanSetting = false;
                    gridItem.CanCopy = false;
                    gridItem.CanExportExcel = true;
                    gridItem.CanExportPicture = true;
                    gridItem.CanLookOriginalData = false;
                    gridItem.CanDelete = false;
                    return;
                }

                widgetModel.WidgetTimer = InitialTimer(widgetModel.TimeDimensions);
                var cancellationTokenSource = new CancellationTokenSource();
                _taskFactory.StartNew(() =>
                {
                    QueryData(widgetModel);
                }, cancellationTokenSource.Token);
                cancellationTokenSources.Add(cancellationTokenSource);

                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                {
                    widgetModel.WidgetTimer.Start();
                }

            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加表格发生错误");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddGridItem", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddGridItem", null);
                }
            }
        }

        /// <summary>
        /// 添加Chart
        /// </summary>
        /// <param name="widgetModel"></param>
        private void AddChartItem(WidgetModel widgetModel)
        {
            try
            {
                ChartItemInfo chartItem = new ChartItemInfo();
                chartItem.Id = widgetModel.Id;
                chartItem.GroupIndex = widgetModel.GroupIndex;
                chartItem.ItemTitle = widgetModel.Title;
                if (widgetModel.Width == 0 && widgetModel.Height == 0)
                {
                    chartItem.ItemHeight = WidgetAreaHeight / 2 - 18;
                    chartItem.ItemWidth = (WidgetAreaWidth - 86) / 3;
                }
                else
                {
                    chartItem.ItemWidth = Math.Round(widgetModel.Width * WidgetAreaWidth, 0);
                    chartItem.ItemHeight = Math.Round(widgetModel.Height * chartItem.ItemWidth, 0);
                }

                chartItem.ChartType = widgetModel.DisplayTypeIndex;
                if (User.UserInfo.AuthorizationId == 3 && SelectedDataCabin.DataCabinTypeId == 1)
                {
                    chartItem.CanSetting = false;
                    chartItem.CanCopy = false;
                    chartItem.CanExportExcel = true;
                    chartItem.CanExportPicture = true;
                    chartItem.CanLookOriginalData = false;
                    chartItem.CanDelete = false;
                }
                else
                {
                    chartItem.CanSetting = true;
                    chartItem.CanCopy = true;
                    chartItem.CanExportExcel = true;
                    chartItem.CanExportPicture = true;
                    chartItem.CanLookOriginalData = true;
                    chartItem.CanDelete = true;
                }

                chartItem.IsLoading = true;
                WidgetList.Add(chartItem);

                if (_systemList.Contains(widgetModel.SystemTypeId))
                {
                    chartItem.IsNoAuthorization = Visibility.Collapsed;
                }
                else
                {
                    chartItem.IsLoading = false;
                    chartItem.CanSetting = false;
                    chartItem.CanCopy = false;
                    chartItem.CanExportExcel = false;
                    chartItem.CanExportPicture = false;
                    chartItem.CanLookOriginalData = false;
                    chartItem.CanDelete = false;
                    return;
                }

                widgetModel.WidgetTimer = InitialTimer(widgetModel.TimeDimensions);
                var cancellationTokenSource = new CancellationTokenSource();
                _taskFactory.StartNew(() =>
                {
                    QueryData(widgetModel);
                }, cancellationTokenSource.Token);
                cancellationTokenSources.Add(cancellationTokenSource);

                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                {
                    widgetModel.WidgetTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加图表发生错误");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddChartItem", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddChartItem", null);
                }
            }
        }

        /// <summary>
        /// 添加Gauge
        /// </summary>
        /// <param name="widgetModel"></param>
        private void AddGaugeItem(WidgetModel widgetModel)
        {
            try
            {
                GaugeItemInfo gaugeItem = new GaugeItemInfo();
                gaugeItem.Id = widgetModel.Id;
                gaugeItem.GroupIndex = widgetModel.GroupIndex;
                gaugeItem.ItemTitle = widgetModel.Title;
                if (widgetModel.Width == 0 && widgetModel.Height == 0)
                {
                    gaugeItem.ItemHeight = WidgetAreaHeight / 2 - 18;
                    gaugeItem.ItemWidth = (WidgetAreaWidth - 86) / 3;
                }
                else
                {
                    gaugeItem.ItemWidth = Math.Round(widgetModel.Width * WidgetAreaWidth, 0);
                    gaugeItem.ItemHeight = Math.Round(widgetModel.Height * gaugeItem.ItemWidth, 0);
                }

                if (User.UserInfo.AuthorizationId == 3 && SelectedDataCabin.DataCabinTypeId == 1)
                {
                    gaugeItem.CanSetting = false;
                    gaugeItem.CanCopy = false;
                    gaugeItem.CanExportExcel = true;
                    gaugeItem.CanExportPicture = true;
                    gaugeItem.CanLookOriginalData = false;
                    gaugeItem.CanDelete = false;
                }
                else
                {
                    gaugeItem.CanSetting = true;
                    gaugeItem.CanCopy = true;
                    gaugeItem.CanExportExcel = true;
                    gaugeItem.CanExportPicture = true;
                    gaugeItem.CanLookOriginalData = true;
                    gaugeItem.CanDelete = true;
                }

                #region 判断显示类型

                switch (widgetModel.DisplayTypeIndex.ToString(CultureInfo.InvariantCulture))
                {
                    case "0":
                        gaugeItem.KnobVisibility = true;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "1":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = true;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "2":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RadialVisibility = true;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "3":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = true;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "4":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.RulerVisibility = true;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "5":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = true;
                        gaugeItem.VolumeVisibility = false;
                        break;
                    case "6":
                        gaugeItem.KnobVisibility = false;
                        gaugeItem.LinearVisibility = false;
                        gaugeItem.RadialVisibility = false;
                        gaugeItem.RegionKnobVisibility = false;
                        gaugeItem.RulerVisibility = false;
                        gaugeItem.SpeedometerVisibility = false;
                        gaugeItem.VolumeVisibility = true;
                        break;
                }

                #endregion

                gaugeItem.IsLoading = true;
                WidgetList.Add(gaugeItem);

                if (_systemList.Contains(widgetModel.SystemTypeId))
                {
                    gaugeItem.IsNoAuthorization = Visibility.Collapsed;
                }
                else
                {
                    gaugeItem.IsLoading = false;
                    gaugeItem.CanSetting = false;
                    gaugeItem.CanCopy = false;
                    gaugeItem.CanExportExcel = false;
                    gaugeItem.CanExportPicture = false;
                    gaugeItem.CanLookOriginalData = false;
                    gaugeItem.CanDelete = false;
                    return;
                }

                widgetModel.WidgetTimer = InitialTimer(widgetModel.TimeDimensions);
                var cancellationTokenSource = new CancellationTokenSource();
                _taskFactory.StartNew(() =>
                {
                    QueryData(widgetModel);
                }, cancellationTokenSource.Token);
                cancellationTokenSources.Add(cancellationTokenSource);

                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                {
                    widgetModel.WidgetTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加仪表盘发生错误");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddGaugeItem", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddGaugeItem", null);
                }
            }
        }

        /// <summary>
        /// 添加Map
        /// </summary>
        /// <param name="widgetModel"></param>
        private void AddMapItem(WidgetModel widgetModel)
        {
            try
            {
                MapItemInfo mapItem = new MapItemInfo();
                mapItem.Id = widgetModel.Id;
                mapItem.GroupIndex = widgetModel.GroupIndex;
                mapItem.ItemTitle = widgetModel.Title;
                if (widgetModel.Width == 0 && widgetModel.Height == 0)
                    if (widgetModel.Width == 0 && widgetModel.Height == 0)
                    {
                        mapItem.ItemHeight = WidgetAreaHeight / 2 - 18;
                        mapItem.ItemWidth = (WidgetAreaWidth - 86) / 3;
                    }
                    else
                    {
                        mapItem.ItemWidth = Math.Round(widgetModel.Width * WidgetAreaWidth, 0);
                        mapItem.ItemHeight = Math.Round(widgetModel.Height * mapItem.ItemWidth, 0);
                    }

                if (User.UserInfo.AuthorizationId == 3 && SelectedDataCabin.DataCabinTypeId == 1)
                {
                    mapItem.CanSetting = false;
                    mapItem.CanCopy = false;
                    mapItem.CanExportExcel = true;
                    mapItem.CanExportPicture = true;
                    mapItem.CanLookOriginalData = false;
                    mapItem.CanDelete = false;
                }
                else
                {
                    mapItem.CanSetting = true;
                    mapItem.CanCopy = true;
                    mapItem.CanExportExcel = true;
                    mapItem.CanExportPicture = true;
                    mapItem.CanLookOriginalData = true;
                    mapItem.CanDelete = true;
                }
                mapItem.IsLoading = true;
                WidgetList.Add(mapItem);

                if (_systemList.Contains(widgetModel.SystemTypeId))
                {
                    mapItem.IsNoAuthorization = Visibility.Collapsed;
                }
                else
                {
                    mapItem.IsLoading = false;
                    mapItem.CanSetting = false;
                    mapItem.CanCopy = false;
                    mapItem.CanExportExcel = true;
                    mapItem.CanExportPicture = true;
                    mapItem.CanLookOriginalData = false;
                    mapItem.CanDelete = false;
                    return;
                }

                widgetModel.WidgetTimer = InitialTimer(widgetModel.TimeDimensions);
                var cancellationTokenSource = new CancellationTokenSource();
                _taskFactory.StartNew(() =>
                {
                    QueryData(widgetModel);
                }, cancellationTokenSource.Token);
                cancellationTokenSources.Add(cancellationTokenSource);

                if (widgetModel.SystemTypeId == (int)SystemTypeEnum.SiteRealtime || widgetModel.SystemTypeId == (int)SystemTypeEnum.TrackRealtime)
                {
                    widgetModel.WidgetTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("添加地图发生错误");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddMapItem", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddMapItem", null);
                }
            }
        }

        /// <summary>
        /// 删除Widget
        /// </summary>
        /// <param name="itemInfo"></param>
        public async void DeleteWidget(IWidgetInfo itemInfo)
        {
            try
            {
                if (itemInfo == null)
                {
                    return;
                }

                if (_trackRealtimeDic.Keys.Contains(itemInfo.Id))
                {
                    _trackRealtimeDic.Remove(itemInfo.Id);
                }

                var deleteWidgetResult = await MainService.DeleteWidget(itemInfo.Id);

                if (((RestResponseBase)(deleteWidgetResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    if (deleteWidgetResult.Data.Value == bool.TrueString)
                    {
                        if (_timerList.Exists(w => w.Id == itemInfo.Id))
                            if (_timerList.Exists(w => w.Id == itemInfo.Id))
                            {
                                var widgetModel = _timerList.Find(w => w.Id == itemInfo.Id);
                                widgetModel.WidgetTimer.Stop();
                                _timerList.Remove(widgetModel);
                            }

                        WidgetList.Remove(itemInfo);
                    }
                    else
                    {
                        ShowMessage.Show("查询错误");
                    }
                }
                else if (((RestResponseBase)(deleteWidgetResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)deleteWidgetResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(deleteWidgetResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("删除Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DeleteWidget", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DeleteWidget", null);
                }
            }
        }

        /// <summary>
        /// 修改Widget
        /// </summary>
        /// <param name="itemInfo"></param>
        public async void ModifyWidget(IWidgetInfo itemInfo)
        {
            try
            {
                if (itemInfo == null)
                {
                    return;
                }

                var widgetResult = await MainService.QueryWidget(itemInfo.Id);

                if (((RestResponseBase)(widgetResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var widget = JsonConvert.DeserializeObject<WidgetModel>(widgetResult.Data.Value);
                    if (widget.SystemTypeId == (int)SystemTypeEnum.Benchmark)
                    {
                        var benchmarkViewModel = IoC.Get<BenchmarkViewModel>();
                        benchmarkViewModel.WidgetModelEntity = widget;
                        benchmarkViewModel.ClientId = ClientSelectedItem.Id;
                        benchmarkViewModel.DataCabinId = SelectedDataCabin.Id;
                        benchmarkViewModel.MetricsCache = _metricsCache;
                        IoC.Get<IWindowManager>().ShowDialog(benchmarkViewModel);
                    }
                    else
                    {
                        var widgetViewModel = IoC.Get<WidgetViewModel>();
                        widgetViewModel.CurrentDate = _currentDateTime;
                        widgetViewModel.WidgetModelEntity = widget;
                        widgetViewModel.ClientId = ClientSelectedItem.Id;
                        widgetViewModel.TimeDimensionsCache = _timeDimensionsCache;
                        widgetViewModel.DimensionsCache = _dimensionsCache;
                        widgetViewModel.DataTypeCache = _dataTypesCache;
                        widgetViewModel.MetricsCache = _metricsCache;
                        widgetViewModel.StartDate = StartDate;
                        widgetViewModel.EndDate = EndDate;
                        IoC.Get<IWindowManager>().ShowDialog(widgetViewModel);
                    }
                }
                else if (((RestResponseBase)(widgetResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)widgetResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(widgetResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("修改Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ModifyWidget", ex);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ModifyWidget", null);
                }
            }
        }

        /// <summary>
        /// 复制Widget
        /// </summary>
        /// <param name="itemInfo"></param>
        public async void CopyWidget(IWidgetInfo itemInfo)
        {
            try
            {
                if (itemInfo == null)
                {
                    return;
                }

                var widgetListStringResult = await MainService.CopyWidget(itemInfo.Id, WidgetList.Count);
                if (((RestResponseBase)(widgetListStringResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var widgetList = JsonConvert.DeserializeObject<List<WidgetModel>>(widgetListStringResult.Data.Value);
                    InitWidgetList(widgetList);
                }
                else if (((RestResponseBase)(widgetListStringResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)widgetListStringResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(widgetListStringResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("复制Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to CopyWidget", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "CopyWidget", null);
                }
            }
        }

        public void ExportGridWidget(Grid grid)
        {
            try
            {
                if (grid == null)
                {
                    return;
                }

                var flexGrid = grid.FindChild<C1FlexGrid>("FlexGrid");

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                //设置文件类型 
                saveFileDialog.Filter = @"Excel|*.xlsx";

                //设置默认文件类型显示顺序 
                saveFileDialog.FilterIndex = 1;

                //保存对话框是否记忆上次打开的目录 
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileName = saveFileDialog.FileName;

                    var book = new C1XLBook();
                    book.Sheets.Clear();
                    var xlSheet = book.Sheets.Add("Sheet1");
                    ExcelFilter.Save(flexGrid, xlSheet);

                    book.Save(fileName, FileFormat.OpenXml);
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("导出Excel出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ExportWidget", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ExportWidget", null);
                }
            }
        }

        public void ExportChartWidget(Grid grid)
        {
            try
            {
                if (grid == null)
                {
                    return;
                }

                var c1Chart1 = grid.FindChild<C1Chart>("C1Chart");

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                //设置文件类型 
                saveFileDialog.Filter = @"Excel|*.xlsx";

                //设置默认文件类型显示顺序 
                saveFileDialog.FilterIndex = 1;

                //保存对话框是否记忆上次打开的目录 
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    c1Chart1.View.Background = Brushes.White;
                    MemoryStream ms = new MemoryStream();
                    c1Chart1.SaveImage(ms, ImageFormat.Png);

                    C1XLBook wb = new C1XLBook();
                    BitmapImage bmps = new BitmapImage();
                    bmps.BeginInit();
                    bmps.StreamSource = ms;
                    bmps.EndInit();
                    WriteableBitmap img = new WriteableBitmap(bmps);
                    XLSheet sheet = wb.Sheets[0];
                    sheet[0, 0].Value = img;

                    wb.Save(saveFileDialog.FileName + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("导出Excel出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ExportWidget", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ExportWidget", null);
                }
            }
        }

        #endregion

        #region DataCabin

        /// <summary>
        /// 添加DataCabin点击事件
        /// </summary>
        public async void AddDataCabin()
        {
            try
            {
                if (string.IsNullOrEmpty(AddDataCabinName) || string.IsNullOrWhiteSpace(AddDataCabinName))
                {
                    return;
                }

                var dataCabinListStringResult = await MainService.AddDataCabin(AddDataCabinName, ClientSelectedItem.Id, User.UserInfo.Id, 3, StartDate, EndDate);
                if (((RestResponseBase)(dataCabinListStringResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var dataCabinList = JsonConvert.DeserializeObject<List<DataCabinModel>>(dataCabinListStringResult.Data.Value);
                    InitDataCabinList(dataCabinList);

                    SelectedDataCabin = DataCabinList.LastOrDefault();
                    WidgetList.Clear();
                    AddDataCabinName = string.Empty;
                }
                else if (((RestResponseBase)(dataCabinListStringResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)dataCabinListStringResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(dataCabinListStringResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("添加DataCabin出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to AddDataCabin", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "AddDataCabin", null);
                }
            }
        }

        /// <summary>
        /// 更新DataCabin
        /// </summary>
        /// <param name="dataCabinModel"></param>
        public async void UpdateDataCabin(DataCabinModel dataCabinModel)
        {
            try
            {
                if (dataCabinModel == null)
                {
                    return;
                }
                var dataCabinListResult = await MainService.UpdateDataCabin(dataCabinModel.Id, dataCabinModel.DataCabinName, dataCabinModel.DataCabinTypeId, dataCabinModel.ClientId, dataCabinModel.UserId, dataCabinModel.DateTypeId, dataCabinModel.StartDate, dataCabinModel.EndDate);
                if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var dataCabinList = JsonConvert.DeserializeObject<List<DataCabinModel>>(dataCabinListResult.Data.Value);
                    InitDataCabinList(dataCabinList);
                }
                else if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)dataCabinListResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("修改DataCabin出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to UpdateDataCabin", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "UpdateDataCabin", null);
                }
            }
        }

        /// <summary>
        /// 删除DataCabin
        /// </summary>
        /// <param name="dataCabinModel"></param>
        public async void DeleteDataCabin(DataCabinModel dataCabinModel)
        {
            try
            {
                if (dataCabinModel == null)
                {
                    return;
                }

                _trackRealtimeDic.Clear();

                var dataCabinListResult = await MainService.DeleteDataCabin(dataCabinModel.Id, dataCabinModel.ClientId, User.UserInfo.Id);
                if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var dataCabinList = JsonConvert.DeserializeObject<List<DataCabinModel>>(dataCabinListResult.Data.Value);
                    InitDataCabinList(dataCabinList);
                }
                else if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)dataCabinListResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(dataCabinListResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("删除DataCabin出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DeleteDataCabin", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DeleteDataCabin", null);
                }
            }
        }

        #endregion

        /// <summary>
        /// DataCabin选择改变事件
        /// </summary>
        public void DataCabinSelectedChanged()
        {
            if (SelectedDataCabin == null)
            {
                return;
            }

            try
            {
                CleanTimerList();
                _trackRealtimeDic.Clear();

                _cancellationToken = new CancellationToken();
                _taskFactory = new TaskFactory(_cancellationToken);

                SetWhetherCanAdd();
                SetDateChecked();
            }
            catch (Exception ex)
            {
                ShowMessage.Show("数据舱选择改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DataCabinSelectedChanged", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DataCabinSelectedChanged", null);
                }
            }
        }

        private void CleanTimerList()
        {
            if (_timerList != null && _timerList.Count > 0)
            {
                foreach (var widgetModel in _timerList)
                {
                    if (widgetModel.WidgetTimer != null)
                    {
                        widgetModel.WidgetTimer.Stop();
                    }
                }
                _timerList.Clear();
            }
        }

        /// <summary>
        /// 原始数据选择改变事件
        /// </summary>
        public void OriginalDataSelectedChanged()
        {
        }

        /// <summary>
        /// 设置显示区域
        /// </summary>
        /// <param name="isVisibility"></param>
        public void SetDisplayAreaVisibility(bool isVisibility)
        {
            if (_selectedTypeEnum == SelectedTypeEnum.OriginalData)
            {
                WidgetAreaVisibility = isVisibility;
                OriginalDataAreaVisibility = !isVisibility;
            }
            else
            {

            }
        }

        /// <summary>
        /// 客户选择改变事件
        /// </summary>
        public async void ClientSelectedChanged()
        {
            try
            {
                var dataCabinListAndWidgetListStringResult = await MainService.GetDataCabinListAndWidgetList(User.UserInfo.Id.ToString(CultureInfo.InvariantCulture), ClientSelectedItem.Id.ToString(CultureInfo.InvariantCulture));

                if (((RestResponseBase)(dataCabinListAndWidgetListStringResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var dataCabinListAndWidgetListResult = dataCabinListAndWidgetListStringResult.Data.ToDictionary(d => d.Key, d => d.Value);
                    var dataCabinListString = dataCabinListAndWidgetListResult["DataCabinList"];
                    var dataCabinList = JsonConvert.DeserializeObject<List<DataCabinModel>>(dataCabinListString);
                    InitDataCabinList(dataCabinList);
                }
                else if (((RestResponseBase)(dataCabinListAndWidgetListStringResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)dataCabinListAndWidgetListStringResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(dataCabinListAndWidgetListStringResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("客户改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ClientSelectedChanged", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ClientSelectedChanged", null);
                }
            }
        }

        /// <summary>
        /// 日期类型转换
        /// </summary>
        public void DateSwitchChecked()
        {
            if (SelectedDataCabin == null || (IsCustomChecked == false && IsYesterdayChecked == false && IsLastWeekChecked == false && IsLastMonthChecked == false))
            {
                return;
            }

            try
            {
                if (IsCustomChecked)
                {
                    DateIsEnabled = true;
                    SelectedDataCabin.DateTypeId = 1;
                    StartDate = SelectedDataCabin.StartDate;
                    EndDate = SelectedDataCabin.EndDate;
                }
                else if (IsYesterdayChecked)
                {
                    DateIsEnabled = false;
                    StartDate = _currentDateTime.AddDays(-1);
                    EndDate = _currentDateTime;
                    SelectedDataCabin.DateTypeId = 2;
                }
                else if (IsLastWeekChecked)
                {
                    DateIsEnabled = false;
                    StartDate = _currentDateTime.AddDays(-7);
                    EndDate = _currentDateTime;
                    SelectedDataCabin.DateTypeId = 3;
                }
                else
                {
                    DateIsEnabled = false;
                    StartDate = _currentDateTime.AddMonths(-1);
                    EndDate = _currentDateTime;
                    SelectedDataCabin.DateTypeId = 4;
                }
            }
            catch (Exception ex)
            {
                ShowMessage.Show("日期类型改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DateSwitchChecked", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DateSwitchChecked", null);
                }
            }

            DateChanged();
        }

        /// <summary>
        /// DataCabin名称修改事件
        /// </summary>
        public async void DataCabinNameEditCompleted()
        {
            try
            {
                var dataCabinNameResult = await MainService.UpdateDataCabinName(SelectedDataCabin.Id, SelectedDataCabin.DataCabinName);
                if (((RestResponseBase)(dataCabinNameResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    if (dataCabinNameResult.Data.Value == bool.TrueString)
                    {
                    }
                    else
                    {
                        ShowMessage.Show("修改失败");
                    }
                }
                else if (((RestResponseBase)(dataCabinNameResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)dataCabinNameResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(dataCabinNameResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("修改数据舱名称出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DataCabinNameEditCompleted", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DataCabinNameEditCompleted", null);
                }
            }
        }

        /// <summary>
        /// 日期改变事件
        /// </summary>
        public async void DateChanged()
        {
            if (SelectedDataCabin == null)
            {
                return;
            }

            try
            {
                var widgetModelListResult = await MainService.GetWidgetList(SelectedDataCabin.Id.ToString(CultureInfo.InvariantCulture), SelectedDataCabin.DateTypeId.ToString(CultureInfo.InvariantCulture), StartDate.ToShortDateString(), EndDate.ToShortDateString());

                if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    SelectedDataCabin.StartDate = StartDate;
                    SelectedDataCabin.EndDate = EndDate;
                    var widgetList = JsonConvert.DeserializeObject<List<WidgetModel>>(widgetModelListResult.Data.Value);
                    InitWidgetList(widgetList);
                }
                else if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)widgetModelListResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("日期改变出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to DateChanged", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "DateChanged", null);
                }
            }
        }

        /// <summary>
        /// 回传值
        /// </summary>
        /// <param name="widgetModel"></param>
        public async void Handle(WidgetModel widgetModel)
        {
            if (widgetModel == null)
            {
                return;
            }

            try
            {
                var widgetModelListResult = await MainService.SaveWidget(JsonConvert.SerializeObject(widgetModel));

                if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var widgetList = JsonConvert.DeserializeObject<List<WidgetModel>>(widgetModelListResult.Data.Value);
                    InitWidgetList(widgetList);
                }
                else if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)widgetModelListResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(widgetModelListResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("保存Widget出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to Handle", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Handle", null);
                }
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public void Logout()
        {
            try
            {
                ShellViewModel shell = IoC.Get<IShell>() as ShellViewModel;
                shell.Email = null;
                shell.Password = null;
                shell.EmailItems = null;
                shell.ErrorMessage = null;

                IoC.Get<IWindowManager>().ShowWindow(shell);

                TryClose();
            }
            catch (Exception ex)
            {
                ShowMessage.Show("退出出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to Logout", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Logout", null);
                }
            }
        }

        /// <summary>
        /// Widget尺寸改变事件
        /// </summary>
        public async void WidgetSizeChanged(IWidgetInfo widgetInfo, Grid grid)
        {
            try
            {
                var width = grid.Width;
                var height = grid.Height;
                widgetInfo.ItemWidth = Math.Round(width / WidgetAreaWidth, 3);
                widgetInfo.ItemHeight = Math.Round(height / width, 3);

                var widgetSizeResult = await MainService.UpdateWidgetSize(widgetInfo.Id, widgetInfo.ItemWidth, widgetInfo.ItemHeight);
                if (((RestResponseBase)(widgetSizeResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    if (widgetSizeResult.Data.Value == bool.TrueString)
                    {
                    }
                    else
                    {
                        ShowMessage.Show("调整失败");
                    }
                }
                else if (((RestResponseBase)(widgetSizeResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if ((int)widgetSizeResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if (((RestResponseBase)(widgetSizeResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("Widget尺寸出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to WidgetSizeChanged", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "WidgetSizeChanged", null);
                }
            }
        }

        /// <summary>
        /// 设置窗口
        /// </summary>
        public void Setting()
        {
            try
            {
                var settingViewModel = IoC.Get<SettingViewModel>();

                IoC.Get<IWindowManager>().ShowDialog(settingViewModel);
                //MainView view = GetView() as MainView;
                //if(view != null)
                //{
                //    view.Flyout.IsOpen = true;
                //}
            }
            catch (Exception ex)
            {
                ShowMessage.Show("打开设置窗体出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to Setting", ex.InnerException);
            }
            finally
            {
                if (LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Setting", null);
                }
            }
        }
    }
}