using _123vendas.Integration.Helpers;

namespace _123vendas.Integration.Fixtures;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    protected readonly IntegrationTestFactory _factory;
    protected readonly HttpClient _client;

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
        => await _factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        _client.WithoutToken();
        return Task.CompletedTask;
    }
}
