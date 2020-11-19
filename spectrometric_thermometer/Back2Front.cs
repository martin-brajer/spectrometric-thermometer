using System;
using System.Collections.Generic;
using System.Drawing;

namespace spectrometric_thermometer
{
    public class Back2Front
    {
        private readonly FormMain front;
        
        public Back2Front(FormMain front)
        {
            this.front = front;
        }

        public double ExposureTime { set => front.tBoxExposureTime.Text = string.Format("{0:#.00}", value);  }
        /// <summary>
        /// Running index of next file to be saved.
        /// </summary>
        public string FilenameIndex { set => front.tBoxFilenameIndex.Text = value; }
        public bool BtnCalibrationEnabled { set => front.btnPlotCalibration.Enabled = value; }
        public List<string> CBoxCalibrationDataSource
        {
            set
            {
                if (value != null)
                {
                    front.coBoxCalibration.DataSource = value;
                }
                front.coBoxCalibration.SelectedIndex = 0;
            }
        }
        public KnownColor? LEDColor { set => front.LEDColor = value; }
        public string PidInfo { set => front.lblInfo.Text = value; }
        public string PidVoltage { set => front.tBoxOutputVoltage.Text = value; }
        public string Pid_P { set => front.tBoxPID_P.Text = value; }
        public string Pid_I { set => front.tBoxPID_I.Text = value; }
        public string Pid_D { set => front.tBoxPID_D.Text = value; }
        public int SwitchButtonTextIndex
        {
            set
            {
                FormMain.Constants constants = front.constants;
                string[] texts = constants.BtnSwitchText;

                if (value > texts.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format("Max index is {0}", texts.Length));
                }
                front.btnSwitch.Text = texts[value];
            }
        }
        public double PlotRightTitleTemperature { set => front.PlotRightTitleTemperature = value;  }

        /// <summary>
        ///  Press STOP and DISCONNECT buttons.
        /// </summary>
        public void Disconnect()
        {
            front.BtnMeasure_Click(sender: this, e: EventArgs.Empty);
            front.BtnInitialize_Click(sender: this, e: EventArgs.Empty);
        }

        public void Plot(IMeasurementPlot measurement,
            SpectrometricThermometer.ITemperatureHistory temperatureHistory)
        {
            front.Plot(measurement, temperatureHistory);
        }

        public void LabelBoldAverage(bool bold)
        {
            FormMain.LabelBold(front.lblAverage, bold);
        }

        public void LabelBoldAutoExposureTime(bool bold)
        {
            FormMain.LabelBold(front.lblAutoExposureTime, bold);
        }

        public void My_msg(string text) { front.My_msg(text); }
    }
}