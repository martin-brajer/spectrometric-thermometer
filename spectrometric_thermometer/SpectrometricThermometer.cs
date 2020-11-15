using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.IO;

namespace spectrometric_thermometer
{
    public partial class SpectrometricThermometer
    {
        /// <summary>
        /// Start spectrometer exposure.
        /// </summary>
        public Timer timerMeasure = new Timer();

        /// <summary>
        /// Delimiter used in WriteColumns() and ConfigurationFileLoad().
        /// </summary>
        private readonly string delimiter = "    ";
        private readonly Back2Front Front = null;

        /// <summary>
        /// Temperature control mode.
        /// Used while switching temperature control device (this program or Eurotherm).
        /// </summary>
        private TemperatureControlMode temperatureControlMode = TemperatureControlMode.None;

        /// <summary>
        /// PID control.
        /// </summary>
        private Timer timerPID = new Timer();

        /// <summary>
        /// This program to Eurotherm switching.
        /// </summary>
        private Timer timerSwitch = new Timer();

        public SpectrometricThermometer(Back2Front front)
        {
            Front = front ?? throw new ArgumentNullException(nameof(front));
            
            // Events.
            Measurement.AveragingFinished += Measurement_AveragingFinished;

            // Config (+calibration).
            ConfigurationFile_Load();

            // Event handler inicialization.
            timerMeasure.Tick += new EventHandler(TimerSpectra_Tick);

            timerPID.Tick += new EventHandler(TimerPID_Tick);
            timerPID.Interval = (int)(PID.Period * 1000);

            timerSwitch.Tick += new EventHandler(TimerSwitch_Tick);
            timerSwitch.Interval = 1000;  // 1 sec.
        }

        /// <summary>
        /// Allowed colors.
        /// </summary>
        private enum LEDcolour
        {
            Off,
            Green,
            Red,
        }

        /// <summary>
        /// Temperature-control-device switching assist. This program to Eurotherm and back.
        /// </summary>
        private enum TemperatureControlMode
        {
            /// <summary>
            /// Nothing is happening.
            /// </summary>
            None = 0,
            /// <summary>
            /// Waiting for switching from Eurotherm to this program.
            /// </summary>
            ET2PC_switch = 1,
            /// <summary>
            /// Waiting for equalizing the two voltages by hand (this program to Eurotherm).
            /// </summary>
            PC2ET_equal = 2,
            /// <summary>
            /// Waiting for switching from this program to Eurotherm.
            /// </summary>
            PC2ET_switch = 3,
        }

        /// <summary>
        /// Analog to digital converter. Eurotherm readout.
        /// </summary>
        public AnalogToDigitalConverter_switcher ADC { get; set; } = null;

        /// <summary>
        /// List of Calibrations as found in "Config.cfg".
        /// </summary>
        public List<ICalibration> Calibrations { get; set; } = new List<ICalibration>();

        /// <summary>
        /// Digital to analog converter. Heater control.
        /// </summary>
        public DigitalToAnalogConverter DAC { get; set; } = null;

        /// <summary>
        /// Single measurement data.
        /// </summary>
        public Measurement Measurement { get; set; } = new Measurement();
        private Parameters mParameters = Parameters.Parameters_Default;

        public TemperatureHistory MTemperatureHistory { get; set; } = new TemperatureHistory();

        /// <summary>
        /// PID
        /// </summary>
        public PIDController PID { get; set; } = new PIDController(
            bufferLen: 3,
            period: 4.5);

        /// <summary>
        /// Filled in BtnInit_Click() => InitState.Initialize.
        /// If no spectrometer chosen => null.
        /// </summary>
        public Spectrometer Spectrometer { get; set; } = null;
        
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
        public double AnalyzeMeasurement()
        {
            double temperature = Measurement.LastTemperature;
            MTemperatureHistory.Add(temperature, Measurement.Time);
            return temperature;
        }

        /// <summary>
        /// Find again absorbtion edge wavelength around clicked point.
        /// Rewrite corresponding temperature and time.
        /// </summary>
        /// <param name="clickedWavelength"></param>
        public double AnalyzeMeasurement(double clickedWavelength)
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
            MTemperatureHistory.RemoveLast();

            // RETRIGGER ANALYSE MEASUREMENT

            return AnalyzeMeasurement();
        }

        /// <summary>
        /// Disconnect the device
        /// and write closing message.
        /// </summary>
        public bool BtnDisconnect()
        {
            if (Spectrometer is null)
                return false;

            if (Spectrometer.DisconnectDevice())
            {
                Front.My_msg("Closing");
                Spectrometer = null;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool BtnInitialize(int selectedIndex)
        {
            try
            {
                Spectrometer = Spectrometer.Factory.Create(selectedIndex);
            }
            catch (DllNotFoundException ex)
            {
                Front.My_msg(ex.Message);
                return false;
            }

            Spectrometer.ExposureFinished += Spectrometer_ExposureFinished;

            // Use the choosen spectrometer class to search for devices.
            Spectrometer.SearchDevices();
            if (Spectrometer.NumberOfDevicesFound == 0)
            {
                Front.My_msg("No spectrometer found.");
                return false;  // initState remains the same.
            }
            else
            {
                Front.My_msg("Spectrometers found:");
                for (int deviceIndex = 0; deviceIndex < Spectrometer.NumberOfDevicesFound; deviceIndex++)
                {
                    Spectrometer.SelectDevice(deviceIndex);
                    Front.My_msg("  " + deviceIndex + "... " + Spectrometer.ModelName + " : " + Spectrometer.SerialNo);
                }
                Spectrometer.SelectDevice();
            }
            return true;
        }

        public bool BtnReloadConfig()
        {
            ConfigurationFile_Load();
            return true;
        }

        public bool BtnSaveTemperatures()
        {
            WriteColumns(
                  Path.GetDirectoryName(mParameters.Filename) + "/TemperatureHistory.txt",
                  MTemperatureHistory.Times,
                  MTemperatureHistory.Temperatures);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choosenDevice"></param>
        /// <param name="exposurePeriodTimes"></param>
        /// <returns></returns>
        public bool BtnSelect(int choosenDevice, out string exposureTime, out string periodTime)
        {
            exposureTime = periodTime = "";
            try
            {
                Spectrometer.SelectDevice(choosenDevice);
            }
            catch (IndexOutOfRangeException ex)
            {
                Front.My_msg("Out of range: " + ex.Message);
                return false;
            }

            Spectrometer.EraceDeviceList();
            Spectrometer.Open();
            Front.My_msg("Openning " + Spectrometer.ModelName + " : " + Spectrometer.SerialNo);

            exposureTime = string.Format("{0:0.0}", Spectrometer.ExposureTime.ToString());
            periodTime = string.Format("{0:0.0}", Spectrometer.Period.ToString());
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stParameters"></param>
        /// <returns></returns>
        public bool BtnStartMeasurement(Parameters stParameters)
        {
            mParameters = stParameters;
            Spectrometer.UseAdaptation = stParameters.Adaptation;
            Measurement.SpectraToLoad = stParameters.Average;

            try
            {
                ISpectrometerParse spectrometer = Spectrometer;
                Spectrometer.MParameters = Spectrometer.Parameters.Parse(
                    periodLength: stParameters.PeriodLength,
                    exposureTime: stParameters.ExposureTime,
                    adaptation: stParameters.Adaptation,
                    spectrometer: ref spectrometer);
            }
            catch (ArgumentException ex)
            {
                Front.My_msg(ex.Message);
                return false;
            }

            if (!MTemperatureHistory.Any())  // Is empty?
            {
                MTemperatureHistory.TimeZero = DateTime.Now;
            }
            Measurement.IndexMax1D = -1;  // Reset.


            Front.My_msg("Parameters set & measuring");
            if (Measurement.SpectraToLoad > 1)
            {
                float time = Spectrometer.Period * Measurement.SpectraToLoad;
                Front.My_msg("Spectrum saved every " + time.ToString() + " s.");
            }
            timerMeasure.Interval = (int)(Spectrometer.Period * 1000);
            timerMeasure.Start();
            return true;
        }

        public bool BtnStopMeasurement()
        {
            timerMeasure.Stop();
            Spectrometer.CancelExposure();
            return true;
        }

        public void Close()
        {
            DAC.Close();
            ADC.Close();
            BtnDisconnect();
        }

        /// <summary>
        /// Load config from "Config.cfg".
        /// Mainly calibration files.
        /// </summary>
        public void ConfigurationFile_Load()
        {
            Regex regex = new Regex("    ");
            //Regex regex = new Regex(delimiter);
            string[] lines = File.ReadAllLines("Config.cfg");

            Calibrations.Clear();
            List<string> calibComboBox = new List<string>();

            foreach (string line in lines)
            {
                string[] items = regex.Split(line);
                switch (items[0])
                {
                    case "calib_points":
                        {
                            try
                            {
                                var output = LoadData(items[1] + ".clb");
                                Calibrations.Add(new Calibration_Points(output.Item1, output.Item2));
                            }
                            catch (FileNotFoundException e)
                            {
                                Front.My_msg(string.Format(
                                    "Config.cfg: FileError: '{0}'.", e.Message));
                                break;
                            }
                            calibComboBox.Add(items[1]);
                            break;
                        }
                    case "calib_polynom":
                        {
                            try
                            {
                                var output = LoadData(items[1] + ".clb");
                                Calibrations.Add(new Calibration_Polynom(output.Item2));
                            }
                            catch (FileNotFoundException e)
                            {
                                Front.My_msg(string.Format(
                                    "Config.cfg: FileError: '{0}'.", e.Message));
                                break;
                            }
                            calibComboBox.Add(items[1]);
                            break;
                        }
                    case "const_skip":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.MParameters.Const_skip = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_eps":
                        {
                            if (double.TryParse(items[1], out double output))
                                Measurement.MParameters.Const_eps = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth1":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.MParameters.Const_smooth1 = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth2":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.MParameters.Const_smooth2 = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_1DHalfW":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.MParameters.Const_1DHalfW = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_slider":
                        {
                            if (double.TryParse(items[1], out double output))
                                Measurement.MParameters.Const_slider = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "DAC":
                        {
                            switch (items[1])
                            {
                                case "lab":
                                    DAC = new DigitalToAnalogConverter.Lab();
                                    break;
                                case "offline":
                                    DAC = new DigitalToAnalogConverter.Offline();
                                    break;
                                default:
                                    Front.My_msg(string.Format(
                                        "Config.cfg: DAC keyError: '{0}'. Setting offline.", items[1]));
                                    DAC = new DigitalToAnalogConverter.Offline();
                                    break;
                            }
                            break;
                        }
                    case "DAC_maxStep":
                        {
                            if (double.TryParse(items[1], out double output))
                                PID.VDeltaAbsMax = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "PID_P":
                        {
                            Front.Pid_P = items[1];
                            break;
                        }
                    case "PID_I":
                        {
                            Front.Pid_I = items[1];
                            break;
                        }
                    case "PID_D":
                        {
                            Front.Pid_D = items[1];
                            break;
                        }

                    case "PID_period":
                        {
                            if (double.TryParse(items[1], out double output))
                                PID.Period = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }

                    case "absorbtion_edge":
                        {
                            // Default case.
                            if (!new[] { "Inchworm", "Inchworm_VIT", "const" }.Contains(items[1]))
                            {
                                Measurement.MParameters.Absorbtion_edge = "const";
                                break;
                            }
                            else
                            {
                                Measurement.MParameters.Absorbtion_edge = items[1];
                            }
                            break;
                        }

                    default:
                        Front.My_msg(string.Format("Config.cfg: KeyError: '{0}'.", items[0]));
                        break;
                }
            }

            // If DAC not in config file.
            if (DAC == null)
                DAC = new DigitalToAnalogConverter.Offline();

            if (Calibrations.Count == 0)
            {
                Front.My_msg("No calibration found!");
                Front.BtnCalibrationEnabled = false;
                return;
            }
            Front.BtnCalibrationEnabled = true;
            // ComboBox.
            Front.CBoxCalibrationDataSource = calibComboBox;
            SelectCalibration(0);
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
        /// 
        /// </summary>
        /// <returns>Success?</returns>
        public bool PIDOff()
        {
            timerPID.Stop();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputVoltage"></param>
        /// <returns>Success?</returns>
        public bool PIDOn(double outputVoltage)
        {
            if (DAC.PortName == null)  // Port is null.
            {
                Front.My_msg("DAC not found.");
                return false;
            }
            if (!MTemperatureHistory.Any())
            {
                Front.My_msg("No initial temperature! Setting DAC voltage only.");

                DAC.SetV(0, 1, save: false);
                DAC.SetV(outputVoltage, 2);
                return false;
            }
            // Initial temperature.
            PID.Reset(MTemperatureHistory.TemperaturesLast);
            // Try it now first (set initV).
            TimerPID_Tick(this, EventArgs.Empty);
            timerPID.Start();
            return true;
        }

        public void ResetTemperatureHistory()
        {
            MTemperatureHistory.Clear();
        }

        public void SelectCalibration(int SelectedIndex)
        {
            Measurement.Calibration = Calibrations[SelectedIndex];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Switch(out double outputVoltage)
        {
            outputVoltage = double.NaN;
            if (temperatureControlMode == TemperatureControlMode.None)
            {
                if (!double.IsNaN(outputVoltage))
                {
                    // Switched to eurotherm, so switching to this app.
                    if (ADC.Switched2Eurotherm())
                    {
                        outputVoltage = ADC.ReadEurothermV();
                        // Round to 12b - this should be given by DAC.
                        outputVoltage = Math.Round(outputVoltage / 10 * 4096) / 409.6;  // ??
                        DAC.SetV(outputVoltage, 2);

                        Front.My_msg("You can switch to PC.");
                        LED(LEDcolour.Green);
                    }
                    // Switched to this app, so switching to eurotherm.
                    else
                    {
                        double percent = DAC.LastWrittenValue / 5 * 100;  // Percentage out of 5 V.
                        var a = DAC.GetV();
                        Front.My_msg("XX  P" + percent + "  DL" + DAC.LastWrittenValue + "  a0" + a[0] + "  a1" + a[1]);  // Analyze. DELETE!!!

                        Front.My_msg("Set Eurotherm to Manual / " + percent.ToString("F1") + " %.");
                        LED(LEDcolour.Red);
                    }
                    timerSwitch.Start();
                    temperatureControlMode = TemperatureControlMode.ET2PC_switch;
                }
                else
                {
                    temperatureControlMode = TemperatureControlMode.PC2ET_equal;
                }
            }
            // Sth is already happening => abort.
            else
            {
                timerSwitch.Stop();
                LED(LEDcolour.Off);
                temperatureControlMode = TemperatureControlMode.None;
            }
            return temperatureControlMode == TemperatureControlMode.None;
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
        /// Is spectrometer unplugged?
        /// If yes, press Stop and Disconnect buttons.
        /// </summary>
        /// <returns>Removed?</returns>
        private bool CheckDeviceRemoved()
        {
            bool removed = Spectrometer.CheckDeviceRemoved();
            if (removed)
            {
                Front.My_msg("Spectrometer status: " + Spectrometer.Status());
                Front.Disconnect();
            }
            return removed;
        }

        /// <summary>
        /// Light app LED and hardware LED (if available).
        /// </summary>
        /// <param name="colour"></param>
        private void LED(LEDcolour colour)
        {
            switch (colour)
            {
                case LEDcolour.Green:
                    Front.LEDColor = KnownColor.ForestGreen;
                    if (ADC.PortName != null)
                        ADC.LED_Green();
                    break;

                case LEDcolour.Red:
                    Front.LEDColor = KnownColor.IndianRed;
                    if (ADC.PortName != null)
                        ADC.LED_Red();
                    break;

                case LEDcolour.Off:
                    Front.LEDColor = null;
                    if (ADC.PortName != null)
                        ADC.LED_Off();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// When averaging is complete, write to file, analyze and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Measurement_AveragingFinished(object sender, EventArgs e)
        {
            // Save to file.
            if (mParameters.Save)
            {
                string fileNameIndexText = mParameters.FilenameIndexText();
                Front.FilenameIndex = fileNameIndexText;
                WriteColumns(
                    filename: mParameters.Filename + fileNameIndexText + ".dat",
                    col1: Measurement.Wavelengths,
                    col2: Measurement.Intensities);
            }

            double temperature = AnalyzeMeasurement();
            Front.Plot(Measurement, MTemperatureHistory, title: temperature.ToString());
            
        }

        /// <summary>
        /// ExposureFinished event => Read and write spectrum.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Spectrometer_ExposureFinished(object sender, Spectrometer.ExposureFinishedEventArgs e)
        {
            Measurement.Load(
                wavelengths: Spectrometer.Wavelengths,
                intensities: Array.ConvertAll(Spectrometer.Intensities, v => (double)v),
                ticks: Spectrometer.Time.Ticks);
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
                time: MTemperatureHistory.TimesLast,
                temperature: MTemperatureHistory.TemperaturesLast,
                vOld: DAC.LastWrittenValue);

            DAC.SetV(MTemperatureHistory.TemperaturesLast / 100, 1, save: false);
            DAC.SetV(VNewAndInfo.Item1, 2);

            Front.PidVoltage = string.Format("{0:#.00}", DAC.LastWrittenValue);
            Front.PidInfo = VNewAndInfo.Item2;
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
                Front.My_msg("Device removed!");
                return;
            }
            Spectrometer.StartExposure();
        }
        /// <summary>
        /// Periodically check switching progress.
        /// </summary>
        /// <exception cref="ApplicationException">Wrong TemperatureControlMode.</exception>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerSwitch_Tick(object sender, EventArgs e)
        {
            switch (temperatureControlMode)
            {
                case TemperatureControlMode.None:
                    throw new ApplicationException("Wrong mode.");

                case TemperatureControlMode.ET2PC_switch:
                    if (!ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        temperatureControlMode = TemperatureControlMode.None;
                        Front.SwitchButtonTextIndex = 0;
                        LED(LEDcolour.Off);
                    }
                    break;

                case TemperatureControlMode.PC2ET_equal:
                    // If switched before the voltages are equalized.
                    if (ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        temperatureControlMode = TemperatureControlMode.None;
                        Front.SwitchButtonTextIndex = 0;
                        LED(LEDcolour.Off);
                        break;
                    }
                    // If difference is lesser than error.
                    Front.My_msg("XX AR" + ADC.ReadEurothermV() + "  AL" + DAC.LastWrittenValue);
                    if (Math.Abs(ADC.ReadEurothermV() - DAC.LastWrittenValue) < 0.05)
                    {
                        temperatureControlMode = TemperatureControlMode.PC2ET_switch;
                        Front.My_msg("You can switch to Eurotherm.");
                        LED(LEDcolour.Green);
                    }
                    break;

                case TemperatureControlMode.PC2ET_switch:
                    // If difference is greater again than error.
                    if (Math.Abs(ADC.ReadEurothermV() - DAC.LastWrittenValue) >= 0.05)
                    {
                        temperatureControlMode = TemperatureControlMode.PC2ET_equal;
                        LED(LEDcolour.Red);
                    }
                    if (ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        temperatureControlMode = TemperatureControlMode.None;
                        Front.SwitchButtonTextIndex = 0;
                        LED(LEDcolour.Off);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}