using System;
using System.Collections.Generic;
using System.Linq;

namespace spectrometric_thermometer
{
    /// <summary>
    /// PID.
    /// </summary>
    public class PIDController
    {
        private double P, I, D;
        private double setPoint;
        private double actualSetPoint;
        private double ramp;

        private double integral;
        // Control voltage for full drive. Empirically for Delta by Jirka.
        private const double vMax = 5.3;  // [V].

        private Buffer<double> times;
        private Buffer<double> errs;

        /// <summary>
        /// Length in seconds
        /// of one ramp / DAC cycle (timerPID).
        /// </summary>
        public double Period { get; set; }

        /// <summary>
        /// Max value of voltage change.
        /// Default value set to 0.1.
        /// </summary>
        public double VDeltaAbsMax { get; set; } = 0.1;

        /// <summary>
        /// Creator.
        /// </summary>
        /// <exception cref="ArgumentException">BufferLen must be at least 2.</exception>
        /// <param name="bufferLen">Length of the buffer
        /// to derive PID parameters from. </param>
        /// <param name="period">Length in seconds
        /// of one ramp / DAC cycle (timerPID).</param>
        public PIDController(int bufferLen, double period)
        {
            if (bufferLen < 2)
                throw new ArgumentException("BufferLen must be at least 2.");
            times = new Buffer<double>(bufferLen);
            errs = new Buffer<double>(bufferLen);

            // Default value.
            Period = period;
        }

        /// <summary>
        /// GUI parameters parsing.
        /// </summary>
        /// <exception cref="ArgumentException">Some parameters are wrong.</exception>
        /// <param name="sP">P const.</param>
        /// <param name="sI">I const.</param>
        /// <param name="sD">D const.</param>
        /// <param name="sSetPoint">Temperature setpoint.</param>
        /// <param name="sRamp">Ramp in (°C / min).</param>
        /// <param name="vDeltaAbsMax">Max voltage change.</param>
        public void ParametersCheck(
            string sP,
            string sI,
            string sD,
            string sSetPoint,
            string sRamp,
            string vDeltaAbsMax,
            string bufferLength)
        {
            if (!double.TryParse(sP, out P))
                throw new ArgumentException(string.Format("P const error! Converted value: {0}.", P));
            if (P <= 0)
                throw new ArgumentException("P const error! Must be positive");
            P = 1 / P;

            if (!double.TryParse(sI, out I))
                throw new ArgumentException(string.Format("I const error! Converted value: {0}.", I));
            if (I < 0)
                throw new ArgumentException("I const error! Must not be negative");
            if (I != 0)
                I = P / I;

            if (!double.TryParse(sD, out D))
                throw new ArgumentException(string.Format("D const error! Converted value: {0}.", D));
            if (D < 0)
                throw new ArgumentException("D const error! Must not be negative");
            D = P * D;

            if (!double.TryParse(sSetPoint, out setPoint))
                throw new ArgumentException(string.Format("Set point error! Converted value: {0}.", setPoint));

            if (!double.TryParse(sRamp, out ramp))
                throw new ArgumentException(string.Format("Ramp error! Converted value: {0}.", ramp));

            if (!double.TryParse(vDeltaAbsMax, out double delta))
            {
                throw new ArgumentException(string.Format("vDeltaAbsMax error! Converted value: {0}.", ramp));
            }
            else
            {
                VDeltaAbsMax = delta;
            }

            if (!int.TryParse(bufferLength, out int bLen))
            {
                throw new ArgumentException(string.Format("Buffer length error! Converted value: {0}.", ramp));
            }
            else
            {
                times.Size = bLen;
                errs.Size = bLen;
            }
        }

        /// <summary>
        /// Initial temperature.
        /// </summary>
        /// <param name="temperature"></param>
        public void Reset(double temperature)
        {
            actualSetPoint = temperature;
            integral = 0;
        }

        /// <summary>
        /// Find new voltage to be send into DAC.
        /// </summary>
        /// <param name="time">Number of seconds since the first measurement.</param>
        /// <param name="temperature">Last temperature.</param>
        /// <param name="vOld">Voltage to be changed.</param>
        /// <returns>New voltage and info string.</returns>
        public Tuple<double, string> Process(
            double time,
            double temperature,
            double vOld)
        {
            // Shift actual setpoint towards global setpoint.
            if (actualSetPoint < setPoint)
            {
                actualSetPoint += ramp * Period / 60;
                if (actualSetPoint > setPoint)
                    actualSetPoint = setPoint;
            }
            else
            {
                actualSetPoint -= ramp * Period / 60;
                if (actualSetPoint < setPoint)
                    actualSetPoint = setPoint;
            }
            string returnInfo = string.Format("{0:#.0} °C => {1:#.0} °C", actualSetPoint, setPoint);

            times.Add(time);
            errs.Add(actualSetPoint - temperature);
            // Not enough information.
            if (!times.Full)
                return Tuple.Create(vOld, returnInfo);  // No change.

            // Find PID parameters.
            double err = errs.Values.Average();
            var popt = MathNet.Numerics.Fit.Line(times.Values, errs.Values);
            double errDerivative = popt.Item2;
            if (integral == 0 && I != 0)
                // Such that next iteration yealds vOut = vOld.
                integral = (vOld / vMax - P * err - D * errDerivative) / I;
            else
                integral += err * Period;
            // Find new voltage.
            double vNew = vMax * (P * err + I * integral + D * errDerivative);
            // Limit into [0, VOutMax].
            vNew = Math.Max(0, vNew);
            vNew = Math.Min(vNew, vMax);

            // Limit voltage change.
            {
                double vDelta = vNew - vOld;  // Voltage change.
                if (vDelta > VDeltaAbsMax)
                    vNew = vOld + VDeltaAbsMax;
                if (vDelta < -VDeltaAbsMax)
                    vNew = vOld - VDeltaAbsMax;
            }

            return Tuple.Create(vNew, returnInfo);
        }
    }

    /// <summary>
    /// Data class. Replace oldest value if new is added into full buffer.
    /// </summary>
    internal class Buffer<T>
    {
        /// <summary>
        /// Buffer size.
        /// </summary>
        private int size = 1;

        private List<T> values = new List<T>();

        /// <summary>
        /// Return stored values as array.
        /// </summary>
        public T[] Values
        {
            get
            {
                return values.ToArray();
            }
        }

        /// <summary>
        /// Is the buffer full?
        /// </summary>
        public bool Full
        {
            get
            {
                return values.Count == Size;
            }
        }

        public int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
                // If buffer is overfilled after size change,
                // delete some items (from the oldest ones).
                TrimToSize();
            }
        }

        private void TrimToSize()
        {
            while (values.Count > Size)
            {
                values.RemoveAt(0);
            }
        }

        /// <summary>
        /// Creator.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Size parameter cannot be lesser than 1.</exception>
        /// <param name="size">Buffer size. Min one.</param>
        public Buffer(int size)
        {
            if (size < 1)
                throw new ArgumentOutOfRangeException("Size parameter cannot be lesser than 1.");
            Size = size;
        }

        /// <summary>
        /// Add new value.
        /// If full, remove the oldest one.
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            TrimToSize();
            values.Add(value);
        }
    }
}
