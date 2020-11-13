using System;
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
        /// Running index of last saved file.
        /// </summary>
        public string FilenameIndex { set => front.tBoxFilenameIndex.Text = value; }
        public KnownColor? LEDColor { set => front.LEDColor = value; }
        public string PidInfo { set => front.lblInfo.Text = value; }
        public string PidVoltage { set => front.tBoxOutputVoltage.Text = value; }
        public int SwitchButtonTextIndex
        {
            set
            {
                var texts = front.MParameters.BtnSwitchText;
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

        public void My_msg(string text) { front.My_msg(text); }
    }
}