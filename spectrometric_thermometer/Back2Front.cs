using System;
using System.Drawing;

namespace spectrometric_thermometer
{
    public interface IBack2Front
    {
        string PidInfo { set; }
        string PidVoltage { set; }
        KnownColor? LEDColor { set; }

        void My_msg(string text);
    }

    public class Back2Front : IBack2Front
    {
        private readonly Form_main front;
        
        public Back2Front(Form_main front)
        {
            this.front = front;
        }

        public string PidVoltage { set => front.tBoxOutputVoltage.Text = value; }
        public string PidInfo { set => front.lblInfo.Text = value; }
        public KnownColor? LEDColor { set => front.LEDColor = value; }

        public void My_msg(string text) { front.My_msg(text); }
    }
}