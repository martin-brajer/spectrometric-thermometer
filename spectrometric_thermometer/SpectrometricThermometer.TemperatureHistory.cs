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
        }

        public class TemperatureHistory : ITemperatureHistory
        {
            private readonly List<double> temperatures;
            /// <summary>
            /// Time of the first measurement (press of the Measure button).
            /// <see cref="times"/> are counted relative to this time.
            /// </summary>
            private DateTime timeZero;
            /// <summary>
            /// In seconds.
            /// </summary>
            private readonly List<double> times;
            private int length;

            public double[] Temperatures => temperatures.ToArray();
            public double[] Times => times.ToArray();

            /// <summary>
            /// Determines whether this history contains any elements.
            /// </summary>
            /// <returns></returns>
            public bool IsEmpty => length == 0;

            public TemperatureHistory()
            {
                temperatures = new List<double>();
                times = new List<double>();
                timeZero = DateTime.Now;
                length = 0;
            }

            public void Add(double temperature, DateTime time)
            {
                temperatures.Add(temperature);
                if (IsEmpty)
                {
                    timeZero = time;
                }
                times.Add(time.Subtract(timeZero).TotalSeconds);
                length++;
            }

            /// <summary>
            /// Remove last element. Pass if empty.
            /// </summary>
            public void RemoveLast()
            {
                if (length > 0)
                {
                    temperatures.RemoveAt(length - 1);
                    times.RemoveAt(length - 1);
                    length--;
                }
            }

            public void Clear()
            {
                temperatures.Clear();
                times.Clear();
                length = 0;
            }
        }
    }
}