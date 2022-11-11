using ElkTest.Device;
using ElkTest.Device.Serial;
using Moq;
using Xunit.Abstractions;

namespace Tests
{
    public class ElkDeviceTests
    {
        [Fact]
        public void SendRequestWritesLineToSerialDevice()
        {
            // Arrange
            var mockSerialDeviceFactory = new Mock<ISerialDeviceFactory>();
            var mockSerialDevice = new Mock<ISerialDevice>();
            mockSerialDeviceFactory.Setup(s => s.Create()).Returns(mockSerialDevice.Object);

            var sut = new ElkDevice(mockSerialDeviceFactory.Object, new ElkDeviceConfig(), new ElkDeviceConfig());

            // Act
            sut.Send(ElkDevice.Pins.Set(10, 20));

            // Assert
            mockSerialDevice.Verify(s => s.WriteLine("0:PinSet:10;20"), Times.Once);
        }
    }
}