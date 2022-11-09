using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace ElkTest.Device;

public class ElkDevice : IDisposable
{
    private readonly SerialPort serialPort;
    private ITestOutputHelper? _output;
    private int _requestId;

    private readonly List<DeviceRequest> _requests = new();

    public ElkDevice(ElkDeviceConfig config)
    {
        serialPort = new SerialPort(config.Port)
        {
            BaudRate = config.BaudRate,
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
        _output = null;
        await SendAndWait(Device.App.Reset());
        _output = output;
        _requests.Clear();
        _requestId = 0;
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

        _output?.WriteLine("-> \t" + request);
        serialPort.WriteLine(request.ToString());
        _requests.Add(request);

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
            _output?.WriteLine("<- \t" + response);
        }
    }
}