using ElkTest.Device;
using ElkTest.Device.Serial;
using Microsoft.Extensions.Internal;
using Moq;
using Xunit.Abstractions;

namespace Tests;

public class ElkDeviceTests
{
    private readonly ITestOutputHelper _output;
    private readonly ElkDeviceConfig _sutDeviceConfig = new("sutDevicePort", 321);
    private readonly ElkDeviceConfig _testDeviceConfig = new("testDevicePort", 123);

    public ElkDeviceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region Ctor

    [Fact]
    public void CtorOpensConnectionToTestDevice()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        // Act
        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Assert
        deviceFactory.Verify(
            s => s.Create(It.Is<ElkDeviceConfig>(p =>
                p.BaudRate == _testDeviceConfig.BaudRate && p.Port == _testDeviceConfig.Port)), Times.Once);
    }

    #endregion

    #region TestData

    [Fact]
    public void TestDeviceDataReceivedStoresSerialOutput()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        testDevice.Raise(foo => foo.OnDataReceived += null, this, new List<string>
        {
            "[SUT]sut output"
        });

        // Assert
        var output = Assert.Single(sut.SerialOutput);
        Assert.Equal("sut output", output.Content);
    }

    #endregion

    #region Requests

    [Fact]
    public void SendRequestWritesToSerialDevice()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        sut.Send(ElkDevice.Pins.Set(10, 20));

        // Assert
        testDevice.Verify(s => s.WriteLine("0:PinSet:10;20"), Times.Once);
    }

    [Fact]
    public void SendRequestIncrementsRequestId()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        sut.Send(ElkDevice.Pins.Set(10, 20));
        sut.Send(ElkDevice.Pins.Set(20, 30));

        // Assert
        testDevice.Verify(s => s.WriteLine("0:PinSet:10;20"), Times.Once);
        testDevice.Verify(s => s.WriteLine("1:PinSet:20;30"), Times.Once);
    }

    [Fact]
    public async void SendAndWaitReturnsRequestResponse()
    {
        // Arrange
        var request = ElkDevice.Pins.Set(10, 20);

        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig)
                .WithTestRequest(request, 200, new List<string> { "a", "b" });

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        var response = await sut.SendAndWait(request);

        // Assert
        Assert.Equal(0, response.Id);
        Assert.Equal(200, response.Status);
        Assert.Equal("a", response.Arguments[0]);
        Assert.Equal("b", response.Arguments[1]);
    }

    [Fact]
    public async void SendAndWaitSerialSendWritesRequestOverUsbIfSerialModeIsUsb()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());
        await sut.Reset(_output, SutSerial.USB);

        var request = ElkDevice.Serial.Send("Test");
        // Act
        var response = await sut.SendAndWait(request);

        // Assert
        sutDevice.Verify(s => s.WriteLine("Test"), Times.Once);
        Assert.Equal(request.Id, response.Id);
        Assert.Equal(200, response.Status);
        Assert.Empty(response.Arguments);
    }

    #endregion


    #region Reset

    [Fact]
    public async void ResetSendsAppResetRequestAndAwaitsResponse()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        var response = await sut.Reset(_output);

        // Assert
        testDevice.Verify(s => s.WriteLine("0:AppReset:"), Times.Once);
        Assert.Equal(0, response.Id);
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async void ResetOpensSutSerialDevice()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());

        // Act
        var response = await sut.Reset(_output, SutSerial.USB);

        // Assert
        deviceFactory.Verify(
            s => s.Create(It.Is<ElkDeviceConfig>(p =>
                p.BaudRate == _sutDeviceConfig.BaudRate && p.Port == _sutDeviceConfig.Port)), Times.Once);

        Assert.Equal(0, response.Id);
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async void ResetDisposesOpenSutSerialDevice()
    {
        // Arrange
        var (deviceFactory, testDevice, sutDevice) =
            ElkDeviceTestHelpers.Setup(_testDeviceConfig, _sutDeviceConfig).WithAppReset();

        var sut = new ElkDevice(deviceFactory.Object, _testDeviceConfig, _sutDeviceConfig, new SystemClock());
        // Create device to be disposed
        await sut.Reset(_output, SutSerial.USB);

        // Act
        await sut.Reset(_output);

        // Assert
        sutDevice.Verify(s => s.Dispose(), Times.Exactly(1));
    }

    #endregion
}