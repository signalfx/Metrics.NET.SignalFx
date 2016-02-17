using System;
using System.Configuration;

namespace Metrics.SignalFx.Configuration
{
    public class SignalFxReporterConfiguration : ConfigurationSection
    {
        public static SignalFxReporterConfiguration FromConfig(string section = "signalFxReporter")
        {
            return (SignalFxReporterConfiguration)ConfigurationManager.GetSection(section);
        }

        [ConfigurationProperty("apiToken", IsRequired = true)]
        public String APIToken
        {
            get { return (String)this["apiToken"]; }
            set { this["apiToken"] = value; }
        }

        [ConfigurationProperty("sampleInterval", DefaultValue = "00:00:05", IsRequired = false)]
        [TimeSpanValidator(MinValueString = "00:00:01", ExcludeRange = false)]
        public TimeSpan SampleInterval
        {
            get { return (TimeSpan)this["sampleInterval"]; }
            set { this["sampleInterval"] = value; }
        }

        [ConfigurationProperty("sourceDimension", IsRequired = false)]
        public String SourceDimension
        {
            get { return (String)this["sourceDimension"]; }
            set { this["sourceDimension"] = value; }
        }

        [ConfigurationProperty("sourceType", IsRequired = false, DefaultValue=SourceType.none)]
        public SourceType SourceType
        {
            get { return (SourceType)this["sourceType"]; }
            set { this["sourceType"] = value; }
        }

        [ConfigurationProperty("sourceValue", IsRequired = false)]
        public String SourceValue
        {
            get { return (String)this["sourceValue"]; }
            set { this["sourceValue"] = value; }
        }

        [ConfigurationProperty("baseURI", DefaultValue = "https://ingest.signalfx.com", IsRequired = false)]
        public String BaseURI
        {
            get { return (String)this["baseURI"]; }
            set { this["baseURI"] = value; }
        }

        [ConfigurationProperty("awsIntegration", IsRequired = false, DefaultValue = "false")]
        public bool AwsIntegration
        {
            get { return (bool)this["awsIntegration"]; }
            set { this["awsIntegration"] = value; }
        }

        [ConfigurationProperty("maxDatapointsPerMessage", DefaultValue = "10000", IsRequired = false)]
        [IntegerValidator(MinValue = 100, MaxValue = 10000, ExcludeRange = false)]
        public int MaxDatapointsPerMessage
        {
            get { return (int)this["maxDatapointsPerMessage"]; }
            set { this["maxDatapointsPerMessage"] = value; }
        }

        [ConfigurationProperty("defaultDimensions", IsDefaultCollection = true, IsRequired = false)]
        [ConfigurationCollection(typeof(DefaultDimensionConfigurationCollection), AddItemName = "defaultDimension")]
        public DefaultDimensionConfigurationCollection DefaultDimensions
        {
            get { return (DefaultDimensionConfigurationCollection)this["defaultDimensions"]; }
            set { this["defaultDimensions"] = value; }
        }

        [ConfigurationProperty("metricDetails", DefaultValue="count,min,mean,min", IsRequired = false)]
        public String MetricDetails
        {
            get { return (String)this["metricDetails"]; }
            set { this["metricDetails"] = value; }
        }
    }
}
