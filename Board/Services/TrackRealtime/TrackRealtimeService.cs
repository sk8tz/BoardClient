using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Board.Models.TrackRealtime;
using Board.Models.Users;
using Board.Resources.ApiResources;

using RestSharp;

namespace Board.Services.TrackRealtime
{
    public class TrackRealtimeService
    {
        public static async Task<Dictionary<string, string>> GetCampaigns()
        {
            TrackRealtimeCampaignRequest.Singleton.InitialRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await TrackRealtimeCampaignRequest.Singleton.GetCampaigns();
            return campaigns;
        }

        public static async Task<List<TrackRealtimeData>> GetRealtimeData(string campaignId, string startDate, string endDate, string dimensions, string metrics, string maxResults, string dataOrderBy)
        {
            var client = new RestClient(TrackRealtimeResource.TrackRealtimeDataClient);
            var convertedDimension = FormatDimensions(dimensions);
            var request = new RestRequest(string.Format(TrackRealtimeResource.TrackRealtimeDataRequest, campaignId, startDate, endDate, convertedDimension, metrics, maxResults, dataOrderBy, User.UserToken.Access_Token), Method.GET);

            var response = await client.ExecuteGetTaskAsync<List<TrackRealtimeData>>(request);
            if(response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var trackRealtimeData = response.Data;

            TrackRealtimeCampaignRequest.Singleton.InitialRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await TrackRealtimeCampaignRequest.Singleton.GetCampaigns();

            TrackRealtimeExtensionRequest.Singleton.InitialRequest(User.UserToken.Access_Token);
            var placement = await TrackRealtimeExtensionRequest.Singleton.GetPlacementNames(campaignId);
            var creative = await TrackRealtimeExtensionRequest.Singleton.GetCreativeNames(campaignId);
            foreach(var item in trackRealtimeData)
            {
                item.Campaigns = campaignId;
                item.CampaignsName = campaigns[campaignId];
                if(item.Media != 0)
                {
                    item.MediaName = await TrackRealtimeExtensionRequest.Singleton.GetMediaName(item.Media);
                }

                if(item.Placement != 0)
                {
                    string placementId = item.Placement.ToString();
                    item.PlacementName = placement[placementId];
                }

                if(item.Creative != 0)
                {
                    string creativeId = item.Creative.ToString();
                    item.CreativeName = creative[creativeId];
                }
                else
                {
                    item.CreativeName = "默认创意";
                }
            }

            return trackRealtimeData;
        }
        private static string FormatDimensions(string originalDimensions)
        {
            if(string.IsNullOrEmpty(originalDimensions))
            {
                return string.Empty;
            }

            string targetDimensions = originalDimensions;

            if(targetDimensions.Contains("campaignsName"))
            {
                targetDimensions = targetDimensions.Replace("campaignsName", string.Empty);
            }

            if(targetDimensions.Contains("mediaName"))
            {
                targetDimensions = targetDimensions.Replace("mediaName", "media");
            }

            if(targetDimensions.Contains("placementName"))
            {
                targetDimensions = targetDimensions.Replace("placementName", "placement");
            }

            if(targetDimensions.Contains("creativeName"))
            {
                targetDimensions = targetDimensions.Replace("creativeName", "creative");
            }

            return targetDimensions;
        }
    }
}
