using System;
using System.Collections.Generic;
using System.Linq;

namespace spectrometric_thermometer
{
    public partial class SpectrometricThermometer
    {
        public interface ITemperatureHistory
        {
            double[] Temperatures { get; }
            double[] Times { get; }
            DateTime TimeZero { get; }
        }

        public class TemperatureHistory : ITemperatureHistory
        {
            private readonly List<double> _temperatures;
            private readonly List<double> _times;
            private int length;

            public double TemperaturesLast => _temperatures.Last();
            public double TimesLast => _times.Last();

            public double[] Temperatures => _temperatures.ToArray();
            public double[] Times => _times.ToArray();
            /// <summary>
            /// Time of the first measurement (press of the Measure button).
            /// From this time, seconds are counted.
            /// </summary>
            public DateTime TimeZero { get; set; }

            public TemperatureHistory()
            {
                _temperatures = new List<double>();
                _times = new List<double>();
                TimeZero = DateTime.Now;
                length = 0;
            }

            /// <summary>
            /// Determines whether this history contains any elements.
            /// </summary>
            /// <returns></returns>
            public bool Any()
            {
                return length > 0;
                //return _temperatures.Any() || _times.Any();
            }

            public void Add(double temperature, DateTime time)
            {
                _temperatures.Add(temperature);

                double totalSeconds = time.Subtract(TimeZero).TotalSeconds;
                _times.Add(totalSeconds);

                length++;
            }

            /// <summary>
            /// Remove last element. Pass if empty.
            /// </summary>
            public void RemoveLast()
            {
                if (length > 0)
                {
                    _temperatures.RemoveAt(length - 1);
                    _times.RemoveAt(length - 1);
                    length--;
                }
            }

            public void Clear()
            {
                _temperatures.Clear();
                _times.Clear();
                TimeZero = DateTime.Now;
                length = 0;
            }
        }
    }
}