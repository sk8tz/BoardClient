using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

using Board.Common;
using Board.Enums;
using Board.Models.Site;
using Board.Models.Sponsor;
using Board.Models.System;
using Board.Models.TrackAnalysisReport;
using Board.Models.TrackRealtime;
using Board.SystemData.Benchmark;
using Board.SystemData.SiteAnalysisReport;
using Board.SystemData.SiteRealtime;
using Board.SystemData.Sponsor;
using Board.SystemData.TrackAnalysisReport;
using Board.SystemData.TrackRealtime;

using Newtonsoft.Json;

namespace Board.SystemData.Base
{
    public static class SystemDataBuilder
    {
        public static BaseSystemData _systemDataService;

        public static dynamic SystemData { get; set; }
        public static dynamic SystemDataList { get; set; }

        public static BaseSystemData SystemBuilder(int systemType)
        {
            switch(systemType)
            {
                case (int)SystemTypeEnum.SiteAnalysisReport:
                    _systemDataService = new SiteAnalysisReportDataService();
                    break;
                case (int)SystemTypeEnum.SiteDataBank:
                    break;
                case (int)SystemTypeEnum.SiteRealtime:
                    _systemDataService = new SiteRealtimeDataService();
                    break;
                case (int)SystemTypeEnum.Sponsor:
                    _systemDataService = new SponsorDataService();
                    break;
                case (int)SystemTypeEnum.TrackAnalysisReport:
                    _systemDataService = new TrackAnalysisReportDataService();
                    break;
                case (int)SystemTypeEnum.TrackDataBank:
                    break;
                case (int)SystemTypeEnum.TrackRealtime:
                    _systemDataService = new TrackRealtimeDataService();
                    break;
                case (int)SystemTypeEnum.Benchmark:
                    //_systemDataService = new BenchmarkDataService();
                    break;
            }
            return _systemDataService;
        }

        public static void BuildSystemData(int systemType, string dataType, ResultValue resultValue, string enHeader)
        {
            try
            {
                switch(systemType)
                {
                    case (int)SystemTypeEnum.SiteAnalysisReport:
                        SystemData = new ObservableCollection<SiteData>();
                        SystemDataList = JsonConvert.DeserializeObject<List<SiteData>>(resultValue.Value);
                        break;
                    case (int)SystemTypeEnum.SiteDataBank:

                        break;
                    case (int)SystemTypeEnum.SiteRealtime:
                        SystemData = new ObservableCollection<ISystemData>();
                        SystemDataList = SiteRealtimeType.GetSiteRealtimeData(dataType, resultValue.Value, enHeader);
                        break;
                    case (int)SystemTypeEnum.Sponsor:
                    case (int)SystemTypeEnum.Benchmark:
                        SystemData = new ObservableCollection<SponsorData>();
                        SystemDataList = JsonConvert.DeserializeObject<List<SponsorData>>(resultValue.Value);
                        break;
                    case (int)SystemTypeEnum.TrackAnalysisReport:
                        SystemData = new ObservableCollection<TrackAnalysisData>();
                        SystemDataList = JsonConvert.DeserializeObject<List<TrackAnalysisData>>(resultValue.Value);
                        break;
                    case (int)SystemTypeEnum.TrackDataBank:

                        break;
                    case (int)SystemTypeEnum.TrackRealtime:
                        SystemData = new ObservableCollection<TrackRealtimeData>();
                        SystemDataList = JsonConvert.DeserializeObject<List<TrackRealtimeData>>(resultValue.Value);
                        break;
                }
            }
            catch(Exception ex)
            {
                ShowMessage.Show("构建数据类型出错");
                LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "Failed to BuildSystemData", ex);
            }
            finally
            {
                if(LogHelper.LogLevel == LogLevelEnum.Debug || LogHelper.LogLevel == LogLevelEnum.Info)
                {
                    LogHelper.LogMessage(MethodBase.GetCurrentMethod().DeclaringType, LogHelper.LogLevel, "BuildSystemData", null);
                }
            }
        }
    }
}