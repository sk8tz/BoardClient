using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Enums;
using Board.Models.System;
using Board.Services.System;

using Newtonsoft.Json;

using RestSharp;

namespace Board.SystemData.System
{
    public static class SystemDataService
    {
        public static async Task<ObservableCollection<SystemTypeModel>> GetSystemList()
        {
            var systemItems = new ObservableCollection<SystemTypeModel>();

            try
            {
                var systemListResult = await WidgetService.GetSystemList();

                if(((RestResponseBase)(systemListResult)).StatusCode == HttpStatusCode.OK) //200
                {
                    var systems = JsonConvert.DeserializeObject<List<SystemTypeModel>>(systemListResult.Data.Value);
                    foreach(var system in systems)
                    {
                        systemItems.Add(system);
                    }
                }
                else if(((RestResponseBase)(systemListResult)).StatusCode == HttpStatusCode.NotFound) //404
                {
                    ShowMessage.Show("访问404错误");
                }
                else if((int)systemListResult.StatusCode == 422)
                {
                    ShowMessage.Show("访问422错误");
                }
                else if(((RestResponseBase)(systemListResult)).StatusCode == HttpStatusCode.InternalServerError) //500
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
                ShowMessage.Show("查询权限系统出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetSystemList", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "GetSystemList", null);
                }
            }

            return systemItems;
        }
    }
}
