namespace spectrometric_thermometer
{
    public interface IBack2Front
    {
        string My_msg { set; }
        string PidInfo { set; }
        string PidVoltage { set; }
    }
    
    public class Back2Front : IBack2Front
    {
        private readonly Form_main front;

        public Back2Front(Form_main front)
        {
            this.front = front;
        }

        public string My_msg {set => front.My_msg(value); }
        public string PidVoltage { set => front.tBoxVoltage.Text = value; }
        public string PidInfo { set => front.lblInfo.Text = value; }
    }
}