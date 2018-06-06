# Metrics.NET.SignalFx
## What is the SignalFx Reporter for Metrics.NET
The Metrics.NET library provides a way of instrumenting applications with custom metrics (timers, histograms, counters etc) that can be reported in various ways and can provide insights on what is happening inside a running application.

This assembly provides a mechanism to report the metrics gathered by Metrics.NET to SignalFx.

Note: Versions 3.0.0 and above now target `.NET Standard 2.0`.

## Sending Dimensions

In order to send dimensions to SignalFx with Metrics.NET you use the MetricTags object and use Metrics.Core.TaggedMetricsContext. This unfortunately(currently) means that you will to have a second context (think . in metric name). MetricTags are currently a list of strings. To send a dimension just add a string that looks like "key=value" to the MetricTags object you use to initialize your metrics. E.g

```csharp

public TaggedMetricsContext getContext() {
   return (TaggedMetricsContext)Metric.Context("app", (ctxName) => { return new TaggedMetricsContext(ctxName); });
}

//Setup counters for API usage
public void setupCounters(string env) {
    this.loginAPICount = getContext().Counter("api.use", Unit.Calls, new MetricTags("environment="+env, "api_type=login"));
    this.purchaseAPICount = getContext().Counter("api.use", Unit.Calls, new MetricTags("environment="+env, "api_type=purchase"));
}
....
public void login() {
    this.loginAPICount.Increment();
    ....
}
```
This will create a context called "app" so metrics reported will look like "yourhostname.app.api.use".
This will allow you to see all of of your api.use metrics together or split it out by environment or by api_type.

## Configuring the SignalFxReporter

**You only need to configure the SignalFxReporter once per application start.**
If you configure multiple reports they will each send any metrics registered with Metrics.NET. So if you call the Metrics.Config.WithReporting(...) 10 times, then each metric will be reported 10 times to SignalFx.

To configure Metrics.Net to report you need to set up two things
 - Your SignalFx API token
 - The default source

### Your SignalFx API Token
Your API SignalFx API token is available if you click on your avatar in the SignalFx UI.

### Default source name
When reporting to SignalFx we need to associate the reported metrics to a "source". Some choices are:
 - NetBIOS Name
 - DNS Name
 - FQDN
 - Custom Source
 - None (don't send any "source" information)

### AWS Integration
If your code will be running on an AWS instance and you have integrated SignalFx with AWS. You can configure the Metrics.Net.SignalFx reporter to send the instance id as one of the dimensions so that you can use the discovered AWS instance attributes to filter and group metrics.

### Default Dimensions
If there are dimensions that you wish to send on all the metrics that you report to SignalFx. You can configure a set of "default dimensions" when you configure the SignalFxReporter

### C# Configuration
#### Basic Configuration
```csharp
// Configure with NetBios Name as the default source
 Metric.Config.WithReporting(report => 
      report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithNetBiosNameSource().Build());
```
```csharp
// Configure with DNS Name as the default source
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithDNSNameSource().Build());
```
```csharp
// Configure with FQDN as the default source
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithFQDNSource().Build());
```
```csharp
// Configure with custom source name
Metric.Config.WithReporting(report => 
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(5)).WithSource("<source name>").Build());
```

#### AWS Integration
```csharp
// Add AWS Integration
Metric.Config.WithReporting(report =>
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(10)).WithAWSInstanceIdDimension().WithNetBiosNameSource().Build());
```

#### Default Dimensions
```csharp
// Add default Dimensions
IDictionary<string, string> defaultDims = new Dictionary<string, string>();
defaultDims["environment"] = "prod";
defaultDims["serverType"] = "API";
Metric.Config.WithReporting(report =>
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(10)).WithDefaultDimensions(defaultDims).WithAWSInstanceIdDimension().WithNetBiosNameSource().Build());
```

#### Aggregations
The Metrics.NET library calculates aggregations for almost all of the metric types:

 - Counter(with items) -> percentage

 - Histogram -> count, last, min, mean, max, stddev, median, percent_75, percent_95, percent_98, percent_99, percent_999

 - Meter(no items) -> rate_mean, rate_1min, rate_5min, rate_15min
 - Meter(with items) -> per Item: percent, rate_mean, rate_1min, rate_5min, rate_15min


 - Timer -> count, active_sessions, rate_mean, rate_1min, rate_5min, rate_15min, last, min, mean, max, stddev, median, percent_75, percent_95, percent_98, percent_99, percent_999

The client can specify which of these aggregations they wish to send. By default count,min,mean,max aggregations are sent.
```csharp
// Send the 99 percentile metrics
Metric.Config.WithReporting(report =>
     report.WithSignalFx("<your API token>", TimeSpan.FromSeconds(10)).WithMetricDetail(MetricDetails.percent_99).Build());
```

### App.Config Configuration
It is also possible to use App.Config to configure the SignalFxReporter.

To configure via App.Config use the following code to initialize your Metrics:
```csharp
Metric.Config.WithReporting(report => report.WithSignalFxFromAppConfig());
```
#### Basic Configuration
You need to first setup a section in the <configSections> portion at the
top of your App.Config file. 
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <section name="signalFxReporter" type="Metrics.SignalFx.Configuration.SignalFxReporterConfiguration, Metrics.NET.SignalFx"/>
  </configSections>
  ....
</configuration>
```

Next you need to add a <signalFxReporter> stanza
You must specify the following attributes:
 - apiToken - Your SignalFx token

The following attributes are optional
 - sourceDimension - What the name of the "source" dimension is. (If just a sourceType is specified and it is not 'none' then the default value for this is sf_source)
 - sourceType - How you would like to configure the default source. Your choices are:
  - none
  - netbios
  - dns
  - fqdn
  - custom - If you specify this you must also specify the "sourceValue" attribute to specify the custom source.
 - sampleInterval - TimeSpan (defaults to 00:00:05, minimum 00:00:01) How often to report metrics to SignalFx
 - maxDatapointPerMessage - Integer (defaults to 10000, min 1, max 10000) The maximum of points to report per message
                            to SignalFx
 - awsIntegration - Boolean (default to false) If set to true then the AWS integration will be turned on. 
E.g
```xml
  <signalFxReporter apiToken="AAABQWDCC" sourceType="netbios" sampleInterval="00:00:05"/> 
```

### Source Value for a Metric
It is often useful, but not required, to identify the "source" of a metric. An example of this is a hostname. There are two things
that needs to be configured:
  - sourceDimension - The name of the "source" metric.
  - defaultSource - The value to send when the source metric value is not specified.

### Default Dimensions
To add default dimensions add a nested <defaultDimensions> in your <signalFxReporter> stanza:
```xml
  <signalFxReporter apiToken="AAABQWDCC" sourceType="netbios" sampleInterval="00:00:05"> 
    <defaultDimensions>
      <defaultDimension name="environment" value="prod"/>
      <defaultDimension name="serverType" value="API"/>
    </defaultDimensions>
  </signalFxReporter>
```

## Additional Metrics
Metrics.NET is designed for measuring for continuously generated metrics and reporting these metrics from unique sources.
This does not fit all use cases. For example, sometimes the "where" something was counted doesn't matter, or something may not be measured continuously. In order to support these use cases Metrics.NET.SignalFx supports some additional metric types

### Incremental Metrics
Sometimes the "where" something is measured is not important, or it might be measured across several servers and need to be grouped by some other dimension (i.e. a customer id). In order for SignalFx to properly account for these types of metrics the following metric types should be used:

#### Counter
By default Metrics.Net counters are cumulative counters (i.e they send every increasing values 0,2,10,20,30). To get a Counter that only sends deltas, use TaggedMetricsContext.IncrementalCounter.

```csharp
public TaggedMetricContext getContext() {
   return (TaggedMetricsContext)Metric.Context("app", (ctxName) => { return new TaggedMetricsContext(ctxName); });
}

//Setup counters for API usage
public void setupCounters(string env) {
    this.loginAPICount = getContext().IncrementalCounter("api.use", Unit.Calls, new MetricTags("environment="+env, "api_type=login"));
    this.purchaseAPICount = getContext().IncrementalCounter("api.use", Unit.Calls, new MetricTags("environment="+env, "api_type=purchase"));
}
....
public void login() {
    this.loginAPICount.Increment();
    ....
}
```
#### Timer
This timer records just like a regular Timer, but it just reports a delta count and an average of samples. These are the only values that are useful in a distributed situation. To get a Timer that only sends deltas, use TaggedMetricsContext.IncrementalTimer.
```csharp
public TaggedMetricContext getContext() {
   return (TaggedMetricsContext)Metric.Context("app", (ctxName) => { return new TaggedMetricsContext(ctxName); });
}

//Setup counters for API usage
public void setupCounters(string env) {
    this.loginAPTime = getContext().IncrementalTimer("api", Unit.Requests, tags: new MetricTags("environment="+env, "api_type=login"));
    this.purchaseAPITime= getContext().IncrementalTimer("api", Unit.Requests, tags: new MetricTags("environment="+env, "api_type=purchase"));
}
....
public void login() {
   using (var context = this.loginAPITime(customerId, "purchase")).NewContext())
    {
        processLogin()
        .....
        // if needed elapsed time is available in context.Elapsed 
    }
}
```

### Non Continuous Metrics
Metrics.NET was designed for continously measurable metrics, however not all metrics fit this profile. If you are recording timings per customer the rate at which a particular customer hits maybe low. The ReportOnUpdate\* metrics are
designed for these use cases:
  * TaggedMetricsContext.ReportOnUpdateCounter
  * TaggedMetricsContext.ReportOnUpdateTimer
  * TaggedMetricsContext.ReportOnUpdateMeter
  * TaggedMetricsContext.ReportOnUpdateHistogram
All of these metrics act exactly the same as the underlying type, however they only report data points to SignalFx when a sample has been added. 
  
```csharp
public TaggedMetricContext getContext() {
   return (TaggedMetricsContext)Metric.Context("app", (ctxName) => { return new TaggedMetricsContext(ctxName); });
}

private Counter getCustomerCounter(string customerId) {
   return getContext().ReportOnUpdateCounter("api.use", Unit.Calls, new MetricTags("customerId="+customerId));
}
private Timer getCustomerAPITimer(string customerId, string api) {
   return getContext().ReportOnUpdateTimer("api.time", Unit.Requests, new MetricTags("api="+api, "customerId="+customerId));
}

public void purchaseAPI(String item, String userId, String customerId, double price) {
    getCustomerCounter(customerId).inc();
    using (var context = getCustomerAPITimer(customerId, "purchase").NewContext())
    {
        processPurchase(item, userId, customerId, price);
        .....
        // if needed elapsed time is available in context.Elapsed 
    }
}
```
