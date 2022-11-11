using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace ElkTest.Device.Serial;

public class ElkSerialDevice : ISerialDevice
{
    private SerialPort serialPort;

    public EventHandler<List<string>> OnDataReceived { get; set; }
    public void WriteLine(string content)
    {
        serialPort.WriteLine(content);
    }

    public void Dispose()
    {
        if (serialPort is { IsOpen: true })
        {
            serialPort.Close();
        }

        serialPort?.Dispose();
    }

    public void Open(ElkDeviceConfig config)
    {
        serialPort = new SerialPort(config.Port)
        {
            BaudRate = config.BaudRate,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            Handshake = Handshake.None,
            ReadTimeout = 500,
            WriteTimeout = 500,
            DtrEnable = true,
            RtsEnable = true
        };

        serialPort.DataReceived += SerialPortDataReceived;
        serialPort.Open();
    }

    private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (serialPort is not { IsOpen: true })
        {
            return;
        }

        var lines = serialPort.ReadExisting()
            .Split("\n")
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        if (!lines.Any())
        {
            return;
        }

        OnDataReceived?.Invoke(this, lines);
    }
}