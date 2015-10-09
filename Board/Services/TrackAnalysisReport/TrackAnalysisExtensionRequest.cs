using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Resources.ApiResources;

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace Board.Services.TrackAnalysisReport
{
    public class TrackAnalysisExtensionRequest
    {
        private readonly RestClient client;
        private readonly string accessToken;

        public TrackAnalysisExtensionRequest(string url, string token)
        {
            this.accessToken = token;
            this.client = new RestClient(url);
        }
        
        public async Task<List<RegionTypeModel>> GetCityInfo(string campaignId)
        {
            var cityInfo = await this.GetExtensionInfo(campaignId, "targetGeos");
            var cities = await this.GetCityName(cityInfo);
            return cities;
        }

        public async Task<List<SiteInfo>> GetSiteInfo(string campaignId)
        {
            var siteInfo = await this.GetExtensionInfo(campaignId, "siteMasterId");
            var siteList = await this.GetSiteName(siteInfo);
            return siteList;
        }

        private async Task<string> GetExtensionInfo(string campaignId, string type)
        {
            string requestUri = string.Format("api_v2/campaigns/{0}", campaignId);
            var request = new RestRequest(requestUri, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            var response = await this.client.ExecuteGetTaskAsync<Dictionary<string, string>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.Data[type];
        }

        private async Task<List<RegionTypeModel>> GetCityName(string CitiesId)
        {
            var boardClient = new RestClient(SystemResource.BoardServiceClient);
            string requestUri = string.Format("BoardService/GetTrackAnalysisReportRegionList/{0}", CitiesId);
            var request = new RestRequest(requestUri, Method.POST);
            if (this.accessToken != null)
            {
                //request.AddParameter("access_token", this.accessToken);
            }

            var response = await boardClient.ExecutePostTaskAsync<Dictionary<string, string>>(request);
            //var response = boardClient.Execute<Dictionary<string, string>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return new List<RegionTypeModel>();
            }

            var data = response.Data["Value"];
            var cities = JsonConvert.DeserializeObject<List<RegionTypeModel>>(data);
            return cities;
        }

        private async Task<List<SiteInfo>> GetSiteName(string sitesId)
        {
            var dictionary = new List<SiteInfo>();
            var siteMap = await this.GetSiteMap();
            var sites = sitesId.Split(',');
            foreach (var siteId in sites)
            {
                string name;
                var isExist = siteMap.TryGetValue(siteId, out name);
                if (!isExist)
                {
                    continue;
                }
                var siteInfo = new SiteInfo
                {
                    Id = siteId,
                    Name = name
                };

                dictionary.Add(siteInfo);
            }

            return dictionary;
        }

        private async Task<Dictionary<string, string>> GetSiteMap()
        {
            var siteClient = new RestClient(TrackAnalysisReportResource.TrackAnalysisDataClient);
            //var sites = new List<SiteInfo>();
            var request = new RestRequest("site_api_v1/my/sites", Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            var response = await siteClient.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var dictionary = new Dictionary<string, string>();
            foreach (var item in response.Data)
            {
                var id = item["id"];
                var name = item["name"];
                dictionary.Add(id, name);
            }

            return dictionary;
        }
    }
}