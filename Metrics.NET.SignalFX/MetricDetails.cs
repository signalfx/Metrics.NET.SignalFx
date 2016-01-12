using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.SignalFX
{
    public enum MetricDetails
    {
        last,
        active_sessions,
        median,
        percent,
        percent_75,
        percent_95,
        percent_98,
        percent_99,
        percent_999,
        max,
        min,
        stddev,
        mean,
        count,
        rate_mean,
        rate_1min,
        rate_5min,
        rate_15min
    }
}
