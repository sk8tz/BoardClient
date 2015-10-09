namespace Board.SystemData.Sponsor
{
    using Board.Common;
    using Board.Controls;
    using Board.Enums;
    using Board.Models.System;
    using Board.Services.Sponsor;
    using Board.Services.TargetAudience;
    using Board.SystemData.Base;

    using global::System;
    using global::System.Collections.Generic;
    using global::System.Collections.ObjectModel;
    using global::System.Reflection;
    using global::System.Threading.Tasks;

    using Newtonsoft.Json;

    public class SponsorDataService : BaseSystemData
    {
        //int systemType, string clientId, int userId
        public override async Task<ObservableCollection<Node>> GetTableSource(Dictionary<string, object> parameterDictionary)
        {
            try
            {
                var nodes = await SponsorService.Singleton.GetSponsorProgramList(parameterDictionary["ClientId"].ToString());
                TableSource = nodes;
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
                if (widgetModel == null)
                {
                    return null;
                }


                // if TA has been configured
                if (widgetModel.Filter !=null)
                {
                    var taInfo = JsonConvert.DeserializeObject<TargetAudienceInfo>(widgetModel.Filter);
                    if (taInfo != null)
                    {
                        var clientId = parameterDictionary["ClientId"].ToString();
                        var programsName = await this.GetProgramsName(clientId, widgetModel.TableName);
                        //var programsName = new string[]{"女神新装","偶滴歌神啊"};
                        //var programsName = new string[]{"女神新装"};
                        //var programsName = new string[] { "偶滴歌神啊" };
                        var taDataSet = await TargetAudienceService.Singleton.GetTaData(programsName, endDate, taInfo);
                        var taData = TargetAudienceService.Singleton.ConvertDataSet(taDataSet, parameterDictionary);
                        if (taDataSet != null)
                        {
                            var data = JsonConvert.SerializeObject(taData);
                            var resultValue = new ResultValue();
                            resultValue.Value = data;
                        }

                        return null;
                    }
                }

                
                var sponsorData = await SponsorService.Singleton.GetSponsorData(widgetModel.TableName, startDate, endDate, widgetModel.DataCount.ToString(), widgetModel.DataOrderBy);
                SystemData = sponsorData;
                
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

        private async Task<string[]> GetProgramsName(string clientId, string programsId)
        {
            var programsName = new List<string>();
            var ids = programsId.Split(',');
            var nodes = await SponsorService.Singleton.GetSponsorProgramList(clientId);

            foreach (var id in ids)
            {
                foreach (var node in nodes)
                {
                    if (node.Id == id)
                    {
                        programsName.Add(node.Title);
                    }
                }
            }

            return programsName.ToArray();


        }
    }
}