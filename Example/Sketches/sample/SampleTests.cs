using ElkTest.Device;
using ElkTest.Test;
using Xunit.Abstractions;

namespace Example.Sketches.sample;

public class SampleTests : ElkTestBase
{
    private readonly ITestOutputHelper _output;

    public SampleTests(ITestOutputHelper output, ElkTestFixture fixture) : base(fixture)
    {
        _output = output;
    }

    [Fact]
    public async void Pin20OutputsSquareWave()
    {
        //Arrange
        var (device, _) = await _fixture.Setup(_output);

        //Act
        //Assert
        var pin20Response = await device.SendAndWait(ElkDevice.Pins.Get(20, 1000, 100));
        var pin20Values = pin20Response.Arguments.Select(int.Parse).ToList();

        var firstValue = pin20Values[0];
        var secondValue = firstValue == 0 ? 1 : 0;
        for (var i = 0; i < pin20Values.Count; i++)
        {
            var expectedValue = i % 2 == 0 ? firstValue : secondValue;
            Assert.Equal(expectedValue, pin20Values[i]);
        }
    }
}