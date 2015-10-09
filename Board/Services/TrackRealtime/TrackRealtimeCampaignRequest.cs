namespace Board.Services.TrackRealtime
{
    using Board.Common;

    using global::System.Collections.Generic;
    using global::System.Net;
    using global::System.Reflection;
    using global::System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    using RestSharp;

    public class TrackRealtimeCampaignRequest
    {
        private static readonly TrackRealtimeCampaignRequest Instance = new TrackRealtimeCampaignRequest();

        private RestClient client;
        private string accessToken;

        public static TrackRealtimeCampaignRequest Singleton
        {
            get
            {
                return Instance;
            }
        }

        public void InitialRequest(string url, string token)
        {
            this.accessToken = token;
            this.client = new RestClient(url);
        }

        public async Task<Dictionary<string, string>> GetCampaigns()
        {
            var campaigns = new Dictionary<string, string>();
            var networksId = await this.GetNetworksId();
            foreach (var networkId in networksId)
            {
                var dictionary = await this.GetCampaignsByNetwork(networkId);
                if (dictionary == null)
                {
                    continue;
                }

                foreach (var item in dictionary)
                {
                    campaigns.Add(item.Key, item.Value);
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, item.Key + "," + item.Value, null);
                }
            }

            return campaigns;
        }

        private async Task<List<string>> GetNetworksId()
        {
            var networkIds = new List<string>();
            var request = new RestRequest("api_v2/session", Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            var response = await this.client.ExecuteGetTaskAsync<Dictionary<string, string>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var networks = response.Data["networks"];
            var items = JObject.Parse(networks);
            foreach (var item in items)
            {
                networkIds.Add(item.Key);
            }

            return networkIds;
        }

        private async Task<Dictionary<string, string>> GetCampaignsByNetwork(string networkId)
        {
            var campaignsId = new Dictionary<string, string>();
            string requestUri = string.Format("api_v2/networks/{0}/campaigns", networkId);
            var request = new RestRequest(requestUri, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            request.AddParameter("maxResults", 2000);

            var response = await this.client.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return campaignsId;
            }

            foreach (var item in response.Data)
            {
                var campaignId = item["id"];
                var campaignName = item["name"];
                campaignsId.Add(campaignId, campaignName);
            }

            return campaignsId;
        }
    }
}
