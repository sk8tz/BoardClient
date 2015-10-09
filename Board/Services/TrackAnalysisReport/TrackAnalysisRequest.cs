using Board.Common;
using Board.Models.TrackAnalysisReport;
using Board.Resources.ApiResources;

using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace Board.Services.TrackAnalysisReport
{
    public class TrackAnalysisRequest
    {
        private static readonly TrackAnalysisRequest Instance = new TrackAnalysisRequest();

        private RestClient client;

        private string accessToken;

        public static TrackAnalysisRequest Singleton
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

        #region send unique request

        public async Task<List<TrackAnalysisData>> GetTrackAnalysisData(string requestType, string campaignId, TrackAnalysisInfo info)
        {
            string requestUri = string.Format(TrackAnalysisReportResource.TrackAnalysisDataRequest, campaignId, requestType);
            var request = this.SetRequest(requestUri, info);

            var response = await this.client.ExecuteGetTaskAsync(request);
            if (!this.IsStatusCodeOK(response.StatusCode))
            {
                return new List<TrackAnalysisData>();
            }

            var data = JsonConvert.DeserializeObject<List<TrackAnalysisData>>(response.Content);

            return data;
        }

        #endregion

        private RestRequest SetRequest(string requestUri, TrackAnalysisInfo info)
        {
            var request = new RestRequest(requestUri, Method.GET);
            if (this.accessToken != null)
            {
                request.AddParameter("access_token", this.accessToken);
            }

            if (info == null)
            {
                return null;
            }

            var dimensions = this.FormatDimensions(info.Dimensions);

            var metrics = info.Metrics;
            if (requestUri.Contains("freqs"))
            {
                metrics = this.FormatFreqsMetrics(info.Metrics);
            }

            request.AddParameter("startDate", info.StartDate);
            request.AddParameter("endDate", info.EndDate);
            request.AddParameter("dimensions", dimensions);
            request.AddParameter("metrics", metrics);
            request.AddParameter("maxResults", info.MaxResults);
            request.AddParameter("startIndex", "0");

            if (info.GeoId != null)
            {
                //for igrp
                request.AddParameter("geo", info.GeoId);
            }

            if (info.SiteId != null)
            {
                //for integrate
                request.AddParameter("siteId", info.SiteId);
            }
            
            return request;
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


        private string FormatDimensions(string originalDimensions)
        {
            if (string.IsNullOrEmpty(originalDimensions))
            {
                return string.Empty;
            }

            string targetDimensions = originalDimensions;

            if (targetDimensions.Contains("campaignsName"))
            {
                targetDimensions = targetDimensions.Replace("campaignsName", string.Empty);
            }

            if (targetDimensions.Contains("mediaName"))
            {
                targetDimensions = targetDimensions.Replace("mediaName", "media");
            }

            if (targetDimensions.Contains("cityName"))
            {
                targetDimensions = targetDimensions.Replace("cityName", "city");
            }

            if (targetDimensions.Contains("placementName"))
            {
                targetDimensions = targetDimensions.Replace("placementName", "placement");
            }

            if (targetDimensions.Contains("provinceName"))
            {
                targetDimensions = targetDimensions.Replace("provinceName", "province");
            }

            if (targetDimensions.Contains("creativeName"))
            {
                targetDimensions = targetDimensions.Replace("creativeName", "creative");
            }

            return targetDimensions;
        }

        private string FormatFreqsMetrics(string originalMetrics)
        {
            string targetMetrics = string.Empty;
            var metrixList = originalMetrics.Split(',');

            for (int i = 0; i < metrixList.Length; i++)
            {
                if (metrixList[i].Equals("imp"))
                {
                    metrixList[i] = "imp1,imp2,imp3,imp4,imp5,imp6a";
                    continue;
                }

                if (metrixList[i].Equals("uimp"))
                {
                    metrixList[i] = "uimp1,uimp2,uimp3,uimp4,uimp5,uimp6a";
                    continue;
                }

                if (metrixList[i].Equals("clk"))
                {
                    metrixList[i] = "clk1,clk2,clk3,clk4,clk5,clk6a";
                    continue;
                }

                if (metrixList[i].Equals("uclk"))
                {
                    metrixList[i] = "uclk1,uclk2,uclk3,uclk4,uclk5,uclk6a";
                }
            }

            foreach (var metrix in metrixList)
            {
                targetMetrics = targetMetrics + metrix + ",";
            }

            targetMetrics = targetMetrics.Trim(',');

            return targetMetrics;
        }
    }
}
