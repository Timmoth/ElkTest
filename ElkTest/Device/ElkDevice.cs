using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ElkTest.Device.Serial;
using Microsoft.Extensions.Internal;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ElkTest.Device;

public class ElkDevice : IDisposable
{
    private readonly ISerialDeviceFactory _deviceFactory;
    private readonly ElkDeviceConfig _sutDeviceConfig;
    private readonly ISystemClock _systemClock;
    private readonly ISerialDevice _testDevice;
    private ITestOutputHelper? _output;
    private int _requestId;
    private ISerialDevice? _sutDevice;
    private SutSerial _sutSerialMode = SutSerial.NONE;

    public ElkDevice(ISerialDeviceFactory deviceFactory, ElkDeviceConfig testDeviceConfig,
        ElkDeviceConfig sutDeviceConfig, ISystemClock systemClock)
    {
        _deviceFactory = deviceFactory;
        _sutDeviceConfig = sutDeviceConfig;
        _systemClock = systemClock;
        _testDevice = _deviceFactory.Create(testDeviceConfig);
        _testDevice.OnDataReceived += OnTestDeviceDataReceived;
    }

    public List<DeviceRequest> Requests { get; } = new();
    public List<SerialOutput> SerialOutput { get; } = new();

    public void Dispose()
    {
        _testDevice?.Dispose();
        _sutDevice?.Dispose();
    }

    public async Task<DeviceResponse> Reset(ITestOutputHelper output, SutSerial sutSerial = SutSerial.NONE)
    {
        _output = output;
        _sutSerialMode = sutSerial;

        // Reset SUT device
        var response = await SendAndWait(App.Reset());

        // Wait for device to start
        await Task.Delay(1000);

        // Dispose open sut device
        _sutDevice?.Dispose();
        if (_sutSerialMode == SutSerial.USB)
        {
            // Open USB serial connection to device
            _sutDevice = _deviceFactory.Create(_sutDeviceConfig);
            _sutDevice!.OnDataReceived += OnSutDeviceDataReceived;
        }

        SerialOutput.Clear();
        Requests.Clear();
        _requestId = 0;

        return response;
    }

    private void OnTestDeviceDataReceived(object sender, List<string> lines)
    {
        foreach (var line in lines)
        {
            if (line.StartsWith("[SUT]"))
            {
                var serialOutput = Device.Serial.SerialOutput.Parse(line.Remove(0, 5));
                if (serialOutput != null)
                {
                    if (_sutSerialMode != SutSerial.NONE)
                    {
                        _output?.WriteLine($"[SUT]\t{serialOutput.Content}");
                    }

                    SerialOutput.Add(serialOutput);
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

    public async Task<SerialOutput> WaitFor(Func<SerialOutput, bool> predicate, TimeSpan timeOutDuration)
    {
        var timeOut = _systemClock.UtcNow.Add(timeOutDuration);

        while (_systemClock.UtcNow < timeOut)
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

    #region Send

    public DeviceRequest Send(DeviceRequest request)
    {
        request.Id = _requestId++;

        Requests.Add(request);

        if (_sutSerialMode == SutSerial.USB && request.Name == "SerialSend")
        {
            _sutDevice?.WriteLine(request.Arguments[0]);
            request.Response = new DeviceResponse(request.Id, 200, new List<string>());
        }
        else
        {
            _testDevice?.WriteLine(request.ToString());
        }

        return request;
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

    #endregion
}