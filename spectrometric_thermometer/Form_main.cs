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
        public SpectrometricThermometer spectrometricThermometer;

        // Controls.
        private readonly IBack2Front Front = null;
        // Plotting.
        private ScottPlot.Plot plt1;
        private ScottPlot.Plot plt2;

        /// <summary>
        /// Connecting device phase.
        /// </summary>
        private InitializationState initializationState = InitializationState.Initial;
        private MeasuringState measuringState = MeasuringState.Idle;
        private TemperatureControlMode temperatureControlMode = TemperatureControlMode.None;
        /// <summary>
        /// Temperature control mode.
        /// Used while switching temperature control device (this program or Eurotherm).
        /// </summary>
        private TemperatureControlMode TemperatureControlMode1
        {
            get => temperatureControlMode;
            set
            {
                Front.My_msg("TCM " + value.ToString());
                temperatureControlMode = value;
            }
        }

        // Set virtual LED color. Nullable!
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

        public Form_main()
        {
            InitializeComponent();
            
            Front = new Back2Front(front: this);
            spectrometricThermometer = new SpectrometricThermometer(front: Front);


            //// Plug-in MEF init.
            //// An aggregate catalog that combines multiple catalogs.
            //var catalog = new AggregateCatalog();
            //// Adds all the parts found in the same assembly as the Program class.
            //catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));
            ////catalog.Catalogs.Add(new DirectoryCatalog("D:\\Programming\\C#\\SimpleCalculator\\SimpleCalculator\\Extensions"));

            //// Create the CompositionContainer with the parts in the catalog.
            //_container = new CompositionContainer(catalog);

            //// Fill the imports of this object.
            //try
            //{
            //    this._container.ComposeParts(this);
            //}
            //catch (CompositionException compositionException)
            //{
            //    Console.WriteLine(compositionException.ToString());
            //}
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
            // Change MsgBox language from CZ to ENG.
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Change language from CZ to ENG (decimal point).
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Buttons.text which can change.
            btnInitialize.Text = Constants.BtnInitializeText[0];
            btnMeasure.Text = Constants.BtnMeasureText[0];
            btnSwitch.Text = Constants.BtnSwitchText[0];
            lblInfo.Text = "";
            // Log message box first line.
            My_msg(Constants.InitialMessage, newline: false);
            // ComboBox initialization.
            cBoxDeviceType.DataSource = Spectrometer.Factory.ListNames();
            cBoxDeviceType.SelectedIndex = 0;  // Select the first item (default).
            // Plotting.
            plt1 = Figs_Initialize(formsPlot1, Constants.Fig1Title, Constants.Fig1LabelX, Constants.Fig1LabelY);
            plt2 = Figs_Initialize(formsPlot2, Constants.Fig2Title, Constants.Fig2LabelX, Constants.Fig2LabelY);
            formsPlot1.MouseClicked += FormsPlot1_MouseClicked;

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
                PlotData(
                    spectrometricThermometer.Measurement.Wavelengths,
                    spectrometricThermometer.Measurement.Intensities,
                    spectrometricThermometer.Measurement.FitGraphics,
                    spectrometricThermometer.Times.ToArray(),
                    spectrometricThermometer.Temperatures.ToArray());
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
        /// Search for spectrometers, choose one, close the communication.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInitialize_Click(object sender, EventArgs e)
        {
            switch (initializationState)
            {
                case InitializationState.Initial:
                    if (spectrometricThermometer.BtnInitialize())
                    {
                        cBoxDeviceType.Enabled = false;
                        initializationState = InitializationState.Select_spectrometer;
                    }
                    break;

                case InitializationState.Select_spectrometer:
                    if (int.TryParse(tboxSelectSpectrometer.Text, out int choosenDevice))
                    {
                        if (spectrometricThermometer.BtnSelect(choosenDevice,
                            out Tuple<string, string> exposureTimePeriodTime))
                        {
                            tBoxExposureTime.Text = exposureTimePeriodTime.Item1;
                            tBoxPeriodLength.Text = exposureTimePeriodTime.Item2;
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
            btnInitialize.Text = Constants.BtnInitializeText[(int)initializationState];  // Button text.
        }

        /// <summary>
        /// Set parameters and start/stop the measurement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMeasure_Click(object sender, EventArgs e)
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
                    if(spectrometricThermometer.BtnStopMeasurement())
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
            btnMeasure.Text = Constants.BtnMeasureText[(int)measuringState];
        }

        /// <summary>
        /// Show help file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string helpFileName = Constants.HelpFileName;
            if (File.Exists(helpFileName))
            {
                Process.Start(helpFileName);
            }
            else
            {
                My_msg("Help file not found.");
            }
        }

        private void BtnExit_Click(object sender, EventArgs e) => Close();

        private void Form_main_FormClosed(object sender, FormClosedEventArgs e) => spectrometricThermometer.Close();

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
        /// Plot and render both charts. Use List.ToArray().
        /// </summary>
        /// <param name="wavelengths"></param>
        /// <param name="intensities"></param>
        /// <param name="fitGraphics"></param>
        /// <param name="times"></param>
        /// <param name="temperatures"></param>
        /// <returns>Success?</returns>
        private void PlotData(
            double[] wavelengths, double[] intensities, Measurement.Fit_graphics fitGraphics,
            double[] times, double[] temperatures)
        {
            // Figure 1.
            if (wavelengths != null && intensities != null)
            {
                plt1.YLabel(Constants.Fig1LabelY);
                plt1.XLabel(Constants.Fig1Title);
                plt1.AxisAuto(horizontalMargin: .9, verticalMargin: .5);
                plt1.Axis(null, null, 0, null);
                plt1.Clear();
                plt1.PlotScatter(wavelengths, intensities, color: Color.Red, markerSize: 0, lineWidth: 0.1);

                if (fitGraphics != null)
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
                        fitGraphics.crossing.X, fitGraphics.crossing.Y,
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
            formsPlot1.Render();

            // Figure 2.
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
            formsPlot2.Render();
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
        public void My_msg(string text)
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
        /// Redraw GUI when size changed.
        /// Implemented as difference to defaultSize const.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_main_SizeChanged(object sender, EventArgs e)
        {
            // Horizontal.
            // How much wider than default.
            int delta = this.Size.Width - Constants.DefaultSize.Width;

            formsPlot1.Width = formsPlot2.Width = Constants.PictureBoxSize + (delta / 2);
            // pictureBox1.Location is constant.
            // 458 = default pictureBox2.Location.X
            formsPlot2.Location = new Point(
                (Constants.PictureBoxSize + 12) + (delta / 2),  // X
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
            formsPlot1.Height = formsPlot2.Height = Constants.PictureBoxSize + delta;

            if (plt1 == null || plt2 == null)
                return;

            // If measurement.Wavelength/Intensities are not null (not 'no data yet').
            PlotData(null, null);
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
        /// Set default size to the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDefaultSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = Constants.DefaultSize;
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
        /// Selection of the Calibration instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBoxCalibration_SelectedIndexChanged(object sender, EventArgs e)
        {
            spectrometricThermometer.SelectCalibration(cBoxCalibration.SelectedIndex);
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

                        if (!spectrometricThermometer.Times.Any())
                        {
                            spectrometricThermometer.TimeZero = modification;
                        }
                        bool save = chBoxSave.Checked;  // Just load, no saving.
                        spectrometricThermometer.Measurement.Load(
                            wavelengths: waveIntens.Item1,
                            intensities: waveIntens.Item2,
                            ticks: modification.Ticks);

                        chBoxSave.Checked = save;  // Reset.
                        spectrometricThermometer.Measurement.IndexMax1D = -1;
                        plt2.Title(spectrometricThermometer.AnalyzeMeasurement());
                    }
                    PlotData(
                        spectrometricThermometer.Measurement.Wavelengths,
                        spectrometricThermometer.Measurement.Intensities,
                        spectrometricThermometer.Measurement.FitGraphics,
                        spectrometricThermometer.Times.ToArray(),
                        spectrometricThermometer.Temperatures.ToArray());
                }
            }
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
            formsPlot2.Render();
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
                spectrometricThermometer.SwitchModeNone(out double outputVoltage);
                if (!double.IsNaN(outputVoltage))
                {
                    tBoxOutputVoltage.Text = outputVoltage.ToString("F3");
                    TemperatureControlMode1 = TemperatureControlMode.ET2PC_switch;
                }
                else
                {
                    TemperatureControlMode1 = TemperatureControlMode.PC2ET_equal;
                }
            }
            // Sth is already happening => abort.
            else
            {
                spectrometricThermometer.SwitchModeElse();
                TemperatureControlMode1 = TemperatureControlMode.None;
            }
            btnSwitch.Text = Constants.BtnSwitchText[TemperatureControlMode1 != TemperatureControlMode.None ? 0 : 1];
        }

        /// <summary>
        /// GUI constants like labels or default size.
        /// </summary>
        public static class Constants
        {
            /// <summary>
            /// First line printed by <see cref="My_msg(string)"/> at program start.
            /// </summary>
            public static string InitialMessage { get; } = "Spectrometric Thermometer (version 3.5)";
            public static string HelpFileName { get; } = "Help.pdf";
            /// <summary>
            /// Default size of the <see cref="Form_main"/>.
            /// </summary>
            public static Size DefaultSize { get; } = new Size(width: 929, height: 743);
            public static int PictureBoxSize { get; } = 446;  // Default.

            // Plotting.
            public static string Fig1Title { get; } = "Spectrum";
            public static string Fig1LabelX { get; } = "Wavelength (nm)";
            public static string Fig1LabelY { get; } = "Intensity(a.u.)";

            public static string Fig2Title { get; } = "T = ? °C";
            public static string Fig2LabelX { get; } = "Time (sec)";
            public static string Fig2LabelY { get; } = "Temperature (°C)";
            
            // Buttons text.
            public static string[] BtnInitializeText { get; } = { "&Initialize", "Choose dev&ice", "Disc&onnect" };
            public static string[] BtnMeasureText { get; } = { "&Measure", "S&top" };
            public static string[] BtnSwitchText { get; } = { "Pr&epare to switch", "Ab&ort" };
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
    }
}