using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Controls;
using Board.Enums;
using Board.Models.System;
using Board.Services.TrackAnalysisReport;
using Board.SystemData.Base;

using Newtonsoft.Json;

namespace Board.SystemData.TrackAnalysisReport
{
    public class TrackAnalysisReportDataService : BaseSystemData
    {
        //int systemType, string clientId, int userId
        public override async Task<ObservableCollection<Node>> GetTableSource(Dictionary<string, object> parameterDictionary)
        {
            try
            {
                var campaignListResult = await TrackService.GetCampaigns();
                if(campaignListResult != null)
                {
                    var campaignItems = new ObservableCollection<Node>();
                    foreach(var site in campaignListResult)
                    {
                        var node = new Node { Id = site.Key, Title = site.Value };
                        campaignItems.Add(node);
                    }
                    TableSource = campaignItems;
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("查询数据表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetTableSource", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetTableSource", null);
                }
            }

            return TableSource;
        }

        //WidgetModel widgetModel, int? interval, string startDate, string endDate
        public override async Task<ResultValue> GetSystemData(Dictionary<string, object> parameterDictionary)
        {
            try
            {
                var widgetModel = parameterDictionary["WidgetModel"] as WidgetModel;
                var startDate = parameterDictionary["StartDate"].ToString();
                var endDate = parameterDictionary["EndDate"].ToString();
                string dimension;
                if(!string.IsNullOrEmpty(widgetModel.TimeDimensions))
                {
                    dimension = widgetModel.TimeDimensions + "," + widgetModel.Dimensions;
                }
                else
                {
                    dimension = widgetModel.Dimensions;
                }

                var dataResult = await TrackService.GetTrackAnalysisData(widgetModel.EnHeader, widgetModel.TableName, dimension, widgetModel.Metrics, startDate, endDate, widgetModel.DataCount.ToString());
                if(dataResult != null)
                {
                    SystemData = new ResultValue
                    {
                        Key = string.Empty,
                        Value = JsonConvert.SerializeObject(dataResult)
                    };
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("查询数据出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetSystemData", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetSystemData", null);
                }
            }

            return SystemData;
        }
    }
}
