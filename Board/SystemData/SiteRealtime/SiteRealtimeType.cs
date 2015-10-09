using System;
using System.Collections.Generic;
using System.Reflection;
using Board.Models.SiteRealtime;
using Board.Models.System;

using Newtonsoft.Json;

namespace Board.SystemData.SiteRealtime
{
    public static class SiteRealtimeType
    {
        public static Type GetSiteRealtimeDataType(string requestType, string dataType)
        {
            Type type;
            if(requestType == null)
            {
                return null;
            }

            switch(requestType)
            {
                case "online":
                case "statistics":
                    type = ObtainRealtimeDataType(dataType);
                    break;
                case "transform":
                    type = ObtainInteractionDataType(dataType);
                    break;
                default:
                    return null;
            }

            return type;
        }

        private static Type ObtainRealtimeDataType(string dataType)
        {
            Type type = null;
            switch(dataType)
            {
                case "summary":
                    type = typeof(SiteRealtimeSummaryData);
                    break;
                case "detail":
                    type = typeof(SiteRealtimeDetailData);
                    break;
                case "source":
                    type = typeof(SiteRealtimeSourceData);
                    break;
                case "province":
                    type = typeof(SiteRealtimeProvinceData);
                    break;
                case "city":
                    type = typeof(SiteRealtimeCityData);
                    break;
                case "ads":
                    type = typeof(SiteRealtimeAdsData);
                    break;
                case "page":
                    type = typeof(SiteRealtimePageData);
                    break;
                case "event":
                    type = typeof(SiteRealtimeEventData);
                    break;
                case "organic":
                    type = typeof(SiteRealtimeOrganicData);
                    break;
                case "keyword":
                    type = typeof(SiteRealtimeKeywordData);
                    break;
            }

            return type;
        }

        private static Type ObtainInteractionDataType(string dataType)
        {
            Type type = null;
            switch(dataType)
            {
                case "summary":
                    type = typeof(SiteRealtimeInteractionSummaryData);
                    break;
                case "hour":
                    type = typeof(SiteRealtimeInteractionHourData);
                    break;
                case "event":
                    type = typeof(SiteRealtimeInteractionEventData);
                    break;
                case "detail":
                    type = typeof(SiteRealtimeInteractionDetailData);
                    break;
            }

            return type;
        }

        public static PropertyInfo[] GetPropertiesByType(Type dataType)
        {
            return dataType != null ? dataType.GetProperties() : new PropertyInfo[0];
        }

        public static dynamic GetSiteRealtimeData(string dataType, string data, string requestType)
        {
            if(dataType == null)
            {
                return new List<ISystemData>();
            }

            dynamic systemDataList;
            string type = dataType.ToLower();
            switch (requestType)
            {
                case "online":
                case "statistics":
                    systemDataList = ObtainRealtimeData(type, data);
                    break;
                case "transform":
                    systemDataList = ObtainInteractionData(type, data);
                    break;
                default:
                    return null;
            }

            return systemDataList;
        }

        private static dynamic ObtainRealtimeData(string dataType, string data)
        {
            if (dataType == null)
            {
                return new List<ISystemData>();
            }

            dynamic systemDataList;
            string type = dataType.ToLower();

            switch (type)
            {
                case "summary":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeSummaryData>>(data);
                    break;
                case "detail":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeDetailData>>(data);
                    break;
                case "source":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeSourceData>>(data);
                    break;
                case "province":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeProvinceData>>(data);
                    break;
                case "city":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeCityData>>(data);
                    break;
                case "ads":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeAdsData>>(data);
                    break;
                case "page":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimePageData>>(data);
                    break;
                case "event":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeEventData>>(data);
                    break;
                case "organic":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeOrganicData>>(data);
                    break;
                case "keyword":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeKeywordData>>(data);
                    break;
                default:
                    return new List<ISystemData>();
            }

            return systemDataList;
        }

        private static dynamic ObtainInteractionData(string dataType, string data)
        {
            if (dataType == null)
            {
                return new List<ISystemData>();
            }

            dynamic systemDataList;
            string type = dataType.ToLower();

            switch (type)
            {
                case "summary":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeInteractionSummaryData>>(data);
                    break;
                case "hour":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeInteractionHourData>>(data);
                    break;
                case "event":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeInteractionEventData>>(data);
                    break;
                case "detail":
                    systemDataList = JsonConvert.DeserializeObject<List<SiteRealtimeInteractionDetailData>>(data);
                    break;
                default:
                    return new List<ISystemData>();
            }

            return systemDataList;
        }
    }
}