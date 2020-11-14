using System;
using System.Collections.Generic;
using System.Drawing;

namespace spectrometric_thermometer
{
    public class Back2Front
    {
        private readonly Form_main front;
        
        public Back2Front(Form_main front)
        {
            this.front = front;
        }

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
                    front.cBoxCalibration.DataSource = value;
                }
                front.cBoxCalibration.SelectedIndex = 0;
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
                Form_main.Constants constants = front.constants;
                string[] texts = constants.btnSwitchText;

                if (value > texts.Length)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format("Max index is {0}", texts.Length));
                }
                front.btnSwitch.Text = texts[value];
            }
        }

        /// <summary>
        ///  Press STOP and DISCONNECT buttons.
        /// </summary>
        public void Disconnect()
        {
            front.BtnMeasure_Click(sender: this, e: EventArgs.Empty);
            front.BtnInitialize_Click(sender: this, e: EventArgs.Empty);
        }

        public void Plot(IMeasurementPlot measurement,
            SpectrometricThermometer.ITemperatureHistory temperatureHistory,
            string title=null)
        {

            if (title != null)
            {
                front.plt2.Title(title);  // MISSING DEG C ???
            }

            front.Plot(measurement, temperatureHistory);
        }

        public void My_msg(string text) { front.My_msg(text); }
    }
}