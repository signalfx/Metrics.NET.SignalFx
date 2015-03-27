using System.Configuration;

namespace Metrics.SignalFx.Configuration
{
    public class DefaultDimensionConfigurationCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DefaultDimension();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DefaultDimension)element).Name;
        }
    }


}
