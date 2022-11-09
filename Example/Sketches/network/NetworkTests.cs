using ElkTest.Api;
using ElkTest.Test;
using Xunit.Abstractions;

namespace Example.Sketches.network
{
    public class NetworkTests : ElkTestBase
    {
        private readonly ITestOutputHelper _output;

        public NetworkTests(ITestOutputHelper output, ElkTestFixture fixture) : base(fixture)
        {
            _output = output;
        }

        [Fact]
        public async void DevicePerformsPostGetPutDeleteRequests()
        {
            //Arrange
            var (device, api) = await _fixture.Setup(_output, new List<ApiEndpoint>()
            {
                new()
                {
                    Method = "POST",
                    Path = "/api/user",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(new { name = "Tim"}),
                    ResponseBody = System.Text.Json.JsonSerializer.Serialize(new { id = 1}),
                },
                new()
                {
                    Method = "GET",
                    Path = "/api/user/1",
                    ResponseBody =  System.Text.Json.JsonSerializer.Serialize(new { name = "Tim"})
                },
                new()
                {
                    Method = "PUT",
                    Path = "/api/user/1",
                    RequestBody = System.Text.Json.JsonSerializer.Serialize(new { name = "Tim1"}),
                },
                new()
                {
                    Method = "DELETE",
                    Path = "/api/user/1",
                },

            });


            //Act
            //Assert
            await api.WaitFor(r => r.Handled && r.Method == "POST" && r.Path == "/api/user", TimeSpan.FromMinutes(1));
            await api.WaitFor(r => r.Handled && r.Method == "GET" && r.Path == "/api/user/1", TimeSpan.FromMinutes(1));
            await api.WaitFor(r => r.Handled && r.Method == "PUT" && r.Path == "/api/user/1", TimeSpan.FromMinutes(1));
            await api.WaitFor(r => r.Handled && r.Method == "DELETE" && r.Path == "/api/user/1", TimeSpan.FromMinutes(1));
        }
    }
}