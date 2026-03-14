using MediatR;
using shs.Domain.Results;

namespace shs.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
