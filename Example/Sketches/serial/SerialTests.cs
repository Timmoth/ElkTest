using ElkTest.Device;
using ElkTest.Test;
using Xunit.Abstractions;

namespace Example.Sketches.serial;

public class SerialTests : ElkTestBase
{
    private readonly ITestOutputHelper _output;

    public SerialTests(ITestOutputHelper output, ElkTestFixture fixture) : base(fixture)
    {
        _output = output;
    }

    [Fact]
    public async void DeviceEchosSerialContentOverUart()
    {
        //Arrange
        var (device, _) = await _fixture.Setup(_output, sutSerial: SutSerial.UART);

        //Act
        var response = await device.SendAndWait(ElkDevice.Serial.Send("Test123"));

        //Assert
        Assert.Equal(200, response.Status);
        await device.WaitFor(o => o.Content == "uart: 'Test123'", TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async void DeviceEchosSerialContentOverUsb()
    {
        //Arrange
        var (device, _) = await _fixture.Setup(_output, sutSerial: SutSerial.USB);

        //Act
        var response = await device.SendAndWait(ElkDevice.Serial.Send("Test123"));

        //Assert
        Assert.Equal(200, response.Status);
        await device.WaitFor(o => o.Content == "usb: 'Test123'", TimeSpan.FromSeconds(30));
    }
}