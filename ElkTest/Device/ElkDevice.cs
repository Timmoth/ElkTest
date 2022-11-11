using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ElkTest.Device;

public class ElkDevice : IDisposable
{
    private readonly List<DeviceRequest> _requests = new();
    private readonly SerialPort serialPort;
    private ITestOutputHelper? _output;
    private int _requestId;

    public ElkDevice(ElkDeviceConfig testDeviceConfig)
    {
        serialPort = new SerialPort(testDeviceConfig.Port)
        {
            BaudRate = testDeviceConfig.BaudRate,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = true,
            RtsEnable = true
        };

        serialPort.DataReceived += SerialPortDataReceived;
        serialPort.Open();
    }

    public void Dispose()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        serialPort.Dispose();
    }

    public async Task Reset(ITestOutputHelper output)
    {
        _output = output;
        await SendAndWait(App.Reset());
        _requests.Clear();
        _requestId = 0;
        await Task.Delay(2000);
    }

    public async Task<DeviceResponse> SendAndWait(DeviceRequest request)
    {
        Send(request);
        while (request.Response == null)
        {
            await Task.Delay(100);
        }

        return request.Response;
    }

    public DeviceRequest Send(DeviceRequest request)
    {
        request.Id = _requestId++;

        _requests.Add(request);
        serialPort.WriteLine(request.ToString());

        return request;
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var lines = serialPort.ReadExisting().Split("\n");

        foreach (var line in lines)
        {
            var response = DeviceResponse.Parse(line);
            if (response == null)
            {
                continue;
            }

            var request = _requests.FirstOrDefault(r => r.Id == response.Id);
            if (request == default)
            {
                continue;
            }

            request.Response = response;

            _output?.WriteLine("[TEST]\t" + request.Name + "(" + string.Join(", ", request.Arguments) + ") > " +
                               (HttpStatusCode)request.Response.Status + "(" +
                               string.Join(", ", request.Response.Arguments) + ")");
        }
    }

    public static class App
    {
        public static DeviceRequest Reset()
        {
            return new DeviceRequest("AppReset");
        }
    }

    public static class Pins
    {
        public static DeviceRequest Get(int pin, int sampleDuration = -1, int sampleRate = -1)
        {
            return new DeviceRequest("PinGet", new List<string>
            {
                pin.ToString(),
                sampleDuration.ToString(),
                sampleRate.ToString()
            });
        }

        public static DeviceRequest Set(int pin, int value)
        {
            return new DeviceRequest("PinSet", new List<string>
            {
                pin.ToString(), value.ToString()
            });
        }
    }
}