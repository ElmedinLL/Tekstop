using Xunit;

namespace TechVault.API.Tests;

[CollectionDefinition("ApiIntegration", DisableParallelization = true)]
public sealed class ApiIntegrationCollection : ICollectionFixture<ApiWebApplicationFactory>
{
}
