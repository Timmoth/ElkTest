using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ElkTest.Device.Serial;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ElkTest.Device;

public class ElkDevice : IDisposable
{
    private readonly ElkDeviceConfig _sutDeviceConfig;
    private readonly ISerialDeviceFactory _deviceFactory;
    private readonly ISerialDevice _testDevice;
    private ISerialDevice? _sutDevice;
    private ITestOutputHelper? _output;
    private SutSerial _sutSerialMode = SutSerial.NONE;
    private int _requestId;

    public ElkDevice(ISerialDeviceFactory deviceFactory, ElkDeviceConfig testDeviceConfig, ElkDeviceConfig sutDeviceConfig)
    {
        _deviceFactory = deviceFactory; 
        _sutDeviceConfig = sutDeviceConfig;
        _testDevice = _deviceFactory.Create();
        _testDevice.OnDataReceived += OnTestDeviceDataReceived;
        _testDevice.Open(testDeviceConfig);
    }

    public List<DeviceRequest> Requests { get; } = new();
    public List<SerialOutput> SerialOutput { get; } = new();

    public void Dispose()
    {
        _testDevice?.Dispose();
        _sutDevice?.Dispose();
    }

    public async Task Reset(ITestOutputHelper output, SutSerial sutSerial = SutSerial.NONE)
    {
        _output = output;
        _sutSerialMode = sutSerial;

        await SendAndWait(App.Reset());
        await Task.Delay(1000);

        if (_sutSerialMode == SutSerial.USB)
        {
            _sutDevice?.Dispose();
            _sutDevice = _deviceFactory.Create();
            _sutDevice!.OnDataReceived += OnSutDeviceDataReceived;
            _sutDevice!.Open(_sutDeviceConfig);
        }
        else
        {
            _sutDevice?.Dispose();
        }
        SerialOutput.Clear();
        Requests.Clear();
        _requestId = 0;
    }

    private void OnTestDeviceDataReceived(object sender, List<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith("[SUT]"))
            {
                if (_sutSerialMode != SutSerial.NONE)
                {
                    var serialOutput = Device.Serial.SerialOutput.Parse(line.Remove(0, 5));
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

    private void OnSutDeviceDataReceived(object sender, List<string> lines)
    {
        if (_sutSerialMode == SutSerial.NONE)
        {
            return;
        }

        foreach (var line in lines)
        {
            var serialOutput = Device.Serial.SerialOutput.Parse(line);
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
        _testDevice?.WriteLine(request.ToString());

        return request;
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