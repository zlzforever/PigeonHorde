using System.Text;

namespace PigeonHorde.Tests;

[Collection("WebApplication collection")]
public class UnitTest1(WebApplicationFactoryFixture fixture)

{
    [Fact]
    public async Task Register()
    {
        var response = await fixture.CreateClient().PutAsync("/v1/agent/service/register",
            new StringContent("""
                              {
                                  "ID": "test-api_255.255.255.255",
                                  "Name": "test-api",
                                  "Tags": [
                                      "dapr"
                                  ],
                                  "Port": 5301,
                                  "Address": "255.255.255.255",
                                  "Meta": {
                                      "DAPR_METRICS_PORT": "51780",
                                      "DAPR_PORT": "51781",
                                      "DAPR_PROFILE_PORT": "-1"
                                  },
                                  "Check": {
                                      "CheckID": "daprHealth:test-api:255.255.255.255",
                                      "Name": "Dapr Health Status",
                                      "Interval": "5s",
                                      "HTTP": "http://192.168.100.254"
                                  },
                                  "Checks": null
                              }
                              """, Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Deregister()
    {
        var id = "test-api_255.255.255.255";
        var response = await fixture.CreateClient().PutAsync($"/v1/agent/service/deregister/{id}",
            new StringContent("", Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
    }
}