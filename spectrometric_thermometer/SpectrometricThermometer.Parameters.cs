using System.IO;
using System;

namespace spectrometric_thermometer
{ 
    public partial class SpectrometricThermometer
    {
        /// <summary>
        /// Represents general parameters.
        /// </summary>
        public struct Parameters
        {
            public Parameters(bool save, bool rewrite, int filenameIndex,
                int filenameIndexLength, float periodLength, int average,
                float exposureTime, bool adaptation, float adaptationMaxExposureTime,
                string filename) : this()
            {
                Save = save;
                Rewrite = rewrite;
                FilenameIndex = filenameIndex;
                FilenameIndexLength = filenameIndexLength;
                PeriodLength = periodLength;
                Average = average;
                ExposureTime = exposureTime;
                Adaptation = adaptation;
                AdaptationMaxExposureTime = adaptationMaxExposureTime;
                Filename = filename ?? throw new ArgumentNullException(nameof(filename));
            }

            public bool Save { get; }
            public bool Rewrite { get; }
            public int FilenameIndex { get; }
            public int FilenameIndexLength { get; }
            public float PeriodLength { get; }
            public int Average { get; }
            public float ExposureTime { get; }
            public bool Adaptation { get; }
            public float AdaptationMaxExposureTime { get; }
            public string Filename { get; }

            /// <summary>
            /// Parse measurement parameters to be handed over to <see cref="ISpectrometer"/>.
            /// </summary>
            /// <exception cref="ArgumentException">One of the arguments is wrong.</exception>
            /// <param name="save">Do save measured spectra?</param>
            /// <param name="rewrite">Keep rewriting one file or number new ones.</param>
            /// <param name="filenameIndex">First file number. Trailing zeros matter.</param>
            /// <param name="periodLength"></param>
            /// <param name="average">How many spectra to average before handling the results.</param>
            /// <param name="exposureTime"></param>
            /// <param name="adaptation">Does expoure time adapt to light condition?</param>
            /// <param name="adaptationMaxExposureTime">While "adaptation" is allowed,
            /// set upper bound on exposure time.</param>
            /// <param name="filename"></param>
            /// <returns></returns>
            public static Parameters Parse(
                bool save,
                bool rewrite,
                string filenameIndex,
                string periodLength,
                string average,
                string exposureTime,
                bool adaptation,
                string adaptationMaxExposureTime,
                string filename)
            {
                // "save" cannot be wrong.
                // "rewrite" cannot be wrong.

                int filenameIndexInt = -1;
                if (!rewrite)  // Ignore numbering while rewriting.
                {
                    if (!int.TryParse(filenameIndex, out filenameIndexInt))
                    {
                        throw new ArgumentException("Numbering error!" + " Converted value: " + filenameIndexInt + ".");
                    }
                }
                int filenameIndexLength = filenameIndex.Length;

                if (!float.TryParse(periodLength, out float periodLengthFloat))
                {
                    throw new ArgumentException("Period error!" + " Converted value: " + periodLengthFloat + ".");
                }

                if (!int.TryParse(average, out int averageInt))
                {
                    throw new ArgumentException("Average error!" + " Converted value: " + average + ".");
                }
                if (averageInt < 1)
                {
                    throw new ArgumentException("Average error!" + " Must be positive.");
                }

                if (!float.TryParse(exposureTime, out float exposureTimeFloat))
                {
                    throw new ArgumentException("Exposure time error!" + " Converted value: " + exposureTime + ".");
                }

                // "adaptation" cannot be wrong.

                if (!float.TryParse(adaptationMaxExposureTime, out float adaptationMaxExposureTimeFloat))
                {
                    throw new ArgumentException("Adaptation time error!" + " Converted value: " + adaptationMaxExposureTime + ".");
                }

                if (!IsValidFileNameOrPath(filename))
                {
                    throw new ArgumentException("Path error!");
                }
                string directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    //My_msg("Creating '" + directory + "'.");
                }


                if (periodLengthFloat < exposureTimeFloat)
                {
                    throw new ArgumentException("Period must be longer or equal than exposure time.");
                }

                return new Parameters(save, rewrite, filenameIndexInt,
                    filenameIndexLength, periodLengthFloat, averageInt,
                    exposureTimeFloat, adaptation, adaptationMaxExposureTimeFloat, filename);
            }
        }
    }
}