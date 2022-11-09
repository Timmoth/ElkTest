using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ElkTest.Api;

public class ElkApi : IDisposable
{
    private readonly List<ApiEndpoint> _endpoints = new();
    private readonly List<ApiRequest> _requestLog = new();
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private ITestOutputHelper? _output;

    public ElkApi()
    {
        _ = Task.Run(() =>
        {
            var builder = WebApplication.CreateBuilder();

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                string body;
                using (var stream = new StreamReader(context.Request.Body))
                {
                    body = await stream.ReadToEndAsync();
                }

                var endPoint = _endpoints.FirstOrDefault(h => ShouldHandle(h, context, body));
                var handled = endPoint != null;

                _requestLog.Add(new ApiRequest()
                {
                    Time = DateTimeOffset.UtcNow,
                    Body = body,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.Value ?? string.Empty,
                    Handled = handled
                });
    

                var handledStatus = handled ? "handled" : "unhandled";
                _output?.WriteLine($"{handledStatus}\t {context.Request.Method}\t {context.Request.Path}");
                _output?.WriteLine($"\tRequest body: \t{body}");

                if (handled)
                {
                    _output?.WriteLine($"\tResponse body: \t{endPoint!.ResponseBody}");
                    await context.Response.WriteAsync(endPoint!.ResponseBody);
                }
                else
                {
                    await next.Invoke();
                }
                _output?.WriteLine("");
            });
            
            app.Run("https://*:6392");
        }, cancellationTokenSource.Token);
    }

    public void Setup(ITestOutputHelper output, List<ApiEndpoint> requests)
    {
        _output = output;
        _endpoints.Clear();
        _endpoints.AddRange(requests);
    }

    public List<ApiRequest> GetRequestLogs()
    {
        return _requestLog;
    }

    public bool ShouldHandle(ApiEndpoint config, HttpContext context, string body)
    {
        if (!string.IsNullOrEmpty(config.Method) && context.Request.Method != config.Method)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(config.Path) && context.Request.Path != config.Path)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(config.QueryString) && context.Request.QueryString.HasValue &&
            context.Request.QueryString.Value != config.QueryString)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(config.RequestBody) && body != config.RequestBody)
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }

    public async Task<ApiRequest?> WaitFor(Func<ApiRequest, bool> predicate, TimeSpan timeOutDuration)
    {
        var timeOut = DateTimeOffset.UtcNow.Add(timeOutDuration);

        while (DateTimeOffset.UtcNow < timeOut)
        {
            var request = _requestLog.FirstOrDefault(predicate);
            if (request != null)
            {
                return request;
            }

            await Task.Delay(250);
        }

        throw new XunitException($"Expected request to be made, but it timed out after {timeOutDuration.TotalSeconds} seconds.");
    }
}