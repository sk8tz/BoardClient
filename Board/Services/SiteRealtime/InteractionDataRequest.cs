using Board.Common;
using Board.Models.SiteRealtime;
using Board.Models.System;
using Board.Resources.ApiResources;

using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using RestSharp;

namespace Board.Services.SiteRealtime
{
    public class InteractionDataRequest
    {
        private static readonly InteractionDataRequest Instance = new InteractionDataRequest();

        private RestClient client;
        private string accessToken;

        public static InteractionDataRequest Singleton
        {
            get
            {
                return Instance;
            }
        }

        public void InitialRequest(string token)
        {
            accessToken = token;
            client = new RestClient(TrackRealtimeResource.TrackRealtimeDataClient);
        }

        public async Task<bool> CompleteData(List<ISystemData> detailsData)
        {
            var campaignsId = new List<string>();
            var mediasId = new List<string>();
            foreach(var detailData in detailsData)
            {
                var interactionData = detailData as IInteractionData;
                if(interactionData == null)
                {
                    break;
                }

                if(!campaignsId.Contains(interactionData.Campaign))
                {
                    campaignsId.Add(interactionData.Campaign);
                }

                if(!mediasId.Contains(interactionData.Media))
                {
                    mediasId.Add(interactionData.Media);
                }
            }

            var campaigns = await GetCampaigns(campaignsId);
            var placement = await GetPlacements(campaignsId);
            var media = await GetMedia(mediasId);

            foreach(var detailData in detailsData)
            {
                var interactionData = detailData as IInteractionData;
                if(interactionData == null)
                {
                    break;
                }

                if(interactionData.Campaign == "other")
                {
                    interactionData.CampaignName = string.Empty;
                    interactionData.PlacementName = string.Empty;
                    interactionData.MediaName = string.Empty;
                }
                else
                {
                    interactionData.CampaignName = campaigns[interactionData.Campaign];
                    var placementByCampaign = placement[interactionData.Campaign];
                    if(placementByCampaign.ContainsKey(interactionData.Placement))
                    {
                        interactionData.PlacementName = placement[interactionData.Campaign][interactionData.Placement];
                    }
                    else
                    {
                        interactionData.PlacementName = string.Empty;
                    }

                    interactionData.MediaName = media[interactionData.Media];
                }
            }

            return true;
        }

        #region get name list

        private async Task<Dictionary<string, string>> GetCampaigns(List<string> campaignsId)
        {
            var campaign = new Dictionary<string, string>();
            foreach(var campaignId in campaignsId)
            {
                if(campaignId == "other")
                {
                    campaign.Add(campaignId, "other");
                    continue;
                }

                var campaignName = await GetCampaignName(campaignId);
                campaign.Add(campaignId, campaignName);
            }

            return campaign;
        }

        private async Task<Dictionary<string, Dictionary<string, string>>> GetPlacements(List<string> campaignsId)
        {
            var dictionary = new Dictionary<string, Dictionary<string, string>>();
            foreach(var campaignId in campaignsId)
            {
                if(campaignId == "other")
                {
                    var dic = new Dictionary<string, string> { { "other", "other" } };
                    dictionary.Add(campaignId, dic);
                    continue;
                }

                var placement = await GetPlacementName(campaignId);
                dictionary.Add(campaignId, placement);
            }

            return dictionary;
        }

        private async Task<Dictionary<string, string>> GetMedia(List<string> mediasId)
        {
            var media = new Dictionary<string, string>();
            foreach(var mediaId in mediasId)
            {
                if(mediaId == "other")
                {
                    media.Add(mediaId, "other");
                    continue;
                }

                var mediaName = await GetMediaName(mediaId);
                media.Add(mediaId, mediaName);
            }

            return media;
        }

        #endregion

        private async Task<string> GetCampaignName(string campaignId)
        {
            var resource = string.Format(SiteRealtimeResource.TrackCampaignDataRequest, campaignId);
            var request = new RestRequest(resource, Method.GET);
            if(accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            var response = await client.ExecuteGetTaskAsync<Dictionary<string, string>>(request);

            if(!IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var compaignName = response.Data["name"];
            return compaignName;
        }

        private async Task<Dictionary<string, string>> GetPlacementName(string campaignId)
        {
            var resource = string.Format(TrackRealtimeResource.TrackPlacementDataRequest, campaignId);
            var request = new RestRequest(resource, Method.GET);
            if(accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            var response = await client.ExecuteGetTaskAsync<List<Dictionary<string, string>>>(request);
            var placements = new Dictionary<string, string>();
            if(!IsStatusCodeOK(response.StatusCode))
            {
                //placements.Add(string.Empty,string.Empty);
                return placements;
            }

            foreach(var item in response.Data)
            {
                var id = item["id"];
                var name = item["name"];
                placements.Add(id, name);
            }

            return placements;
        }

        private async Task<string> GetMediaName(string mediaId)
        {
            var mediaClient = new RestClient("http://lib.admasterapi.com/");
            var resource = string.Format(TrackRealtimeResource.TrackMediaDataRequest, mediaId);
            var request = new RestRequest(resource, Method.GET);
            if(this.accessToken != null)
            {
                //request.AddParameter("access_token", this.accessToken);
            }

            var response1 = mediaClient.Execute<Dictionary<string, string>>(request);
            var response = await mediaClient.ExecuteGetTaskAsync<Dictionary<string, string>>(request);

            if(!this.IsStatusCodeOK(response.StatusCode))
            {
                return string.Empty;
            }

            var mediaName = response.Data["name_cn"];
            return mediaName;
        }

        private bool IsStatusCodeOK(HttpStatusCode statusCode)
        {
            if(statusCode == HttpStatusCode.OK)
            {
                return true;
            }

            var status = "Http statusCode:" + statusCode;
            LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, status, null);

            return false;
        }
    }
}
