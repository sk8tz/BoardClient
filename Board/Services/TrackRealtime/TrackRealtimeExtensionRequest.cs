using Board.Common;
using Board.Resources.ApiResources;

using RestSharp;

using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Board.Services.TrackRealtime
{
    class TrackRealtimeExtensionRequest
    {
        private static readonly TrackRealtimeExtensionRequest Instance = new TrackRealtimeExtensionRequest();

        private RestClient client;
        private string accessToken;

        public static TrackRealtimeExtensionRequest Singleton
        {
            get
            {
                return Instance;
            }
        }

        public void InitialRequest(string token)
        {
            this.accessToken = token;
            this.client = new RestClient(TrackRealtimeResource.TrackRealtimeDataClient);
        }

        public async Task<Dictionary<string,string>> GetCreativeNames(string campaignId)
        {
            var resource = string.Format(TrackRealtimeResource.TrackCreativeDataRequest, campaignId);
            var request = new RestRequest(resource, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            var response = await this.client.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);

            if (!this.IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var creatives = new Dictionary<string, string>();
            foreach (var item in response.Data)
            {
                var id = item["id"];
                var name = item["name"];
                creatives.Add(id, name);
            }

            return creatives;
        }


        public async Task<Dictionary<string, string>> GetPlacementNames(string campaignId)
        {
            var resource = string.Format(TrackRealtimeResource.TrackPlacementDataRequest, campaignId);
            var request = new RestRequest(resource, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            var response = await this.client.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);

            if (!this.IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var placements = new Dictionary<string, string>();
            foreach (var item in response.Data)
            {
                var id = item["id"];
                var name = item["name"];
                placements.Add(id, name);
            }

            return placements;
        }

        public async Task<string> GetMediaName(int mediaId)
        {
            var mediaClient = new RestClient("http://lib.admasterapi.com/");
            var resource = string.Format(TrackRealtimeResource.TrackMediaDataRequest, mediaId);
            var request = new RestRequest(resource, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            //var response1 = mediaClient.Execute(request);
            var response = await mediaClient.ExecuteGetTaskAsync<Dictionary<string, string>>(request);

            if (!this.IsStatusCodeOK(response.StatusCode))
            {
                return string.Empty;
            }

            var mediaName = response.Data["name_cn"];
            return mediaName;
        }

        private bool IsStatusCodeOK(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.OK)
            {
                return true;
            }

            var status = "Http statusCode:" + statusCode;
            LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, status, null);

            return false;
        }
    }
}
