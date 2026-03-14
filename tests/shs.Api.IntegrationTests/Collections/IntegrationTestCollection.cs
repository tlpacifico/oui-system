using shs.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace shs.Api.IntegrationTests.Collections;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<PostgresContainerFixture>;
