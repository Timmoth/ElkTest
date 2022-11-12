namespace ElkTest.Device.Serial;

public class SerialDeviceFactory : ISerialDeviceFactory
{
    public ISerialDevice Create(ElkDeviceConfig config)
    {
        return new ElkSerialDevice(config);
    }
}