using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Enums;
using Board.Models.Control;
using Board.Models.Sponsor;
using Board.Models.System;
using Board.Services.Benchmark;
using Newtonsoft.Json;
using RestSharp;

namespace Board.SystemData.Benchmark
{
    public static class BenchmarkDataService
    {
        public static async Task<Dictionary<string, string>> GetBaseData(Dictionary<string, object> parameterDictionary = null)
        {
            var baseDataDictionary = new Dictionary<string, string>();

            try
            {
                var baseDataResult = await BenchmarkService.GetBenchmarkBaseDatas();

                if(((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    baseDataDictionary = baseDataResult.Data.ToDictionary(d => d.Key, d => d.Value);
                }
                else if(((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if((int)baseDataResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if(((RestResponseBase)(baseDataResult)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ShowMessage.Show("访问500错误");
                }
                else
                {
                    ShowMessage.Show("未知错误");
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("加载数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetBaseData", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetBaseData", null);
                }
            }

            return baseDataDictionary;
        }

        public static async Task<ResultValue> GetSystemData(Dictionary<string, object> parameterDictionary)
        {
            var systemDataResult = new ResultValue();
            try
            {
                var dateType = parameterDictionary["DateType"].ToString();
                var brandType = parameterDictionary["BrandType"].ToString();
                var category = parameterDictionary["Category"] == null ? string.Empty : parameterDictionary["Category"].ToString();
                var category1 = string.Empty;
                var category2 = string.Empty;
                if(!string.IsNullOrEmpty(category))
                {
                    var categorys = category.Split(new[] { "*" }, StringSplitOptions.RemoveEmptyEntries);
                    category1 = categorys[0];
                    category2 = categorys.Count() == 2 ? categorys[1] : string.Empty;
                }

                var dimensions = parameterDictionary["Dimensions"] == null ? string.Empty : parameterDictionary["Dimensions"].ToString();
                var metrics = parameterDictionary["Metrics"].ToString();
                var benchmarkDataValue = await BenchmarkService.GetBenchmarkDatas(dateType, brandType, category1, category2, dimensions, metrics);

                if(((RestResponseBase)(benchmarkDataValue)).StatusCode == HttpStatusCode.OK) //200
                {
                    systemDataResult = benchmarkDataValue.Data;
                }
                else if(((RestResponseBase)(benchmarkDataValue)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if((int)benchmarkDataValue.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if(((RestResponseBase)(benchmarkDataValue)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ShowMessage.Show("访问500错误");
                }
                else
                {
                    ShowMessage.Show("未知错误");
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("加载数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetSystemData", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetSystemData", null);
                }
            }

            return systemDataResult;
        }

        public static async Task<ResultValue> GetDataByType(string clientId, int typeId)
        {
            var categorys1ListResult = new ResultValue();
            try
            {
                var benchmarkTypeDataValue = await BenchmarkService.GetCategorys1Datas(clientId, typeId.ToString());

                if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.OK) //200
                {
                    categorys1ListResult = benchmarkTypeDataValue.Data;
                }
                else if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if((int)benchmarkTypeDataValue.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ShowMessage.Show("访问500错误");
                }
                else
                {
                    ShowMessage.Show("未知错误");
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("加载数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetDataByType", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetDataByType", null);
                }
            }

            return categorys1ListResult;
        }

        public static async Task<ResultValue> GetDataByCategorys1(string categorys1)
        {
            var categorys2ListResult = new ResultValue();
            try
            {
                var benchmarkTypeDataValue = await BenchmarkService.GetCategorys2Datas(categorys1);

                if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.OK) //200
                {
                    categorys2ListResult = benchmarkTypeDataValue.Data;
                }
                else if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if((int)benchmarkTypeDataValue.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if(((RestResponseBase)(benchmarkTypeDataValue)).StatusCode == HttpStatusCode.InternalServerError) //500
                {
                    ShowMessage.Show("访问500错误");
                }
                else
                {
                    ShowMessage.Show("未知错误");
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("加载数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetDataByType", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetDataByType", null);
                }
            }

            return categorys2ListResult;
        }

        public static ObservableCollection<ColumnConfig> ConvertToGridData(WidgetModel widgetModel, List<DimensionTypeModel> dimensionsCache, List<MetricTypeModel> metricsCache)
        {
            var columnConfigList = new ObservableCollection<ColumnConfig>();

            try
            {
                #region 设置维度列

                if(widgetModel.Dimensions != null)
                {
                    var dimensions = widgetModel.Dimensions.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var dimension in dimensions)
                    {
                        var dimensionx = dimensionsCache.Find(m => m.EnName.ToUpperInvariant() == dimension.ToUpperInvariant() && m.SystemTypeId == (int)SystemTypeEnum.Sponsor);
                        var dd = dimension.Substring(0, 1).ToUpperInvariant() + dimension.Substring(1, dimension.Length - 1);
                        columnConfigList.Add(new ColumnConfig
                        {
                            ColumnName = dd,
                            HeaderName = "数据类型",
                            HeaderDescription = "数据类型",
                            Width = 130,
                            FormatString = dimensionx.Format,
                            HeaderColorString = dimensionx.Color
                        });
                    }
                }

                #endregion

                #region 设置指标列

                var metrics = widgetModel.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var metric in metrics)
                {
                    var metricx = metricsCache.Find(m => m.EnName == metric && m.SystemTypeId == widgetModel.SystemTypeId);
                    var mm = metric.Substring(0, 1).ToUpperInvariant() + metric.Substring(1, metric.Length - 1);
                    columnConfigList.Add(new ColumnConfig
                    {
                        ColumnName = mm,
                        HeaderName = metricx.CnName,
                        HeaderDescription = metricx.CnDescription,
                        Width = metricx.CnName.Length * 20,
                        FormatString = metricx.Format,
                        HeaderColorString = metricx.Color
                    });
                }

                #endregion
            }
            catch(Exception ex)
            {
                ShowMessage.Show("转换表格数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to ConvertToGridData", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "ConvertToGridData", null);
                }
            }

            return columnConfigList;
        }


        public static List<object> ConvertToChartData(WidgetModel widgetModel, ResultValue resultValue, List<MetricTypeModel> metricsCache)
        {
            var datas = new ObservableCollection<string>();
            var series = new ObservableCollection<MyReport>();
            var axisDic = new Dictionary<string, Tuple<string, string>>();
            var values = new List<decimal>();

            var metrics = widgetModel.Metrics.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var metric in metrics)
            {
                var metricX = metricsCache.FirstOrDefault(m => m.SystemTypeId == widgetModel.SystemTypeId && m.EnName == metric);
                datas.Add(metricX.CnName);

                axisDic.Add(metric, new Tuple<string, string>(metricX.CnName, "Y"));
            }

            #region

            var label = string.Empty;
            switch(widgetModel.DataType)
            {
                case "1":
                    label = "所有节目";
                    break;
                case "2":
                    label = "指定节目";
                    break;
                case "3":
                    label = "节目类型";
                    break;
                case "4":
                    label = "赞助级别";
                    break;
                case "5":
                    label = "电视台";
                    break;
                case "6":
                    label = "视频网站自制";
                    break;
                case "7":
                    label = "品牌品类";
                    break;
            }

            switch(widgetModel.DateTypeId)
            {
                case 5:
                    label += "-本期";
                    break;
                case 6:
                    label += "-上期";
                    break;
                case 7:
                    label += "-历史";
                    break;
            }

            #endregion

            var report = new MyReport();
            report.Label = label;

            var data = new ObservableCollection<C1Data>();
            Type systemType = typeof(SponsorData);
            var resultX = JsonConvert.DeserializeObject<SponsorData>(resultValue.Value);

            foreach(var metric in metrics)
            {
                PropertyInfo mPropertyInfo = systemType.GetProperty(metric.Substring(0, 1).ToUpperInvariant() + metric.Substring(1, metric.Length - 1).TrimEnd());

                decimal value;
                if(mPropertyInfo.GetValue(resultX, null) == null)
                {
                    value = 0;
                }
                else
                {
                    if(mPropertyInfo.GetValue(resultX, null) is int)
                    {
                        value = Convert.ToInt32(mPropertyInfo.GetValue(resultX, null));
                    }
                    else if(mPropertyInfo.GetValue(resultX, null) is double || mPropertyInfo.GetValue(resultX, null) is decimal)
                    {
                        value = Convert.ToDecimal(mPropertyInfo.GetValue(resultX, null));
                    }
                    else
                    {
                        value = 0;
                    }
                }
                data.Add(new C1Data { Value = value });
                values.Add(value);
            }

            report.Data = data;
            series.Add(report);

            return new List<object> { datas, series, axisDic, values };
        }
    }
}
