using Bank.Api.IntegrationTest.Base;
using System.Threading.Tasks;
using Xunit;

namespace Bank.Api.IntegrationTest
{
    public class TransactionControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public TransactionControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ShouldReturnOK()
        {
            var client = _factory.GetAnonymousClient();

            var response = await client.GetAsync($"/api/fund-transfer/{"fda81897-9c25-4d18-9347-023e9a5d4f1b"}");
            
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task ShouldReturnNotFound()
        {
            var client = _factory.GetAnonymousClient();

            var response = await client.GetAsync($"/api/fund-transfer/{"fda81897-9c25-4d18-9347-023e9a5d4333"}");

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }
    }
}
