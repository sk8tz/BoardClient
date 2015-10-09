using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Controls;
using Board.Enums;
using Board.Models.System;
using Board.Services.TrackRealtime;
using Board.SystemData.Base;

using Newtonsoft.Json;

namespace Board.SystemData.TrackRealtime
{
    public class TrackRealtimeDataService : BaseSystemData
    {
        //int systemType, string clientId, int userId
        public override async Task<ObservableCollection<Node>> GetTableSource(Dictionary<string, object> parameterDictionary)
        {
            try
            {
                var campaignListResult = await TrackRealtimeService.GetCampaigns();
                if(campaignListResult != null)
                {
                    var campaignItems = new ObservableCollection<Node>();
                    foreach(var campaign in campaignListResult)
                    {
                        var node = new Node { Id = campaign.Key, Title = campaign.Value };
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

                var trackRealtimeDataResult = await TrackRealtimeService.GetRealtimeData(widgetModel.TableName, startDate, endDate, widgetModel.Dimensions + "," + widgetModel.TimeDimensions, widgetModel.Metrics, widgetModel.DataCount.ToString(), widgetModel.DataOrderBy);
                if(trackRealtimeDataResult == null)
                {
                    SystemData = new ResultValue
                    {
                        Key = string.Empty,
                        Value = string.Empty
                    };
                }
                else
                {
                    var data = trackRealtimeDataResult;
                    SystemData = new ResultValue { Key = "", Value = JsonConvert.SerializeObject(data) };
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
