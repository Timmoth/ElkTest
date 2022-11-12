using System.Collections.Generic;
using System.Linq;

namespace ElkTest.Device;

public class DeviceResponse
{
    public DeviceResponse(int id, int status, List<string> arguments)
    {
        Id = id;
        Status = status;
        Arguments = arguments;
    }

    public int Id { get; init; }
    public int Status { get; init; }
    public List<string> Arguments { get; init; }

    public override string ToString()
    {
        return $"{Id}:{Status}:{string.Join(";", Arguments)}";
    }

    public static DeviceResponse? Parse(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            return null;
        }

        command = command.Replace("\n", string.Empty).Replace("\r", string.Empty);

        var components = command.Split(":");
        if (components.Length < 2)
        {
            return null;
        }

        var args = new List<string>();
        if (components.Length > 2)
        {
            args = components[2].Split(";").ToList();
        }

        return new DeviceResponse(int.Parse(components[0]), int.Parse(components[1]), args);
    }
}