using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bank.Api.IntegrationTest.AuthenticationMocks
{
    public class MockSchemeProvider : AuthenticationSchemeProvider
    {
        public MockSchemeProvider(IOptions<AuthenticationOptions> options) : base(options)
        {
        }

        protected MockSchemeProvider(
            IOptions<AuthenticationOptions> options,
            IDictionary<string, AuthenticationScheme> schemes
        )
            : base(options, schemes)
        {
        }

        public override Task<AuthenticationScheme> GetSchemeAsync(string name)
        {
            var mockScheme = new AuthenticationScheme(
                IdentityConstants.ApplicationScheme,
                IdentityConstants.ApplicationScheme,
                typeof(MockAuthenticationHandler)
            );
            return Task.FromResult(mockScheme);
        }
    }
}
