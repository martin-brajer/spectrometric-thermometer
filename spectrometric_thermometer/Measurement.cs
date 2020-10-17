using System;
using System.Drawing;
using System.Linq;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Spectrum measurement data-structure.
    /// </summary>
    public class Measurement
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
        private int spectraToLoad = 0;
        private int _spectraLoaded = 0;

        /// <summary>
        /// Event on finished averaging.
        /// </summary>
        public event EventHandler<EventArgs> AveragingFinished;

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
                return spectraToLoad;
            }

            set
            {
                spectraToLoad = value;

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
                    OnAveragingFinished(this, EventArgs.Empty);
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
        public Fit_Graphics FitGraphics { get; } = new Fit_Graphics()
        {
            active = false,
        };

        /// <summary>
        /// Constants used in FindAbsorbtionEdge(). With defaults.
        /// </summary>
        public MConstants Constants { get; } = new MConstants
        {
            const_skip = 5,
            const_eps = 1.2,
            const_smooth1 = 10,
            const_smooth2 = 10,
            const_1DHalfW = 20,
            const_slider = 0,
        };

        /// <summary>
        /// Creator.
        /// </summary>
        public Measurement()
        {
        }

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
        public bool Load(double[] wavelengths, double[] intensities, long ticks, bool onlyOne = false)
        {
            // Initialize new data.
            if (SpectraLoaded == 0 || onlyOne)
            {
                FitGraphics.active = false;

                Wavelengths = wavelengths;
                _intensities2Add = intensities;
                _ticks2Add = ticks;

                SpectraLoaded = 1;
                return false;

            }
            // Add new data to already set up object.
            else
            {
                _intensities2Add = _intensities2Add.Zip(intensities, (x, y) => x + y).ToArray();
                _ticks2Add += ticks;

                SpectraLoaded++;
                return true;
            }
        }

        /// <summary>
        /// Event invoking method on finished exposure.
        /// If allowed, run <see cref="ExposureTimeAdaptation"/>
        /// and send info through <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnAveragingFinished(object sender, EventArgs e)
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

            AveragingFinished?.Invoke(this, EventArgs.Empty);
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
                Smooth(Intensities, windowHalf: Constants.const_smooth1));
            // Max in smoothed first derivative.
            double[] derivative = new double[Wavelengths.Length];
            for (int i = 0; i < Wavelengths.Length; i++)
            {
                derivative[i] = interpolation.Differentiate(
                    Wavelengths[i]);
            }
            derivative = Smooth(derivative, windowHalf: Constants.const_smooth2);
            {
                if (IndexMax1D == -1)  // First time => search whole vector.
                {
                    IndexMax1D = derivative.ToList().IndexOf(derivative.Max());
                }
                else  // Next time in shortened version. Still must be inside the whole one.
                {
                    int startIndex = IndexMax1D - Constants.const_1DHalfW;  // Start.
                    if (startIndex < 0)
                        startIndex = 0;

                    int len = (2 * Constants.const_1DHalfW) + 1;
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
            int slider = iLeft - Constants.const_skip;
            if (slider < 0)  // Skip only if possible.
                slider = 0;
            double derivMinimum = derivative[slider];
            while (
                slider > 0 &&  // No OutOfRange.
                               // Search for line. Puls forgiving slider constant.
                derivative[slider] <= derivMinimum + Constants.const_slider &&
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
            switch (Constants.absorbtion_edge)
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
                FitGraphics.active = true;
                Func<double, double[], double> Line = MathNet.Numerics.Polynomial.Evaluate;

                FitGraphics.LL = new PointF((float)LLx, (float)Line(LLx, poptL));
                FitGraphics.LR = new PointF((float)LRx, (float)Line(LRx, poptL));
                FitGraphics.RL = new PointF((float)RLx, 0);
                FitGraphics.RR = new PointF((float)RRx, (float)intensitiesMax);

                FitGraphics.crossing = new PointF((float)absEdge, (float)Line(absEdge, poptR));

                FitGraphics.LIndexes = new int[2] { iLeft, iRight - iLeft + 1 };
                FitGraphics.RIndexes = new int[2] { iLeft2, iRight2 - iLeft2 + 1 };
            }
            else
            {
                // Next time search again the whole vector.
                IndexMax1D = -1;
                FitGraphics.active = false;
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
                    epsMax = eps * Constants.const_eps;
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

            Fit_Data fit;

            while (rSquareLast < (rSquareMin + Constants.const_eps))
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

                fit = Inchworm_fit(iLeftLoc, iRightLoc);

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
                return Inchworm_fit(iLeft, iRight).popt;
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
        private Fit_Data Inchworm_fit(int iLeft, int iRight)
        {
            Fit_Data ret = new Fit_Data();

            int len = iRight - iLeft + 1;  // Start with three points.
            // Fit line a subarray.
            // Subarrays, where fittting takes place.
            double[] xx = new double[len];  // Copy out.
            double[] yy = new double[len];  // Copy out.
            Array.Copy(Wavelengths, iLeft, xx, 0, len);
            Array.Copy(Intensities, iLeft, yy, 0, len);
            var popt = MathNet.Numerics.Fit.Line(xx, yy);

            // Tuple<double, double> to  double[2].
            ret.popt = new double[] { popt.Item1, popt.Item2 };

            // Goodness of fit.
            double[] yFit = new double[len];
            for (int i = 0; i < len; i++)
            {
                yFit[i] = MathNet.Numerics.Polynomial.Evaluate(z: Wavelengths[i + iLeft], coefficients: ret.popt);
            }
            ret.rSquared = MathNet.Numerics.GoodnessOfFit.RSquared(yFit, yy);

            return ret;
        }

        /// <summary>
        /// Data-structure used in Measurements class.
        /// Store information about fitting graphics.
        /// </summary>
        public class Fit_Graphics
        {
            /// <summary>
            /// Are the fit parametrs actual?
            /// I.e. should it be drawn?
            /// </summary>
            public bool active = false;

            /// <summary>
            /// Left line, left end.
            /// </summary>
            public PointF LL;
            /// <summary>
            /// Left line, right end.
            /// </summary>
            public PointF LR;
            /// <summary>
            /// Right line, left end.
            /// </summary>
            public PointF RL;
            /// <summary>
            /// Right line, right end.
            /// </summary>
            public PointF RR;
            /// <summary>
            /// Crossing point of left and right line.
            /// </summary>
            public PointF crossing;

            /// <summary>
            /// Left line indexes, where fitting was done.
            /// Start index and length.
            /// </summary>
            public int[] LIndexes;
            /// <summary>
            /// Right line indexes, where fitting was done.
            /// Start index and length.
            /// </summary>
            public int[] RIndexes;
        }

        /// <summary>
        /// Data-structure used in Measurements class.
        /// Store empirical parameters used in
        /// Measurement.FindAbsorbtionEdge().
        /// </summary>
        public class MConstants
        {
            /// <summary>
            /// FindAbsorbtionEdge => skipToLeft.
            /// </summary>
            public int const_skip;
            /// <summary>
            /// Inchworm => rSquared minimising forgiveness.
            /// </summary>
            public double const_eps;
            /// <summary>
            /// FindAbsorbtionEdge => Intensities smooth window width.
            /// </summary>
            public int const_smooth1;
            /// <summary>
            /// FindAbsorbtionEdge => derivatives smooth window width.
            /// </summary>
            public int const_smooth2;
            /// <summary>
            /// FindAbsorbtionEdge => search around last point half-width.
            /// </summary>
            public int const_1DHalfW;
            /// <summary>
            /// How much the slider is forgiving.
            /// </summary>
            public double const_slider;
            /// <summary>
            /// Which method to use in search
            /// for the two lines.
            /// </summary>
            public string absorbtion_edge;
        }

        /// <summary>
        /// Data structure for fitting results.
        /// </summary>
        public struct Fit_Data
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
    }
}
