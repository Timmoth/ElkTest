using System.Text.Json.Serialization;

namespace ElkTest.Device;

public record ElkDeviceConfig(string Port, int BaudRate)
{
    [JsonConstructor]
    public ElkDeviceConfig() : this("", 0)
    {
    }
}