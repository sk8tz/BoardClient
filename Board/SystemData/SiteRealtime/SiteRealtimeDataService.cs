using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Controls;
using Board.Enums;
using Board.Models.System;
using Board.Services.SiteRealtime;
using Board.SystemData.Base;

using Newtonsoft.Json;

namespace Board.SystemData.SiteRealtime
{
    public class SiteRealtimeDataService : BaseSystemData
    {
        //int systemType, string clientId, int userId
        public override async Task<ObservableCollection<Node>> GetTableSource(Dictionary<string, object> parameterDictionary)
        {
            try
            {
                var campaignListResult = await SiteRealtimeService.GetCampaigns();
                if(IsStatusCodeOK(campaignListResult.StatusCode))
                {
                    var campaignItems = new ObservableCollection<Node>();
                    foreach(var site in campaignListResult.Data)
                    {
                        var node = new Node
                        {
                            Id = site["id"],
                            Title = site["name"]
                        };

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
                var interval = Convert.ToInt32(parameterDictionary["Interval"].ToString());

                var siteRealtimeDataResult = await SiteRealtimeService.GetRealtimeData(widgetModel.EnHeader, widgetModel.DataType, widgetModel.TableName, startDate, endDate, widgetModel.DataCount.ToString(), interval);
                if(siteRealtimeDataResult != null)
                {
                    SystemData = new ResultValue
                    {
                        Key = string.Empty,
                        Value = JsonConvert.SerializeObject(siteRealtimeDataResult)
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

        private bool IsStatusCodeOK(HttpStatusCode statusCode)
        {
            if(statusCode == HttpStatusCode.OK)
            {
                return true;
            }

            var status = "Http statusCode:" + statusCode;
            LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, status, null);
            return false;
        }
    }
}