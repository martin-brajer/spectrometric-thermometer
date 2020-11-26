using System;
using System.IO.Ports;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Base class for serial port communication.
    /// </summary>
    public abstract class SerialPortConverter
    {
        protected SerialPort port = null;

        /// <summary>
        /// No port returns null.
        /// </summary>
        public virtual string PortName
        {
            get
            {
                if (port == null)
                    return null;

                else
                    return port.PortName;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SerialPortConverter()
        {
            Close();
        }

        /// <summary>
        /// Go through ports and find DAC.
        /// </summary>
        /// <exception cref="ApplicationException">No port found.</exception>
        public abstract void FindPort();
        /// <summary>
        /// Open port.
        /// </summary>
        public void Open()
        {
            port?.Open();
        }
        /// <summary>
        /// Close port.
        /// </summary>
        public void Close()
        {
            port?.Close();
        }

        /// <summary>
        /// Read port after a brief delay,
        /// so the message can arrive.
        /// </summary>
        /// <param name="mPort"></param>
        /// <returns></returns>
        protected string PortRead(ref SerialPort mPort)
        {
            System.Threading.Thread.Sleep(100);
            return mPort.ReadExisting();
        }
        /// <summary>
        /// Read the actual port.
        /// </summary>
        /// <returns></returns>
        protected string PortRead()
        {
            return PortRead(ref port);
        }
    }


    /// <summary>
    /// Abstract class for digital to analog converters.
    /// Search serial ports for DAC and
    /// communicate with it.
    /// </summary>
    public abstract class DigitalToAnalogConverter : SerialPortConverter
    {
        protected int numberOfChannels;

        /// <summary>
        /// Last value written by DAC.
        /// </summary>
        public double LastWrittenValue { get; protected set; } = 0;

        /// <summary>
        /// Without bool parameter. Default it to "save" (true).
        /// </summary>
        /// <param name="V"></param>
        /// <param name="N"></param>
        public void SetV(double V, int N)
        {
            SetV(V: V, N: N, save: true);
        }
        /// <summary>
        /// Writes voltage value into DAC channel N.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Wrong channel number N.</exception>
        /// <exception cref="ApplicationException">Wrong reply.</exception>
        /// <param name="V">Voltage.</param>
        /// <param name="N">Channel number.</param>
        /// <param name="save">Change LastWrittenValue?</param>
        public abstract void SetV(double V, int N, bool save);
        /// <summary>
        /// Read all DAC outputs.
        /// </summary>
        /// <exception cref="ApplicationException">Wrong reply.</exception>
        /// <returns>Array of outputs.</returns>
        public abstract double[] GetV();


        /// <summary>
        /// Inherits from DigitalToAnalogConverter.
        /// Specified to the lab's DAC.
        /// </summary>
        public class Lab : DigitalToAnalogConverter
        {
            /// <summary>
            /// Setting order accepted and executed answer.
            /// </summary>
            readonly string ackOK = "*B10" + (char)13;

            /// <summary>
            /// Creator.
            /// </summary>
            public Lab()
            {
                numberOfChannels = 2;
            }

            public override void FindPort()
            {
                string reply;

                string[] portNames = SerialPort.GetPortNames();
                if (portNames.Length == 0)
                    throw new ApplicationException("No port found (DAC).");
                string failMessage = "DAC not found.";

                foreach (string portName in portNames)
                {
                    // Create the serial port with basic settings
                    SerialPort mPort = new SerialPort(portName,  // "COM1", ....
                        9600, Parity.None, 8, StopBits.One);

                    // Begin communications.
                    try
                    {
                        mPort.Open();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        failMessage += string.Format("\r\n  {0} access denied.", portName);
                        continue;
                    }

                    // Write a string
                    mPort.Write("*B1VR" + (char)13);
                    reply = PortRead(ref mPort);

                    // If everything is ok.
                    if (reply.StartsWith("*B10 1 1 2 1"))
                    {
                        port = mPort;
                        return;
                    }
                    // Not everything is ok, but still responding.
                    else if (!String.IsNullOrEmpty(reply))
                    {
                        // Settings 1.
                        mPort.Write("*B1VS 1 1" + (char)13);
                        reply = PortRead(ref mPort);

                        if (reply.StartsWith(ackOK))
                        {
                            // Settings 2.
                            mPort.Write("*B1VS 2 1" + (char)13);
                            reply = PortRead(ref mPort);

                            if (reply.StartsWith(ackOK))
                            {
                                // If both settings responded ackOK.
                                port = mPort;
                                return;
                            }
                        }
                    }
                    mPort.Close();
                }
                throw new ApplicationException(failMessage);
            }

            /// <summary>
            /// Writes voltage value into DAC channel N.
            /// </summary>
            /// <param name="V">Voltage.</param>
            /// <param name="N">Channel number (starting by 1).</param>
            public override void SetV(double V, int N, bool save)
            {
                if (N < 1 || N > numberOfChannels)
                    throw new ArgumentOutOfRangeException(string.Format(
                        "Max channel number is {0}. Called {1}.", numberOfChannels, N));

                int B = V2B(V);
                port.Write("*B1RS " + N + " " + B + (char)13);

                string reply = PortRead(ref port);
                if (reply != ackOK)
                {
                    port = null;
                    throw new ApplicationException(string.Format(
                        "Wrong answer while writing: '{0}'. Closing port.", reply));
                }

                if (save)
                    LastWrittenValue = V;
            }

            /// <summary>
            /// Read both DAC outputs.
            /// </summary>
            /// <returns></returns>
            public override double[] GetV()
            {
                string reply;
                double V1, V2 = 0;

                port.Write("*B1RR" + (char)13);
                reply = PortRead(ref port);
                if (!string.IsNullOrEmpty(reply))
                {
                    string[] splitted = reply.Split(' ');
                    if (splitted.Length == 5)  // Means 4 spaces.
                    {
                        if (int.TryParse(splitted[3], out int V))
                            V1 = B2V(V);
                        else
                            throw new FormatException("Error in 'int.TryParse(splitted[3])'.");
                        if (int.TryParse(splitted[4], out V))
                            V2 = B2V(V);
                        else
                            throw new FormatException("Error in 'int.TryParse(splitted[4])'.");
                    }
                    else
                        throw new ApplicationException(string.Format(
                            "DAC read error. Wrong length of splitted var ({0})", splitted.Length));
                }
                else
                    throw new ApplicationException("DAC read error. Null or empty reply string.");

                return new double[] { V1, V2 };
            }

            /// <summary>
            /// Voltage (0 - 10 V) to int(0 - ushort.MaxValue).
            /// </summary>
            /// <param name="V">Voltage.</param>
            /// <returns>From zero to ushort.MaxValue.</returns>
            private int V2B(double V)
            {
                int B = (int)Math.Round(V / 10 * ushort.MaxValue, 0);
                B = Math.Max(0, B);
                B = Math.Min(B, ushort.MaxValue);
                return B;
            }

            /// <summary>
            /// Int(0 - ushort.MaxValue) to voltage (0 - 10 V).
            /// </summary>
            /// <param name="V">From zero to ushort.MaxValue.</param>
            /// <returns>Voltage.</returns>
            private double B2V(int B)
            {
                double V = B / ushort.MaxValue * 10;
                return V;
            }
        }

        /// <summary>
        /// Inherits from DigitalToAnalogConverter.
        /// Offline version.
        /// </summary>
        public class Offline : DigitalToAnalogConverter
        {
            private readonly double[] V = new double[3] { 0d, 0d, 0d };

            public Offline()
            {
                numberOfChannels = 3;
            }

            public override string PortName { get; } = "COM_name";

            public override void FindPort()
            { }
            public override double[] GetV()
            {
                return V;
            }
            public override void SetV(double V, int N, bool save)
            {
                this.V[N - 1] = V;

                if (save)
                    LastWrittenValue = V;
            }
        }
    }
    /// <summary>
    /// Analog to digital converter.
    /// <para>Used for Eurotherm voltage readout,
    /// LED control and switch state tracking.</para>
    /// </summary>
    public class AnalogToDigitalConverter_switcher : SerialPortConverter
    {
        /// <summary>
        /// Search through ports for ADC.
        /// </summary>
        /// <exception cref="ApplicationException">No ports found.</exception>
        public override void FindPort()
        {
            string[] portNames = SerialPort.GetPortNames();
            if (portNames.Length == 0)
                throw new ApplicationException("No port found (ADC).");
            string failMessage = "ADC not found.";

            foreach (string portName in portNames)
            {
                // Create the serial port with basic settings.
                SerialPort mPort = new SerialPort(portName, // "COM1", ....
                    115200, Parity.None, 8, StopBits.One);

                // Begin communications.
                try
                {
                    mPort.Open();
                }
                catch (UnauthorizedAccessException)
                {
                    failMessage += string.Format("\r\n {0} access denied.", portName);
                    continue;
                }

                // Set D0-D3 as output, rest as input.
                mPort.Write(":060000000F.." + (char)13 + (char)10);  // Problems after matlab, now need PC restarted.
                // First answer is not ok?
                if (!PortRead(ref mPort).StartsWith(":060000000FEB" + (char)13))
                {
                    mPort.Close();
                    continue;
                }

                // Set output D0-D3 to push-pull.
                mPort.Write(":060001000F.." + (char)13 + (char)10);
                // Second answer is not ok?
                if (!PortRead(ref mPort).StartsWith(":060001000FEA" + (char)13))
                {
                    mPort.Close();
                    continue;
                }

                port = mPort;
                return;
            }
            throw new ApplicationException(failMessage);
        }

        /// <summary>
        /// Read Eurotherm voltage.
        /// </summary>
        /// <exception cref="ApplicationException">Wrong reply.</exception>
        /// <returns>Voltage.</returns>
        public double ReadEurothermV()
        {
            // Read value at input A6.
            port.Write(":0400060001.." + (char)13 + (char)10);
            string reply = PortRead();
            // Sth. is wrong.
            if (reply.Length < 9)
            {
                throw new ApplicationException(string.Format("Reading ADC failed. {0}", reply));
            }
            reply = reply.Substring(6, 4);  // Equivalent to "repl(6:9)" in matlab.
                                            // ADC range = 2.5 V.
                                            // Resistance delta ratio (???) = 2.32 V.
            double V = (double)Convert.ToInt32(reply, fromBase: 16) / ushort.MaxValue * 2.5d / 2.32d;
            // Max V should be 5.1 V?
            if (V > 5.3)
                V = 0;

            return V;
        }

        /// <summary>
        /// Switch state.
        /// </summary>
        /// <exception cref="ApplicationException">Wrong reply.</exception>
        /// <returns>Is the switch set to Eurotherm (true)
        /// or to the program (DAC) (false)?</returns>
        public bool Switched2Eurotherm()
        {
            // Read input IO bites (D0..D7).
            port.Write(":0300030001.." + (char)13 + (char)10);
            string reply = PortRead();
            // Sth. is wrong.
            if (reply.Length < 9)
            {
                throw new ApplicationException("Reading ADC failed.");
            }
            System.Windows.Forms.MessageBox.Show("X" + reply + "X\nX" + reply.Substring(6, 4) + "X");
            System.Windows.Forms.MessageBox.Show(Convert.ToInt32(reply, fromBase: 16).ToString());
            reply = reply.Substring(6, 4);  // Matlab: "hex2dec(repl(6:9))".

            // Is 7th bit equal to 1?
            if ((Convert.ToInt32(reply, fromBase: 16) & 128) == 128)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Switch on green LED.
        /// </summary>
        public void LED_Green()
        {
            port.Write(":0600020002.." + (char)13 + (char)10);
            PortRead();  // Empty buffer.
        }

        /// <summary>
        /// Switch on red LED.
        /// </summary>
        public void LED_Red()
        {
            port.Write(":0600020008.." + (char)13 + (char)10);
            PortRead();  // Empty buffer.
        }

        /// <summary>
        /// Switch the LEDs off.
        /// </summary>
        public void LED_Off()
        {
            port.Write(":0600020000.." + (char)13 + (char)10);
            PortRead();  // Empty buffer.
        }
    }
}
