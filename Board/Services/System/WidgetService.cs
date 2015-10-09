using System.Collections.Generic;
using System.Threading.Tasks;

using Board.Models.System;
using Board.Models.Users;
using Board.Resources.ApiResources;

using RestSharp;

namespace Board.Services.System
{
    public class WidgetService
    {
        private static readonly RestClient Client;

        static WidgetService()
        {
            Client = new RestClient(SystemResource.BoardServiceClient);
        }

        public static async Task<IRestResponse<List<ResultValue>>> GetBaseData()
        {
            var request = new RestRequest(string.Format(SystemResource.GetBaseDataRequest, User.UserInfo.Id), Method.POST);

            return await Client.ExecutePostTaskAsync<List<ResultValue>>(request);
        }

        public static async Task<IRestResponse<List<ResultValue>>> GetDataBySystemType(int systemTypeId)
        {
            var request = new RestRequest(string.Format(SystemResource.GetDataBySystemTypeRequest, systemTypeId), Method.POST);

            return await Client.ExecutePostTaskAsync<List<ResultValue>>(request);
        }

        public static async Task<IRestResponse<ResultValue>> GetSystemList()
        {
            var request = new RestRequest(SystemResource.GetSystemListRequest, Method.POST);
            request.AddUrlSegment("userId", User.UserInfo.Id.ToString());

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }
    }
}
