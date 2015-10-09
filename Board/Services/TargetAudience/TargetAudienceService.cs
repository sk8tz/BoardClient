using Board.Models.Sponsor;
using Board.Models.System;
using Board.ServiceForSei;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Board.Services.TargetAudience
{
    

    public class TargetAudienceService
    {
        private readonly SEISeviceClient tcpClient;


        private static readonly TargetAudienceService Instance= new TargetAudienceService();
        public TargetAudienceService()
        {
            this.tcpClient = new SEISeviceClient();
        }

        public static TargetAudienceService Singleton
        {
            get
            {
                return Instance;
            }
        }

        public async Task<DataSet> GetTaData(string[] programName, string endDate, TargetAudienceInfo taInfo)
        {
            var taConfigDto = new TaConfigDto();
            taConfigDto.ParamModel = new SpecimenModel
            {
                Age = taInfo.Age,
                Gender = taInfo.Gender,
                Education = taInfo.Education,
                Marriage = taInfo.Marriage,
                Income = taInfo.Income,
                City = taInfo.Zone
            };

            taConfigDto.TimeString = endDate;
            taConfigDto.Programs = programName;

            var requestId = this.tcpClient.TaRequest(taConfigDto);
            DataSet dataSet = this.GetGetTaDataSet(requestId);
            return dataSet;
        }

        public static async Task<int> GetTaCount(TargetAudienceInfo taInfo)
        {
            //var model = new ServiceForSei.SpecimenInfo();
            return 0;
        }

        public List<SponsorData> ConvertDataSet(DataSet taDataSet, Dictionary<string, object> parameterDictionary)
        {
            var dataTable = this.GetDataTable(taDataSet);
            if (dataTable == null)
            {
                return null;
            }
            var columns = this.GetColumnsName(parameterDictionary);
            if (columns == null)
            {
                return null;
            }

            try
            {
                var currentTable = dataTable.DefaultView.ToTable(false, columns);
                if (currentTable.Rows.Count == 0)
                {
                    return null;
                }

                var sponsorData = DataTableConverter.Singleton.GetList<SponsorData>(currentTable);
                return sponsorData;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        private string[] GetColumnsName(Dictionary<string, object> parameterDictionary)
        {
            var widgetModel = parameterDictionary["WidgetModel"] as WidgetModel;
            var columns = this.GetColumns(widgetModel.Dimensions, widgetModel.Metrics);

//            var dimensionCollection = parameterDictionary["Dimensions"] as List<DimensionTypeModel>;
//            var dimensions = dimensionCollection.Where(c => columns.Contains(c.EnName)).Select(c => c.CnName);
//            var MetricCollection = parameterDictionary["Metrics"] as List<MetricTypeModel>;
//            var metrics = MetricCollection.Where(c => columns.Contains(c.EnName)).Select(c => c.CnName);
//
//            var columnsName = new List<string>();
//            columnsName.AddRange(dimensions);
//            columnsName.AddRange(metrics);

            var columnsName = this.InitialColumns();
            var tempColumns = new List<string>();
            foreach (var column in columns)
            {
                if (!columnsName.ContainsKey(column))
                {
                    continue;
                }

                var columnName = columnsName[column];
                tempColumns.Add(columnName);
            }

            return tempColumns.ToArray();
        }

        private List<string> GetColumns(string dimensions, string metrics)
        {
            var columns = new List<string>();
            columns.AddRange(dimensions.Split(','));
            columns.AddRange(metrics.Split(','));
            return columns;
        }

        private DataSet GetGetTaDataSet(string requestId)
        {
            DataSet dataSet = null;

            while (dataSet == null || dataSet.Tables.Count == 0)
            {
                E_TaStatus status;
                dataSet = this.tcpClient.GetTaDataSet(requestId, out status);

                switch (status)
                {
                    case E_TaStatus.Calculating:
                        Thread.Sleep(1000);
                        continue;
                    case E_TaStatus.Error:
                        return null;
                    case E_TaStatus.OK:
                        return dataSet;
                    default:
                        return null;
                }
            }

            return dataSet;
        }

        private DataTable GetDataTable(DataSet dataSet)
        {
            if (dataSet == null)
            {
                return null;
            }

            if (dataSet.Tables.Count != 1)
            {
                return null;
            }

            var dataTable = dataSet.Tables[0];

            return dataTable;
        }


        private string GetCnName(string column)
        {
            switch (column)
            {
                case "赞助商名称":
                    return "赞助商";
                default:
                    return column;
            }
        }

        private Dictionary<string, string> InitialColumns()
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("program","节目名称");
            dictionary.Add("firstOnAir", "本季首播时间");
            dictionary.Add("onAirTime", "本期播出时间");
            dictionary.Add("sponsor", "赞助商");
//            dictionary.Add("sponsorEn", "Sponsor");
//            dictionary.Add("week", "期数");
            dictionary.Add("viewership", "节目关注度");
            dictionary.Add("sentiment", "节目喜爱度");
//            dictionary.Add("", "节目喜爱度TOP1");
            dictionary.Add("attentionIndex", "关注指数");
//            dictionary.Add("attentionDifference", "关注指数变化");
            dictionary.Add("positivePosts1", "参与量");
            dictionary.Add("socialIndex", "社交指数");
            dictionary.Add("searchCount", "搜索量");
            dictionary.Add("searchIndex", "搜索指数");
            dictionary.Add("engagementIndex", "参与指数");
//            dictionary.Add("engagementDifference", "参与指数变化");
            dictionary.Add("programIndex", "节目表现指数");
//            dictionary.Add("programDifference", "节目表现指数变化");
            dictionary.Add("associationAvg", "品牌回想度");
//            dictionary.Add("", "品牌回想度(三期平均)");
            dictionary.Add("recallIndex", "回想指数");
//            dictionary.Add("recallDifference", "回想指数变化");
            dictionary.Add("positivePosts2", "相关量");
            dictionary.Add("linkageIndex", "相关指数");
//            dictionary.Add("linkageDifference", "相关指数变化");
            dictionary.Add("associationIndex", "品牌关联指数");
//            dictionary.Add("associationDifference", "品牌关联指数变化");
            dictionary.Add("postAwarenessAvg", "后测认知度");
//            dictionary.Add("", "后测认知度(三期平均)");
//            dictionary.Add("", "后测喜爱度1");
//            dictionary.Add("", "后测喜爱度2");
//            dictionary.Add("", "后测喜爱度3");
//            dictionary.Add("", "后测喜爱度4");
//            dictionary.Add("", "后测喜爱度5");
            dictionary.Add("postPreference", "后测喜爱度");
//            dictionary.Add("", "后测喜爱度(三期平均)");
//            dictionary.Add("", "后测购买倾向1");
//            dictionary.Add("", "后测购买倾向2");
//            dictionary.Add("", "后测购买倾向3");
//            dictionary.Add("", "后测购买倾向4");
//            dictionary.Add("", "后测购买倾向5");
            dictionary.Add("postPurchase", "后测购买倾向");
//            dictionary.Add("", "后测购买倾向(三期平均)");
            dictionary.Add("preAwareness", "前测认知度");
//            dictionary.Add("", "前测喜爱度1");
//            dictionary.Add("", "前测喜爱度2");
//            dictionary.Add("", "前测喜爱度3");
//            dictionary.Add("", "前测喜爱度4");
//            dictionary.Add("", "前测喜爱度5");
            dictionary.Add("prePreference", "前测喜爱度");
//            dictionary.Add("", "前测购买倾向1");
//            dictionary.Add("", "前测购买倾向2");
//            dictionary.Add("", "前测购买倾向3");
//            dictionary.Add("", "前测购买倾向4");
//            dictionary.Add("", "前测购买倾向5");
            dictionary.Add("prePurchase", "前测购买倾向");
            dictionary.Add("preferenceIndex", "喜爱度指数");
            dictionary.Add("preferenceLift", "喜爱度提升指数");
//            dictionary.Add("", "喜爱度提升指数变化");
            dictionary.Add("purchaseIndex", "购买倾向指数");
            dictionary.Add("purchaseLift", "购买倾向提升指数");
//            dictionary.Add("", "购买倾向提升指数变化");
            dictionary.Add("commitmentIndex", "品牌收益指数");
//            dictionary.Add("", "品牌收益指数变化");
            dictionary.Add("sei", "赞助评估指数");
//            dictionary.Add("", "赞助评估指数变化");
            dictionary.Add("", "认知样本量");

            return dictionary;
        }
    }
}