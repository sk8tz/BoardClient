using Board.Common;
using Board.Models.SiteRealtime;
using Board.Models.System;
using Board.Resources.ApiResources;

using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace Board.Services.SiteRealtime
{
    public class SiteRealtimeRequest
    {
        private static readonly SiteRealtimeRequest Instance = new SiteRealtimeRequest();

        private RestClient client;
        private string accessToken;

        public static SiteRealtimeRequest Singleton
        {
            get
            {
                return Instance;
            }
        }

        public void InitialRequest(string url, string token)
        {
            accessToken = token;
            client = new RestClient(url);
            InteractionDataRequest.Singleton.InitialRequest(token);
        }

        #region send request

        public async Task<List<ISystemData>> GetRealtimeData(string siteId, string dataType, int? interval)
        {
            if (interval == null)
            {
                return new List<ISystemData>();
            }

            var resource = string.Format(SiteRealtimeResource.SiteRealtimeDataRequest, siteId);
            var request = new RestRequest(resource, Method.GET);
            if (accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            request.AddParameter("Interval", interval);
            var response = await client.ExecuteGetTaskAsync<Dictionary<string, string>>(request);

            if (!IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var systemData = ConvertData(dataType, response.Data);
            //return response.Data;
            return systemData;
        }

       public async Task<List<ISystemData>> GetInteractionData(string siteId, string dataType, InteractionInfo interactionInfo)
       {
           var systemData = new List<ISystemData>();
           switch (dataType)
           {
               case "summary":
                   systemData = await GetInteractionSummaryData(siteId, dataType);
                   break;
               case "hour":
                   systemData = await GetInteractionSummaryData(siteId, dataType);
                   break;
               case "detail":
                   systemData = await GetInteractionDetailData(siteId, interactionInfo);
                   break;
               case "event":
                   systemData = await GetInteractionEventData(siteId, interactionInfo);
                   break;
           }

           return systemData;
       }

        private async Task<List<ISystemData>> GetInteractionSummaryData(string siteId, string dataType)
        {
            var resource = string.Format(SiteRealtimeResource.SiteRealtimeInteractionSummaryRequest, siteId);
            var request = new RestRequest(resource, Method.GET);

            if (accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            var response = await client.ExecuteGetTaskAsync<Dictionary<string, string >>(request);

            if (!IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var systemData = ConvertInteractionData(dataType, response.Data);

            //return response.Data;
            return systemData;
        }

        private async Task<List<ISystemData>> GetInteractionDetailData(string siteId, InteractionInfo interactionInfo)
        {
            var resource = string.Format(SiteRealtimeResource.SiteRealtimeInteractionDetailRequest, siteId);
            var request = new RestRequest(resource, Method.GET);

            if (accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            if (interactionInfo != null)
            {
                request.AddParameter("start-hour", interactionInfo.StartHour);
                request.AddParameter("end-hour", interactionInfo.EndHour);
                request.AddParameter("start-index", interactionInfo.StartIndex);
                request.AddParameter("max-results", interactionInfo.MaxResults);
            }

            var response = await client.ExecuteGetTaskAsync<List<string>>(request);

            if (!IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var systemData = GetInteractionDetailData(response.Data[0]);
            await InteractionDataRequest.Singleton.CompleteData(systemData);
            
            //return response.Data;
            return systemData;
        }

        private async Task<List<ISystemData>> GetInteractionEventData(string siteId, InteractionInfo interactionInfo)
        {
            var resource = string.Format(SiteRealtimeResource.SiteRealtimeInteractionEventRequest, siteId);
            var request = new RestRequest(resource, Method.GET);
            if (accessToken != null)
            {
                request.AddParameter("access_token", accessToken);
            }

            if (interactionInfo != null)
            {
                request.AddParameter("start-hour", interactionInfo.StartHour);
                request.AddParameter("end-hour", interactionInfo.EndHour);
                request.AddParameter("start-index", interactionInfo.StartIndex);
                request.AddParameter("max-results", interactionInfo.MaxResults);
            }

            var response = await client.ExecuteGetTaskAsync<List<string>>(request);
            

            if (!IsStatusCodeOK(response.StatusCode))
            {
                return null;
            }

            var systemData = GetInteractionEventData(response.Data[0]);
            await InteractionDataRequest.Singleton.CompleteData(systemData);
            //return response.Data;
            return systemData;
        }

        #endregion

        #region process data

        private List<ISystemData> ConvertData(string dataType, Dictionary<string, string> dictionary)
        {
            var systemDataList = new List<ISystemData>();
            var type = dataType.ToLower();
            var originalData = dictionary[type];
            var data = JsonConvert.DeserializeObject<HeadBodyInfo>(originalData);
            switch (dataType)
            {
                case "summary":
                    systemDataList = getSummaryData(data.Body);
                    break;
                case "detail":
                    systemDataList = getDetailData(data.Body);
                    break;
                case "source":
                    systemDataList = getSourceData(data.Body);
                    break;
                case "province":
                    systemDataList = getProvinceData(data.Body);
                    break;
                case "city":
                    systemDataList = getCityData(data.Body);
                    break;
                case "ads":
                    systemDataList = getAdsData(data.Body);
                    break;
                case "page":
                    systemDataList = getPageData(data.Body);
                    break;
                case "event":
                    systemDataList = getEventData(data.Body);
                    break;
                case "organic":
                    systemDataList = getOrganicData(data.Body);
                    break;
                case "keyword":
                    systemDataList = getKeywordData(data.Body);
                    break;
            }

            return systemDataList;
        }

        private List<ISystemData> ConvertInteractionData(string dataType, Dictionary<string, string> dictionary)
       {
           var systemDataList = new List<ISystemData>();
            var type = dataType.ToLower();
            var data = JsonConvert.DeserializeObject<HeadBodyInfo>(dictionary[type]);
           switch (dataType)
           {
               case "summary":
                   systemDataList = getInteractionSummaryData(data.Body);
                   break;
               case "hour":
                   systemDataList = getInteractionHourData(data.Body);
                   break;
           }

           return systemDataList;
       }

       private List<ISystemData> getInteractionHourData(List<Dictionary<string, string>> body)
       {
           var systemData = new List<ISystemData>();
           foreach (var item in body)
           {
               var summaryData = new SiteRealtimeInteractionHourData
               {
                   TotalEvents = item["mt:totalEvents"],
                   Hour = item["hour"]
               };
               systemData.Add(summaryData);
           }

           return systemData;
       }

       private List<ISystemData> getInteractionSummaryData(List<Dictionary<string, string>> body)
       {
           var systemData = new List<ISystemData>();
           foreach (var item in body)
           {
               var summaryData = new SiteRealtimeInteractionSummaryData
               {
                   Total = item["total"]
               };
               systemData.Add(summaryData);
           }

           return systemData;
       }

       #endregion

        #region current & daily
        private List<ISystemData> getSummaryData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeSummaryData
                {
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getDetailData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeDetailData
                {
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"],
                    Time = item["time"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getSourceData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeSourceData
                {
                    SourceType = item["dm:sourceType"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getProvinceData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeProvinceData
                {
                    Province = item["dm:province"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getCityData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeCityData
                {
                    City = item["dm:city"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getAdsData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeAdsData
                {
                    Placement = item["dm:placement"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
                
            }

            return systemData;
        }

        private List<ISystemData> getPageData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimePageData
                {
                    PageUrl = item["dm:pageURL"],
                    PageTitle = item["dm:pageTitle"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getEventData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeEventData
                {
                    Category = item["dm:category"],
                    Action = item["dm:action"],
                    TotalEvents = item["mt:totalEvents"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getOrganicData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeOrganicData
                {
                    Domain = item["domain"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        private List<ISystemData> getKeywordData(List<Dictionary<string, string>> body)
        {
            var systemData = new List<ISystemData>();
            foreach (var item in body)
            {
                var summaryData = new SiteRealtimeKeywordData
                {
                    Keyword = item["keyword"],
                    PageViews = item["mt:pageviews"],
                    Visitors = item["mt:visitors"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }

        #endregion

        #region interaction

       private List<ISystemData> GetInteractionDetailData(string content)
       {
           var data = JsonConvert.DeserializeObject<HeadBodyInfo>(content);
           var systemData = new List<ISystemData>();
           foreach (var item in data.Body)
           {
               var summaryData = new SiteRealtimeInteractionDetailData
               {
                   Campaign = item["campaign"],
                   Media = item["media"],
                   Placement = item["placement"],
                   Keyword = item["keyword"],
                   Category = item["category"],
                   Action = item["action"],
                   Label = item["label"],
                   Time = item["time"]
               };
               systemData.Add(summaryData);
           }

           return systemData;
       }

        private List<ISystemData> GetInteractionEventData(string content)
        {
            var data = JsonConvert.DeserializeObject<HeadBodyInfo>(content);
            var systemData = new List<ISystemData>();
            foreach (var item in data.Body)
            {
                var summaryData = new SiteRealtimeInteractionEventData
                {
                    Campaign = item["campaign"],
                    Media = item["media"],
                    Placement = item["placement"],
                    Keyword = item["keyword"],
                    TotalValue = item["totalvalue"],
                    Total = item["total"],
                    Category = item["category"],
                    Action = item["action"]
                };
                systemData.Add(summaryData);
            }

            return systemData;
        }  

        #endregion


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
