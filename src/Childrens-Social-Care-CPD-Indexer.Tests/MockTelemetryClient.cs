using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

public class MockTelemetryChannel : ITelemetryChannel
{
    public ConcurrentBag<ITelemetry> SentTelemtries = new ConcurrentBag<ITelemetry>();
    public bool IsFlushed { get; private set; }
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; }

    public void Send(ITelemetry item)
    {
        this.SentTelemtries.Add(item);
    }

    public void Flush()
    {
        this.IsFlushed = true;
    }

    public void Dispose()
    {

    }
}

internal static class MockTelemetryClient
{
    public static TelemetryClient Create()
    {
        var mockTelemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = new MockTelemetryChannel(),
        };

        return new TelemetryClient(mockTelemetryConfig);
    }

}
