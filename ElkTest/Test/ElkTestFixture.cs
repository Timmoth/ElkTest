using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ElkTest.Api;
using ElkTest.Device;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace ElkTest.Test;

public class ElkTestFixture : IDisposable
{
    private readonly ElkApi _api;
    private readonly ElkDevice _device;

    public ElkTestFixture()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var deviceConfig = configuration.GetSection("ElkDevice").Get<ElkDeviceConfig>();

        _device = new ElkDevice(deviceConfig);
        _api = new ElkApi();
    }

    public void Dispose()
    {
        _api.Dispose();
        _device.Dispose();
    }

    public async Task<ElkDevice> Setup(ITestOutputHelper output)
    {
        _api.Setup(output, new List<ApiEndpoint>());
        await _device.Reset(output);
        return _device;
    }

    public async Task<(ElkDevice device, ElkApi api)> Setup(ITestOutputHelper output, List<ApiEndpoint> requests)
    {
        _api.Setup(output, requests);
        await _device.Reset(output);
        return (_device, _api);
    }
}