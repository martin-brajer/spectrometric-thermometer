﻿using System;

namespace spectrometric_thermometer
{

    public abstract partial class Spectrometer
    {
        /// <summary>
        /// Represents spectrometer parameters needed to measure a spectrum.
        /// </summary>
        public class Parameters
        {
            public Parameters()
                : this(periodLength: 0f, exposureTime: 0f, adaptation: false)
            { }

            public Parameters(float periodLength, float exposureTime, bool adaptation)
            {
                PeriodLength = periodLength;
                ExposureTime = exposureTime;
                Adaptation = adaptation;
            }

            float PeriodLength { get; set; }
            float ExposureTime { get; set; }
            bool Adaptation { get; set; }

            public static Parameters Parse(string periodLength, string exposureTime,
                bool adaptation, ref ISpectrometerParse spectrometer)
            {
                throw new NotImplementedException();
            }

            public static Parameters Parse(float periodLength, float exposureTime,
                bool adaptation, ref ISpectrometerParse spectrometer)
            {
                // Spectrometer bounds.
                if (exposureTime < spectrometer.MinExposureTime)
                {
                    throw new ArgumentException(
                        "Minimal exposure time is " + spectrometer.MinExposureTime.ToString());
                }
                if (exposureTime > spectrometer.MaxExposureTime)
                {
                    throw new ArgumentException(
                        "Maximal exposure time is " + spectrometer.MaxExposureTime.ToString());
                }
                
                return new Parameters(periodLength: periodLength, exposureTime: exposureTime,
                    adaptation: adaptation);
            }
        }
    }
}