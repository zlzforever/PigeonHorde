namespace PigeonHorde.Tests;

public class WebApplicationFactoryFixture : IDisposable
{
    public Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> Instance { get; private set; } = new();

    public HttpClient CreateClient()
    {
        var client = Instance.CreateClient();

        return client;
    }

    public void Dispose()
    {
        Instance.Dispose();
    }
}

[CollectionDefinition("WebApplication collection")]
public class WebApplicationFactoryCollection : ICollectionFixture<WebApplicationFactoryFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}