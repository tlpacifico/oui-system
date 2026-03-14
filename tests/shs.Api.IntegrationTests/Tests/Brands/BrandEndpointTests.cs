using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using shs.Api.IntegrationTests.Infrastructure;
using Oui.Modules.Inventory.Application.Brands;
using Xunit;

namespace shs.Api.IntegrationTests.Tests.Brands;

public class BrandEndpointTests : IntegrationTestBase
{
    public BrandEndpointTests(PostgresContainerFixture dbFixture) : base(dbFixture) { }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoBrandsExist()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/brands");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var brands = await response.Content.ReadFromJsonAsync<List<BrandListResponse>>();
        brands.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_ReturnsCreated_WithValidData()
    {
        var client = Factory.CreateAuthenticatedClient();
        var request = new { Name = "Nike", Description = "Sportswear brand", LogoUrl = (string?)null };

        var response = await client.PostAsJsonAsync("/api/brands", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var brand = await response.Content.ReadFromJsonAsync<BrandDetailResponse>();
        brand.Should().NotBeNull();
        brand!.Name.Should().Be("Nike");
        brand.Description.Should().Be("Sportswear brand");
        brand.ExternalId.Should().NotBeEmpty();
        brand.CreatedBy.Should().Be("thacio.pacifico@gmail.com");
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenDuplicateName()
    {
        var client = Factory.CreateAuthenticatedClient();
        var request = new { Name = "Adidas", Description = (string?)null, LogoUrl = (string?)null };

        await client.PostAsJsonAsync("/api/brands", request);
        var response = await client.PostAsJsonAsync("/api/brands", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var client = Factory.CreateAuthenticatedClient();
        var request = new { Name = "", Description = (string?)null, LogoUrl = (string?)null };

        var response = await client.PostAsJsonAsync("/api/brands", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ReturnsBrand_WhenExists()
    {
        var client = Factory.CreateAuthenticatedClient();
        var createResponse = await client.PostAsJsonAsync("/api/brands",
            new { Name = "Zara", Description = "Fashion brand", LogoUrl = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDetailResponse>();

        var response = await client.GetAsync($"/api/brands/{created!.ExternalId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var brand = await response.Content.ReadFromJsonAsync<BrandDetailResponse>();
        brand!.Name.Should().Be("Zara");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDoesNotExist()
    {
        var client = Factory.CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/brands/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedData()
    {
        var client = Factory.CreateAuthenticatedClient();
        var createResponse = await client.PostAsJsonAsync("/api/brands",
            new { Name = "OldName", Description = (string?)null, LogoUrl = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDetailResponse>();

        var updateRequest = new { Name = "NewName", Description = "Updated description", LogoUrl = (string?)null };
        var response = await client.PutAsJsonAsync($"/api/brands/{created!.ExternalId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<BrandDetailResponse>();
        updated!.Name.Should().Be("NewName");
        updated.Description.Should().Be("Updated description");
        updated.UpdatedBy.Should().Be("thacio.pacifico@gmail.com");
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_AndExcludesFromGetAll()
    {
        var client = Factory.CreateAuthenticatedClient();
        var createResponse = await client.PostAsJsonAsync("/api/brands",
            new { Name = "ToDelete", Description = (string?)null, LogoUrl = (string?)null });
        var created = await createResponse.Content.ReadFromJsonAsync<BrandDetailResponse>();

        var deleteResponse = await client.DeleteAsync($"/api/brands/{created!.ExternalId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var allResponse = await client.GetAsync("/api/brands");
        var brands = await allResponse.Content.ReadFromJsonAsync<List<BrandListResponse>>();
        brands.Should().NotContain(b => b.ExternalId == created.ExternalId);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var client = Factory.CreateClient(); // no auth headers

        var response = await client.PostAsJsonAsync("/api/brands",
            new { Name = "Test", Description = (string?)null, LogoUrl = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
