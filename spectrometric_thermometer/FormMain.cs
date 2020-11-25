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
    public partial class FormMain : Form
    {
        public Constants constants = Constants.Constants_EN;
        private readonly Back2Front back2Front = null;
        public SpectrometricThermometer spectrometricThermometer;
        private ScottPlot.Plot plotLeft;
        public ScottPlot.Plot plotRight;
        /// <summary>
        /// 
        /// </summary>
        private bool plotLeftMouseDataControl;

        /// <summary>
        /// Connecting device phase.
        /// </summary>
        private InitializationState initializationState = InitializationState.Initial;
        private MeasuringState measuringState = MeasuringState.Idle;
        
        /// <summary>
        /// Set virtual LED color. Nullable - null means neutral color / off.
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

        /// <summary>
        /// Print formatted temperature in degrees Celsius in <see cref="plotRight"/> title.
        /// </summary>
        public double PlotRightTitleTemperature
        {
            set
            {
                plotRight.Title(string.Format("T = {0:0.0} °C", value));
            }
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


        public FormMain()
        {
            InitializeComponent();
            
            back2Front = new Back2Front(front: this);
            spectrometricThermometer = new SpectrometricThermometer(front: back2Front);
        }

        private void Form_main_Load(object sender, EventArgs e)
        {
            {
                TextBox[] textBoxes = pnlTemp.Controls.OfType<TextBox>().ToArray();
                for (int i = 0; i < textBoxes.Length; i++)
                {
                    textBoxes[i].TextChanged += PlnTemp_textBoxes_TextChanged;
                }
            }
            // Change MsgBox language from CZ to ENG.
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Change language from CZ to ENG (decimal point).
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-GB");
            // Buttons.text which can change.
            btnInitialize.Text = constants.BtnInitializeText[0];
            btnMeasure.Text = constants.BtnMeasureText[0];
            btnSwitch.Text = constants.BtnSwitchText[0];
            lblInfo.Text = "";
            // Log message box first line.
            My_msg(constants.InitialMessage, newline: false);
            // ComboBox initialization.
            coBoxDeviceType.DataSource = Spectrometer.Factory.ListNames();
            coBoxDeviceType.SelectedIndex = 0;  // Select the first item (default).
            // Plotting.
            plotLeft = PlotInitialize(formsPlotLeft, constants.PlotLeft_Title,
                constants.PlotLeft_XLabel, constants.PlotLeft_YLabel);
            plotRight = PlotInitialize(formsPlotRight, constants.PlotRight_Title,
                constants.PlotRight_XLabel, constants.PlotRight_YLabel);
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

            BtnDefaultSize_Click(this, EventArgs.Empty);
            this.CenterToScreen();
            ChkBoxPlotControl_CheckedChanged(this, EventArgs.Empty);
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            spectrometricThermometer.Close();
        }

        /// <summary>
        /// Initialize <see cref="ScottPlot.Plot"/> object.
        /// </summary>
        /// <param name="formsPlot">Where to fit the fig to.</param>
        /// <param name="labelTitle">Fig label.</param>
        /// <param name="labelX">X axis label.</param>
        /// <param name="labelY">Y axis label.</param>
        /// <returns></returns>
        private ScottPlot.Plot PlotInitialize(ScottPlot.FormsPlot formsPlot,
            string labelTitle, string labelX, string labelY)
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
        /// Set label font to bold (true) or regular.
        /// </summary>
        /// <param name="label">Target label.</param>
        /// <param name="bold">True => bold.</param>
        public static void LabelBold(Label label, bool bold)
        {
            label.Font = new Font(label.Font, bold ? FontStyle.Bold : FontStyle.Regular);
        }

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
                    if (spectrometricThermometer.BtnInitialize(coBoxDeviceType.SelectedIndex))
                    {
                        coBoxDeviceType.Enabled = false;
                        initializationState = InitializationState.Select_spectrometer;
                    }
                    break;

                case InitializationState.Select_spectrometer:
                    if (int.TryParse(tboxDeviceID.Text, out int deviceID))
                    {
                        if (spectrometricThermometer.BtnSelect(deviceID))
                        {
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
                        coBoxDeviceType.Enabled = true;
                        initializationState = InitializationState.Initial;
                    }
                    break;

                default:
                    break;
            }
            btnInitialize.Text = constants.BtnInitializeText[(int)initializationState];  // Button text.
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
                            periodLength: tBoxPeriod.Text,
                            average: tBoxAverage.Text,
                            exposureTime: tBoxExposureTime.Text,
                            adaptation: chBoxAutoExposureTime.Checked,
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
                        LabelBold(lblAutoExposureTime, false);
                        LabelBold(lblAverage, false);
                        My_msg("Stop");
                        if (chBoxPID.Checked)  // No regulation without new data.
                        {
                            chBoxPID.Checked = false;
                            My_msg("PID stopped (no new data)!");
                        }
                        measuringState = MeasuringState.Idle;
                    }
                    break;

                default:
                    break;
            }
            pnlMain.Enabled = measuringState == MeasuringState.Idle;  // Disable settings while measuring.
            btnMeasure.Text = constants.BtnMeasureText[(int)measuringState];
        }

        /// <summary>
        /// Plot and render both charts. Accessor. Null to skip.
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="temperatureHistory"></param>
        public void Plot(
            ISpectraProcessorPlot measurement,
            SpectrometricThermometer.ITemperatureHistory temperatureHistory)
        {
            if (!(chBoxPlot.Checked && chBoxPlot.Enabled)) { return; }

            if (measurement != null)
            {
                PlotLeft(measurement.Wavelengths, measurement.Intensities, measurement.MFitGraphics);
            }

            if (temperatureHistory != null)
            {
                PlotRight(temperatureHistory.Times, temperatureHistory.Temperatures);
            }
        }

        /// <summary>
        /// The left plot is mainly used for spectra.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fitGraphics">Skip by null or
        /// <see cref="SpectraProcessor.FitGraphics.Empty"/></param>
        /// <param name="xLabel">Fill null >> default from <see cref="Constants"/>.</param>
        /// <param name="yLabel">Fill null >> default from <see cref="Constants"/>.</param>
        private void PlotLeft(double[] x, double[] y,
            SpectraProcessor.FitGraphics fitGraphics,
            string xLabel = null, string yLabel = null)
        {
            if (plotLeft == null) { return; }
            if (x.Length != y.Length) { return; }
            
            plotLeft.XLabel(xLabel ?? constants.PlotLeft_XLabel);
            plotLeft.YLabel(yLabel ?? constants.PlotLeft_YLabel);
            plotLeft.AxisAuto(horizontalMargin: .9, verticalMargin: .5);
            plotLeft.Axis(null, null, 0, null);
            plotLeft.Clear();
            plotLeft.PlotScatter(x, y, markerSize: 0, color: Color.Red, lineWidth: 0.1);

            if (!SpectraProcessor.FitGraphics.IsNullOrEmpty(fitGraphics))
            {
                // Plot fit lines and crossing point.
                plotLeft.PlotScatter(fitGraphics.LeftLineXs, fitGraphics.LeftLineYs,
                    markerSize: 0, color: Color.Black);
                plotLeft.PlotScatter(fitGraphics.RightLineXs, fitGraphics.RightLineYs,
                    markerSize: 0, color: Color.Black);
                plotLeft.PlotPoint(fitGraphics.Intersection.X, fitGraphics.Intersection.Y,
                    markerSize: 5, color: Color.Blue);
                
                // Mark points where fitting occured.
                double[] xFit, yFit;
                // Left line.
                (xFit, yFit) = fitGraphics.MarkedPlotLeft(x, y);
                plotLeft.PlotScatter(xFit, yFit, markerSize: 3, color: Color.Black, lineWidth: 0);
                // Right line.
                (xFit, yFit) = fitGraphics.MarkedPlotRight(x, y);
                plotLeft.PlotScatter(xFit, yFit, markerSize: 3, color: Color.Black, lineWidth: 0);
            }
            formsPlotLeft.Render();
        }

        /// <summary>
        /// The right plot is mainly used for temperature history.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xLabel">Fill null >> default from <see cref="Constants"/>.</param>
        /// <param name="yLabel">Fill null >> default from <see cref="Constants"/>.</param>
        private void PlotRight(double[] x, double[] y, string xLabel = null, string yLabel = null)
        {
            if (plotRight == null) { return; }
            double length;
            if(x.Length == y.Length)
            {
                length = x.Length;
            }
            else { return; }

            if (length > 0)
            {
                double? timesMin, timesMax, temperaturesMin, temperaturesMax;  // Axis limits.
                if (length == 1)
                {
                    timesMin = x[0] - 1;
                    timesMax = x[0] + 1;
                    temperaturesMin = y.Min() - 1;
                    temperaturesMax = y.Max() + 1;
                }
                else
                {
                    timesMin = timesMax = temperaturesMin = temperaturesMax = null;
                    bool timesEqual = x.Min() == x.Max();
                    bool temperaturesEqual = y.Min() == y.Max();

                    if (timesEqual)
                    {
                        timesMin = x.Min() - 1;
                        timesMax = x.Max() + 1;
                    }
                    if (temperaturesEqual)
                    {
                        temperaturesMin = y.Min() - 1;
                        temperaturesMax = y.Max() + 1;
                    }
                    if (!(timesEqual || temperaturesEqual))  // Main case.
                    {
                        plotRight.AxisAuto(.9, .5);
                        if (x.Min() >= 0)
                        {
                            timesMin = 0d;
                        };
                    }
                }
                plotRight.Axis(timesMin, timesMax, temperaturesMin, temperaturesMax);
                plotRight.XLabel(xLabel ?? constants.PlotRight_XLabel);
                plotRight.YLabel(yLabel ?? constants.PlotRight_YLabel);
                plotRight.Clear();
                plotRight.PlotScatter(x, y, color: Color.Red);
            }
            formsPlotRight.Render();
        }

        /// <summary>
        /// Reset temperature history. Data and plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            spectrometricThermometer.ResetTemperatureHistory();
            plotRight.Clear();
        }

        /// <summary>
        /// Set default size to the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDefaultSize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = ModifierKeys.HasFlag(Keys.Control) ? this.MinimumSize : constants.DefaultSize;
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Show help file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHelp_Click(object sender, EventArgs e)
        {
            string helpFileName = constants.HelpFileName;
            if (File.Exists(helpFileName))
            {
                Process.Start(helpFileName);
            }
            else
            {
                My_msg("Help file not found.");
            }
        }


        private void ChkBoxPlotControl_CheckedChanged(object sender, EventArgs e)
        {
            plotLeftMouseDataControl = chkBoxPlotControl.Checked;
            formsPlotLeft.Configure(
                enablePanning: !plotLeftMouseDataControl,
                enableRightClickMenu: !plotLeftMouseDataControl);
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
                    // Plot last loaded.
                    PlotRightTitleTemperature = spectrometricThermometer.BtnLoadSpectra(openFileDialog.FileNames);
                    Plot(spectrometricThermometer.MSpectraProcessor, spectrometricThermometer.MTemperatureHistory);
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
                double? temperature = spectrometricThermometer.MSpectraProcessor.Calibration.Use(x[i]);
                if (temperature != null)
                {
                    y[i] = (double)temperature;
                }
            }
            PlotRight(x, y, xLabel: "Temperature (°C)", yLabel: "Calibration");
        }

        /// <summary>
        /// Reload config.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReloadConfig_Click(object sender, EventArgs e)
        {
            if (spectrometricThermometer.ConfigurationFile_Load())
            {
                My_msg("Configuration file reloaded.");
            }
        }

        /// <summary>
        /// Save temperature history into a file.
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
            if (chBoxPID.Checked)
            {
                My_msg("Stop PID control first.");
                return;
            }
            bool temperatureControlMode_None = spectrometricThermometer.Switch(out double outputVoltage);
            tBoxOutputVoltage.Text = outputVoltage.ToString("F3");
            btnSwitch.Text = constants.BtnSwitchText[temperatureControlMode_None ? 0 : 1];
        }

        /// <summary>
        /// Selection of the Calibration instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CBoxCalibration_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (spectrometricThermometer != null)  // Program start.
            {
                spectrometricThermometer.SelectCalibration(coBoxCalibration.SelectedIndex);
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
        /// Rewriting enabled == Indexing disabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChBoxRwt_CheckedChanged(object sender, EventArgs e)
        {
            tBoxFilenameIndex.Enabled = !chBoxRewrite.Checked;
        }

        /// <summary>
        /// Disabling saving disables rewrite checkbox (for clarity purposes).
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
        /// Redraw GUI when size changes.
        /// Implemented as difference to <see cref="Constants.DefaultSize"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            int horizontal = this.Size.Width - constants.DefaultSize.Width;
            formsPlotLeft.Width = formsPlotRight.Width = constants.FormsPlotSize + (horizontal / 2);
            // pictureBoxLeft.Location is constant.
            formsPlotRight.Location = new Point(
                x: (constants.FormsPlotSize + 12) + (horizontal / 2),
                y: formsPlotRight.Location.Y);
            if (horizontal < 0)  // From now on, defaults between minimum and default.
            {
                horizontal = 0;
            }
            pnlMain.Left = 323 + horizontal;  // 323 = pnlSettings default left.
            pnlPlot.Left = 323 + horizontal;  // 323 = pnlPlot default left.
            pnlTemp.Left = 637 + horizontal;  // 637 = pnlTemp default left.
            pnlPID.Left = 749 + horizontal;  // 749 = pnlPID default left.
            tBoxLog.Width = 199 + horizontal;  // 199 = tBoxLog default width.
            lineShapePlot.X2 = 904 + horizontal;  // 904 = lineShape1 default X2.

            int vertical = this.Size.Height - constants.DefaultSize.Height;
            formsPlotLeft.Height = formsPlotRight.Height = constants.FormsPlotSize + vertical;
            
            chBoxPlot.Enabled = this.Size != this.MinimumSize;
        }

        /// <summary>
        /// Print wavelength on left-mouse click. Recalculate absorption edge on ctrl-click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormsPlotLeft_MouseClicked(object sender, MouseEventArgs e)
        {
            if (!plotLeftMouseDataControl) { return; }

            double clickedWavelength = plotLeft.CoordinateFromPixelX(e.X);
            if (e.Button == MouseButtons.Left)
            {
                My_msg(string.Format("Clicked: {0:0.0} nm", clickedWavelength));
            }
            else if (e.Button == MouseButtons.Right)
            {
                // If clicked at app start.
                if (spectrometricThermometer.MSpectraProcessor.Wavelengths == null) { return; }

                PlotRightTitleTemperature = spectrometricThermometer.AnalyzeMeasurement(clickedWavelength);
                Plot(spectrometricThermometer.MSpectraProcessor, spectrometricThermometer.MTemperatureHistory);
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
                {
                    tBoxFilename.Text = Path.Combine(fbd.SelectedPath, "Spectrum");
                }
            }
        }

        /// <summary>
        /// Add text to the message window.
        /// </summary>
        /// <param name="text">Text to be added.</param>
        /// <param name="newline">Add a newline character in front of the text?</param>
        public void My_msg(string text, bool newline=true)
        {
            if (newline)
            {
                tBoxLog.AppendText("\r\n");
            }
            tBoxLog.AppendText(text);
        }

        /// <summary>
        /// Any TextBox in Pln2 changed its text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlnTemp_textBoxes_TextChanged(object sender, EventArgs e)
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
                chBoxPID.Checked = false;
                My_msg(ex.Message);
            }
        }
    }
}
