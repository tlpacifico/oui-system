using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using shs.Api.IntegrationTests.Infrastructure;
using Oui.Modules.Auth.Application.Users;
using Xunit;

namespace shs.Api.IntegrationTests.Tests.Users;

public class UserEndpointTests : IntegrationTestBase
{
    public UserEndpointTests(PostgresContainerFixture dbFixture) : base(dbFixture) { }

    private static object CreateUserRequest(string email = "test@example.com", string password = "Test123!", string? displayName = "Test User")
        => new { Email = email, Password = password, DisplayName = displayName };

    private async Task<(HttpResponseMessage Response, Guid ExternalId)> CreateTestUser(HttpClient client, string? email = null)
    {
        var request = CreateUserRequest(email ?? $"user-{Guid.NewGuid():N}@test.com");
        var response = await client.PostAsJsonAsync("/api/users", request);
        var body = await response.Content.ReadFromJsonAsync<ExternalIdResponse>();
        return (response, body?.ExternalId ?? Guid.Empty);
    }

    private record ExternalIdResponse(Guid ExternalId);

    // ── Create ──────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsCreated_WithValidData()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", CreateUserRequest("newuser@test.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ExternalIdResponse>();
        body.Should().NotBeNull();
        body!.ExternalId.Should().NotBeEmpty();

        // Cleanup
        await client.DeleteAsync($"/api/users/{body.ExternalId}");
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateEmail()
    {
        var client = Factory.CreateAuthenticatedClient();
        var email = $"dup-{Guid.NewGuid():N}@test.com";

        var (first, externalId) = await CreateTestUser(client, email);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await client.PostAsJsonAsync("/api/users", CreateUserRequest(email));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Cleanup
        await client.DeleteAsync($"/api/users/{externalId}");
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenEmailIsEmpty()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", CreateUserRequest(email: ""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenPasswordTooShort()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/users", CreateUserRequest(password: "12345"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/users", CreateUserRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Update ──────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedDisplayName()
    {
        var client = Factory.CreateAuthenticatedClient();
        var (_, externalId) = await CreateTestUser(client);

        var updateResponse = await client.PutAsJsonAsync($"/api/users/{externalId}", new { DisplayName = "Updated Name" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await client.GetAsync($"/api/users/{externalId}");
        var user = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        user!.DisplayName.Should().Be("Updated Name");

        // Cleanup
        await client.DeleteAsync($"/api/users/{externalId}");
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", new { DisplayName = "Name" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Delete ──────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsNoContent_AndRemovesUser()
    {
        var client = Factory.CreateAuthenticatedClient();
        var (_, externalId) = await CreateTestUser(client);

        var deleteResponse = await client.DeleteAsync($"/api/users/{externalId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/users/{externalId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Full Cycle ──────────────────────────────────────────────

    [Fact]
    public async Task FullCycle_CreateUpdateDelete_CleansUpCorrectly()
    {
        var client = Factory.CreateAuthenticatedClient();
        var email = $"cycle-{Guid.NewGuid():N}@test.com";

        // Create
        var createResponse = await client.PostAsJsonAsync("/api/users", CreateUserRequest(email, displayName: "Original"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ExternalIdResponse>();
        var externalId = created!.ExternalId;

        // Verify in list
        var listResponse = await client.GetAsync("/api/users");
        var listBody = await listResponse.Content.ReadAsStringAsync();
        listBody.Should().Contain(email);

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/api/users/{externalId}", new { DisplayName = "Changed" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify update
        var getResponse = await client.GetAsync($"/api/users/{externalId}");
        var user = await getResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        user!.DisplayName.Should().Be("Changed");

        // Delete
        var deleteResponse = await client.DeleteAsync($"/api/users/{externalId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getAfterDelete = await client.GetAsync($"/api/users/{externalId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
