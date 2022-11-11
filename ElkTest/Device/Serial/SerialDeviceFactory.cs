namespace ElkTest.Device.Serial;

public class SerialDeviceFactory : ISerialDeviceFactory
{
    public ISerialDevice Create()
    {
        return new ElkSerialDevice();
    }
}