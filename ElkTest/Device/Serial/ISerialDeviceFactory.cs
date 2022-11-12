namespace ElkTest.Device.Serial;

public interface ISerialDeviceFactory
{
    ISerialDevice Create(ElkDeviceConfig config);
}