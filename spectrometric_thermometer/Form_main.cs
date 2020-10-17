using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Main class handling GUI.
    /// </summary>
    public partial class Form_main : Form
    {
        // Plug-ins.
        private CompositionContainer _container;
        //[Import(typeof(ISpectrometricThermometer))]
        public ISpectrometricThermometer spectrometricThermometer;

        // Controls.
        /// <summary>
        /// Connecting device phase.
        /// Used in BtnInit_Click().
        /// </summary>
        private InitState initializationState = InitState.Initialize;
        /// <summary>
        /// True = measuring, False = setting.
        /// </summary>
        private bool measuringState = false;
        private TemperatureControlMode temperatureControlMode = TemperatureControlMode.None;
        
        /// <summary>
        /// Is spectrum saved in Measurement_AveragingFinished?
        /// Initialized in Form_main_Load.
        /// Actualized in ChBoxSave_CheckedChanged.
        /// </summary>
        private bool save = true;
        /// <summary>
        /// File path + filename without optional numbers and extension.
        /// </summary>
        private string fileNamePart;
        /// <summary>
        /// Numbering of the saved files.
        /// </summary>
        private int fileNameNumber = 0;

        // Plotting.
        private ScottPlot.Plot plt1;
        private ScottPlot.Plot plt2;


        /// <summary>
        /// Temperature control mode.
        /// Used while switching temperature control
        /// device (this program or Eurotherm).
        /// </summary>
        private TemperatureControlMode TemperatureControlMode1
        {
            get => temperatureControlMode;
            set
            {
                My_msg("TCM " + value.ToString());
                temperatureControlMode = value;
            }
        }

        public Form_main()
        {
            InitializeComponent();

            // Plug-in MEF init.
            // An aggregate catalog that combines multiple catalogs.
            var catalog = new AggregateCatalog();
            // Adds all the parts found in the same assembly as the Program class.
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            //catalog.Catalogs.Add(new DirectoryCatalog("D:\\Programming\\C#\\SimpleCalculator\\SimpleCalculator\\Extensions"));

            // Create the CompositionContainer with the parts in the catalog.
            _container = new CompositionContainer(catalog);

            // Fill the imports of this object.
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }
        }

        /// <summary>
        /// Connecting device phase. Used in BtnInit_Click().
        /// </summary>
        private enum InitState
        {
            /// <summary>
            /// Instantiate appropriate class and search for connected spectrometers.
            /// </summary>
            Initialize = 0,
            /// <summary>
            /// Select one of the found ones.
            /// </summary>
            Select_spectrometer = 1,
            /// <summary>
            /// Disconnect currently selected spectrometer and deselect it.
            /// </summary>
            Connected = 2,
        }

        /// <summary>
        /// Used while switching temperature control device (this program or Eurotherm).
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
        /// Initialization procedure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_Load(object sender, EventArgs e)
        {
            // See https://docs.microsoft.com/en-us/dotnet/framework/mef/.
            var p = new Form_main();

            {
                TextBox[] textBoxes = pnl2.Controls.OfType<TextBox>().ToArray();
                for (int i = 0; i < textBoxes.Length; i++)
                {
                    textBoxes[i].TextChanged += Pln2_textBoxes_TextChanged;
                }
            }
            // Set Msg method pointer.
            spectrometricThermometer = new SpectrometricThermometer(my_msg: this.My_msg);
            // Events.
            spectrometricThermometer.PidUpdated += SpectrometricThermometer_PidUpdated;
            spectrometricThermometer.Measurement.AveragingFinished += Measurement_AveragingFinished;
            // Change MsgBox language from CZ to ENG.
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Change language from CZ to ENG (decimal point).
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Buttons.text which can change.
            btnInit.Text = Constants.InitText[0];
            btnMeas.Text = Constants.MeasText[0];
            btnSwitch.Text = Constants.SwitchText[0];
            lblInfo.Text = "";
            // Log message box first line.
            My_msg(Constants.InitLine, newline: false);
            // ComboBox initialization.
            cBoxSpect.DataSource = Spectrometer.Factory.ListNames();
            cBoxSpect.SelectedIndex = 0;  // Select the first item (default).
            // Plotting.
            plt1 = Figs_Initialize(formsPlot1, Constants.Fig1Title, Constants.Fig1LabelX, Constants.Fig1LabelY);
            plt2 = Figs_Initialize(formsPlot2, Constants.Fig2Title, Constants.Fig2LabelX, Constants.Fig2LabelY);
            formsPlot1.MouseClicked += FormsPlot1_MouseClicked;

            // Config (+calibration).
            ConfigurationFile_Load();
            save = chBoxSave.Checked;
            // DAC.
            try
            {
                spectrometricThermometer.DAC.FindPort();
            }
            catch (ApplicationException ex)
            {
                My_msg(ex.Message);
            }
            // ADC.
            spectrometricThermometer.ADC = new AnalogToDigitalConverter_switcher();
            try
            {
                spectrometricThermometer.ADC.FindPort();
            }
            catch (ApplicationException ex)
            {
                My_msg(ex.Message);
                btnSwitch.Enabled = false;
            }

            //Start offline measurement for testing.
            //chBoxSave.Checked = false;
            //cBoxSpect.SelectedIndex = 2;
            //tBoxAverage.Text = "2";
            //BtnInit_Click(this, EventArgs.Empty);
            //BtnInit_Click(this, EventArgs.Empty);
            //BtnMeas_Click(this, EventArgs.Empty);

            BtnSize_Click(this, EventArgs.Empty);
            this.CenterToScreen();
        }

        private void SpectrometricThermometer_PidUpdated(object sender, SpectrometricThermometer.PidUpdatedEventArgs e)
        {
            tBoxVoltage.Text = e.Voltage;
            lblInfo.Text = e.Info;
        }

        /// <summary>
        /// Any TextBox in Pln2 changed its text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pln2_textBoxes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                spectrometricThermometer.PID.ParametersCheck(
                    sP: tBoxP.Text,
                    sI: tBoxI.Text,
                    sD: tBoxD.Text,
                    sSetPoint: tBoxSP.Text,
                    sRamp: tBoxRamp.Text,
                    vDeltaAbsMax: tBoxVChange.Text,
                    bufferLength: tBoxPIDAverage.Text);
            }
            catch (ArgumentException ex)
            {
                // Uncheck induces CheckChanged event!
                chBoxPID.Checked = false;
                My_msg(ex.Message);
            }
        }

        /// <summary>
        /// Print wavelength on left-mouse click.
        /// Recalculate absorption edge on ctrl-click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormsPlot1_MouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }

            double clickedWavelength = plt1.CoordinateFromPixelX(e.X);
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // If clicked at app start.
                if (spectrometricThermometer.Measurement.Wavelengths == null)
                    return;
                plt2.Title(spectrometricThermometer.AnalyzeMeasurement(clickedWavelength));
                PlotData();
            }
            else
            {
                My_msg(string.Format("Clicked: {0:0.0} nm", clickedWavelength));
            }
        }

        /// <summary>
        /// Initialize ScottPlot.figure object.
        /// </summary>
        /// <param name="formsPlot">Where to fit the fig to.</param>
        /// <param name="labelTitle">Fig label.</param>
        /// <param name="labelX">X axis label.</param>
        /// <param name="labelY">Y axis label.</param>
        /// <returns></returns>
        private ScottPlot.Plot Figs_Initialize(
            ScottPlot.FormsPlot formsPlot,
            string labelTitle,
            string labelX,
            string labelY)
        {
            ScottPlot.Plot plt = formsPlot.plt;
            plt.Grid(enable: false);
            plt.Style(figBg: Color.Transparent, dataBg: Color.White);
            plt.Title(labelTitle);
            plt.XLabel(labelX);
            plt.YLabel(labelY);

            plt.Axis(null, null, 0, null);
            formsPlot.Render();

            return plt;
        }

        /// <summary>
        /// Handles Initialization button click.
        /// Search for spectrometers, choose one, close the communication (by initState).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInit_Click(object sender, EventArgs e)
        {
            switch (initializationState)
            {
                case InitState.Initialize:
                    // Verify whether the needed library exists and
                    // instantiate the respective Spectrometer class.
                    try
                    {
                        spectrometricThermometer.Spectrometer = Spectrometer.Factory.Create(cBoxSpect.SelectedIndex);
                    }
                    catch (DllNotFoundException ex)
                    {
                        My_msg(ex.Message);
                        return;
                    }

                    // ExposureFinished event.
                    spectrometricThermometer.Spectrometer.ExposureFinished += new EventHandler<Spectrometer.ExposureFinishedEventArgs>(Spectrometer_ExposureFinished);

                    // Use the choosen spectrometer class to search for devices.
                    spectrometricThermometer.Spectrometer.SearchDevices();
                    if (spectrometricThermometer.Spectrometer.NumberOfDevicesFound == 0)
                    {
                        My_msg("No spectrometer found.");
                        // initState remains the same.
                    }
                    else
                    {
                        My_msg("Spectrometers found:");
                        for (int deviceIndex = 0; deviceIndex < spectrometricThermometer.Spectrometer.NumberOfDevicesFound; deviceIndex++)
                        {
                            spectrometricThermometer.Spectrometer.SelectDevice(deviceIndex);
                            My_msg("  " + deviceIndex + "... " + spectrometricThermometer.Spectrometer.ModelName + " : " + spectrometricThermometer.Spectrometer.SerialNo);
                        }
                        spectrometricThermometer.Spectrometer.SelectDevice();
                        initializationState = InitState.Select_spectrometer;
                        cBoxSpect.Enabled = false;
                    }
                    break;

                case InitState.Select_spectrometer:
                    int choosenDevice;
                    if (!int.TryParse(tboxSpect.Text, out choosenDevice))
                    {
                        My_msg("Not an integer");
                        return;
                    }

                    try
                    {
                        spectrometricThermometer.Spectrometer.SelectDevice(choosenDevice);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        My_msg("Out of range: " + ex.Message);
                        return;
                    }

                    spectrometricThermometer.Spectrometer.EraceDeviceList();
                    spectrometricThermometer.Spectrometer.Open();
                    My_msg("Openning " + spectrometricThermometer.Spectrometer.ModelName + " : " + spectrometricThermometer.Spectrometer.SerialNo);

                    tBoxExpTime.Text = string.Format("{0:0.0}", spectrometricThermometer.Spectrometer.ExposureTime.ToString());
                    tBoxPeriod.Text = string.Format("{0:0.0}", spectrometricThermometer.Spectrometer.Period.ToString());
                    // If adaptive exposure time enabled, show actual exposure time in lblExpTo.
                    if ((tBoxAdaptation.Text == "") && chBoxAdaptation.Checked)  // Empty and allowed.
                    {
                        tBoxAdaptation.Text = tBoxExpTime.Text;
                    }

                    initializationState = InitState.Connected;
                    break;

                case InitState.Connected:
                    if (measuringState)
                    {
                        My_msg("Stop measuring first.");
                        break;
                    }
                    spectrometricThermometer.DisconnectSpectrometer();
                    initializationState = InitState.Initialize;
                    cBoxSpect.Enabled = true;
                    break;

                default:
                    break;
            }
            btnInit.Text = Constants.InitText[(int)initializationState];  // Button text.
        }

        /// <summary>
        /// Handle Measurement button click.
        /// Set parameters and start the measurement.
        /// Then can stop it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMeas_Click(object sender, EventArgs e)
        {
            if (initializationState != InitState.Connected)
            {
                My_msg("Initialize spectrometer first!");
                return;
            }

            if (!measuringState)  // Not measuring. => Start it.
            {
                try
                {
                    spectrometricThermometer.Spectrometer.ParameterCheck(
                        tBoxPeriod: tBoxPeriod.Text,
                        tBoxExpTime: tBoxExpTime.Text,
                        chBoxAdaptation: chBoxAdaptation.Checked,
                        tBoxAdaptation: tBoxAdaptation.Text);

                    this.ParameterCheck(
                        tBoxAverage: tBoxAverage.Text,
                        chBoxRewrite: chBoxRewrite.Checked,
                        tBoxIndex: tBoxIndex.Text,
                        tBoxFilename: tBoxFilename.Text);
                }
                catch (ArgumentException ex)
                {
                    My_msg(ex.Message);
                    return;
                }

                if (!spectrometricThermometer.Times.Any())  // Is empty?
                {
                    spectrometricThermometer.TimeZero = DateTime.Now;
                }
                spectrometricThermometer.Measurement.IndexMax1D = -1;  // Reset.


                My_msg("Parameters set & measuring");
                if (spectrometricThermometer.Measurement.SpectraToLoad > 1)
                {
                    float time = spectrometricThermometer.Spectrometer.Period * spectrometricThermometer.Measurement.SpectraToLoad;
                    My_msg("Spectrum saved every " + time.ToString() + " s.");
                }
                //LabelBold(lblSetExp, false);  // Exposure.

                firstWait = true;
                timerMeasure.Interval = (int)((spectrometricThermometer.Spectrometer.Period - spectrometricThermometer.Spectrometer.ExposureTime) * 1000);
                timerMeasure.Start();
            }
            else
            {
                My_msg("Stop");

                //LabelBold(lblSetExp, false);  // Stop.
                LabelBold(lblAdaptation, false);  // Stop.
                LabelBold(lblAverage, false);  // Stop.

                timerMeasure.Stop();

                spectrometricThermometer.Spectrometer.CancelExposure();
                // No regulation without new data.
                chBoxPID.Checked = false;
            }

            measuringState = !measuringState;  // Switch.
            pnlSettings.Enabled = !measuringState;  // Disable settings while measuring.
            btnMeas.Text = Constants.MeasText[(measuringState ? 1 : 0)];  // Button label.
        }

        /// <summary>
        /// Show help file on Help button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string helpFile = "Help.pdf";
            if (File.Exists(helpFile))
            {
                Process.Start(helpFile);
            }
            else
            {
                My_msg("Help file not found.");
            }
        }

        /// <summary>
        /// Close the connection and exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// After closing, disconnect the devices.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_FormClosed(object sender, FormClosedEventArgs e)
        {
            spectrometricThermometer.DAC.Close();
            spectrometricThermometer.ADC.Close();
            spectrometricThermometer.DisconnectSpectrometer();
        }

        /// <summary>
        /// Index spectra files? Toogle the textBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxRwt_CheckedChanged(object sender, EventArgs e)
        {
            tBoxIndex.Enabled = !chBoxRewrite.Checked;  // Toogle.
        }

        /// <summary>
        /// Adapt the exposure time according
        /// to saturation?
        /// Toogle the textBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxAdaptation_CheckedChanged(object sender, EventArgs e)
        {
            tBoxAdaptation.Enabled = chBoxAdaptation.Checked;  // Toogle.
            // Empty and just allowed.
            if ((tBoxAdaptation.Text == "") && chBoxAdaptation.Checked)
            {
                tBoxAdaptation.Text = tBoxExpTime.Text;
            }
        }

        /// <summary>
        /// FolderBrowserDialog definition.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBoxFilename_DoubleClick(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    tBoxFilename.Text = System.IO.Path.Combine(fbd.SelectedPath, "Spectrum");
            }
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
        /// Plot and render both charts.
        /// </summary>
        /// <returns>Was the plot successful?</returns>
        private bool PlotData()
        {
            // Figure 1.
            double[] x1 = spectrometricThermometer.Measurement.Wavelengths;
            double[] y1 = spectrometricThermometer.Measurement.Intensities;
            if (x1 == null || y1 == null)
                return false;

            plt1.YLabel(Constants.Fig1LabelY);
            plt1.XLabel(Constants.Fig1Title);
            plt1.AxisAuto(horizontalMargin: .9, verticalMargin: .5);
            plt1.Axis(null, null, 0, null);
            plt1.Clear();
            plt1.PlotScatter(x1, y1, color: Color.Red, markerSize: 0, lineWidth: 0.1);
            // Fitting graphics.
            if (spectrometricThermometer.Measurement.FitGraphics != null)
            {
                var mfg = spectrometricThermometer.Measurement.FitGraphics;
                plt1.PlotScatter(
                    new double[2] { mfg.LL.X, mfg.LR.X },
                    new double[2] { mfg.LL.Y, mfg.LR.Y },
                    markerSize: 0, color: Color.Black);
                plt1.PlotScatter(
                    new double[2] { mfg.RL.X, mfg.RR.X },
                    new double[2] { mfg.RL.Y, mfg.RR.Y },
                    markerSize: 0, color: Color.Black);
                plt1.PlotPoint(
                    mfg.crossing.X, mfg.crossing.Y,
                    markerSize: 5, color: Color.Blue);
                // Mark points where fitting occured.
                double[] x;
                double[] y;

                x = new double[mfg.LIndexes[1]];
                y = new double[mfg.LIndexes[1]];
                Array.Copy(spectrometricThermometer.Measurement.Wavelengths, mfg.LIndexes[0],
                    x, 0, mfg.LIndexes[1]);
                Array.Copy(spectrometricThermometer.Measurement.Intensities, mfg.LIndexes[0],
                    y, 0, mfg.LIndexes[1]);
                plt1.PlotScatter(x, y, markerSize: 3,
                    color: Color.Black, lineWidth: 0);

                x = new double[mfg.RIndexes[1]];
                y = new double[mfg.RIndexes[1]];
                Array.Copy(spectrometricThermometer.Measurement.Wavelengths, mfg.RIndexes[0],
                    x, 0, mfg.RIndexes[1]);
                Array.Copy(spectrometricThermometer.Measurement.Intensities, mfg.RIndexes[0],
                    y, 0, mfg.RIndexes[1]);
                plt1.PlotScatter(x, y, markerSize: 3, color: Color.Black);
            }

            formsPlot1.Render();

            // Figure 2.
            double[] x2 = spectrometricThermometer.Times.ToArray();
            double[] y2 = spectrometricThermometer.Temperatures.ToArray();

            // Cannot PlotLine and AxisAuto() with one point.
            if (y2.Length == 0)
            {
                formsPlot2.Render();
                return true; ;
            }
            else if (y2.Length == 1)
            {
                plt2.Axis(x2[0] - 1, x2[0] + 1, y2[0] - 1, y2[0] + 1);
            }
            else
            {
                if (x2.Min() == x2.Max() || y2.Min() == y2.Max())
                {
                    plt2.Axis(x2.Min() - 1, x2.Max() + 1, y2.Min() - 1, y2.Max() + 1);
                }
                else
                    plt2.AxisAuto(.9, .5);
                // Lower bound only for positive times.
                if (x2.Min() < 0)
                {
                    plt2.Axis(null, null, null, null);
                }
                else
                {
                    plt2.Axis(0d, null, null, null);
                }
            }
            plt2.Clear();
            plt2.PlotScatter(x2, y2, color: Color.Red);

            formsPlot2.Render();
            return true;
        }

        /// <summary>
        /// Add line to message window (lblMsg).
        /// </summary>
        /// <param name="text">Text to be added.</param>
        /// <param name="newline">Add a newline character ahead of text?</param>
        private void My_msg(string text, bool newline)
        {
            if (newline)
                tBoxLog.AppendText("\r\n");

            tBoxLog.AppendText(text);
        }
        private void My_msg(string text)
        {
            My_msg(text, true);
        }

        /// <summary>
        /// Set label font to bold or regular.
        /// </summary>
        /// <param name="label">Target label.</param>
        /// <param name="bold">True => bold.</param>
        private static void LabelBold(Label label, bool bold)
        {
            label.Font = new Font(label.Font, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        /// <summary>
        /// Is spectrometer unplugged?
        /// If yes, press Stop and Disconnect buttons.
        /// </summary>
        /// <returns>Removed?</returns>
        private bool CheckDeviceRemoved()
        {
            bool removed = spectrometricThermometer.Spectrometer.CheckDeviceRemoved();
            if (removed)
            {
                My_msg("Spectrometer status: " + spectrometricThermometer.Spectrometer.Status());
                BtnMeas_Click(sender: spectrometricThermometer.Spectrometer, e: EventArgs.Empty);  // Press STOP button.
                BtnInit_Click(sender: spectrometricThermometer.Spectrometer, e: EventArgs.Empty);  // Press DISCONNECT button.
            }
            return removed;
        }

        /// <summary>
        /// Redraw GUI when size changed.
        /// Implemented as difference to defaultSize const.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_SizeChanged(object sender, EventArgs e)
        {
            // Squares of this edge length defaults to 446 (while this.Size == default).
            int pictureBoxSize = 446;

            // Horizontal.
            // How much wider than default.
            int delta = this.Size.Width - Constants.DefaultSize.Width;

            formsPlot1.Width = pictureBoxSize + (delta / 2);
            formsPlot2.Width = pictureBoxSize + (delta / 2);
            // pictureBox1.Location is constant.
            // 458 = default pictureBox2.Location.X
            formsPlot2.Location = new Point(
                (int)((pictureBoxSize + 12) + (delta / 2)),  // X
                formsPlot2.Location.Y);  // Y

            // Follows part, which is constant between minimum and default.
            if (delta < 0)
                delta = 0;
            pnlSettings.Left = 323 + delta;  // 323 = pnlSettings default left.
            pnl2.Left = 642 + delta;  // 642 = pnl2 default left.
            tBoxLog.Width = 199 + delta;  // 199 = tBoxLog default width.
            lineShape1.X2 = 903 + delta;  // 903 = lineShape1 default X2.

            // Vertical.
            delta = this.Size.Height - Constants.DefaultSize.Height;
            formsPlot1.Height = pictureBoxSize + delta;
            formsPlot2.Height = pictureBoxSize + delta;

            if (plt1 == null || plt2 == null)
                return;

            // If measurement.Wavelength/Intensities are not null (not 'no data yet').
            if (!PlotData())
            {
                formsPlot1.Render();
                formsPlot2.Render();
            }
        }

        /// <summary>
        /// GUI parameters parsing.
        /// </summary>
        /// <exception cref="ArgumentException">One of arguments is wrong.</exception>
        /// <param name="chBoxRewrite">Rewrite or number saved files.</param>
        /// <param name="tBoxIndex">File numbering.</param>
        /// <param name="tBoxAverage">How many spectra to average.
        /// Less than two means no averaging.</param>
        /// <param name="tBoxFilename">Filename.</param>
        private void ParameterCheck(
            bool chBoxRewrite,
            string tBoxIndex,
            string tBoxAverage,
            string tBoxFilename)
        {
            // Numbering.
            if (!chBoxRewrite)  // Ignoring input while rewriting.
            {
                if (!int.TryParse(tBoxIndex, out int fileNameInt))
                {
                    throw new ArgumentException("Numbering error!" + " Converted value: " + fileNameInt + ".");
                }
                this.fileNameNumber = fileNameInt;
            }

            // Averaging the spectra.
            {
                if (!int.TryParse(tBoxAverage, out int average))
                {
                    throw new ArgumentException("Average error!" + " Converted value: " + average + ".");
                }
                if (average < 1)
                {
                    throw new ArgumentException("Average error!" + " Must be positive.");
                }
                spectrometricThermometer.Measurement.SpectraToLoad = average;
            }

            // Path.
            fileNamePart = tBoxFilename;

            // Check its validity.
            if (!SpectrometricThermometer.IsValidFileNameOrPath(fileNamePart))
            {
                throw new ArgumentException("Path error!");
            }

            // Extract directory.
            string dir = Path.GetDirectoryName(fileNamePart);
            // Create it if it doesn't exists.
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                My_msg("Creating '" + dir + "'.");
            }
        }

        /// <summary>
        /// Load config from "Config.cfg".
        /// Mainly calibration files.
        /// </summary>
        private void ConfigurationFile_Load()
        {
            Regex regex = new Regex("    ");
            //Regex regex = new Regex(delimiter);
            string[] lines = File.ReadAllLines("Config.cfg");

            spectrometricThermometer.Calibrations.Clear();
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
                                var output = spectrometricThermometer.LoadData(items[1] + ".clb");
                                spectrometricThermometer.Calibrations.Add(new Calibration_Points(output.Item1, output.Item2));
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
                                var output = spectrometricThermometer.LoadData(items[1] + ".clb");
                                spectrometricThermometer.Calibrations.Add(new Calibration_Polynom(output.Item2));
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
                                spectrometricThermometer.Measurement.Constants.const_skip = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_eps":
                        {
                            if (double.TryParse(items[1], out double output))
                                spectrometricThermometer.Measurement.Constants.const_eps = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth1":
                        {
                            if (int.TryParse(items[1], out int output))
                                spectrometricThermometer.Measurement.Constants.const_smooth1 = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_smooth2":
                        {
                            if (int.TryParse(items[1], out int output))
                                spectrometricThermometer.Measurement.Constants.const_smooth2 = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_1DHalfW":
                        {
                            if (int.TryParse(items[1], out int output))
                                spectrometricThermometer.Measurement.Constants.const_1DHalfW = output;
                            else
                                My_msg(string.Format(
                                    "Config.cfg: Parse error: '{0}'.", output));
                            break;
                        }
                    case "const_slider":
                        {
                            if (double.TryParse(items[1], out double output))
                                spectrometricThermometer.Measurement.Constants.const_slider = output;
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
                                    spectrometricThermometer.DAC = new DigitalToAnalogConverter.Lab();
                                    break;
                                case "offline":
                                    spectrometricThermometer.DAC = new DigitalToAnalogConverter.Offline();
                                    break;
                                default:
                                    My_msg(string.Format(
                                        "Config.cfg: DAC keyError: '{0}'. Setting offline.", items[1]));
                                    spectrometricThermometer.DAC = new DigitalToAnalogConverter.Offline();
                                    break;
                            }
                            break;
                        }
                    case "DAC_maxStep":
                        {
                            if (double.TryParse(items[1], out double output))
                                spectrometricThermometer.PID.VDeltaAbsMax = output;
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
                                spectrometricThermometer.Measurement.Constants.absorbtion_edge = "const";
                                break;
                            }

                            spectrometricThermometer.Measurement.Constants.absorbtion_edge = items[1];
                            break;
                        }

                    default:
                        My_msg(string.Format(
                            "Config.cfg: KeyError: '{0}'.", items[0]));
                        break;
                }
            }

            // If DAC not in config file.
            if (spectrometricThermometer.DAC == null)
                spectrometricThermometer.DAC = new DigitalToAnalogConverter.Offline();

            if (spectrometricThermometer.Calibrations.Count == 0)
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
        /// Disabling saving disables rewrite checkbox.
        /// For clarity purpose.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxSave_CheckedChanged(object sender, EventArgs e)
        {
            chBoxRewrite.Enabled = chBoxSave.Checked;
            if (!chBoxRewrite.Checked)
            {
                tBoxIndex.Enabled = chBoxSave.Checked;
            }

            save = chBoxSave.Checked;
        }

        /// <summary>
        /// Set default size to the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = Constants.DefaultSize;
        }

        /// <summary>
        /// Reload config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConfig_Click(object sender, EventArgs e)
        {
            My_msg("Configuration file reloaded.");
            ConfigurationFile_Load();
        }

        /// <summary>
        /// Selection of the Calibration instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBoxCalib_SelectedIndexChanged(object sender, EventArgs e)
        {
            spectrometricThermometer.Measurement.Calibration = spectrometricThermometer.Calibrations[cBoxCalib.SelectedIndex];
        }

        /// <summary>
        /// Reset temperature history. Data and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            // Needed when Clear btn is pressed while measurement runs.
            spectrometricThermometer.TimeZero = DateTime.Now;

            spectrometricThermometer.Times.Clear();
            spectrometricThermometer.Temperatures.Clear();
            // Just rewriting is not enough, because PlotLines
            // is not called for one point only.
            plt2.Clear();
        }

        /// <summary>
        /// Load spectra as if they were measured.
        /// Time is derived from file creation time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            if (measuringState)
            {
                My_msg("Stop measuring first.");
                return;
            }

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "dat files (*.dat)|*.dat|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    spectrometricThermometer.Measurement.SpectraToLoad = 1;  // No averaging.

                    foreach (string file in openFileDialog.FileNames)
                    {
                        DateTime modification = File.GetLastWriteTime(file);
                        var columns = spectrometricThermometer.LoadData(file);

                        if (!spectrometricThermometer.Times.Any())
                        {
                            spectrometricThermometer.TimeZero = modification;
                        }
                        save = false;  // Just load, no saving.
                        spectrometricThermometer.Measurement.Load(
                            wavelengths: columns.Item1,
                            intensities: columns.Item2,
                            ticks: modification.Ticks);

                        save = chBoxSave.Checked;  // Reset.
                        spectrometricThermometer.Measurement.IndexMax1D = -1;
                        plt2.Title(spectrometricThermometer.AnalyzeMeasurement());
                    }
                    PlotData();
                }
            }
        }

        /// <summary>
        /// Save temperature history into a file.
        /// Use spectra save folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            spectrometricThermometer.WriteColumns(
                Path.GetDirectoryName(fileNamePart) + "/TemperatureHistory.txt",
                spectrometricThermometer.Times.ToArray(),
                spectrometricThermometer.Temperatures.ToArray());
            My_msg("Temperature history saved.");
        }

        /// <summary>
        /// PID regulation "button" pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxPID_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxPID.Checked)
            {
                if (spectrometricThermometer.DAC.PortName == null)  // Port is null.
                {
                    My_msg("DAC not found.");
                    chBoxPID.Checked = false;
                    return;
                }
                // Initial voltage (equal to eurotherm).
                {
                    if (!double.TryParse(tBoxVoltage.Text, out double initV) || initV < 0)
                    {
                        My_msg("Invalid initial voltage.");
                        chBoxPID.Checked = false;
                        return;
                    }
                }
                if (spectrometricThermometer.Temperatures.Count == 0)
                {
                    My_msg("No initial temperature! Setting DAC voltage only.");
                    if (!double.TryParse(tBoxVoltage.Text, out double initV) || initV < 0)
                    {
                        My_msg("Invalid initial voltage.");
                        chBoxPID.Checked = false;
                        return;
                    }
                    spectrometricThermometer.DAC.SetV(0, 1, save: false);
                    spectrometricThermometer.DAC.SetV(initV, 2);

                    chBoxPID.Checked = false;
                    return;
                }
                // Initial temperature.
                spectrometricThermometer.PID.Reset(spectrometricThermometer.Temperatures[spectrometricThermometer.Temperatures.Count - 1]);
                tBoxVoltage.Enabled = false;
                // Try it now first (set initV).
                TimerPID_Tick(sender, e);
                timerPID.Start();
            }
            else
            {
                tBoxVoltage.Enabled = true;
                timerPID.Stop();
            }
        }

        /// <summary>
        /// Draw actual calibration into the left pictureBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCalib_Click(object sender, EventArgs e)
        {
            double[] x = ScottPlot.DataGen.Range(400, 1200, 1);
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                y[i] = spectrometricThermometer.Measurement.Calibration.Use(x[i]);
            }
            plt2.YLabel("Temperature (°C)");  // Set back in PlotData().
            plt2.Title("Calibration");  // Set back in PlotData().
            plt2.AxisAuto(horizontalMargin: .9, verticalMargin: .9);
            plt2.Clear();
            plt2.PlotScatter(x, y);
            formsPlot2.Render();
        }

        /// <summary>
        /// Prepare for heater control voltage source change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSwitch_Click(object sender, EventArgs e)
        {
            // Stop if PID control is running.
            if (chBoxPID.Checked)
            {
                My_msg("Stop PID control first.");
                return;
            }

            if (TemperatureControlMode1 == TemperatureControlMode.None)
            {
                btnSwitch.Text = Constants.SwitchText[1];
                // Switched to eurotherm, so switching to this app.
                if (spectrometricThermometer.ADC.Switched2Eurotherm())
                {
                    TemperatureControlMode1 = TemperatureControlMode.ET2PC_switch;

                    double V = spectrometricThermometer.ADC.ReadEurothermV();
                    // Round to 12b - this should be given by DAC.
                    V = Math.Round(V / 10 * 4096) / 409.6;  // ??
                    spectrometricThermometer.DAC.SetV(V, 2);

                    tBoxVoltage.Text = V.ToString("F3");
                    My_msg("You can switch to PC.");
                    LED("Green");
                }
                // Switched to this app, so switching to eurotherm.
                else
                {
                    TemperatureControlMode1 = TemperatureControlMode.PC2ET_equal;
                    double percent = spectrometricThermometer.DAC.LastWrittenValue / 5 * 100;  // Percentage out of 5 V.

                    var a = spectrometricThermometer.DAC.GetV();
                    My_msg("XX  P" + percent + "  DL" + spectrometricThermometer.DAC.LastWrittenValue + "  a0" + a[0] + "  a1" + a[1]);  // Analyze. DELETE!!!

                    My_msg("Set Eurotherm to Manual / " + percent.ToString("F1") + " %.");
                    LED("Red");
                }
                timerSwitch.Start();
            }
            // Sth is already happening => abort.
            else
            {
                timerSwitch.Stop();
                TemperatureControlMode1 = TemperatureControlMode.None;
                btnSwitch.Text = Constants.SwitchText[0];
                LED("Off");
            }
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
        /// Light app LED and hardware LED (if available).
        /// Possible inputs:
        ///     "Red", "Green", "Off".
        /// </summary>
        /// <exception cref="ArgumentException">Only defined states are allowed.</exception>
        /// <param name="colour"></param>
        private void LED(string colour)
        {
            // Default colour.
            KnownColor lblColour = KnownColor.Control;

            switch (colour)
            {
                case "Green":
                    lblColour = KnownColor.ForestGreen;
                    if (ADC.PortName != null)
                        ADC.LED_Green();
                    break;

                case "Red":
                    lblColour = KnownColor.IndianRed;
                    if (ADC.PortName != null)
                        ADC.LED_Red();
                    break;

                case "Off":
                    // Keep default colour.
                    if (ADC.PortName != null)
                        ADC.LED_Off();
                    break;

                default:
                    throw new ArgumentException("Unknown LED state.");
            }

            lblSwitch.BackColor = Color.FromKnownColor(lblColour);
        }

        public static class Constants
        {
            /// <summary>
            /// First line printed by My_msg().
            /// </summary>
            public static string InitLine { get; } = "Spectrometric Thermometer (version 3.5)";
            /// <summary>
            /// Default size of the Form.
            /// </summary>
            public static Size DefaultSize { get; } = new Size(width: 929, height: 743);

            // Plotting.
            public static string Fig1Title { get; } = "Spectrum";
            public static string Fig1LabelX { get; } = "Wavelength (nm)";
            public static string Fig1LabelY { get; } = "Intensity(a.u.)";

            public static string Fig2Title { get; } = "T = ? °C";
            public static string Fig2LabelX { get; } = "Time (sec)";
            public static string Fig2LabelY { get; } = "Temperature (°C)";
            
            // Buttons text.
            public static string[] InitText { get; } = { "&Initialize", "Choose dev&ice", "Disc&onnect" };
            public static string[] MeasText { get; } = { "&Measure", "S&top" };
            public static string[] SwitchText { get; } = { "Pr&epare to switch", "Ab&ort" };
        }
    }
}