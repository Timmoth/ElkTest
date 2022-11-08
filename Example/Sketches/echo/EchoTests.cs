using ElkTest.Api;
using ElkTest.Device;
using ElkTest.Test;
using Xunit.Abstractions;

namespace Example.Sketches.echo
{
    public class EchoTests : ElkTestBase
    {
        private readonly ITestOutputHelper _output;

        public EchoTests(ITestOutputHelper output, ElkTestFixture fixture) : base(fixture)
        {
            _output = output;
        }

        [Theory]
        [InlineData(255, 1023)]
        [InlineData(0, 0)]
        public async void AnalogOutputPinEchosAnalogInputPin(int input, int expectedOutput)
        {
            //Arrange
            var device = await _fixture.Setup(_output);

            //Act
            await device.SendAndWait(Device.Pins.Set(28, input));

            await Task.Delay(1000);

            //Assert
            var pinResponse = await device.SendAndWait(Device.Pins.Get(27));
            var actualValue = int.Parse(pinResponse.Arguments[0]);
            Assert.InRange(actualValue, expectedOutput - 4, expectedOutput + 4);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(0, 0)]
        public async void DigitalOutputPinEchosDigitalInputPin(int input, int expectedOutput)
        {
            //Arrange
            var device = await _fixture.Setup(_output);

            //Act
            await device.SendAndWait(Device.Pins.Set(21, input));

            await Task.Delay(1000);

            //Assert
            var pinResponse = await device.SendAndWait(Device.Pins.Get(20));
            var actualValue = int.Parse(pinResponse.Arguments[0]);
            Assert.Equal(expectedOutput, actualValue);
        }
    }
}