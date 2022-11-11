using System;
using System.Collections.Generic;

namespace ElkTest.Device.Serial;

public interface ISerialDevice : IDisposable
{
    void Open(ElkDeviceConfig config);
    public EventHandler<List<string>> OnDataReceived { get; set; }
    void WriteLine(string content);
}