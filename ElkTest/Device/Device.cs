using System.Collections.Generic;

namespace ElkTest.Device;

public static class Device
{
    public static class App
    {
        public static DeviceRequest Reset()
        {
            return new("AppReset");
        }
    }

    public static class Pins
    {
        public static DeviceRequest Get(int pin)
        {
            return new("PinGet", new List<string>
            {
                pin.ToString()
            });
        }

        public static DeviceRequest Set(int pin, int value)
        {
            return new("PinSet", new List<string>
            {
                pin.ToString(), value.ToString()
            });
        }
    }
}