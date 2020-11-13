using System;

namespace spectrometric_thermometer
{

    public abstract partial class Spectrometer
    {
        /// <summary>
        /// Represents spectrometer parameters needed to measure a spectrum.
        /// </summary>
        public struct Parameters
        {
            public Parameters(float periodLength, float exposureTime, bool adaptation, float adaptationET) : this()
            {
                PeriodLength = periodLength;
                ExposureTime = exposureTime;
                Adaptation = adaptation;
                AdaptationET = adaptationET;
            }

            float PeriodLength { get; }
            float ExposureTime { get; }
            bool Adaptation { get; }
            float AdaptationET { get; }

            public static Parameters Parse(float periodLength, float exposureTime, bool adaptation, float adaptationET, ref ISpectrometerParse spectrometer)
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
                
                if (spectrometer.UseAdaptation)
                {
                    if (adaptationET < exposureTime)
                    {
                        throw new ArgumentException("Max ET Error!" + " Higher than exposure time.");
                    }
                }

                return new Parameters(
                    periodLength: periodLength,
                    exposureTime: exposureTime,
                    adaptation: adaptation,
                    adaptationET: adaptationET);
            }
        }
    }
}