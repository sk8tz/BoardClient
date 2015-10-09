using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Models.Users;
using Board.Resources.ApiResources;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Board.Services.TrackAnalysisReport
{
    public static class TrackService
    {
        public static async Task<Dictionary<string, string>> GetCampaigns()
        {
            TrackAnalysisCompaignRequest.Singleton.InitialRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await TrackAnalysisCompaignRequest.Singleton.GetCampaigns();
            return campaigns;
        }

        public static async Task<List<RegionTypeModel>> GetCityInfo(string campaignId)
        {
            var request = new TrackAnalysisExtensionRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await request.GetCityInfo(campaignId);
            return campaigns;
        }

        public static async Task<List<SiteInfo>> GetSiteInfo(string campaignId)
        {
            var request = new TrackAnalysisExtensionRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await request.GetSiteInfo(campaignId);
            return campaigns;
        }

        public static async Task<List<TrackAnalysisData>> GetTrackAnalysisData(string requestType, string campaignId, string dimensions, string metrics, string startDate, string endDate, string maxResults)
        {
            TrackAnalysisRequest.Singleton.InitialRequest(TrackAnalysisReportResource.TrackAnalysisDataServer, User.UserToken.Access_Token);
            var requiredStartDate = startDate.Split(' ')[0];
            var requiredEndDate = endDate.Split(' ')[0];
            var trackAnalysisInfo = new TrackAnalysisInfo
            {
                StartDate = requiredStartDate,
                EndDate = requiredEndDate,
                Dimensions = dimensions,
                Metrics = metrics,
                MaxResults = maxResults
            };

            var data = await TrackAnalysisRequest.Singleton.GetTrackAnalysisData(requestType, campaignId, trackAnalysisInfo);
            if (dimensions.IndexOf("CampaignsName", StringComparison.OrdinalIgnoreCase) == -1)
            {
                return data;
            }

            TrackAnalysisCompaignRequest.Singleton.InitialRequest(TrackAnalysisReportResource.TrackAnalysisDataClient, User.UserToken.Access_Token);
            var campaigns = await TrackAnalysisCompaignRequest.Singleton.GetCampaigns();

            var campaignName = campaigns[campaignId];
            foreach (var item in data)
            {
                item.Campaigns = campaignId;
                item.CampaignsName = campaignName;
            }

            
            return data;
        }
    }
}