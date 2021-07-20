using System.Collections.Generic;
using System.Security.Claims;

namespace Bank.Api.IntegrationTest.AuthenticationMocks
{
    public class MockClaimSeed
    {
        private readonly IEnumerable<Claim> _seed;

        public MockClaimSeed(IEnumerable<Claim> seed)
        {
            _seed = seed;
        }

        public IEnumerable<Claim> getSeeds() => _seed;
    }
}
