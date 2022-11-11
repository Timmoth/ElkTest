using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ElkTest.Api;
using ElkTest.Device;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace ElkTest.Test;

public enum SutSerial
{
    NONE,
    UART,
    USB
}

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

        var testDeviceConfig = configuration.GetSection("ElkTestDevice").Get<ElkDeviceConfig>();
        var sutDeviceConfig = configuration.GetSection("ElkSUTDevice").Get<ElkDeviceConfig>();

        _device = new ElkDevice(testDeviceConfig, sutDeviceConfig);
        _api = new ElkApi();
    }

    public void Dispose()
    {
        _api.Dispose();
        _device.Dispose();
    }

    public async Task<(ElkDevice device, ElkApi api)> Setup(ITestOutputHelper output, List<ApiEndpoint> requests = null,
        SutSerial sutSerial = SutSerial.NONE)
    {
        _api.Setup(output, requests ?? new List<ApiEndpoint>());

        await _device.Reset(output, sutSerial);

        return (_device, _api);
    }
}