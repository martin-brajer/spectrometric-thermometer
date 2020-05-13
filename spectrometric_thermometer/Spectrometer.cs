using System;
using System.IO;
using System.Linq;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Representss the base class for the classes
    /// that mediate specific spectrometer devices
    /// functionality.
    /// <para>The class uses seconds as time unit.</para>
    /// <para>Instantiate subclasses via <see cref="Factory"/>.</para>
    /// </summary>
    public abstract partial class Spectrometer : IDisposable
    {
        // Saturation intensity used in ExposureTimeAdaptation.
        protected float saturationLevel = 0f;

        // Constants.
        // How often is checked, if exposure is finished, if no such a event is available.
        protected readonly float timeReserve = 0.1f;  // Seconds.

        /// <summary>
        /// Event on finished exposure.
        /// </summary>
        public event EventHandler<ExposureFinishedEventArgs> ExposureFinished;

        /// <summary>
        /// Data properties. Wavelenghts array.
        /// </summary>
        public double[] Wavelengths { get; protected set; }

        /// <summary>
        /// Data properties. Intensities array.
        /// </summary>
        public float[] Intensities { get; protected set; }

        /// <summary>
        /// Time properties. Time of the last measurement.
        /// Preciselly of the ExposureEvent invoke.
        /// </summary>
        public DateTime Time { get; protected set; }

        /// <summary>
        /// Model name.
        /// Read from device if available.
        /// </summary>
        public abstract string ModelName { get; }

        /// <summary>
        /// Serial number.
        /// Read from device if available.
        /// </summary>
        public abstract string SerialNo
        {
            get;
        }

        /// <summary>
        /// Device exposure time.
        /// Set by ParameterCheck() method.
        /// </summary>
        public abstract float ExposureTime { get; protected set; }

        /// <summary>
        /// Used in local automatic exposure time correction.
        /// Derived from exposure time input.
        /// </summary>
        public float MaxExposureTimeUser { get; protected set; } = float.PositiveInfinity;

        /// <summary>
        /// Device maximal exposure time.
        /// </summary>
        public abstract float MaxExposureTime { get; }

        /// <summary>
        /// Device minimal exposure time.
        /// </summary>
        public abstract float MinExposureTime { get; }

        /// <summary>
        /// Duration of the whole cycle: exposure + wait.
        /// </summary>
        public float Period { get; protected set; }

        /// <summary>
        /// If true, run <see cref="ExposureTimeAdaptation"/>
        /// in <see cref="OnExposureFinished"/> method.
        /// </summary>
        public bool UseAdaptation { get; set; } = false;

        /// <summary>
        /// Number of devices found by SearchDevices() method.
        /// </summary>
        public int NumberOfDevicesFound { get; protected set; } = 0;

        /// <summary>
        /// Creator.
        /// </summary>
        public Spectrometer()
        {
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Spectrometer()
        {
            //I am *not* calling you from Dispose, it's *not* safe
            Dispose(false);
        }

        /// <summary>
        /// Event invoking method on finished exposure.
        /// If allowed, run <see cref="ExposureTimeAdaptation"/>
        /// and send info through <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnExposureFinished(object sender, EventArgs e)
        {
            SaveSpectrum();
            Time = DateTime.Now;

            bool adapted = false;
            if (UseAdaptation)
            {
                adapted = ExposureTimeAdaptation();
            }
            ExposureFinished?.Invoke(this, new ExposureFinishedEventArgs(adapted));
        }

        /// <summary>
        /// Search for spectrometers.
        /// Fill numberOfDevicesFound.
        /// </summary>
        public abstract void SearchDevices();

        /// <summary>
        /// Erace list of devices found by SearchDevices().
        /// Use before open, when the list is not needed anymore.
        /// </summary>
        public abstract void EraceDeviceList();

        /// <summary>
        /// Select one of the found spectrometers.
        /// Overload with int parameter.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Param index greater than number of devices found.</exception>
        /// <param name="index">Index of the array.</param>
        public abstract void SelectDevice(int index);

        /// <summary>
        /// Deselect the selected spectrometer.
        /// Overload with no parameter.
        /// </summary>
        public abstract void SelectDevice();

        /// <summary>
        /// Return true, if a device is selected.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSelected();

        /// <summary>
        /// Open and init.
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// Is the device open?
        /// </summary>
        /// <returns>Is it?</returns>
        public abstract bool IsOpen();

        /// <summary>
        /// Close the device.
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// GUI parameters parsing.
        /// </summary>
        /// <exception cref="ArgumentException">Some parameters are wrong.</exception>
        /// <param name="tBoxPeriod">Period.</param>
        /// <param name="tBoxExpTime">Exposure time.</param>
        /// <param name="chBoxRewrite">Rewrite?</param>
        /// <param name="chBoxAdaptation">Expoure time adaptation?</param>
        /// <param name="tBoxAdaptation">While allowed, input max exposure time.</param>
        public void ParameterCheck(
            string tBoxPeriod,
            string tBoxExpTime,
            bool chBoxAdaptation,
            string tBoxAdaptation)
        {
            // Exposure time
            ExposureTime = ParameterCheck_ExposureLikeTime(tBoxExpTime);

            // Period.
            // Must be below exposure time.
            if (tBoxPeriod.Contains(","))  // CZ decimal point character.
            {
                throw new ArgumentException("Period contains ','.");
            }
            if (float.TryParse(tBoxPeriod, out float period))
            {
                if (period < ExposureTime)
                {
                    throw new ArgumentException("Period must be longer or equal than exposure time.");
                }
            }
            else
            {
                throw new ArgumentException("Period error!" + " Converted value: " + period + ".");
            }
            Period = period;

            // Exposure time adaptation.
            UseAdaptation = chBoxAdaptation;
            if (UseAdaptation)
            {
                float maxExposureTimeUser = ParameterCheck_ExposureLikeTime(tBoxAdaptation);
                if (maxExposureTimeUser < ExposureTime)
                {
                    throw new ArgumentException("Max ET Error!" + " Higher than exposure time.");
                }
                MaxExposureTimeUser = maxExposureTimeUser;
            }
        }

        /// <summary>
        /// Try parse exposure time.
        /// </summary>
        /// <exception cref="ArgumentException">Wrong exposure time format.</exception>
        /// <param name="text">String input.</param>
        /// <returns>Parsed input.</returns>
        private float ParameterCheck_ExposureLikeTime(string text)
        {
            // Exposure time
            if (text.Contains(","))  // CZ decimal point character.
            {
                throw new ArgumentException("Exposure time contains ','.");
            }
            if (float.TryParse(text, out float exposureTime))
            {
                // Spectrometer bounds.
                if (exposureTime < MinExposureTime)
                    throw new ArgumentException("Minimal exposure time is " + MinExposureTime.ToString());
                if (exposureTime > MaxExposureTime)
                    throw new ArgumentException("Maximal exposure time is " + MaxExposureTime.ToString());
            }
            else
            {
                throw new ArgumentException("Exposure time error!" + " Converted value: " + exposureTime + ".");
            }
            return exposureTime;
        }

        /// <summary>
        /// Start exposure.
        /// </summary>
        public abstract void StartExposure();

        /// <summary>
        /// Cancel exposure.
        /// </summary>
        public abstract void CancelExposure();

        /// <summary>
        /// Automatic exposure time adaptation.
        /// Settings:
        ///     30 % of points in the top 10 % intensities or
        ///     max intensity lower than last saturation level / 5.
        /// Initial exposure time shall never be exceeded.
        /// </summary>
        /// <returns>Was exposure time altered?</returns>
        public bool ExposureTimeAdaptation()
        {
            float exposureTime = ExposureTime;
            float maxInt = Intensities.Max();
            // Lower limit of top X percent. X = 10.
            float topPercent = 10;
            float lowTopInt = maxInt * (1 - (topPercent / 100));

            // Count percentage of points being in the top X %.
            int topCount = 0;
            foreach (var intensity in Intensities)
            {
                if (intensity >= lowTopInt)
                    topCount++;
            }
            // Float topCount, so it can be divided by intensities.Lenght (int).
            float topPoints = (float)topCount / Intensities.Length * 100;
            // More than 30 % points in top X % of intensity.
            if (topPoints > 30)
            {
                exposureTime = exposureTime / 2;
                saturationLevel = maxInt;
            }
            // Maximum lower than one fifth of saturation.
            else if (maxInt < saturationLevel / 5)
            {
                exposureTime = exposureTime * 2;
            }
            // No problem found.
            else
            {
                return false;  // No change this time.
            }

            // Bounds.
            exposureTime = Math.Min(exposureTime,
                Math.Min(MaxExposureTimeUser, MaxExposureTime));  // User and spectrometer defined max.
            exposureTime = Math.Max(exposureTime, MinExposureTime);  // Default min.

            ExposureTime = exposureTime;  // Finally write to device.
            return true;  // Exposure time has been changed.
        }

        /// <summary>
        /// Is spectrometer unplugged?
        /// </summary>
        /// <returns>Removed?</returns>
        public abstract bool CheckDeviceRemoved();

        /// <summary>
        /// Device status string.
        /// </summary>
        /// <returns>tatus.</returns>
        public abstract string Status();

        /// <summary>
        /// Check if the device is taking spectrum.
        /// </summary>
        /// <returns>Is it?</returns>
        public abstract bool StatusIsTakingSpectrum();

        /// <summary>
        /// Get spectrum and save it to intensities array.
        /// </summary>
        public abstract void SaveSpectrum();

        /// <summary>
        /// Disconnect the device.
        /// </summary>
        /// <returns>Should <see cref="Form_main"/> print closing message?</returns>
        public bool DisconnectDevice()
        {
            bool msg = false;  // Return value. If true, print "Closing" message.

            if (IsSelected())
            {
                if (IsOpen())
                {
                    if (StatusIsTakingSpectrum())
                    {
                        CancelExposure();
                    }
                    Close();
                    msg = true;
                }
                SelectDevice();
            }
            return msg;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing">itIsSafeToAlsoFreeManagedObjects</param>
        protected abstract void Dispose(bool disposing);


        /// <summary>
        /// Factory creating <see cref="Spectrometer"/> subclasses instances.
        /// Include information about all the subtypes.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Data storage for individual <see cref="Spectrometer"/>
            /// subclasses parameters.
            /// </summary>
            private static readonly Description[] spectrometer_array = new Description[]
            {
                new Description(name: "Offline test", dll: null, device: Device.Offline_test),
            };

            /// <summary>
            /// Instantiate of one of <see cref="Spectrometer"/> subclasses
            /// based on <see cref="Device"/> value.
            /// Check whether respective library exists.
            /// </summary>
            /// <exception cref="IndexOutOfRangeException"></exception>
            /// <exception cref="DllNotFoundException"></exception>
            /// <exception cref="ArgumentException">Wrong Device value.</exception>
            /// <returns>Appropriate Spectrometer subclass instance.</returns>
            /// <param name="i">Number of spectrometer type found in <see cref="ListNames"/>.</param>
            /// <returns>Spectrometer subclass.</returns>
            public static Spectrometer Create(int i)
            {
                Description spectrometer = spectrometer_array[i];
                string dll = spectrometer.Dll;

                // Null for spectrometer_offline. I.e. don't check.
                if (!string.IsNullOrEmpty(dll))
                {
                    // Verify whether the needed library exists.
                    if (!File.Exists(dll))
                    {
                        // Not found.
                        throw new DllNotFoundException("File '" + dll + "' not found!");
                    }
                }

                return Create(spectrometer.Device);
            }

            /// <summary>
            /// Instantiate of one of <see cref="Spectrometer"/> subclasses.
            /// </summary>
            /// <exception cref="ArgumentException">Wrong Device value.</exception>
            /// <returns>Appropriate Spectrometer subclass instance.</returns>
            /// <param name="device">Spectrometer type.</param>
            /// <returns>Spectrometer subclass.</returns>
            public static Spectrometer Create(Device device)
            {
                // Return respective class instance.
                switch (device)
                {
                    case Device.Offline_test:
                        return new Offline();

                    default:
                        throw new ArgumentException("Wrong 'device' parameter in creator.");
                }
            }

            /// <summary>
            /// List spectrometers
            /// </summary>
            /// <returns></returns>
            public static string[] ListNames()
            {
                string[] names = new string[spectrometer_array.Length];
                for (int i = 0; i < spectrometer_array.Length; i++)
                {
                    names[i] = spectrometer_array[i].Name;
                }
                return names;
            }
        }

        /// <summary>
        /// Describe individual spectrometer class parameters.
        /// </summary>
        private struct Description
        {
            /// <summary>
            /// Creator.
            /// </summary>
            /// <param name="name">Spectrometer name.</param>
            /// <param name="dll">Respective dll file name.
            /// Will be checked whether exists.</param>
            /// <param name="device">Conected to actual Spectrometer class descendant.</param>
            public Description(string name, string dll, Device device)
            {
                Name = name;
                Dll = dll;
                Device = device;
            }

            /// <summary>
            /// Spectrometer type name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Library (*.dll) filename.
            /// </summary>
            public string Dll { get; private set; }

            /// <summary>
            /// <summary>
            /// Spectrometer type.
            /// </summary>
            public Device Device { get; private set; }
        }

        /// <summary>
        /// Spectrometer type.
        /// To be connected to the actual Spectrometer
        /// class descendant instance.
        /// </summary>
        public enum Device
        {
            /// <summary>
            /// Virtual spectrometer for testing purposes.
            /// </summary>
            Offline_test = 0,
        }

        /// <summary>
        /// Carry information (bool) about exposure time adaptation.
        /// True means adaptation was needed this turn.
        /// </summary>
        public class ExposureFinishedEventArgs : EventArgs
        {
            /// <summary>
            /// Creator.
            /// </summary>
            /// <param name="adapted">Was the exposure
            /// time adapted this turn?</param>
            public ExposureFinishedEventArgs(bool adapted)
            {
                Adapted = adapted;
            }

            /// <summary>
            /// Was the exposure time adapted this turn?
            /// </summary>
            public bool Adapted { get; private set; }
        }
    }
}