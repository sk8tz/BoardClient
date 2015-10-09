using Board.Common;
using Board.Controls;
using Board.Models.System;
using Board.Resources.ApiResources;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RestSharp;

namespace Board.Services.Sponsor
{
    public class SponsorService
    {
        private readonly RestClient client;

        private static readonly SponsorService Instance = new SponsorService();

        public SponsorService()
        {
            this.client = new RestClient(SystemResource.BoardServiceClient);
        }

        public static SponsorService Singleton
        {
            get
            {
                return Instance;
            }
        }

        public async Task<ObservableCollection<Node>> GetSponsorProgramList(string clientId)
        {
            var request = new RestRequest(string.Format(SponsorResource.GetSponsorProgramListRequest, clientId), Method.POST);

            var response = await this.client.ExecutePostTaskAsync<ResultValue>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var programItems = new ObservableCollection<Node>();
            var programs = JsonConvert.DeserializeObject<List<ProgramModel>>(response.Data.Value);
            foreach (var program in programs)
            {
                programItems.Add(new Node { Id = program.Id.ToString(), Title = program.ProgramName });
            }

            return programItems;
        }

        public async Task<ResultValue> GetSponsorData(string programName, string startDate, string endDate, string dataFilterCount, string dataOrderBy)
        {
            if(string.IsNullOrEmpty(dataOrderBy))
            {
                dataOrderBy = ",";
            }

            var request = new RestRequest(string.Format(SponsorResource.SponsorDataRequest, programName, startDate, endDate, dataFilterCount, dataOrderBy), Method.POST);

            var data = new ResultValue();
            var response = await this.client.ExecutePostTaskAsync<ResultValue>(request);
            if (((RestResponseBase)(response)).StatusCode == HttpStatusCode.OK) //200
            {
                data = response.Data;
            }
            else if (((RestResponseBase)(response)).StatusCode == HttpStatusCode.NotFound) //404
            {
                ShowMessage.Show("访问404错误");
            }
            else if ((int)response.StatusCode == 422)
            {
                ShowMessage.Show("访问422错误");
            }
            else if (((RestResponseBase)(response)).StatusCode == HttpStatusCode.InternalServerError) //500
            {
                ShowMessage.Show("访问500错误");
            }
            else
            {
                ShowMessage.Show("未知错误");
            }

            return data;
        }
    }
}