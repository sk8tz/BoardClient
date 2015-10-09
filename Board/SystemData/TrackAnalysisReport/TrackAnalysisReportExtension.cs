using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;

using Board.Common;
using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Services.TrackAnalysisReport;

namespace Board.SystemData.TrackAnalysisReport
{
    public static class TrackAnalysisReportExtension
    {
        public async static Task<ObservableCollection<RegionTypeModel>> GetCityList(string campaignId)
        {
            try
            {
                var cities = new List<List<RegionTypeModel>>();
                var campaigns = campaignId.Split(',');
                foreach (var campaign in campaigns)
                {
                    var cityInfo = await TrackService.GetCityInfo(campaign);
                    if (cityInfo == null)
                    {
                        continue;
                    }

                    cities.Add(cityInfo);
                }

                var list = EraseDuplacatedCity(cities);
                return new ObservableCollection<RegionTypeModel>(list);
            }
            catch(Exception ex)
            {
                ShowMessage.Show("获取城市列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetTableSource", ex);
            }

            return new ObservableCollection<RegionTypeModel>();

        }

        public async static Task<ObservableCollection<SiteInfo>> GetSiteList(string campaignId)
        {
            try
            {
                var sites = new List<List<SiteInfo>>();
                var campaigns = campaignId.Split(',');
                foreach (var campaign in campaigns)
                {
                    var siteInfo = await TrackService.GetSiteInfo(campaign);
                    if (siteInfo == null)
                    {
                        continue;
                    }

                    sites.Add(siteInfo);
                }

                var list = EraseDuplacatedSite(sites);
                return new ObservableCollection<SiteInfo>(list);
            }
            catch(Exception ex)
            {
                ShowMessage.Show("获取站点列表出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to GetTableSource", ex);
            }

            return new ObservableCollection<SiteInfo>();
        }

        private static List<RegionTypeModel> EraseDuplacatedCity(List<List<RegionTypeModel>> citiesList)
        {
            var list = new  List<RegionTypeModel>();
            foreach (var cities in citiesList)
            {
                foreach (var city in cities)
                {
                    if (list.Contains(city))
                    {
                        continue;
                    }

                    list.Add(city);
                }
            }

            return list;
        }

        private static List<SiteInfo> EraseDuplacatedSite(List<List<SiteInfo>> sitesList)
        {
            var list = new List<SiteInfo>();
            foreach (var sites in sitesList)
            {
                foreach (var site in sites)
                {
                    if (list.Contains(site))
                    {
                        continue;
                    }

                    list.Add(site);
                }
            }

            return list;
        }
    }
}