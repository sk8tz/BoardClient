using System.Collections.Generic;
using System.Threading.Tasks;

using Board.Models.System;
using Board.Models.Users;
using Board.Resources.ApiResources;
using Newtonsoft.Json;
using RestSharp;

namespace Board.Services.Benchmark
{
    public static class BenchmarkService
    {
        private static readonly RestClient Client;
        static BenchmarkService()
        {
            Client = new RestClient(SystemResource.BoardServiceClient);
        }

        public static async Task<IRestResponse<List<ResultValue>>> GetBenchmarkBaseDatas()
        {
            var request = new RestRequest(BenchmarkResource.GetBenchmarkBaseDataRequest, Method.POST);
            request.AddUrlSegment("userId", User.UserInfo.Id.ToString());

            return await Client.ExecutePostTaskAsync<List<ResultValue>>(request);
        }

        public static async Task<IRestResponse<ResultValue>> GetCategorys1Datas(string clientId, string benchmarkTypeId)
        {
            var request = new RestRequest(BenchmarkResource.GetBenchmarkTypeDataRequest, Method.POST);
            request.AddUrlSegment("clientId", clientId);
            request.AddUrlSegment("benchmarkTypeId", benchmarkTypeId);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> GetCategorys2Datas(string categorys1)
        {
            var request = new RestRequest(BenchmarkResource.GetCategory2DataRequest, Method.POST);
            request.AddUrlSegment("categorys1", categorys1);

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }

        public static async Task<IRestResponse<ResultValue>> GetBenchmarkDatas(string timeType, string brandType, string category1, string category2, string dimensions, string metrics)
        {
            var request = new RestRequest(BenchmarkResource.GetBenchmarkDataRequest, Method.POST);
            
            var dic = new Dictionary<string, string>();
            dic.Add("TimeType", timeType);
            dic.Add("BrandType", brandType);
            dic.Add("Category1", category1 == string.Empty ? "null" : category1);
            dic.Add("Category2", category2 == string.Empty ? "null" : category2);
            dic.Add("Dimensions", dimensions == string.Empty ? "null" : dimensions);
            dic.Add("Metrics", metrics);

            request.AddJsonBody(JsonConvert.SerializeObject(dic));

            return await Client.ExecutePostTaskAsync<ResultValue>(request);
        }
    }
}
