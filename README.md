#Metrics.NET.SignalFX
## What is the SignalFX Reporter for Metrics.NET

The Metrics.NET library provides a way of instrumenting applications with custom metrics (timers, histograms, counters etc) that can be reported in various ways and can provide insights on what is happening inside a running application.

This assembly provides a mechanism to report the metrics gathered by Metrics.NET to SignalFuse.

## Configuration
To configure Metrics.Net to report you need to set up two things
 - Your SignalFX API token
 - The default source

###Your SignalFX API Token
Your API SignalFX API token is available if you click on your avatar in the SignalFuse UI.

###Default source name
When reporting to SignalFuse we need to associate the reported metrics to a "source". Some choices are:
```csharp
// Configure with NetBios Name as the default source
 Metric.Config.WithReporting(report => 
      report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithNetBiosNameSource());
```
```csharp
// Configure with DNS Name as the default source
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithDNSNameSource());
```
```csharp
// Configure with FQDN as the default source
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithFQDNSource());
```
```csharp
// Configure with AWS instance id as the default source
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithAwsInstanceIdSource());
```
```csharp
// Configure with custom source name
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithSource("<source name>"));
```

