using Board.Models.SiteRealtime;
using Board.Models.System;
using Board.Models.Users;
using Board.Resources.ApiResources;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using RestSharp;

namespace Board.Services.SiteRealtime
{
    public class SiteRealtimeService
    {
        public static async Task<IRestResponse<List<Dictionary<string, string>>>> GetCampaigns()
        {
            var client = new RestClient(SiteRealtimeResource.SiteRealtimeDataClient);
            // find vaild sites' id of the designed user
            var request = new RestRequest(SiteRealtimeResource.SiteRealtimeUserRequest, Method.GET);
            request.AddParameter("access_token", User.UserToken.Access_Token);
            //var resp = client.Execute<List<Dictionary<string,string>>>(request);
            return await client.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);
        }

        public static async Task<List<ISystemData>> GetRealtimeData(string requestType, string dataType, string campaignId, string startDate, string endDate, string maxResults, int? interval)
        {
            SiteRealtimeRequest.Singleton.InitialRequest(SiteRealtimeResource.SiteRealtimeDataClient, User.UserToken.Access_Token);

            // input parameter
            InteractionInfo interactionInfo = new InteractionInfo
            {
                StartHour = "0:00:00",
                EndHour = "23:59:59",
                StartIndex = "0",
                MaxResults = maxResults
            };

            // input parameter
            string siteId = campaignId;
            // input parameter
            //string requestType = "Current";//[Current Daily InteractionSummary InteractionDetail InteractionEvent]
            //string dataType = "Summary";//[Summary(for current and daily and interaction) Detail Source Province City Ads Page Event Organic Keyword Hour(for interaction)]           

            
            try
            {
                List<ISystemData> responseData;
                switch (requestType)
                {
                    case "online":
                        responseData = await SiteRealtimeRequest.Singleton.GetRealtimeData(siteId, dataType, interval);
                        break;
                    case "statistics":
                        responseData = await SiteRealtimeRequest.Singleton.GetRealtimeData(siteId, dataType, interval);
                        break;
                    case "transform":
                        responseData = await SiteRealtimeRequest.Singleton.GetInteractionData(siteId, dataType, interactionInfo);
                        break;
                    default:
                        responseData = new List<ISystemData>();
                        break;
                }

                return responseData;
            }
            catch (Exception ex)
            {
                return new List<ISystemData>();
            }
        }
    }
}