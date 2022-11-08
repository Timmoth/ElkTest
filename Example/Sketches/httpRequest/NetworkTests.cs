using ElkTest.Api;
using ElkTest.Test;
using Xunit.Abstractions;

namespace Example.Sketches.httpRequest
{
    public class NetworkTests : ElkTestBase
    {
        private readonly ITestOutputHelper _output;

        public NetworkTests(ITestOutputHelper output, ElkTestFixture fixture) : base(fixture)
        {
            _output = output;
        }

        [Fact]
        public async void DevicePostsDataToServerAfterGetFromServer()
        {
            //Arrange
            var number = new Random().Next(0, 100);
            var (device, api) = await _fixture.Setup(_output, new List<ApiEndpoint>()
            {
                new()
                {
                    Method = "GET",
                    Path = "/api/test/data",
                    ResponseBody =  System.Text.Json.JsonSerializer.Serialize(new { value = number})
                },
                new()
                {
                    Method = "POST",
                    Path = "/api/test/data",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(new { value = number+1})
                }
            });


            //Act
            //Assert
            await api.WaitFor(r => r.Handled && r.Method == "GET" && r.Path == "/api/test/data", TimeSpan.FromMinutes(1));
            await api.WaitFor(r => r.Handled && r.Method == "POST" && r.Path == "/api/test/data", TimeSpan.FromMinutes(1));
        }
    }
}