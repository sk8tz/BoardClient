using System.Threading.Tasks;

using Board.Models.System;
using Board.Models.Users;
using Board.Resources.ApiResources;

using RestSharp;

namespace Board.Services.System
{
    public static class LoginService
    {
        public static async Task<IRestResponse<UserToken>> GetToken(string email, string password)
        {
            var client = new RestClient(SystemResource.GetTokenClient);
            var request = new RestRequest(SystemResource.GetTokenRequest, Method.POST);
            const string clientId = "6245c9d6cc3e51d82b88";
            const string clientSecret = "4653add678ec42e2de1e8e7a0344402ed9bfe53c";
            const string grantType = "password";
            request.AddParameter("client_id", clientId);
            request.AddParameter("client_secret", clientSecret);
            request.AddParameter("grant_type", grantType);
            request.AddParameter("email", email);
            request.AddParameter("password", password);

            return await client.ExecutePostTaskAsync<UserToken>(request);
        }

        public static async Task<IRestResponse<ResultValue>> IsAuthorization(string email, string versionNumber)
        {
            var client = new RestClient(SystemResource.BoardServiceClient);
            var request = new RestRequest(string.Format(SystemResource.IsAuthorizationRequest, email, versionNumber), Method.POST);

            return await client.ExecutePostTaskAsync<ResultValue>(request);
        }
    }
}