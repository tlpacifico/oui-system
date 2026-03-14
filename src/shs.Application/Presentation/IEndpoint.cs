using Microsoft.AspNetCore.Routing;

namespace shs.Application.Presentation;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
