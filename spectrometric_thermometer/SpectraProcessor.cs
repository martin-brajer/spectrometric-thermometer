using System;
using System.Drawing;
using System.Linq;

namespace spectrometric_thermometer
{
    public interface ISpectraProcessorPlot
    {
        double[] Wavelengths { get; }
        double[] Intensities { get; }
        SpectraProcessor.FitGraphics MFitGraphics { get; }
    }

    /// <summary>
    /// Spectrum measurement data-structure.
    /// </summary>
    public partial class SpectraProcessor : ISpectraProcessorPlot
    {
        /// <summary>
        /// DateTime.ticks sum.
        /// Meant to be divided by spectraLoaded to get average.
        /// </summary>
        private long timeBufferTicks;
        /// <summary>
        /// Intensity arrays sum.
        /// Meant to be divided by spectraLoaded to get average.
        /// </summary>
        private double[] intensitiesBuffer;
        private int spectraLoaded = 0;

        public double[] Wavelengths { get; private set; } = null;
        public double[] Intensities { get; private set; } = null;
        public DateTime Time { get; private set; }
        public double? AbsorptionEdge { get; private set; } = null;
        public double? Temperature { get; private set; } = null;
        /// <summary>
        /// Data for plotting fitting graphics.
        /// </summary>
        public FitGraphics MFitGraphics { get; private set; }

        /// <summary>
        /// Enough (<see cref="SpectraToLoad"/>) spectra gathered, then analysed.
        /// </summary>
        public event EventHandler<DataReadyEventArgs> DataReady;

        /// <summary>
        /// Calibration of temperature vs absorbtion edge wavelength.
        /// </summary>
        public ICalibration Calibration { get; set; } = null;
        /// <summary>
        /// Constants used in <see cref="FindAbsorptionEdge"/>. With defaults.
        /// </summary>
        public Parameters MParameters { get; set; } = new Parameters();
        /// <summary>
        /// Index at which 1st derivative maximum was found. Nullable.
        /// </summary>
        public int? MaxDerivativeIndex { get; set; } = null;
        /// <summary>
        /// Number of spectra used in averaging.
        /// Setter also resets number of loaded spectra.
        /// </summary>
        public int SpectraToLoad { get; set; } = 1;
        /// <summary>
        /// How many spectra has already been loaded.
        /// If full, invoke <see cref="OnDataReady"/> event.
        /// </summary>
        private int SpectraLoaded
        {
            get => spectraLoaded;
            set
            {
                spectraLoaded = value;
                if (spectraLoaded >= SpectraToLoad)
                {
                    OnDataReady();
                }
            }
        }

        /// <summary>
        /// Add new spectrum.
        /// </summary>
        /// <param name="wavelengths"></param>
        /// <param name="intensities"></param>
        /// <param name="ticks">DataTime.ticks.</param>
        public void AddSpectra(double[] wavelengths, double[] intensities, long ticks)
        {
            if (SpectraLoaded == 0)
            {
                Wavelengths = wavelengths;
                intensitiesBuffer = new double[Wavelengths.Length];  // Defaults to zeros.
                timeBufferTicks = 0;
            }
            intensitiesBuffer = intensitiesBuffer.Zip(intensities, (x, y) => x + y).ToArray();
            timeBufferTicks += ticks;
            SpectraLoaded++;
        }

        /// <summary>
        /// Event invoking method on finished exposure.
        /// If allowed, run <see cref="ExposureTimeAdaptation"/>
        /// and send info through <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDataReady()
        {
            intensitiesBuffer = SmoothPointPeaks(intensitiesBuffer);
            intensitiesBuffer = SmoothBoxcar(intensitiesBuffer,
                windowHalf: MParameters.SmoothingIntensities);

            Intensities = SpectraLoaded == 1 ? intensitiesBuffer :
                intensitiesBuffer.Select(item => item / SpectraLoaded).ToArray();
            Time = new DateTime(timeBufferTicks / SpectraLoaded);
            AbsorptionEdge = FindAbsorptionEdge();
            Temperature = Calibration?.Use(wavelength: AbsorptionEdge);

            SpectraLoaded = 0;  // Reset.
            DataReady?.Invoke(this, new DataReadyEventArgs(multipleSpectraLoaded: SpectraLoaded > 1));
        }

        /// <summary>
        /// Find absorption edge.
        /// </summary>
        /// <returns>Absorption edge wavelength. Null on failure.</returns>
        private double? FindAbsorptionEdge()
        {
            // Find index of maximum of smoothed first derivative => MaxDerivativeIndex.
            double[] derivative = new double[Wavelengths.Length];
            var interpolation = MathNet.Numerics.Interpolate.CubicSpline(Wavelengths, Intensities);
            for (int i = 0; i < Wavelengths.Length; i++)
            {
                derivative[i] = interpolation.Differentiate(Wavelengths[i]);
            }
            derivative = SmoothBoxcar(derivative, windowHalf: MParameters.SmoothingDerivatives);
            {
                double[] shortDerivative = derivative;
                int startIndex = 0;
                // Derivative maximum was found last time => search around that wavelength.
                if (MaxDerivativeIndex != null)
                {
                    startIndex = (int)MaxDerivativeIndex - MParameters.SearchHalfWidth;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                    int lenght = (2 * MParameters.SearchHalfWidth) + 1;
                    if (lenght + startIndex > Wavelengths.Length)
                    {
                        lenght = Wavelengths.Length - startIndex;
                    }
                    shortDerivative = new double[lenght];
                    Array.Copy(derivative, startIndex, shortDerivative, 0, lenght);
                }
                MaxDerivativeIndex = Array.IndexOf(shortDerivative, shortDerivative.Max()) + startIndex;
            }

            // Inchworm around max of 1st derivative.
            int iLeft = (int)MaxDerivativeIndex;
            int iRight = (int)MaxDerivativeIndex;
            Inchworm(ref iLeft, ref iRight, goLeft: true, method: MParameters.InchwormMethodRight);
            double[] poptRight = Inchworm(ref iLeft, ref iRight, goLeft: false, method: MParameters.InchwormMethodRight);

            // Slide left as long as line or saddle is found.
            int slider = iLeft - MParameters.PointsToSkip;  // Skip a few points between the two lines.
            slider = slider < 0 ? 0 : slider;  // Skip only if possible.

            double derivativeMinimum = derivative[slider];
            while (
                slider > 0 &&  // No OutOfRange.
                               // Search for line. Puls forgiving slider constant.
                derivative[slider] <= derivativeMinimum + MParameters.SliderLimit &&
                derivative[slider] >= 0)  // Search for saddle.
            {
                if (derivative[slider] <= derivativeMinimum)
                {
                    derivativeMinimum = derivative[slider];
                }
                slider--;
            }
            
            // Inchworm around the slider point.
            int iLeft2 = slider;
            int iRight2 = slider;
            Inchworm(ref iLeft2, ref iRight2, goLeft: false, method: MParameters.InchwormMethodLeft);
            double[] poptLeft = Inchworm(ref iLeft2, ref iRight2, goLeft: true, method: MParameters.InchwormMethodLeft);

            // Setting up FitGraphics.
            if (poptRight == null || poptLeft == null)
            {
                MaxDerivativeIndex = null;  // Fail => next time search again the whole vector.
                return null;
            }
            else
            {
                double lineLeft(double x) => MathNet.Numerics.Polynomial.Evaluate(x, poptLeft);
                double lineRight(double x) => MathNet.Numerics.Polynomial.Evaluate(x, poptRight);
                double lineRightInverse(double y) => (y - poptRight[0]) / poptRight[1];
                
                double intensitiesMax = Intensities.Max();
                double WMin = Wavelengths[0];
                double WIMax = Wavelengths[Array.IndexOf(Intensities, intensitiesMax)];
                // W: LeftLine(W) == RightLine(W)
                double absorptionEdge = (poptLeft[0] - poptRight[0]) / (poptRight[1] - poptLeft[1]);

                MFitGraphics = new FitGraphics(
                    leftLineXs: new[] { WMin, WIMax },
                    leftLineYs: new[] { lineLeft(WMin), lineLeft(WIMax) },
                    rightLineXs: new[] { lineRightInverse(0), lineRightInverse(intensitiesMax) },
                    rightLineYs: new[] { 0, intensitiesMax },
                    intersection: new PointF((float)absorptionEdge, (float)lineRight(absorptionEdge)),
                    markedLeft: Tuple.Create(iLeft, iRight - iLeft + 1),
                    markedRight: Tuple.Create(iLeft2, iRight2 - iLeft2 + 1));
                return absorptionEdge;
            }
        }

        /// <summary>
        /// Fit a line at as wide interval as possible.
        /// Continue as long as the fit is "good".
        /// </summary>
        /// <exception cref="ArgumentException">iLeft must not be greater than iRight!</exception>
        /// <param name="iLeft">Left-end index.</param>
        /// <param name="iRight">Right-end index.</param>
        /// <param name="goLeft">Widening of the interval direction. True means left.</param>
        /// <returns>Fitting parameters.
        /// The first one is constant, the second is slope.</returns>
        private double[] Inchworm(ref int iLeft, ref int iRight, bool goLeft, InchwormMethod method)
        {
            if (iLeft > iRight)
            {
                throw new ArgumentException("iLeft must not be greater than iRight!");
            }
            if (iLeft == iRight)  // Len is 1 point. Add one in proper direction.
            {
                if (iLeft == 0)  // Left end.
                {
                    iRight++;
                }
                else if (iRight == Wavelengths.Length - 1)  // Right end.
                {
                    iLeft--;
                }
                else  // In between.
                {
                    if (goLeft)
                    {
                        iLeft--;
                    }
                    else
                    {
                        iRight++;
                    }
                }
            }  // Now we have at least 2 points. 3rd is added

            switch (method)
            {
                case InchwormMethod.Old:
                    return InchwormOld(ref iLeft, ref iRight, goLeft: goLeft);

                case InchwormMethod.Vit:
                    return InchwormVit(ref iLeft, ref iRight, goLeft: goLeft);

                case InchwormMethod.Constant:
                    return new double[2] { Intensities[iLeft], 0 };

                default:
                    throw new ArgumentException();
            }
        }

        private double[] InchwormOld(ref int iLeft, ref int iRight, bool goLeft)
        {
            int iLeftLoc = iLeft;
            int iRightLoc = iRight;

            double rSquaredLast = 0d;
            double rSquaredMin = double.MaxValue / 2;

            while (rSquaredLast < (rSquaredMin + MParameters.EpsilonLimit))
            {
                if (goLeft)  // Widen the interval (if possible).
                {
                    if (iLeftLoc - 1 < 0) { break; }
                    iLeftLoc--;
                }
                else
                {
                    if (iRightLoc + 1 > Wavelengths.Length - 1) { break; }
                    iRightLoc++;
                }

                FitData fit = LineFitBetween(iLeftLoc, iRightLoc);
                //rSquaredLast = fit.rSquared;
                rSquaredLast = Math.Log(1 - fit.rSquared);
                if (rSquaredLast < rSquaredMin)
                {
                    rSquaredMin = rSquaredLast;
                    iLeft = iLeftLoc;
                    iRight = iRightLoc;
                }
            }
            // Drop the last point.
            // Was the loop ever executed? (iLeft==0 || iRight==Wavelengths.Length-1)
            if (iLeft < iRight)
            {
                return LineFitBetween(iLeft, iRight).popt;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Widens inteval given in parametrss by fitting a line there.
        /// Continue as long as the fit is "good".
        /// Set in config file: const_eps = 3.5.
        /// 
        /// Vit's variation.
        /// </summary>
        /// <param name="iLeft">Left-end index.</param>
        /// <param name="iRight">Right-end index.</param>
        /// <param name="goLeft">Widening of the interval direction. True means left.</param>
        /// <returns>Fitting parameters.
        /// The first one is intercept, the second is slope.</returns>
        private double[] InchwormVit(ref int iLeft, ref int iRight, bool goLeft)
        {
            // Make three points out of one, when iLeft = iRight.
            if (goLeft)
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
                if (goLeft)
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
                if (goLeft)
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
        /// Fit subarray by line and find R^2.
        /// </summary>
        /// <param name="iLeft">Start index.</param>
        /// <param name="iRight">End index (included).</param>
        /// <returns>Fit parameters and RSquared.</returns>
        private FitData LineFitBetween(int iLeft, int iRight)
        {
            // Trim data.
            int length = iRight - iLeft + 1;  // Start with three points. "iRight" included.
            double[] wavelengths = new double[length];  // Copy out.
            double[] intensities = new double[length];  // Copy out.
            Array.Copy(this.Wavelengths, iLeft, wavelengths, 0, length);
            Array.Copy(this.Intensities, iLeft, intensities, 0, length);
            // Fit.
            var popt = MathNet.Numerics.Fit.Polynomial(wavelengths, intensities, 1);
            // Goodness of fit.
            double[] modelledValues = wavelengths.Select(
                x => MathNet.Numerics.Polynomial.Evaluate(x, popt)).ToArray();
            double rSquared = MathNet.Numerics.GoodnessOfFit.CoefficientOfDetermination(modelledValues, intensities);
            
            return new FitData() { popt = popt, rSquared = rSquared };
        }

        /// <summary>
        /// Smooth input array by boxcar algorithm.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="windowHalf">Window width is (2 * windowHalf + 1).</param>
        /// <returns>Inpput-like.</returns>
        public static double[] SmoothBoxcar(double[] input, int windowHalf = 5)
        {
            if (windowHalf == 0)
                return input;

            int length = input.Length;
            double[] output = new double[length];

            for (int i = 0; i < length; i++)  // Box position.
            {
                // Trim overshoots. Find box's left and right half-widths.
                int left = i - windowHalf < 0 ? i : windowHalf;
                int right = i + windowHalf > length ? length - i : windowHalf;

                double boxSum = 0;
                for (int j = i - left; j < i + right; j++)  // Inside the box.
                {
                    boxSum += input[j];
                }
                output[i] = boxSum / (left + right + 1);  // Box average.
            }
            return output;
        }

        /// <summary>
        /// Replace point peaks with neighbors' average.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="maxDifference">Max allowed difference of
        /// value and neighbourhood average.</param>
        public static double[] SmoothPointPeaks(double[] input, double maxDifference = 20)
        {
            for (int i = 1; i < input.Length - 2; i++)  // Ignore the first and the last point.
            {
                double neighbourhoodAverage = (input[i - 1] + input[i + 1]) / 2;
                // Peak in "qwe_00075" differs by 1436 to its surrounding.
                if (Math.Abs(input[i] - neighbourhoodAverage) > maxDifference)
                {
                    input[i] = neighbourhoodAverage;
                }
            }
            return input;
        }

        public enum InchwormMethod
        {
            /// <summary>
            /// 
            /// </summary>
            Old,
            /// <summary>
            /// 
            /// </summary>
            Vit,
            /// <summary>
            /// Horizontal line at "first point" intensity.
            /// </summary>
            Constant,
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
        public class DataReadyEventArgs : EventArgs
        {
            public DataReadyEventArgs(bool multipleSpectraLoaded)
            {
                MultipleSpectraLoaded = multipleSpectraLoaded;
            }
            public bool MultipleSpectraLoaded { get; private set; }
        }
    }
}
