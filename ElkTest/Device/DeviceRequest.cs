namespace ElkTest.Device;

public class DeviceRequest
{
    public DeviceRequest(string name, List<string> arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public DeviceRequest(string name)
    {
        Name = name;
        Arguments = new List<string>();
    }

    public int Id { get; set; }
    public string Name { get; init; }
    public List<string> Arguments { get; init; }
    public DeviceResponse? Response { get; set; }

    public override string ToString()
    {
        return $"{Id}:{Name}:{string.Join(";", Arguments)}";
    }
}