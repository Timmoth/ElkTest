using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ElkTest.Test;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ElkTest.Device;

public class ElkDevice : IDisposable
{
    private readonly ElkDeviceSerialLogger _elkDeviceSerialLogger;
    private readonly SerialPort serialPort;
    private ITestOutputHelper? _output;
    private int _requestId;
    private SutSerial _sutSerialMode = SutSerial.NONE;

    public ElkDevice(ElkDeviceConfig testDeviceConfig, ElkDeviceConfig sutDeviceConfig)
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

        _elkDeviceSerialLogger = new ElkDeviceSerialLogger(sutDeviceConfig);
    }

    public List<DeviceRequest> Requests { get; } = new();
    public List<SerialOutput> SerialOutput { get; } = new();

    public void Dispose()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }

        _elkDeviceSerialLogger?.Dispose();
        serialPort.Dispose();
    }

    public async Task Reset(ITestOutputHelper output, SutSerial sutSerial = SutSerial.NONE)
    {
        _sutSerialMode = sutSerial;

        _output = output;
        await SendAndWait(App.Reset());
        if (_sutSerialMode == SutSerial.USB)
        {
            await Task.Delay(1000);
            await _elkDeviceSerialLogger.Setup(output);
            _elkDeviceSerialLogger.OnDataReceived += OnDataReceived;
        }
        else
        {
            _elkDeviceSerialLogger.Dispose();
        }

        Requests.Clear();
        _requestId = 0;

        SerialOutput.Clear();
        await Task.Delay(1000);
    }

    private void OnDataReceived(object sender, List<string> lines)
    {
        if (_sutSerialMode == SutSerial.NONE)
        {
            return;
        }

        foreach (var line in lines)
        {
            var serialOutput = Device.SerialOutput.Parse(line);
            if (serialOutput == null)
            {
                continue;
            }

            _output?.WriteLine($"[SUT]\t{serialOutput.Content}");
            SerialOutput.Add(serialOutput);
        }
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

    public async Task<SerialOutput> WaitFor(Func<SerialOutput, bool> predicate, TimeSpan timeOutDuration)
    {
        var timeOut = DateTimeOffset.UtcNow.Add(timeOutDuration);

        while (DateTimeOffset.UtcNow < timeOut)
        {
            var serialOutput = SerialOutput.FirstOrDefault(predicate);
            if (serialOutput != null)
            {
                return serialOutput;
            }

            await Task.Delay(250);
        }

        throw new XunitException(
            $"Expected serial output, but it timed out after {timeOutDuration.TotalSeconds} seconds.");
    }

    public DeviceRequest Send(DeviceRequest request)
    {
        request.Id = _requestId++;

        Requests.Add(request);
        serialPort.WriteLine(request.ToString());

        return request;
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var lines = serialPort.ReadExisting().Split("\n");

        foreach (var line in lines)
        {
            if (line.StartsWith("[SUT]"))
            {
                if (_sutSerialMode != SutSerial.NONE)
                {
                    var serialOutput = Device.SerialOutput.Parse(line.Remove(0, 5));
                    if (serialOutput != null)
                    {
                        _output?.WriteLine($"[SUT]\t{serialOutput.Content}");
                        SerialOutput.Add(serialOutput);
                    }
                }

                continue;
            }

            var response = DeviceResponse.Parse(line);
            if (response == null)
            {
                continue;
            }

            var request = Requests.FirstOrDefault(r => r.Id == response.Id);
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

    public static class Serial
    {
        public static DeviceRequest Send(string content)
        {
            return new DeviceRequest("SerialSend", new List<string>
            {
                content
            });
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