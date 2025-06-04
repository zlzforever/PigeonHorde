using System.Text;
using System.Text.Json;
using PigeonHorde.Dto.Agent;
using PigeonHorde.Dto.Health;

namespace PigeonHorde.Tests;

[Collection("WebApplication collection")]
public class UnitTest1(WebApplicationFactoryFixture fixture)
{
    [Fact]
    public async Task RegisterAndDeregister()
    {
        var name = "test-api1";
        var id = "test-api1_255.255.255.255";
        var client = fixture.CreateClient();
        var response1 = await client.PutAsync("/v1/agent/service/register",
            new StringContent($$"""
                                {
                                    "ID": "{{id}}",
                                    "Name": "{{name}}",
                                    "Tags": [
                                        "dapr"
                                    ],
                                    "Port": 5301,
                                    "Address": "255.255.255.255",
                                    "Meta": {
                                        "DAPR_METRICS_PORT": "1",
                                        "DAPR_PORT": "2",
                                        "DAPR_PROFILE_PORT": "3"
                                    },
                                    "Check": {
                                        "CheckID": "daprHealth:{{name}}:255.255.255.255",
                                        "Name": "Dapr Health Status",
                                        "Interval": "5s",
                                        "HTTP": "http://10.0.10.190"
                                    },
                                    "Checks": null
                                }
                                """, Encoding.UTF8, "application/json"));
        response1.EnsureSuccessStatusCode();

        var json = await client.GetStringAsync($"/v1/agent/service/{id}");

        var item = JsonSerializer.Deserialize<ListServicesItemDto>(json);
        Assert.NotNull(item);
        Assert.Equal(name, item.Name);
        Assert.Equal(id, item.Id);
        Assert.Equal("5FCF5AF390F5BA2F".ToLower(), item.ContentHash);

        await Deregister(id);

        var json2 = await client.GetStringAsync($"/v1/agent/service/{id}");
        Assert.Equal("", json2);
    }

    [Fact]
    public async Task ListServiceInstancesForService()
    {
        var name = "test-api2";
        var id = "test-api2_255.255.255.255";
        var client = fixture.CreateClient();
        var response1 = await client.PutAsync("/v1/agent/service/register",
            new StringContent($$"""
                                {
                                  "ID": "{{id}}",
                                  "Name": "{{name}}",
                                  "Tags": [
                                    "dapr"
                                  ],
                                  "Port": 5301,
                                  "Address": "255.255.255.255",
                                  "Meta": {
                                    "DAPR_METRICS_PORT": "4",
                                    "DAPR_PORT": "5",
                                    "DAPR_PROFILE_PORT": "6"
                                  },
                                  "Check": {
                                    "CheckID": "daprHealth:{{name}}:255.255.255.255",
                                    "Name": "Dapr Health Status",
                                    "Interval": "5s",
                                    "HTTP": "http://10.0.10.190"
                                  },
                                  "Checks": [
                                    {
                                      "CheckID": "daprHealth:{{name}}_2:255.255.255.255",
                                      "Name": "Dapr Health Status 2",
                                      "Interval": "5s",
                                      "HTTP": "http://10.0.10.190"
                                    }
                                  ]
                                }
                                """, Encoding.UTF8, "application/json"));
        response1.EnsureSuccessStatusCode();

        var response = await client.GetAsync($"/v1/health/service/{name}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<ListServiceInstancesDto>>(json);
        Assert.NotNull(items);
        Assert.Single(items);
        var item = items[0];
        Assert.Equal(id, item.Service.Id);
        Assert.Equal(name, item.Service.Name);
        Assert.NotNull(item.Checks);
        Assert.Equal(2, item.Checks.Count);

        var check1 = item.Checks[1];
        Assert.Equal($"daprHealth:{name}:255.255.255.255", check1.CheckId);
        Assert.Equal("Dapr Health Status", check1.Name);
        Assert.Equal("5s", check1.Interval);
        Assert.Equal("HTTP", check1.Type);
        var check2 = item.Checks[0];
        Assert.Equal($"daprHealth:{name}_2:255.255.255.255", check2.CheckId);
        Assert.Equal("Dapr Health Status 2", check2.Name);
        Assert.Equal("5s", check2.Interval);
        Assert.Equal("HTTP", check2.Type);

        await Deregister(id);
    }

    [Fact]
    public async Task ListServices()
    {
        await Deregister("test-api1");
        var client = fixture.CreateClient();
        var json = await client.GetStringAsync($"/v1/agent/services");
        var items = JsonSerializer.Deserialize<Dictionary<string, ListServicesItemDto>>(json);
        Assert.NotNull(items);
        var item = items.First(x => x.Key == "PigeonHorde").Value;
        Assert.Equal("127.0.0.1", item.Address);
        Assert.Equal(Defaults.DataCenter, item.Datacenter);
        Assert.False(item.EnableTagOverride);
        Assert.Equal("PigeonHorde", item.Id);
        Assert.Equal("8500", item.Meta["PORT"]);
        Assert.Equal("PigeonHorde Service", item.Name);
        Assert.Equal("PigeonHorde", item.Tags[0]);
        Assert.Equal("Infra", item.Tags[1]);
        Assert.Equal(8500, item.Port);
        Assert.Equal(1, item.Weights.Passing);
        Assert.Equal(1, item.Weights.Warning);
        Assert.Equal("127.0.0.1", item.TaggedAddresses["lan_ipv4"].Address);
        Assert.Equal(8500, item.TaggedAddresses["lan_ipv4"].Port);
        Assert.Equal("127.0.0.1", item.TaggedAddresses["wan_ipv4"].Address);
        Assert.Equal(8500, item.TaggedAddresses["wan_ipv4"].Port);
    }

    private async Task Deregister(string id)
    {
        var response = await fixture.CreateClient().PutAsync($"/v1/agent/service/deregister/{id}",
            new StringContent("", Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();
    }
}