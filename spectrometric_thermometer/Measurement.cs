using System;
using System.Drawing;
using System.Linq;

namespace spectrometric_thermometer
{
    public interface IMeasurementPlot
    {
        double[] Wavelengths { get; }
        double[] Intensities { get; }
        Measurement.FitGraphics MFitGraphics { get; }
    }

    /// <summary>
    /// Spectrum measurement data-structure.
    /// </summary>
    public partial class Measurement : IMeasurementPlot
    {
        /// <summary>
        /// DateTime.ticks sum.
        /// Meant to be divided by spectraLoaded to get average.
        /// </summary>
        private long _ticks2Add;
        /// <summary>
        /// Intensity arrays sum.
        /// Meant to be divided by spectraLoaded to get average.
        /// </summary>
        private double[] _intensities2Add;
        private int _spectraToLoad = 0;
        private int _spectraLoaded = 0;

        /// <summary>
        /// Event on finished averaging.
        /// </summary>
        public event EventHandler<AveragingFinishedEventArgs> AveragingFinished;

        /// <summary>
        /// Wavelength property.
        /// Readable only if not null.
        /// </summary>
        public double[] Wavelengths { get; private set; } = null;

        /// <summary>
        /// Intensity property.
        /// Readable only if not null.
        /// </summary>
        public double[] Intensities { get; private set; } = null;


        public double LastTemperature { get; private set; } = -1;

        /// <summary>
        /// Time property.
        /// Readable only if readyToRead.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Number of spectra used in averaging.
        /// Setter also resets number of loaded spectra.
        /// </summary>
        public int SpectraToLoad
        {
            get
            {
                return _spectraToLoad;
            }

            set
            {
                _spectraToLoad = value;
                SpectraLoaded = 0;  // Reset.
            }
        }

        /// <summary>
        /// Already loaded spectra to average.
        /// If full, invoke AveragingFinished event.
        /// </summary>
        private int SpectraLoaded
        {
            get => _spectraLoaded;
            set
            {
                _spectraLoaded = value;
                if (SpectraLoaded == SpectraToLoad)
                {
                    OnAveragingFinished();
                }
            }
        }

        /// <summary>
        /// Calibration of temperature vs absorbtion edge wavelength.
        /// Plus type of interpolation.
        /// </summary>
        public ICalibration Calibration { get; set; } = null;

        /// <summary>
        /// Index at which max 1st derivative was found.
        /// Value (-1) means no search has been done yet.
        /// Reset in BtnMeas_Click() and BtnLoad_Click().
        /// </summary>
        public int IndexMax1D { get; set; } = -1;

        /// <summary>
        /// Data for plotting fitting graphics.
        /// </summary>
        public FitGraphics MFitGraphics { get; private set; }

        /// <summary>
        /// Constants used in <see cref="FindAbsorptionEdge"/>. With defaults.
        /// </summary>
        public Parameters MParameters { get; set; } = new Parameters();

        /// <summary>
        /// Find absorbtion edge and convert
        /// its value to temperature.
        /// </summary>
        /// <exception cref="InvalidOperationException">No calibration found.</exception>
        /// <returns>Temperature.</returns>
        public double Analyze()
        {
            // No calibration, no fun (temperature history).
            if (Calibration == null)
                throw new InvalidOperationException("No calibration found.");

            // Find the edge.
            double absEdge = FindAbsorptionEdge();

            if (double.IsNegativeInfinity(absEdge))
                throw new InvalidOperationException("Absorption edge not found");

            // Temperature calibration.
            return Calibration.Use(wavelength: absEdge);
        }

        /// <summary>
        /// Set up new averaging (and nulls fitGraphics).
        /// Or add new data.
        /// </summary>
        /// <param name="wavelengths"></param>
        /// <param name="intensities"></param>
        /// <param name="ticks">DataTime.ticks.</param>
        /// <param name="onlyOne">Load data only once. Instead of setting spectraToLoad to one.</param>
        /// <returns>Is it the first package to load?</returns>
        public void Load(double[] wavelengths, double[] intensities, long ticks, bool onlyOne = false)
        {
            // Initialize new data.
            if (SpectraLoaded == 0 || onlyOne)
            {
                Wavelengths = wavelengths;
                _intensities2Add = intensities;
                _ticks2Add = ticks;

                SpectraLoaded = 1;
                //return false;
            }
            // Add new data to already set up object.
            else
            {
                _intensities2Add = _intensities2Add.Zip(intensities, (x, y) => x + y).ToArray();
                _ticks2Add += ticks;

                SpectraLoaded++;
                //return true;
            }
        }

        /// <summary>
        /// Event invoking method on finished exposure.
        /// If allowed, run <see cref="ExposureTimeAdaptation"/>
        /// and send info through <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnAveragingFinished()
        {
            SmoothOutPeaks(ref _intensities2Add);
            if (SpectraLoaded > 1)
            {
                Intensities = _intensities2Add.Select(r => r / SpectraLoaded).ToArray();
            }
            else
            {
                Intensities = _intensities2Add;
            }
            Time = new DateTime(_ticks2Add / SpectraLoaded);

            SpectraLoaded = 0;  // Reset.
            LastTemperature = Analyze();
            AveragingFinished?.Invoke(this, new AveragingFinishedEventArgs(
                multipleSpectraLoaded: SpectraToLoad > 1,
                temperature: LastTemperature));
        }


        /// <summary>
        /// Find absorption edge.
        /// Return double.NegativeInfinity if not found.
        /// </summary>
        /// <returns>Absorption edge wavelength.</returns>
        private double FindAbsorptionEdge()
        {
            double absEdge = double.NegativeInfinity;

            // Interpolation on smoothed intensities.
            var interpolation = MathNet.Numerics.Interpolate.CubicSpline(
                Wavelengths,
                Smooth(Intensities, windowHalf: MParameters.SmoothingIntensities));
            // Max in smoothed first derivative.
            double[] derivative = new double[Wavelengths.Length];
            for (int i = 0; i < Wavelengths.Length; i++)
            {
                derivative[i] = interpolation.Differentiate(
                    Wavelengths[i]);
            }
            derivative = Smooth(derivative, windowHalf: MParameters.SmoothingDerivatives);
            {
                if (IndexMax1D == -1)  // First time => search whole vector.
                {
                    IndexMax1D = derivative.ToList().IndexOf(derivative.Max());
                }
                else  // Next time in shortened version. Still must be inside the whole one.
                {
                    int startIndex = IndexMax1D - MParameters.SearchHalfWidth;  // Start.
                    if (startIndex < 0)
                        startIndex = 0;

                    int len = (2 * MParameters.SearchHalfWidth) + 1;
                    if (len + startIndex > Wavelengths.Length)
                        len = Wavelengths.Length - startIndex;

                    double[] shortDerivative = new double[len];
                    Array.Copy(derivative, startIndex, shortDerivative, 0, len);

                    IndexMax1D = shortDerivative.ToList().IndexOf(shortDerivative.Max()) + startIndex;
                }
            }
            // Inchworm around max of 1st derivative.
            int iLeft = IndexMax1D;
            int iRight = IndexMax1D;
            Inchworm(ref iLeft, ref iRight, direction: true);
            double[] poptR = Inchworm(ref iLeft, ref iRight, direction: false);
            // Slide left as long as line or saddle is found.
            // Skip a few points between the two lines.
            int slider = iLeft - MParameters.PointsToSkip;
            if (slider < 0)  // Skip only if possible.
                slider = 0;
            double derivMinimum = derivative[slider];
            while (
                slider > 0 &&  // No OutOfRange.
                               // Search for line. Puls forgiving slider constant.
                derivative[slider] <= derivMinimum + MParameters.SliderLimit &&
                derivative[slider] >= 0)  // Search for saddle.
            {
                if (derivative[slider] <= derivMinimum)
                    derivMinimum = derivative[slider];
                slider--;
            }
            // Inchworm around the second point.
            int iLeft2 = slider;
            int iRight2 = slider;
            double[] poptL = null;
            switch (MParameters.Absorbtion_edge)
            {
                case "Inchworm":
                    {
                        Inchworm(ref iLeft2, ref iRight2, direction: false);
                        poptL = Inchworm(ref iLeft2, ref iRight2, direction: true);
                        break;
                    }

                case "Inchworm_VIT":
                    {
                        Inchworm_VIT(ref iLeft2, ref iRight2, direction: false);
                        poptL = Inchworm_VIT(ref iLeft2, ref iRight2, direction: true);
                        break;
                    }

                case "const":
                    {
                        poptL = new double[2] { Intensities[slider], 0 };
                        //popt2 = new double[2] { interpolation.Interpolate(slider), 0 };
                        break;
                    }

                default:
                    break;
            }
            // Setting up the fitting graphics.
            if (poptR != null && poptL != null)
            {
                double LLx = Wavelengths[0];
                double intensitiesMax = Intensities.Max();
                int maxIndex = Intensities.ToList().IndexOf(intensitiesMax);
                double LRx = Wavelengths[maxIndex];
                double RLx = -poptR[0] / poptR[1];
                double RRx = (intensitiesMax - poptR[0]) / poptR[1];

                absEdge = (poptL[0] - poptR[0]) / (poptR[1] - poptL[1]);

                // FitGraphics.
                Func<double, double[], double> Line = MathNet.Numerics.Polynomial.Evaluate;

                var LL = new PointF((float)LLx, (float)Line(LLx, poptL));
                var LR = new PointF((float)LRx, (float)Line(LRx, poptL));
                var RL = new PointF((float)RLx, 0);
                var RR = new PointF((float)RRx, (float)intensitiesMax);

                var crossing = new PointF((float)absEdge, (float)Line(absEdge, poptR));

                var LIndexes = new int[2] { iLeft, iRight - iLeft + 1 };
                var RIndexes = new int[2] { iLeft2, iRight2 - iLeft2 + 1 };
                MFitGraphics = new FitGraphics(LL, LR, RL, RR, crossing, LIndexes, RIndexes);
            }
            else
            {
                // Next time search again the whole vector.
                IndexMax1D = -1;
            }
            return absEdge;
        }

        /// <summary>
        /// Smooth input line by boxcar.
        /// Static.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="windowHalf">Window width is 2 * wH + 1.</param>
        /// <returns>Inpput-like.</returns>
        public static double[] Smooth(double[] input, int windowHalf = 5)
        {
            if (windowHalf == 0)
                return input;

            int len = input.Length;
            double sum;
            double[] output = new double[len];

            for (int pos = 0; pos < len; pos++)
            {
                int left = windowHalf;
                if (pos - left < 0) left = pos;
                int right = windowHalf;
                if (pos + right > len) right = len - pos;

                sum = 0;
                for (int i = pos - left; i < pos + right; i++)
                    sum += input[i];
                output[pos] = sum / (left + right + 1);
            }
            return output;
        }

        /// <summary>
        /// Replace point peaks with neighbors' average.
        /// </summary>
        /// <param name="input"></param>
        public static void SmoothOutPeaks(ref double[] input)
        {
            double average;

            for (int pos = 1; pos < input.Length - 2; pos++)
            {
                average = (input[pos - 1] + input[pos] + input[pos + 1]) / 3;
                // Peak in "qwe_00075" differs by 1436 to its surrounding.
                if (input[pos] > average + 20)
                {
                    input[pos] = (input[pos - 1] + input[pos + 1]) / 2;
                }
            }
        }

        /// <summary>
        /// Widens inteval given in parametrss by fitting a line there.
        /// Continue as long as the fit is "good".
        /// Set in config file: const_eps = 3.5.
        /// 
        /// Vit's variation.
        /// </summary>
        /// <param name="iLeft">Left-side index.</param>
        /// <param name="iRight">Right-side index.</param>
        /// <param name="direction">True means left.</param>
        /// <returns>Fitting parameters.
        /// The first one is intercept, the second is slope.</returns>
        private double[] Inchworm_VIT(ref int iLeft, ref int iRight, bool direction)
        {
            // Make three points out of one, when iLeft = iRight.
            if (direction)
                iLeft--;
            else
                iRight++;
            // Where to stop Inchworming.
            double epsMax = double.MaxValue;

            double[] poptDouble = new double[2];
            double eps = 0;  // Distance of the last point and fit.
            int len = 0;

            while (eps < epsMax && iLeft > 0 && iRight < Wavelengths.Length)
            {
                int index;
                if (direction)
                {
                    iLeft--;
                    index = iLeft;
                }
                else
                {
                    iRight++;
                    index = iRight;
                }
                len = iRight - iLeft + 1;  // Start with three points.
                // Fit line a subarray.
                // Subarrays, where fittting takes place.
                double[] xx = new double[len];  // Copy out.
                double[] yy = new double[len];  // Copy out.
                Array.Copy(Wavelengths, iLeft, xx, 0, len);
                Array.Copy(Intensities, iLeft, yy, 0, len);
                Tuple<double, double> popt = MathNet.Numerics.Fit.Line(xx, yy);
                // Tuple<double, double> to  double[2].
                poptDouble = new double[] { popt.Item1, popt.Item2 };
                // Distance in intensity of the last point
                // and fit value at the same wavelength.
                eps = Math.Abs(MathNet.Numerics.Polynomial.Evaluate(
                        Wavelengths[index], poptDouble)
                        - Intensities[index]);

                // First time def it a bit higher than first eps.
                // Eps should go down with number of points.
                if (epsMax == double.MaxValue)
                {
                    epsMax = eps * MParameters.EpsilonLimit;
                }
            }

            // Drop the last point.
            if (len > 0)  // Was the loop ever executed?
            {
                if (direction)
                    iLeft++;
                else
                    iRight--;
                return poptDouble;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Widens inteval given in parametrss by fitting a line there.
        /// Continue as long as the fit is "good".
        /// </summary>
        /// <exception cref="ArgumentException">iLeft must not be greater than iRight!</exception>
        /// <param name="iLeft">Left-side index.</param>
        /// <param name="iRight">Right-side index.</param>
        /// <param name="direction">True means left.</param>
        /// <returns>Fitting parameters.
        /// The first one is intercept, the second is slope.</returns>
        private double[] Inchworm(ref int iLeft, ref int iRight, bool direction)
        {
            if (iLeft > iRight)
            {
                throw new ArgumentException("iLeft must not be greater than iRight!");
            }
            // Len is less than 2 points.
            if (iRight == iLeft)
            {
                if (iLeft == 0)
                {
                    iRight++;
                }
                else if (iRight == Wavelengths.Length - 1)
                {
                    iLeft--;
                }
                else
                {
                    if (direction)
                        iLeft--;
                    else
                        iRight++;
                }
            }

            // Locals.
            int iLeftLoc = iLeft;
            int iRightLoc = iRight;

            double rSquareLast = 0d;
            double rSquareMin = double.MaxValue;

            FitData fit;

            while (rSquareLast < (rSquareMin + MParameters.EpsilonLimit))
            {
                if (direction)
                {
                    if (iLeftLoc - 1 < 0) break;
                    iLeftLoc--;
                }
                else
                {
                    if (iRightLoc + 1 >= Wavelengths.Length) break;
                    iRightLoc++;
                }

                fit = InchwormFit(iLeftLoc, iRightLoc);

                rSquareLast = Math.Log(1 - fit.rSquared);

                if (rSquareLast < rSquareMin)
                {
                    rSquareMin = rSquareLast;
                    iLeft = iLeftLoc;
                    iRight = iRightLoc;
                }
            }

            // Drop the last point.
            if (iLeft < iRight)  // Was the loop ever executed?
            {
                return InchwormFit(iLeft, iRight).popt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Fit subarray of Wavelengths by line.
        /// </summary>
        /// <param name="iLeft"></param>
        /// <param name="iRight"></param>
        /// <returns>Fit parameters and RSquared.</returns>
        private FitData InchwormFit(int iLeft, int iRight)
        {
            int len = iRight - iLeft + 1;  // Start with three points.
            // Fit line a subarray.
            // Subarrays, where fittting takes place.
            double[] xx = new double[len];  // Copy out.
            double[] yy = new double[len];  // Copy out.
            Array.Copy(Wavelengths, iLeft, xx, 0, len);
            Array.Copy(Intensities, iLeft, yy, 0, len);
            var popt = MathNet.Numerics.Fit.Line(xx, yy);

            // Tuple<double, double> to  double[2].
            var _popt = new double[] { popt.Item1, popt.Item2 };

            // Goodness of fit.
            double[] yFit = new double[len];
            for (int i = 0; i < len; i++)
            {
                yFit[i] = MathNet.Numerics.Polynomial.Evaluate(z: Wavelengths[i + iLeft], coefficients: _popt);
            }
            var _rSquared = MathNet.Numerics.GoodnessOfFit.RSquared(yFit, yy);

            return new FitData() { popt = _popt, rSquared = _rSquared };
        }

        /// <summary>
        /// Data structure for fitting results.
        /// </summary>
        public struct FitData
        {
            /// <summary>
            /// Array of fitting parameters.
            /// </summary>
            public double[] popt;
            /// <summary>
            /// R squared. Goodness of fit.
            /// </summary>
            public double rSquared;
        }

        /// <summary>
        /// Carry information about finished measurement.
        /// </summary>
        public class AveragingFinishedEventArgs : EventArgs
        {
            public AveragingFinishedEventArgs(bool multipleSpectraLoaded, double temperature)
            {
                MultipleSpectraLoaded = multipleSpectraLoaded;
                Temperature = temperature;
            }
            public bool MultipleSpectraLoaded { get; private set; }
            public double Temperature { get; private set; }
        }
    }
}
