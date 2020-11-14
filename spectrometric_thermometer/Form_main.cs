using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Main class handling GUI.
    /// </summary>
    public partial class Form_main : Form
    {
        public SpectrometricThermometer spectrometricThermometer;

        // Controls.
        private readonly Back2Front back2Front = null;
        /// <summary>
        /// Connecting device phase.
        /// </summary>
        private InitializationState initializationState = InitializationState.Initial;

        private MeasuringState measuringState = MeasuringState.Idle;
        // Plotting.
        private ScottPlot.Plot plt1;

        public ScottPlot.Plot plt2;
        public Form_main()
        {
            InitializeComponent();
            
            back2Front = new Back2Front(front: this);
            spectrometricThermometer = new SpectrometricThermometer(front: back2Front);
            spectrometricThermometer.Measurement.AveragingFinished += Measurement_AveragingFinished;
        }

        /// <summary>
        /// Connecting device phase.
        /// </summary>
        private enum InitializationState
        {
            /// <summary>
            /// Instantiate appropriate class and search for connected spectrometers.
            /// </summary>
            Initial = 0,
            /// <summary>
            /// Select one of the found spectrometers.
            /// </summary>
            Select_spectrometer = 1,
            /// <summary>
            /// Disconnect currently selected spectrometer and deselect it.
            /// </summary>
            Connected = 2,
        }

        private enum MeasuringState
        {
            Idle = 0,
            Measuring = 1,
        }

        /// <summary>
        /// Set virtual LED color. Nullable (null means neutral color / off).
        /// </summary>
        public KnownColor? LEDColor
        {
            set
            {
                if (value == null)
                {
                    lblLED.BackColor = Color.FromKnownColor(KnownColor.Control);
                }
                else
                {
                    lblLED.BackColor = Color.FromKnownColor((KnownColor)value);
                }
            }
        }

        public Constants constants = Constants.Constants_EN;
        /// <summary>
        /// Search for spectrometers, choose one, close the communication.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnInitialize_Click(object sender, EventArgs e)
        {
            switch (initializationState)
            {
                case InitializationState.Initial:
                    if (spectrometricThermometer.BtnInitialize(cBoxDeviceType.SelectedIndex))
                    {
                        cBoxDeviceType.Enabled = false;
                        initializationState = InitializationState.Select_spectrometer;
                        spectrometricThermometer.Spectrometer.ExposureFinished +=
                            Spectrometer_ExposureFinished;
                    }
                    break;

                case InitializationState.Select_spectrometer:
                    if (int.TryParse(tboxSelectSpectrometer.Text, out int choosenDevice))
                    {
                        if (spectrometricThermometer.BtnSelect(choosenDevice,
                            out string exposureTime, out string periodTime))
                        {
                            tBoxExposureTime.Text = exposureTime;
                            tBoxPeriodLength.Text = periodTime;
                            // If adaptive exposure time is enabled, show actual exposure time in lblExpTo.
                            if ((tBoxETAdaptation.Text == "") && chBoxETAdaptation.Checked)  // Empty and allowed.
                            {
                                tBoxETAdaptation.Text = tBoxExposureTime.Text;
                            }
                            initializationState = InitializationState.Connected;
                        }
                    }
                    else
                    {
                        My_msg("Not an integer");
                    }
                    break;

                case InitializationState.Connected:
                    if (measuringState == MeasuringState.Measuring)
                    {
                        My_msg("Stop measuring first.");
                        break;
                    }
                    if (spectrometricThermometer.BtnDisconnect())
                    {
                        cBoxDeviceType.Enabled = true;
                        initializationState = InitializationState.Initial;
                    }
                    break;

                default:
                    break;
            }
            btnInitialize.Text = constants.btnInitializeText[(int)initializationState];  // Button text.
        }

        /// <summary>
        /// Set parameters and start/stop the measurement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnMeasure_Click(object sender, EventArgs e)
        {
            if (initializationState != InitializationState.Connected)
            {
                My_msg("Initialize spectrometer first!");
                return;
            }

            switch (measuringState)
            {
                case MeasuringState.Idle:
                    SpectrometricThermometer.Parameters parameters;
                    try
                    {
                        parameters = SpectrometricThermometer.Parameters.Parse(
                            save: chBoxSave.Checked,
                            rewrite: chBoxRewrite.Checked,
                            filenameIndex: tBoxFilenameIndex.Text,
                            periodLength: tBoxPeriodLength.Text,
                            average: tBoxAverage.Text,
                            exposureTime: tBoxExposureTime.Text,
                            adaptation: chBoxETAdaptation.Checked,
                            adaptationMaxExposureTime: tBoxETAdaptation.Text,
                            filename: tBoxFilename.Text);
                    }
                    catch (ArgumentException ex)
                    {
                        My_msg(ex.Message);
                        return;
                    }
                    if (spectrometricThermometer.BtnStartMeasurement(parameters))
                    {
                        measuringState = MeasuringState.Measuring;
                    }
                    break;

                case MeasuringState.Measuring:
                    if (spectrometricThermometer.BtnStopMeasurement())
                    {
                        //LabelBold(lblSetExp, false);  // Stop.
                        LabelBold(lblETAdaptation, false);  // Stop.
                        LabelBold(lblAverage, false);  // Stop.

                        My_msg("Stop");
                        chBoxPID.Checked = false;  // No regulation without new data.

                        measuringState = MeasuringState.Idle;
                    }
                    break;

                default:
                    break;
            }

            // Disable settings while measuring.
            pnlSettings.Enabled = measuringState == MeasuringState.Idle;
            btnMeasure.Text = constants.btnMeasureText[(int)measuringState];
        }

        /// <summary>
        /// Add line to the message window.
        /// </summary>
        /// <param name="text">Text to be added.</param>
        public void My_msg(string text)
        {
            My_msg(text, true);
        }

        /// <summary>
        /// Set label font to bold (true) or regular.
        /// </summary>
        /// <param name="label">Target label.</param>
        /// <param name="bold">True => bold.</param>
        private static void LabelBold(Label label, bool bold)
        {
            label.Font = new Font(label.Font, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        /// <summary>
        /// Reset temperature history. Data and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            spectrometricThermometer.ResetTemperatureHistory();
            // Just rewriting is not enough, because PlotLines is not called for one point only.
            plt2.Clear();
        }

        /// <summary>
        /// Set default size to the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDefaultSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = constants.defaultSize;
        }

        private void BtnExit_Click(object sender, EventArgs e) => Close();

        /// <summary>
        /// Show help file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string helpFileName = constants.helpFileName;
            if (File.Exists(helpFileName))
            {
                Process.Start(helpFileName);
            }
            else
            {
                My_msg("Help file not found.");
            }
        }

        /// <summary>
        /// Load spectra as if they were measured.
        /// Time is derived from file creation time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLoadSpectra_Click(object sender, EventArgs e)
        {
            if (measuringState == MeasuringState.Measuring)
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
                        var waveIntens = spectrometricThermometer.LoadData(file);

                        if (!spectrometricThermometer.MTemperatureHistory.Any())
                        {
                            spectrometricThermometer.MTemperatureHistory.TimeZero = modification;
                        }
                        bool save = chBoxSave.Checked;  // Just load, no saving.
                        spectrometricThermometer.Measurement.Load(
                            wavelengths: waveIntens.Item1,
                            intensities: waveIntens.Item2,
                            ticks: modification.Ticks);
                        chBoxSave.Checked = save;  // Reset.

                        spectrometricThermometer.Measurement.IndexMax1D = -1;
                        plt2.Title(string.Format(
                                "T = {0:0.0} °C",
                                spectrometricThermometer.AnalyzeMeasurement()));
                    }
                    Plot(
                        spectrometricThermometer.Measurement,
                        spectrometricThermometer.MTemperatureHistory);
                }
            }
        }

        /// <summary>
        /// Draw actual calibration into the left pictureBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPlotCalibration_Click(object sender, EventArgs e)
        {
            double[] x = ScottPlot.DataGen.Range(start: 400, stop: 1200, step: 1);
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
            formsPlotRight.Render();
        }

        /// <summary>
        /// Reload config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadConfig_Click(object sender, EventArgs e)
        {
            try
            {
                if (spectrometricThermometer.BtnReloadConfig())
                { };
            }
            catch (Exception ex)
            {
                My_msg(ex.Message);
                return;
            }
            My_msg("Configuration file reloaded.");
        }

        /// <summary>
        /// Save temperature history into a file.
        /// Use spectra save folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveTemperatures_Click(object sender, EventArgs e)
        {
            if (spectrometricThermometer.BtnSaveTemperatures())
            {
                My_msg("Temperature history saved.");
            }
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

            bool temperatureControlMode_None = spectrometricThermometer.Switch(out double outputVoltage);
            tBoxOutputVoltage.Text = outputVoltage.ToString("F3");
            btnSwitch.Text = constants.btnSwitchText[temperatureControlMode_None ? 0 : 1];
        }

        /// <summary>
        /// Selection of the Calibration instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBoxCalibration_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (spectrometricThermometer != null)
                spectrometricThermometer.SelectCalibration(cBoxCalibration.SelectedIndex);
        }

        /// <summary>
        /// Adapt the exposure time according to saturation?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxAdaptation_CheckedChanged(object sender, EventArgs e)
        {
            tBoxETAdaptation.Enabled = chBoxETAdaptation.Checked;
            if (tBoxETAdaptation.Text == "" && chBoxETAdaptation.Checked)  // Empty and just enabled.
            {
                tBoxETAdaptation.Text = tBoxExposureTime.Text;
            }
        }

        /// <summary>
        /// PID regulation on/off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxPID_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxPID.Checked)  // Switched on.
            {
                // Initial voltage (equal to eurotherm).
                if (!double.TryParse(tBoxOutputVoltage.Text, out double initV) || initV < 0)
                {
                    My_msg("Invalid initial voltage.");
                    chBoxPID.Checked = false;
                    return;
                }
                if (!spectrometricThermometer.PIDOn(initV))
                {
                    chBoxPID.Checked = false;
                }
            }
            else  // Switched off.
            {
                spectrometricThermometer.PIDOff();
            }
            tBoxOutputVoltage.Enabled = !chBoxPID.Checked;
        }

        /// <summary>
        /// Rewriting enabled = Indexing disabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxRwt_CheckedChanged(object sender, EventArgs e)
        {
            tBoxFilenameIndex.Enabled = !chBoxRewrite.Checked;
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
                tBoxFilenameIndex.Enabled = chBoxSave.Checked;
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

        private void Form_main_FormClosed(object sender, FormClosedEventArgs e)
        {
            spectrometricThermometer.Close();
        }

        /// <summary>
        /// Initialization procedure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_Load(object sender, EventArgs e)
        {
            {
                TextBox[] textBoxes = pnl2.Controls.OfType<TextBox>().ToArray();
                for (int i = 0; i < textBoxes.Length; i++)
                {
                    textBoxes[i].TextChanged += Pln2_textBoxes_TextChanged;
                }
            }
            // Change MsgBox language from CZ to ENG.
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Change language from CZ to ENG (decimal point).
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Buttons.text which can change.
            btnInitialize.Text = constants.btnInitializeText[0];
            btnMeasure.Text = constants.btnMeasureText[0];
            btnSwitch.Text = constants.btnSwitchText[0];
            lblInfo.Text = "";
            // Log message box first line.
            My_msg(constants.initialMessage, newline: false);
            // ComboBox initialization.
            cBoxDeviceType.DataSource = Spectrometer.Factory.ListNames();
            cBoxDeviceType.SelectedIndex = 0;  // Select the first item (default).
            // Plotting.
            plt1 = Figs_Initialize(formsPlotLeft, constants.fig1Title, constants.fig1LabelX, constants.fig1LabelY);
            plt2 = Figs_Initialize(formsPlotRight, constants.fig2Title, constants.fig2LabelX, constants.fig2LabelY);
            formsPlotLeft.MouseClicked += FormsPlotLeft_MouseClicked;

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

            BtnDefaultSize_Click(this, EventArgs.Empty);
            this.CenterToScreen();
        }

        /// <summary>
        /// Redraw GUI when size changed.
        /// Implemented as difference to defaultSize const.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_SizeChanged(object sender, EventArgs e)
        {
            // Horizontal.
            // How much wider than default.
            int delta = this.Size.Width - constants.defaultSize.Width;

            formsPlotLeft.Width = formsPlotRight.Width = constants.formsPlotSize + (delta / 2);
            // pictureBox1.Location is constant.
            // 458 = default pictureBox2.Location.X
            formsPlotRight.Location = new Point(
                (constants.formsPlotSize + 12) + (delta / 2),  // X
                formsPlotRight.Location.Y);  // Y

            // Follows part, which is constant between minimum and default.
            if (delta < 0)
                delta = 0;
            pnlSettings.Left = 323 + delta;  // 323 = pnlSettings default left.
            pnl2.Left = 642 + delta;  // 642 = pnl2 default left.
            tBoxLog.Width = 199 + delta;  // 199 = tBoxLog default width.
            lineShape1.X2 = 903 + delta;  // 903 = lineShape1 default X2.

            // Vertical.
            delta = this.Size.Height - constants.defaultSize.Height;
            formsPlotLeft.Height = formsPlotRight.Height = constants.formsPlotSize + delta;

            if (plt1 != null && plt2 != null)
            {
                Plot(null, null);  // Render only.
            }
        }

        /// <summary>
        /// Print wavelength on left-mouse click.
        /// Recalculate absorption edge on ctrl-click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormsPlotLeft_MouseClicked(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { return; }

            double clickedWavelength = plt1.CoordinateFromPixelX(e.X);
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                // If clicked at app start.
                if (spectrometricThermometer.Measurement.Wavelengths == null)
                    return;
                plt2.Title(string.Format(
                    "T = {0:0.0} °C",
                    spectrometricThermometer.AnalyzeMeasurement(clickedWavelength)));
                Plot(
                    spectrometricThermometer.Measurement,
                    spectrometricThermometer.MTemperatureHistory);
            }
            else
            {
                My_msg(string.Format("Clicked: {0:0.0} nm", clickedWavelength));
            }
        }

        /// <summary>
        /// Add line to the message window.
        /// </summary>
        /// <param name="text">Text to be added.</param>
        /// <param name="newline">Add a newline character ahead of text?</param>
        private void My_msg(string text, bool newline)
        {
            if (newline)
                tBoxLog.AppendText("\r\n");

            tBoxLog.AppendText(text);
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
                    sP: tBoxPID_P.Text,
                    sI: tBoxPID_I.Text,
                    sD: tBoxPID_D.Text,
                    sSetPoint: tBoxSetpoint.Text,
                    sRamp: tBoxRamp.Text,
                    vDeltaAbsMax: tBoxVoltageStep.Text,
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
        /// Plot and render both charts. Null to render only.
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="temperatureHistory"></param>
        public void Plot(
            IMeasurementPlot measurement,
            SpectrometricThermometer.ITemperatureHistory temperatureHistory)
        {

            // Figure 1.
            if (measurement != null)
            {
                double[] wavelengths = measurement.Wavelengths;
                double[] intensities = measurement.Intensities;
                Measurement.Fit_graphics fitGraphics = measurement.FitGraphics;

                plt1.YLabel(constants.fig1LabelY);
                plt1.XLabel(constants.fig1Title);
                plt1.AxisAuto(horizontalMargin: .9, verticalMargin: .5);
                plt1.Axis(null, null, 0, null);
                plt1.Clear();
                plt1.PlotScatter(wavelengths, intensities, color: Color.Red, markerSize: 0, lineWidth: 0.1);

                //if (fitGraphics != null)
                {
                    plt1.PlotScatter(
                        new double[2] { fitGraphics.LL.X, fitGraphics.LR.X },
                        new double[2] { fitGraphics.LL.Y, fitGraphics.LR.Y },
                        markerSize: 0, color: Color.Black);
                    plt1.PlotScatter(
                        new double[2] { fitGraphics.RL.X, fitGraphics.RR.X },
                        new double[2] { fitGraphics.RL.Y, fitGraphics.RR.Y },
                        markerSize: 0, color: Color.Black);
                    plt1.PlotPoint(
                        fitGraphics.Crossing.X, fitGraphics.Crossing.Y,
                        markerSize: 5, color: Color.Blue);

                    // Mark points where fitting occured.
                    // Left line.
                    double[] x = new double[fitGraphics.LIndexes[1]];
                    double[] y = new double[fitGraphics.LIndexes[1]];
                    Array.Copy(wavelengths, fitGraphics.LIndexes[0], x, 0, fitGraphics.LIndexes[1]);
                    Array.Copy(intensities, fitGraphics.LIndexes[0], y, 0, fitGraphics.LIndexes[1]);
                    plt1.PlotScatter(x, y, markerSize: 3, color: Color.Black, lineWidth: 0);
                    // Right line.
                    x = new double[fitGraphics.RIndexes[1]];
                    y = new double[fitGraphics.RIndexes[1]];
                    Array.Copy(wavelengths, fitGraphics.RIndexes[0], x, 0, fitGraphics.RIndexes[1]);
                    Array.Copy(intensities, fitGraphics.RIndexes[0], y, 0, fitGraphics.RIndexes[1]);
                    plt1.PlotScatter(x, y, markerSize: 3, color: Color.Black, lineWidth: 0);
                }
            }
            formsPlotLeft.Render();

            // Figure 2.
            if (temperatureHistory != null)
            {
                double[] times = temperatureHistory.Times;
                double[] temperatures = temperatureHistory.Temperatures;
                if (temperatures.Length > 0)
                {
                    // Axis.
                    if (temperatures.Length == 1)
                    {
                        plt2.Axis(times[0] - 1, times[0] + 1, temperatures[0] - 1, temperatures[0] + 1);
                    }
                    else
                    {
                        double? timesMin, timesMax, temperaturesMin, temperaturesMax;
                        timesMin = timesMax = temperaturesMin = temperaturesMax = null;
                        bool timesEqual = times.Min() == times.Max();
                        bool temperatureEqual = temperatures.Min() == temperatures.Max();

                        if (timesEqual)
                        {
                            timesMin = times.Min() - 1;
                            timesMax = times.Max() + 1;
                        }
                        if (temperatureEqual)
                        {
                            temperaturesMin = temperatures.Min() - 1;
                            temperaturesMax = temperatures.Max() + 1;
                        }
                        if (!(timesEqual || temperatureEqual))  // Main case.
                        {
                            plt2.AxisAuto(.9, .5);
                            if (times.Min() >= 0) { timesMin = 0d; };
                        }
                        plt2.Axis(timesMin, timesMax, temperaturesMin, temperaturesMax);
                    }
                    // Plot.
                    plt2.Clear();
                    plt2.PlotScatter(times, temperatures, color: Color.Red);
                }
            }
            formsPlotRight.Render();
        }

        private void Measurement_AveragingFinished(object sender, Measurement.AveragingFinishedEventArgs e)
        {
            if (e.LoadingMultipleSpectra)
            {
                LabelBold(lblAverage, true);
            }
        }

        private void Spectrometer_ExposureFinished(object sender, Spectrometer.ExposureFinishedEventArgs e)
        {
            if (chBoxETAdaptation.Checked)
            {
                LabelBold(lblETAdaptation, e.Adapted);
                tBoxExposureTime.Text = string.Format("{0:#.00}", e.ExposureTime);
            }
        }
        /// <summary>
        /// FolderBrowserDialog on doubleclick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TBoxFilename_DoubleClick(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                //fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.SelectedPath = Application.StartupPath;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    tBoxFilename.Text = Path.Combine(fbd.SelectedPath, "Spectrum");
            }
        }
        /// <summary>
        /// GUI constants like labels or default size.
        /// </summary>
        public struct Constants
        {
            // Buttons text.
            public readonly string[] btnInitializeText;
            public readonly string[] btnMeasureText;
            public readonly string[] btnSwitchText;
            /// <summary>
            /// Default size of the <see cref="Form_main"/> window.
            /// </summary>
            public readonly Size defaultSize;
            public readonly string fig1LabelX;
            public readonly string fig1LabelY;
            // Plotting - Figure 1.
            public readonly string fig1Title;
            public readonly string fig2LabelX;
            public readonly string fig2LabelY;
            // Plotting - Figure 2.
            public readonly string fig2Title;
            public readonly int formsPlotSize;
            public readonly string helpFileName;
            /// <summary>
            /// First line printed by <see cref="My_msg(string)"/> at program start.
            /// </summary>
            public readonly string initialMessage;

            public Constants(string[] btnInitializeText, string[] btnMeasureText,
                string[] btnSwitchText, Size defaultSize, string fig1LabelX, string fig1LabelY,
                string fig1Title, string fig2LabelX, string fig2LabelY, string fig2Title,
                int formsPlotSize, string helpFileName, string initialMessage)
            {
                this.btnInitializeText = btnInitializeText ?? throw new ArgumentNullException(nameof(btnInitializeText));
                this.btnMeasureText = btnMeasureText ?? throw new ArgumentNullException(nameof(btnMeasureText));
                this.btnSwitchText = btnSwitchText ?? throw new ArgumentNullException(nameof(btnSwitchText));
                this.defaultSize = defaultSize;
                this.fig1LabelX = fig1LabelX ?? throw new ArgumentNullException(nameof(fig1LabelX));
                this.fig1LabelY = fig1LabelY ?? throw new ArgumentNullException(nameof(fig1LabelY));
                this.fig1Title = fig1Title ?? throw new ArgumentNullException(nameof(fig1Title));
                this.fig2LabelX = fig2LabelX ?? throw new ArgumentNullException(nameof(fig2LabelX));
                this.fig2LabelY = fig2LabelY ?? throw new ArgumentNullException(nameof(fig2LabelY));
                this.fig2Title = fig2Title ?? throw new ArgumentNullException(nameof(fig2Title));
                this.formsPlotSize = formsPlotSize;
                this.helpFileName = helpFileName ?? throw new ArgumentNullException(nameof(helpFileName));
                this.initialMessage = initialMessage ?? throw new ArgumentNullException(nameof(initialMessage));
            }

            public static Constants Constants_EN => new Constants(
                initialMessage: "Spectrometric Thermometer (version 3.5)",
                helpFileName: "Help.pdf",
                defaultSize: new Size(width: 929, height: 743),
                formsPlotSize: 446,
                fig1Title: "Spectrum",
                fig1LabelX: "Wavelength (nm)",
                fig1LabelY: "Intensity(a.u.)",
                fig2Title: "T: ? °C",
                fig2LabelX: "Time (sec)",
                fig2LabelY: "Temperature (°C)",
                btnInitializeText: new string[] { "&Initialize", "Choose dev&ice", "Disc&onnect" },
                btnMeasureText: new string[] { "&Measure", "S&top" },
                btnSwitchText: new string[] { "Pr&epare to switch", "Ab&ort" });
        }
    }
}