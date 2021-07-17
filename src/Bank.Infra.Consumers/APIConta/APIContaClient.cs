using Bank.Domain.Apps.MessageQueues;
using Bank.Domain.Apps.Services;
using Bank.Domain.Models.APIConta;
using Bank.Domain.Models.MQ;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;

namespace Bank.Infra.Consumers.APIConta
{
    public class APIContaClient : IAPIContaClient
    {
        private string _url;
        private string _getEndPoint;
        private string _postEndPoint;
        private readonly MQSettings _configuration;

        public APIContaClient(IOptions<MQSettings> option)
        {
            _configuration = option.Value;
            _url = _configuration.APIContaSettings.Url;
            _getEndPoint = _configuration.APIContaSettings.GetEndPoint;
            _postEndPoint = _configuration.APIContaSettings.PostEndPoint;
        }

        public async Task<Account> GetAccountByNumberAsync(string accountNumber)
        {
            var client = new RestClient(_url + _getEndPoint + accountNumber);
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<Account>(response.Content);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Account() { Id = 0 };
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                return null;
            }
            return null;
        }

        public async Task<BalanceAdjustmentResponse> PostTransferAsync(BalanceAdjustment balanceAdjustment)
        {
            var client = new RestClient(_url + _postEndPoint);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                AccountNumber = balanceAdjustment.AccountNumber,
                Value = balanceAdjustment.Value,
                Type = balanceAdjustment.Type

            });
            IRestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new BalanceAdjustmentResponse() { Response = "Success" };
            }            
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return new BalanceAdjustmentResponse() { Response = JsonConvert.DeserializeObject<string>(response.Content) };
            }
            else
            {
                return new BalanceAdjustmentResponse() { Response = "Error" };
            }
        }
    }
}
