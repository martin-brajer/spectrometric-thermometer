using System.IO;
using System;

namespace spectrometric_thermometer
{ 
    public partial class SpectrometricThermometer
    {
        /// <summary>
        /// Represents parameters needed for spectra manipulation.
        /// </summary>
        public class Parameters
        {
            /// <summary>
            /// Defaults.
            /// </summary>
            public Parameters()
            : this(
                save: false,
                rewrite: true,
                filenameIndex: 0,
                filenameIndexLength: 1,
                periodLength: 2f,
                average: 1,
                exposureTime: 1f,
                adaptation: false,
                filename: "Spectrum",
                delimiter: "    ")
            { }

            /// <summary>
            /// Preferably instantiate through static Parse methods.
            /// </summary>
            /// <param name="save"></param>
            /// <param name="rewrite"></param>
            /// <param name="filenameIndex"></param>
            /// <param name="filenameIndexLength"></param>
            /// <param name="periodLength"></param>
            /// <param name="average"></param>
            /// <param name="exposureTime"></param>
            /// <param name="adaptation"></param>
            /// <param name="filename"></param>
            public Parameters(bool save, bool rewrite, int filenameIndex,
                int filenameIndexLength, float periodLength, int average,
                float exposureTime, bool adaptation, string filename, string delimiter)
            {
                Save = save;
                Rewrite = rewrite;
                FilenameIndex = filenameIndex;
                FilenameIndexLength = filenameIndexLength;
                PeriodLength = periodLength;
                Average = average;
                ExposureTime = exposureTime;
                AutoExposureTime = adaptation;
                Filename = filename ?? throw new ArgumentNullException(nameof(filename));
                Delimiter = delimiter;
            }

            public bool Save { get; set; }
            public bool Rewrite { get; set; }
            public int FilenameIndex { get; set; }
            public int FilenameIndexLength { get; set; }
            public float PeriodLength { get; set; }
            public int Average { get; set; }
            public float ExposureTime { get; set; }
            public bool AutoExposureTime { get; set; }
            public string Filename { get; set; }
            public string Delimiter { get; set; }

            /// <summary>
            /// Handle file numbering. If enabled, add one to index.
            /// </summary>
            /// <returns></returns>
            public string FilenameIndexText()
            {
                if (Rewrite)
                {
                    return "";
                }
                else
                {
                    string filenameIndexText = FilenameIndex.ToString(
                        string.Format("D{0}", FilenameIndexLength));

                    if (IsAllNines(filenameIndexText))
                    {
                        FilenameIndexLength++;
                    }
                    FilenameIndex++;
                    return filenameIndexText;
                }
            }

            private bool IsAllNines(string input)
            {
                foreach (char ch in input)
                {
                    if (ch != '9') { return false; }
                }
                return true;
            }

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
            /// <param name="adaptation">Does expoure time adapt to light conditions?</param>
            /// <param name="adaptationMaxExposureTime">While "adaptation" is allowed,
            /// set upper bound on exposure time.</param>
            /// <param name="filename"></param>
            /// <returns></returns>
            public static Parameters Parse(
                bool save = false,
                bool rewrite = true,
                string filenameIndex = "0",
                string periodLength = "2",
                string average = "1",
                string exposureTime = "1",
                bool adaptation = false,
                string filename = "Spectrum",
                string delimiter = "    ")
            {
                // "save" cannot be wrong.
                // "rewrite" cannot be wrong.

                int filenameIndexInt = -1;
                if (!rewrite)  // Ignore numbering while rewriting.
                {
                    if (!int.TryParse(filenameIndex, out filenameIndexInt))
                    {
                        throw new ArgumentException(string.Format(
                            "Numbering error! Converted value: {0}.", filenameIndexInt));
                    }
                }
                int filenameIndexLength = filenameIndex.Length;

                if (!float.TryParse(periodLength, out float periodLengthFloat))
                {
                    throw new ArgumentException(string.Format(
                        "Period error! Converted value: {0}.", periodLengthFloat));
                }

                if (!int.TryParse(average, out int averageInt))
                {
                    throw new ArgumentException(string.Format(
                        "Average error! Converted value: {0}.", average));
                }
                if (averageInt < 1)
                {
                    throw new ArgumentException("Average error! Must be positive.");
                }

                if (!float.TryParse(exposureTime, out float exposureTimeFloat))
                {
                    throw new ArgumentException(string.Format(
                        "Exposure time error! Converted value: {0}.", exposureTime));
                }

                // "adaptation" cannot be wrong.

                if (!IsValidFileNameOrPath(filename))
                {
                    throw new ArgumentException("Path error!");
                }
                string directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return Parse(save, rewrite, filenameIndexInt,
                    filenameIndexLength, periodLengthFloat, averageInt,
                    exposureTimeFloat, adaptation, filename, delimiter);
            }

            public static Parameters Parse(
                bool save = false,
                bool rewrite = true,
                int filenameIndex = 0,
                int filenameIndexLength = 1,
                float periodLength = 2,
                int average = 1,
                float exposureTime = 1,
                bool adaptation = false,
                string filename = "Spectrum",
                string delimiter = "    ")
            {
                if (periodLength < exposureTime)
                {
                    throw new ArgumentException("Period must be longer or equal than exposure time.");
                }

                return new Parameters(save, rewrite, filenameIndex,
                    filenameIndexLength, periodLength, average,
                    exposureTime, adaptation, filename, delimiter);
            }

            /// <summary>
            /// Validity of path.
            /// </summary>
            /// <param name="name">Path to be checked.</param>
            /// <returns>Correct?</returns>
            public static bool IsValidFileNameOrPath(string name)
            {
                if (name == null)
                {
                    return false;
                }
                // Determines if there are bad characters in the name.
                foreach (char badChar in Path.GetInvalidPathChars())
                {
                    if (name.IndexOf(badChar) > -1)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}