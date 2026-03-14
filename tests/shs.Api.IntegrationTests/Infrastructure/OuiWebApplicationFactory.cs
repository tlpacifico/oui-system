using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using shs.Infrastructure.Services;

namespace shs.Api.IntegrationTests.Infrastructure;

public class OuiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresContainerFixture _dbFixture;

    public OuiWebApplicationFactory(PostgresContainerFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _dbFixture.ConnectionString);
        builder.UseSetting("Firebase:ProjectId", "test-project");

        builder.ConfigureServices(services =>
        {
            // Replace Firebase JWT auth with test auth handler
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });

            // Remove background hosted service that isn't needed in tests
            var hostedServiceDescriptor = services
                .FirstOrDefault(d => d.ImplementationType == typeof(EcommerceReservationExpirationService));
            if (hostedServiceDescriptor != null)
                services.Remove(hostedServiceDescriptor);
        });
    }
}
