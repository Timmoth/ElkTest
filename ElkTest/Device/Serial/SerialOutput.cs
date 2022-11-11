namespace ElkTest.Device.Serial;

public class SerialOutput
{
    public string Content { get; set; }

    public static SerialOutput Parse(string input)
    {
        return new SerialOutput
        {
            Content = input.Replace("\n", string.Empty).Replace("\r", string.Empty)
        };
    }
}