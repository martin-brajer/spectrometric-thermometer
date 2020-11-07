using System.IO;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace spectrometric_thermometer
{
    public interface ISpectrometricThermometer
    { }

    //[Export(typeof(ISpectrometricThermometer))]
    public partial class SpectrometricThermometer : ISpectrometricThermometer
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

        private readonly IBack2Front Front = null;
        
        /// <summary>
        /// Start spectrometer exposure.
        /// </summary>
        public Timer timerMeasure = new Timer();
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
        public bool firstWait = true;
        /// <summary>
        /// File path + filename without optional numbers and extension.
        /// </summary>
        private string fileNamePart;
        /// <summary>
        /// Numbering of the saved files.
        /// </summary>
        private int fileNameNumber = 0;


        public SpectrometricThermometer(IBack2Front front)
        {
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

            Front = front ?? throw new ArgumentNullException(nameof(front));
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

            if (firstWait)
            {
                firstWait = false;
                timerMeasure.Interval = (int)(Spectrometer.Period * 1000);
            }
            Spectrometer.StartExposure();
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
        /// 
        /// </summary>
        /// <returns></returns>
        public bool BtnInitialize()
        {
            try
            {
                Spectrometer = Spectrometer.Factory.Create(cBoxSpect.SelectedIndex);
            }
            catch (DllNotFoundException ex)
            {
                Front.My_msg(ex.Message);
                return false;
            }

            // ExposureFinished event.
            Spectrometer.ExposureFinished += new EventHandler<Spectrometer.ExposureFinishedEventArgs>(Spectrometer_ExposureFinished);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choosenDevice"></param>
        /// <param name="exposurePeriodTimes"></param>
        /// <returns></returns>
        public bool BtnSelect(int choosenDevice, out Tuple<string, string> exposurePeriodTimes)
        {
            exposurePeriodTimes = new Tuple<string, string> ( "", "" );
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

            exposurePeriodTimes = new Tuple<string, string>(
                string.Format("{0:0.0}", Spectrometer.ExposureTime.ToString()),
                string.Format("{0:0.0}", Spectrometer.Period.ToString()));
            return true;
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
        /// <param name="parameters"></param>
        /// <returns></returns>
        public bool BtnStartMeasurement(Parameters parameters)
        {
            // Spectrometer bounds.
            if (exposureTime < MinExposureTime)
                throw new ArgumentException("Minimal exposure time is " + MinExposureTime.ToString());
            if (exposureTime > MaxExposureTime)
                throw new ArgumentException("Maximal exposure time is " + MaxExposureTime.ToString());

            Measurement.SpectraToLoad = parameters.Average;
            fileNamePart = parameters.Filename;
            Spectrometer.UseAdaptation = parameters.Adaptation;
            if (Spectrometer.UseAdaptation)
            {
                float maxExposureTimeUser = parameters.AdaptationMaxExposureTime;
                if (maxExposureTimeUser < parameters.ExposureTime)
                {
                    throw new ArgumentException("Max ET Error!" + " Higher than exposure time.");
                }
                Spectrometer.MaxExposureTimeUser = maxExposureTimeUser;
            }

            if (!Times.Any())  // Is empty?
            {
                TimeZero = DateTime.Now;
            }
            Measurement.IndexMax1D = -1;  // Reset.


            Front.My_msg("Parameters set & measuring");
            if (Measurement.SpectraToLoad > 1)
            {
                float time = Spectrometer.Period * Measurement.SpectraToLoad;
                Front.My_msg("Spectrum saved every " + time.ToString() + " s.");
            }
            //LabelBold(lblSetExp, false);  // Exposure.

            firstWait = true;
            timerMeasure.Interval = (int)((Spectrometer.Period - Spectrometer.ExposureTime) * 1000);
            timerMeasure.Start();

            return true;
        }

        public bool BtnStopMeasurement()
        {
            timerMeasure.Stop();
            Spectrometer.CancelExposure();
            return true;
        }

        public bool BtnSaveTemperatures()
        {
            WriteColumns(
                  Path.GetDirectoryName(fileNamePart) + "/TemperatureHistory.txt",
                  Times.ToArray(),
                  Temperatures.ToArray());
            return true;
        }

        public bool BtnReloadConfig()
        {
            ConfigurationFile_Load();
            return true;
        }

        public void SelectCalibration(int SelectedIndex)
        {
            Measurement.Calibration = Calibrations[SelectedIndex];
        }

        public void ResetTemperatureHistory()
        {
            // Needed when Clear btn is pressed while measurement runs.
            TimeZero = DateTime.Now;

            Times.Clear();
            Temperatures.Clear();
        }

        public void Close()
        {
            DAC.Close();
            ADC.Close();
            BtnDisconnect();
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
                BtnMeas_Click(sender: spectrometricThermometer.Spectrometer, e: EventArgs.Empty);  // Press STOP button.
                BtnInit_Click(sender: spectrometricThermometer.Spectrometer, e: EventArgs.Empty);  // Press DISCONNECT button.
            }
            return removed;
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

            for (int i = 0; i < lines.Length; i++)
            {
                string[] items = regex.Split(lines[i]);
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
                                My_msg(string.Format(
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
                                My_msg(string.Format(
                                    "Config.cfg: FileError: '{0}'.", e.Message));
                                break;
                            }
                            calibComboBox.Add(items[1]);
                            break;
                        }
                    case "const_skip":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.Constants.const_skip = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_eps":
                        {
                            if (double.TryParse(items[1], out double output))
                                Measurement.Constants.const_eps = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth1":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.Constants.const_smooth1 = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth2":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.Constants.const_smooth2 = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_1DHalfW":
                        {
                            if (int.TryParse(items[1], out int output))
                                Measurement.Constants.const_1DHalfW = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_slider":
                        {
                            if (double.TryParse(items[1], out double output))
                                Measurement.Constants.const_slider = output;
                            else
                                My_msg(string.Format(
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
                                    My_msg(string.Format(
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
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "PID_P":
                        {
                            tBoxP.Text = items[1];
                            break;
                        }
                    case "PID_I":
                        {
                            tBoxI.Text = items[1];
                            break;
                        }
                    case "PID_D":
                        {
                            tBoxD.Text = items[1];
                            break;
                        }

                    case "PID_period":
                        {
                            if (double.TryParse(items[1], out double output))
                                PID.Period = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }

                    case "absorbtion_edge":
                        {
                            // Default case.
                            if (!new[] { "Inchworm", "Inchworm_VIT", "const" }.Contains(items[1]))
                            {
                                Measurement.Constants.absorbtion_edge = "const";
                                break;
                            }

                            Measurement.Constants.absorbtion_edge = items[1];
                            break;
                        }

                    default:
                        My_msg(string.Format(
                            "Config.cfg: KeyError: '{0}'.", items[0]));
                        break;
                }
            }

            // If DAC not in config file.
            if (DAC == null)
                DAC = new DigitalToAnalogConverter.Offline();

            if (Calibrations.Count == 0)
            {
                My_msg("No calibration found!");
                btnCalib.Enabled = false;
                return;
            }
            btnCalib.Enabled = true;
            // ComboBox.
            cBoxCalib.DataSource = calibComboBox;
            cBoxCalib.SelectedIndex = 0;
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
        /// When averaging is complete,
        /// write to file, analyze and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Measurement_AveragingFinished(object sender, EventArgs e)
        {
            if (spectrometricThermometer.Measurement.SpectraToLoad > 1)
                LabelBold(lblAverage, true);

            // Save to file.
            if (save)
            {
                // Handle numbering if enabled.
                string fileNameIntText = "";
                if (!chBoxRewrite.Checked)
                {
                    fileNameIntText = fileNameNumber.ToString("D" + tBoxIndex.Text.Length);
                    tBoxIndex.Text = fileNameIntText;
                    fileNameNumber++;
                }

                spectrometricThermometer.WriteColumns(filename: fileNamePart + fileNameIntText + ".dat",
                    col1: spectrometricThermometer.Measurement.Wavelengths, col2: spectrometricThermometer.Measurement.Intensities);
            }

            plt2.Title(spectrometricThermometer.AnalyzeMeasurement());
            PlotData();
        }

        /// <summary>
        /// ExposureFinished event => Read and write spectrum.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Spectrometer_ExposureFinished(object sender, Spectrometer.ExposureFinishedEventArgs e)
        {
            // Exposure time adaptation display.
            if (spectrometricThermometer.Spectrometer.UseAdaptation)
            {
                LabelBold(lblAdaptation, e.Adapted);  // Change => true => bold.
                tBoxExpTime.Text = string.Format("{0:#.00}", spectrometricThermometer.Spectrometer.ExposureTime);
            }

            // Load wavelength, intensity and time.
            if (spectrometricThermometer.Measurement.Load(
                wavelengths: spectrometricThermometer.Spectrometer.Wavelengths,
                intensities: Array.ConvertAll(spectrometricThermometer.Spectrometer.Intensities, v => (double)v),
                ticks: spectrometricThermometer.Spectrometer.Time.Ticks))
            {
                // If first, switch off.
                LabelBold(lblAverage, false);
            }
            //LabelBold(lblSetExp, false);  // Not exposure.
        }

        public bool PIDOn(double outputVoltage)
        {
            if (DAC.PortName == null)  // Port is null.
            {
                Front.My_msg("DAC not found.");
                return false;
            }
            if (Temperatures.Count == 0)
            {
                Front.My_msg("No initial temperature! Setting DAC voltage only.");

                DAC.SetV(0, 1, save: false);
                DAC.SetV(outputVoltage, 2);
                return false;
            }
            // Initial temperature.
            PID.Reset(Temperatures[Temperatures.Count - 1]);
            // Try it now first (set initV).
            TimerPID_Tick(this, EventArgs.Empty);
            timerPID.Start();
            return true;
        }

        public bool PIDOff()
        {
            timerPID.Stop();
            return true;
        }

        public bool SwitchModeNone(out double outputVoltage)
        {
            outputVoltage = double.NaN;
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
            return true;
        }

        public bool SwitchModeElse()
        {
            timerSwitch.Stop();
            LED(LEDcolour.Off);
            return true;
        }

        /// <summary>
        /// Periodically check switching progress.
        /// </summary>
        /// <exception cref="ApplicationException">Wrong TemperatureControlMode.</exception>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerSwitch_Tick(object sender, EventArgs e)
        {
            switch (TemperatureControlMode1)
            {
                case TemperatureControlMode.None:
                    throw new ApplicationException("Wrong mode.");

                case TemperatureControlMode.ET2PC_switch:
                    if (!ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        TemperatureControlMode1 = TemperatureControlMode.None;
                        btnSwitch.Text = Constants.SwitchText[0];
                        LED("Off");
                    }
                    break;

                case TemperatureControlMode.PC2ET_equal:
                    // If switched before the voltages are equalized.
                    if (ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        TemperatureControlMode1 = TemperatureControlMode.None;
                        btnSwitch.Text = Constants.SwitchText[0];
                        LED("Off");
                        break;
                    }
                    // If difference is lesser than error.
                    My_msg("XX AR" + ADC.ReadEurothermV() + "  AL" + DAC.LastWrittenValue);
                    if (Math.Abs(ADC.ReadEurothermV() - DAC.LastWrittenValue) < 0.05)
                    {
                        TemperatureControlMode1 = TemperatureControlMode.PC2ET_switch;
                        My_msg("You can switch to Eurotherm.");
                        LED("Green");
                    }
                    break;

                case TemperatureControlMode.PC2ET_switch:
                    // If difference is greater again than error.
                    if (Math.Abs(ADC.ReadEurothermV() - DAC.LastWrittenValue) >= 0.05)
                    {
                        TemperatureControlMode1 = TemperatureControlMode.PC2ET_equal;
                        LED("Red");
                    }
                    if (ADC.Switched2Eurotherm())
                    {
                        timerSwitch.Stop();
                        TemperatureControlMode1 = TemperatureControlMode.None;
                        btnSwitch.Text = Constants.SwitchText[0];
                        LED("Off");
                    }
                    break;

                default:
                    break;
            }
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
        /// Allowed colors.
        /// </summary>
        private enum LEDcolour
        {
            Off,
            Green,
            Red,
        }
    }
}