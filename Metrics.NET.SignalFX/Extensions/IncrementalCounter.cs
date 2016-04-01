using Metrics.Core;
using Metrics.MetricData;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.SignalFX.Extensions
{
    public class IncrementalCounter : CounterImplementation
    {
        public static readonly string INC_COUNTER_PREFIX = "aaasignalfxinccounteraaaaa";
        private AtomicLong counter = new AtomicLong();
        private long lastValue = 0;
        public CounterValue Value
        {
            get
            {
                var currentTotalCounter = this.counter.Value;
                var currentTotalCounterChange = currentTotalCounter - this.lastValue;
                this.lastValue = currentTotalCounter;
                return new CounterValue(currentTotalCounterChange, new CounterValue.SetItem[0]);
            }
        }

        public CounterValue GetValue(bool resetMetric = false)
        {
            var value = this.Value;
            if (resetMetric)
            {
                this.Reset();
            }
            return value;
        }

        public void Increment()
        {
            this.counter.Increment();
        }

        public void Increment(long value)
        {
            this.counter.Add(value);
        }

        public void Decrement()
        {
            this.counter.Decrement();
        }

        public void Decrement(long value)
        {
            this.counter.Add(-value);
        }

        public void Reset()
        {
            this.counter.SetValue(0L);
        }

        public void Increment(string item)
        {
            this.Increment();
        }

        public void Increment(string item, long amount)
        {
            this.Increment(amount);
        }

        public void Decrement(string item)
        {
            this.Decrement();
        }

        public void Decrement(string item, long amount)
        {
            this.Decrement(amount);

        }
    }
}
