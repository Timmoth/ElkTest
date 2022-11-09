using System;

namespace ElkTest.Api;

public class ApiRequest
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string QueryString { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset Time { get; set; }
    public bool Handled { get; set; }
}