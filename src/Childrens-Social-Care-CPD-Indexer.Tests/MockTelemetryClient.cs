using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Concurrent;

namespace Childrens_Social_Care_CPD_Indexer.Tests;

internal sealed class MockTelemetryChannel : ITelemetryChannel
{
    public ConcurrentBag<ITelemetry> SentTelemtries = [];
    public bool IsFlushed { get; private set; }
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; } = string.Empty;

    public void Send(ITelemetry item) => SentTelemtries.Add(item);
    public void Flush() => IsFlushed = true;
    public void Dispose() {}
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
