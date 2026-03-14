namespace shs.Api.IntegrationTests.Infrastructure;

public static class HttpClientExtensions
{
    private const string DefaultAdminEmail = "thacio.pacifico@gmail.com";

    public static HttpClient CreateAuthenticatedClient(
        this OuiWebApplicationFactory factory,
        string email = DefaultAdminEmail)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserEmailHeader, email);
        return client;
    }
}
