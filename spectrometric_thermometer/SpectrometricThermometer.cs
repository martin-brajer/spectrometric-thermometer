using System.IO;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace spectrometric_thermometer
{
    public interface ISpectrometricThermometer
    {
        ISpectrometer Spectrometer { get; set; }
        List<ICalibration> Calibrations { get; set; }
        Measurement Measurement { get; set; }
        DigitalToAnalogConverter DAC { get; set; }
        PIDController PID { get; set; }
        AnalogToDigitalConverter_switcher ADC { get; set; }

        List<double> Temperatures { get; set; }
        List<double> Times { get; set; }
        DateTime TimeZero { get; set; }

        void WriteColumns(string filename, double[] col1, double[] col2);
        Tuple<double[], double[]> LoadData(string filename, string delimiter = null);
        string AnalyzeMeasurement();
        string AnalyzeMeasurement(double clickedWavelength);
        void DisconnectSpectrometer();

        event EventHandler<SpectrometricThermometer.PidUpdatedEventArgs> PidUpdated;
    }

    //[Export(typeof(ISpectrometricThermometer))]
    public class SpectrometricThermometer : ISpectrometricThermometer
    {
        /// <summary>
        /// Delimiter used in WriteColumns() and ConfigurationFileLoad().
        /// </summary>
        private readonly string delimiter = "    ";
        /// <summary>
        /// Filled in BtnInit_Click() => InitState.Initialize.
        /// If no spectrometer chosen => null.
        /// </summary>
        public ISpectrometer Spectrometer { get; set; } = null;
        /// <summary>
        /// List of Calibrations as found in "Config.cfg".
        /// </summary>
        public List<ICalibration> Calibrations { get; set; } = new List<ICalibration>();
        /// <summary>
        /// Digital to analog converter.
        /// Heater control.
        /// </summary>
        public DigitalToAnalogConverter DAC { get; set; } = null;
        /// <summary>
        /// Analog to digital converter.
        /// Eurotherm readout.
        /// </summary>
        public AnalogToDigitalConverter_switcher ADC { get; set; }
        /// <summary>
        /// PID
        /// </summary>
        public PIDController PID { get; set; } = new PIDController(
            bufferLen: 3,
            period: 4.5);

        /// <summary>
        /// Single measurement data.
        /// </summary>
        public Measurement Measurement { get; set; } = new Measurement();
        public List<double> Temperatures { get; set; } = new List<double>();
        public List<double> Times { get; set; } = new List<double>();
        /// <summary>
        /// Time of the first measurement (press of the Measure button).
        /// From this time, seconds are counted.
        /// </summary>
        public DateTime TimeZero { get; set; }

        private readonly Action<string> My_msg = null;
        public event EventHandler<PidUpdatedEventArgs> PidUpdated;

        /// <summary>
        /// Start spectrometer exposure.
        /// </summary>
        private Timer timerMeasure = new Timer();
        /// <summary>
        /// PID control.
        /// </summary>
        private Timer timerPID = new Timer();
        /// <summary>
        /// This program to Eurotherm switching.
        /// </summary>
        private Timer timerSwitch = new Timer();
        /// <summary>
        /// The first waiting time is short and without measuring.
        /// </summary>
        private bool firstWait = true;

        public SpectrometricThermometer(Action<string> my_msg)
        {
            // Event handler inicialization.
            timerMeasure.Tick += new EventHandler(TimerSpectra_Tick);

            timerPID.Tick += new EventHandler(TimerPID_Tick);
            timerPID.Interval = (int)(PID.Period * 1000);

            timerSwitch.Tick += new EventHandler(TimerSwitch_Tick);
            timerSwitch.Interval = 1000;  // 1 sec.

            My_msg = my_msg;
        }

        /// <summary>
        /// Tick every 10 sec. Changes DAC voltage
        /// based on temperature set point nad PID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerPID_Tick(object sender, EventArgs e)
        {

            var VNewAndInfo = PID.Process(
                time: Times[Times.Count - 1],
                temperature: Temperatures[Temperatures.Count - 1],
                vOld: DAC.LastWrittenValue);

            DAC.SetV(Temperatures[Temperatures.Count - 1] / 100, 1, save: false);
            DAC.SetV(VNewAndInfo.Item1, 2);

            OnPidUpdated(
                voltage: string.Format("{0:#.00}", DAC.LastWrittenValue),
                info: VNewAndInfo.Item2);
        }

        protected virtual void OnPidUpdated(string voltage, string info)
        {
            PidUpdated?.Invoke(this, new PidUpdatedEventArgs(voltage, info));
        }
        
        /// <summary>
        /// Timer tick => StartExposure().
        /// And exposure time adaptation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerSpectra_Tick(object sender, EventArgs e)
        {
            // If unplugged!
            if (Spectrometer.CheckDeviceRemoved())
            {
                My_msg("Device removed!");
                return;
            }

            if (firstWait)
            {
                firstWait = false;
                timerMeasure.Interval = (int)(Spectrometer.Period * 1000);
            }
            Spectrometer.StartExposure();
        }

        /// <summary>
        /// Disconnect the device
        /// and write closing message.
        /// </summary>
        public void DisconnectSpectrometer()
        {
            if (Spectrometer is null)
                return;

            if (Spectrometer.DisconnectDevice())
            {
                My_msg("Closing");
                Spectrometer = null;
            }
        }

        /// <summary>
        /// Validity of path.
        /// </summary>
        /// <param name="name">Path to be checked.</param>
        /// <returns>Correct?</returns>
        public static bool IsValidFileNameOrPath(string name)
        {
            // Determines if the name is Nothing.
            if (name is null)
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

            // The name passes basic validation.
            return true;
        }

        /// <summary>
        /// Find absorbtion edge wavelength
        /// Add corresponding temperature and time
        /// to their respective fields.
        /// </summary>
        public string AnalyzeMeasurement()
        {
            double temp;
            //try
            {
                temp = Measurement.Analyze();
            }
            //catch (InvalidOperationException)
            //{
            //    return;
            //}

            // Save it.
            Temperatures.Add(temp);
            Times.Add(Measurement.Time.Subtract(TimeZero).TotalSeconds);

            return string.Format("T = {0:0.0} °C", temp);
        }

        /// <summary>
        /// Find again absorbtion edge wavelength around clicked point.
        /// Rewrite corresponding temperature and time.
        /// </summary>
        /// <param name="clickedWavelength"></param>
        public string AnalyzeMeasurement(double clickedWavelength)
        {
            // clickedWavelength => clickedIndex.
            // Apply any defensive coding here as necessary.
            var values = Measurement.Wavelengths;
            var minDifference = double.MaxValue;
            int clickedIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                double difference = Math.Abs(values[i] - clickedWavelength);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    clickedIndex = i;
                }
            }

            Measurement.IndexMax1D = clickedIndex;

            if (Temperatures.Any()) //prevent IndexOutOfRangeException for empty list
            {
                Temperatures.RemoveAt(Temperatures.Count - 1);
            }
            if (Times.Any()) //prevent IndexOutOfRangeException for empty list
            {
                Times.RemoveAt(Times.Count - 1);
            }

            return AnalyzeMeasurement();
        }

        /// <summary>
        /// Write two double columns into a file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        public void WriteColumns(string filename, double[] col1, double[] col2)
        {
            // Prepare the string array to be written.
            int lines = col1.Length;
            string[] mWrite = new string[lines];
            for (int i = 0; (i <= (lines - 1)); i++)
            {
                mWrite[i] = col1[i]
                    + delimiter + col2[i];
            }

            // Actually write wavelenghts and intensities to a "*.dat" file.
            File.WriteAllLines(filename, mWrite);
        }

        /// <summary>
        /// Load txt file with two columns divided by 'delimiter'.
        /// </summary>
        /// <param name="filename">Path.</param>
        /// <param name="delimiter">Default null means to use program-wide constant 'delimiter'.</param>
        /// <returns>Tuple of two double arrays.</returns>
        public Tuple<double[], double[]> LoadData(string filename, string delimiter = null)
        {
            if (delimiter == null)
                delimiter = this.delimiter;

            string[] lines = File.ReadAllLines(filename);
            // SpectraSuite Data File recognize.
            if (lines.Length > 0 && lines[0] == "SpectraSuite Data File")
            {
                lines = LoadData_SpectraSuiteDataFile(lines, delimiter);
            }

            Regex regex = new Regex(delimiter);

            int rows = lines.Length;
            double[] column1 = new double[rows];
            double[] column2 = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                string line = lines[i];
                // Last line.
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                string[] fields = regex.Split(line);
                column1[i] = _LoadData_parse(text: fields[0]);
                column2[i] = _LoadData_parse(text: fields[1]);
            }

            return Tuple.Create(column1, column2);
        }

        /// <summary>
        /// Delete header and lat line in SpectraSuiteDataFile.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string[] LoadData_SpectraSuiteDataFile(
            string[] input, string delimiter)
        {
            int len = input.Length - 17 - 1;
            string[] output = new string[len];
            Array.Copy(input, 17, output, 0, len);
            for (int i = 0; i < len; i++)
                output[i] = output[i].Replace(',', '.').Replace("\t", delimiter);
            return output;
        }

        /// <summary>
        /// Help method to LoadData().
        /// Parsing loaded data to double.
        /// </summary>
        /// <exception cref="FileLoadException">Wrong file data format.</exception>
        /// <param name="text">Text to parse.</param>
        /// <returns>Parsed.</returns>
        private static double _LoadData_parse(string text)
        {
            if (double.TryParse(text, out double result))
            {
                return result;
            }
            else
            {
                throw new FileLoadException("Exception raised while parsing the file.");
            }

        }

        /// <summary>
        /// Voltage value and info string.
        /// </summary>
        public class PidUpdatedEventArgs : EventArgs
        {
            public PidUpdatedEventArgs(string voltage, string info)
            {
                Voltage = voltage;
                Info = info;
            }

            public string Voltage { get; private set; }
            public string Info { get; private set; }
        }
    }
}