using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Board.Models.System;
using Board.Resources.ApiResources;

using RestSharp;

namespace Board.Services.System
{
    public static class MainService
    {
        private static readonly RestClient Client;

        static MainService()
        {
            Client = new RestClient(SystemResource.BoardServiceClient);
        }

        public static async Task<IRestResponse<List<ResultValue>>> GetConfigData(string email)
        {
            var request = new RestRequest(string.Format(SystemResource.GetConfigDataRequest, email), Method.POST);

            return await Client.ExecutePostTaskAsync<List<ResultValue>>(request);
        }

        public static async Task<IRestResponse<ResultValue>> AddDataCabin(string dataCabiName, int clientId, int? userId, int dateTypeId, DateTime? startDate, DateTime? endDate)
        {
            var startDatex = startDate.ToString().Replace('/', '-');
            var endDatex = endDate.ToString().Replace('/', '-');
            var request = new RestRequest(String.Format(SystemResource.AddDataCabinRequest, dataCabiName, clientId, userId, dateTypeId, startDatex, endDatex), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> UpdateDataCabin(int id, string dataCabinName, int dataCabinTypeId, int clientId, int? userId, int dateTypeId, DateTime? startDate, DateTime? endDate)
        {
            var startDatex = startDate.ToString().Replace('/', '-');
            var endDatex = endDate.ToString().Replace('/', '-');
            var request = new RestRequest(String.Format(SystemResource.UpdateDataCabinRequest, id, dataCabinName, dataCabinTypeId, clientId, userId, dateTypeId, startDatex, endDatex), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> DeleteDataCabin(int id, int clientId, int? userId)
        {
            var request = new RestRequest(String.Format(SystemResource.DeleteDataCabinRequest, id, clientId, userId), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> SaveWidget(string widgetString)
        {
            var request = new RestRequest(SystemResource.SaveWidgetRequest, Method.POST);
            request.AddJsonBody(widgetString);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> GetWidgetList(string datacarbinId, string dateTypeId, string startDate, string endDate)
        {
            var request = new RestRequest(string.Format(SystemResource.GetWidgetListRequest, datacarbinId, dateTypeId, startDate.Replace('/', '-'), endDate.Replace('/', '-')), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<List<ResultValue>>> GetDataCabinListAndWidgetList(string userId, string clientId)
        {
            var request = new RestRequest(string.Format(SystemResource.GetDataCabinListAndWidgetListRequest, userId, clientId), Method.POST);

            return await Client.ExecutePostTaskAsync<List<ResultValue>>(request);
        }

        public static async Task<IRestResponse<ResultValue>> DeleteWidget(int widgetId)
        {
            var request = new RestRequest(string.Format(SystemResource.DeleteWidgetRequest, widgetId), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> QueryWidget(int widgetId)
        {
            var request = new RestRequest(string.Format(SystemResource.GetWidgetRequest, widgetId), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> CopyWidget(int widgetId, int widgetCount)
        {
            var request = new RestRequest(string.Format(SystemResource.CopyWidgetRequest, widgetId, widgetCount), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> UpdateDataCabinName(int dataCabinId, string dataCabinName)
        {
            var request = new RestRequest(string.Format(SystemResource.UpdateDataCabinNameRequest, dataCabinId, dataCabinName), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> UpdateDataCabinDateType(int dataCabinId, string dataCabinDateTypeId)
        {
            var request = new RestRequest(string.Format(SystemResource.UpdateDataCabinDateTypeRequest, dataCabinId, dataCabinDateTypeId), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> UpdateWidgetSize(int widgetId, double widget, double height)
        {
            var request = new RestRequest(string.Format(SystemResource.UpdateWidgetSizeRequest, widgetId, widget, height), Method.POST);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }
    }
}