using ElkTest.Device;
using ElkTest.Device.Serial;
using Moq;

namespace Tests;

public static class ElkDeviceTestHelpers
{
    public static (Mock<ISerialDeviceFactory> DeviceFactory, Mock<ISerialDevice> TestDevice, Mock<ISerialDevice>
        SutDevice) Setup(ElkDeviceConfig testDeviceConfig, ElkDeviceConfig sutDeviceConfig)
    {
        var mockSerialDeviceFactory = new Mock<ISerialDeviceFactory>();
        var mockTestSerialDevice = new Mock<ISerialDevice>();
        mockSerialDeviceFactory.Setup(s => s.Create(It.Is<ElkDeviceConfig>(c => c == testDeviceConfig)))
            .Returns(mockTestSerialDevice.Object);

        var mockSutSerialDevice = new Mock<ISerialDevice>();
        mockSerialDeviceFactory.Setup(s => s.Create(It.Is<ElkDeviceConfig>(c => c == sutDeviceConfig)))
            .Returns(mockSutSerialDevice.Object);

        return (mockSerialDeviceFactory, mockTestSerialDevice, mockSutSerialDevice);
    }

    public static (Mock<ISerialDeviceFactory> DeviceFactory, Mock<ISerialDevice> TestDevice, Mock<ISerialDevice>
        SutDevice) WithAppReset(
            this (Mock<ISerialDeviceFactory> DeviceFactory, Mock<ISerialDevice> TestDevice, Mock<ISerialDevice>
                SutDevice) devices)
    {
        var request = ElkDevice.App.Reset();
        request.Id = 0;
        return devices.WithTestRequest(request, 200, new List<string>());
    }

    public static (Mock<ISerialDeviceFactory> DeviceFactory, Mock<ISerialDevice> TestDevice, Mock<ISerialDevice>
        SutDevice) WithTestRequest(
            this (Mock<ISerialDeviceFactory> DeviceFactory, Mock<ISerialDevice> TestDevice, Mock<ISerialDevice>
                SutDevice) devices, DeviceRequest request, int status, List<string> arguments)
    {
        devices.TestDevice.Setup(p => p.WriteLine(It.Is<string>(c => c == request.ToString())))
            .Raises(d =>
            {
                request.Response = new DeviceResponse(request.Id, status, arguments);
                d.OnDataReceived += null;
            }, devices.TestDevice.Object, new List<string>
            {
                new DeviceResponse(request.Id, status, arguments).ToString()
            });

        return devices;
    }
}