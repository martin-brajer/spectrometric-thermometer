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
            MSpectraProcessor.DataReady += SpectraProcessor_DataReady;

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
        public SpectraProcessor MSpectraProcessor { get; set; } = new SpectraProcessor();
        private Parameters mParameters = new Parameters();
        public TemperatureHistory MTemperatureHistory { get; set; } = new TemperatureHistory();

        /// <summary>
        /// PID
        /// </summary>
        public PIDController PID { get; set; } = new PIDController(
            bufferLen: 3,
            period: 4.5);

        /// <summary>
        /// 
        /// </summary>
        public Spectrometer Spectrometer { get; set; } = null;
        
        /// <summary>
        /// Find absorbtion edge wavelength
        /// Add corresponding temperature and time
        /// to their respective fields.
        /// </summary>
        public double AnalyzeMeasurement()
        {
            double? temperature = MSpectraProcessor.Temperature;
            if (temperature != null)
            {
                MTemperatureHistory.Add((double)temperature, MSpectraProcessor.Time);
            }
            return (double)temperature;
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
            var wavelengths = MSpectraProcessor.Wavelengths;
            var minDifference = double.MaxValue;
            int clickedIndex = 0;
            for (int i = 0; i < wavelengths.Length; i++)
            {
                double difference = Math.Abs(wavelengths[i] - clickedWavelength);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    clickedIndex = i;
                }
            }
            MTemperatureHistory.RemoveLast();
            MSpectraProcessor.MaxDerivativeIndex = clickedIndex;
            MSpectraProcessor.OnDataReady(reanalyze: true);

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
        /// <param name="deviceID"></param>
        /// <param name="exposurePeriodTimes"></param>
        /// <returns></returns>
        public bool BtnSelect(int deviceID)
        {
            try
            {
                Spectrometer.SelectDevice(deviceID);
            }
            catch (IndexOutOfRangeException ex)
            {
                Front.My_msg("Out of range: " + ex.Message);
                return false;
            }

            Spectrometer.EraceDeviceList();
            Spectrometer.Open();
            Front.My_msg("Openning " + Spectrometer.ModelName + " : " + Spectrometer.SerialNo);
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
            Spectrometer.UseAdaptation = stParameters.AutoExposureTime;
            MSpectraProcessor.SpectraToLoad = stParameters.Average;

            try
            {
                ISpectrometerParse spectrometer = Spectrometer;
                Spectrometer.MParameters = Spectrometer.Parameters.Parse(
                    periodLength: stParameters.PeriodLength,
                    exposureTime: stParameters.ExposureTime,
                    adaptation: stParameters.AutoExposureTime,
                    spectrometer: ref spectrometer);
            }
            catch (ArgumentException ex)
            {
                Front.My_msg(ex.Message);
                return false;
            }

            MSpectraProcessor.MaxDerivativeIndex = null;  // Reset.


            Front.My_msg("Parameters set & measuring");
            if (MSpectraProcessor.SpectraToLoad > 1)
            {
                float time = Spectrometer.Period * MSpectraProcessor.SpectraToLoad;
                Front.My_msg(string.Format("Spectrum ready every {0} s.", time));
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
        public bool ConfigurationFile_Load()
        {
            Regex regex = new Regex(delimiter);
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
                                MSpectraProcessor.MParameters.PointsToSkip = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_eps":
                        {
                            if (double.TryParse(items[1], out double output))
                                MSpectraProcessor.MParameters.EpsilonLimit = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth1":
                        {
                            if (int.TryParse(items[1], out int output))
                                MSpectraProcessor.MParameters.SmoothingIntensities = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth2":
                        {
                            if (int.TryParse(items[1], out int output))
                                MSpectraProcessor.MParameters.SmoothingDerivatives = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_1DHalfW":
                        {
                            if (int.TryParse(items[1], out int output))
                                MSpectraProcessor.MParameters.SearchHalfWidth = output;
                            else
                                Front.My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_slider":
                        {
                            if (double.TryParse(items[1], out double output))
                                MSpectraProcessor.MParameters.SliderLimit = output;
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

                    case "inchwormMethod_left":
                            switch (items[1])
                            {
                                case "old":
                                    MSpectraProcessor.MParameters.InchwormMethodLeft = SpectraProcessor.InchwormMethod.Old;
                                    break;
                                case "vit":
                                    MSpectraProcessor.MParameters.InchwormMethodLeft = SpectraProcessor.InchwormMethod.Vit;
                                    break;
                                case "constant":
                                    MSpectraProcessor.MParameters.InchwormMethodLeft = SpectraProcessor.InchwormMethod.Constant;
                                    break;
                                default:
                                    MSpectraProcessor.MParameters.InchwormMethodLeft = SpectraProcessor.InchwormMethod.Constant;
                                    break;
                            }
                        break;
                    case "inchwormMethod_right":
                        switch (items[1])
                        {
                            case "old":
                                MSpectraProcessor.MParameters.InchwormMethodRight = SpectraProcessor.InchwormMethod.Old;
                                break;
                            case "vit":
                                MSpectraProcessor.MParameters.InchwormMethodRight = SpectraProcessor.InchwormMethod.Vit;
                                break;
                            case "constant":
                                MSpectraProcessor.MParameters.InchwormMethodRight = SpectraProcessor.InchwormMethod.Constant;
                                break;
                            default:
                                MSpectraProcessor.MParameters.InchwormMethodRight = SpectraProcessor.InchwormMethod.Constant;
                                break;
                        }
                        break;

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
                return true;
            }
            Front.BtnCalibrationEnabled = true;
            // ComboBox.
            Front.CBoxCalibrationDataSource = calibComboBox;
            SelectCalibration(0);
            return true;
        }

        /// <summary>
        /// Return last temperature.
        /// </summary>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public double BtnLoadSpectra(string[] fileNames)
        {
            MSpectraProcessor.SpectraToLoad = 1;  // No averaging.

            double temperature = -1;
            bool save = mParameters.Save;  // Just load, no saving. Add 'siletn' AddSpectra().
            mParameters.Save = false;
            foreach (string file in fileNames)
            {
                DateTime modification = File.GetLastWriteTime(file);
                var waveIntens = LoadData(file);

                MSpectraProcessor.AddSpectra(
                    wavelengths: waveIntens.Item1,
                    intensities: waveIntens.Item2,
                    ticks: modification.Ticks);

                MSpectraProcessor.MaxDerivativeIndex = null;
                temperature = AnalyzeMeasurement();
            }
            mParameters.Save = save;  // Reset.
            return temperature;
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
                column1[i] = LoadData_parse(text: fields[0]);
                column2[i] = LoadData_parse(text: fields[1]);
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
            if (MTemperatureHistory.IsEmpty)
            {
                Front.My_msg("No initial temperature! Setting DAC voltage only.");

                DAC.SetV(0, 1, save: false);
                DAC.SetV(outputVoltage, 2);
                return false;
            }
            // Initial temperature.
            PID.Reset(MTemperatureHistory.Temperatures.Last());
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
            MSpectraProcessor.Calibration = Calibrations[SelectedIndex];
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
        private static double LoadData_parse(string text)
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
        /// When spectroscopic data are ready, write to file, analyze and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpectraProcessor_DataReady(object sender, SpectraProcessor.DataReadyEventArgs e)
        {
            // Save to file.
            if (mParameters.Save)
            {
                string fileNameIndexText = mParameters.FilenameIndexText();
                Front.FilenameIndex = fileNameIndexText;
                WriteColumns(
                    filename: mParameters.Filename + fileNameIndexText + ".dat",
                    col1: MSpectraProcessor.Wavelengths,
                    col2: MSpectraProcessor.Intensities);
            }
            if (e.MultipleSpectraLoaded)
            {
                Front.LabelBoldAverage(true);
            }
            Front.PlotRightTitleTemperature = AnalyzeMeasurement();
            Front.Plot(MSpectraProcessor, MTemperatureHistory);
        }

        /// <summary>
        /// ExposureFinished event => Read and write spectrum.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Spectrometer_ExposureFinished(object sender, Spectrometer.ExposureFinishedEventArgs e)
        {
            MSpectraProcessor.AddSpectra(
                wavelengths: Spectrometer.Wavelengths,
                intensities: Array.ConvertAll(Spectrometer.Intensities, v => (double)v),
                ticks: Spectrometer.Time.Ticks);

            if (mParameters.AutoExposureTime)
            {
                Front.LabelBoldAutoExposureTime(e.Adapted);
                Front.ExposureTime = e.ExposureTime;
            }
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
                time: MTemperatureHistory.Times.Last(),
                temperature: MTemperatureHistory.Temperatures.Last(),
                vOld: DAC.LastWrittenValue);

            DAC.SetV(MTemperatureHistory.Temperatures.Last() / 100, 1, save: false);
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