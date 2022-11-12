using System;
using System.Collections.Generic;

namespace ElkTest.Device.Serial;

public interface ISerialDevice : IDisposable
{
    event EventHandler<List<string>> OnDataReceived;
    void WriteLine(string content);
}