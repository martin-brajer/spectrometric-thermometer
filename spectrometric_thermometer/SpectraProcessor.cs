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
        public double? Temperature { get; private set; } = null;
        /// <summary>
        /// Data for plotting fitting graphics.
        /// </summary>
        public FitGraphics MFitGraphics { get; private set; }

        /// <summary>
        /// Enough (<see cref="SpectraToLoad"/>) data gathered, then analysed.
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
        /// Index at which max 1st derivative was found.
        /// Value (-1) means no search has been done yet.
        /// Reset in BtnMeas_Click() and BtnLoad_Click().
        /// </summary>
        public int IndexMax1D { get; set; } = -1;
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
        /// Event invoking method on finished exposure.
        /// If allowed, run <see cref="ExposureTimeAdaptation"/>
        /// and send info through <see cref="EventArgs"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDataReady()
        {
            intensitiesBuffer = SmoothPointPeaks(intensitiesBuffer);
            if (SpectraLoaded > 1)
            {
                Intensities = intensitiesBuffer.Select(r => r / SpectraLoaded).ToArray();
            }
            else
            {
                Intensities = intensitiesBuffer;
            }
            Time = new DateTime(timeBufferTicks / SpectraLoaded);

            SpectraLoaded = 0;  // Reset.
            Temperature = Analyze();
            DataReady?.Invoke(this, new DataReadyEventArgs(
                multipleSpectraLoaded: SpectraToLoad > 1,
                temperature: (double)Temperature));
        }

        /// <summary>
        /// Find absorbtion edge and convert its value to temperature.
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
                intensitiesBuffer = intensities;
                timeBufferTicks = ticks;

                SpectraLoaded = 1;
            }
            // Add new data to already set up object.
            else
            {
                intensitiesBuffer = intensitiesBuffer.Zip(intensities, (x, y) => x + y).ToArray();
                timeBufferTicks += ticks;

                SpectraLoaded++;
            }
        }

        /// <summary>
        /// Find absorption edge.
        /// Return double.NegativeInfinity if not found.
        /// </summary>
        /// <returns>Absorption edge wavelength.</returns>
        private double FindAbsorptionEdge()
        {
            double absorptionEdge = double.NegativeInfinity;

            // Interpolation on smoothed intensities.
            var interpolation = MathNet.Numerics.Interpolate.CubicSpline(
                Wavelengths,
                SmoothBoxcar(Intensities, windowHalf: MParameters.SmoothingIntensities));
            // Max in smoothed first derivative.
            double[] derivative = new double[Wavelengths.Length];
            for (int i = 0; i < Wavelengths.Length; i++)
            {
                derivative[i] = interpolation.Differentiate(
                    Wavelengths[i]);
            }
            derivative = SmoothBoxcar(derivative, windowHalf: MParameters.SmoothingDerivatives);
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
            double[] poptRight = Inchworm(ref iLeft, ref iRight, direction: false);
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
            double[] poptLeft = null;
            switch (MParameters.Absorbtion_edge)
            {
                case "Inchworm":
                    {
                        Inchworm(ref iLeft2, ref iRight2, direction: false);
                        poptLeft = Inchworm(ref iLeft2, ref iRight2, direction: true);
                        break;
                    }

                case "Inchworm_VIT":
                    {
                        Inchworm_VIT(ref iLeft2, ref iRight2, direction: false);
                        poptLeft = Inchworm_VIT(ref iLeft2, ref iRight2, direction: true);
                        break;
                    }

                case "const":
                    {
                        poptLeft = new double[2] { Intensities[slider], 0 };
                        //popt2 = new double[2] { interpolation.Interpolate(slider), 0 };
                        break;
                    }

                default:
                    break;
            }
            // Setting up the fitting graphics. See documentation.
            if (poptRight != null && poptLeft != null)
            {
                Func<double, double[], double> line = MathNet.Numerics.Polynomial.Evaluate;
                double lineInverseRight(double y) => (y - poptRight[0]) / poptRight[1];
                double intensitiesMax = Intensities.Max();

                double W_Min = Wavelengths[0];
                double W_IMax = Wavelengths[Array.IndexOf(Intensities, intensitiesMax)];
                //double W_IMax = Wavelengths[Intensities.ToList().IndexOf(intensitiesMax)];
                // W: LeftLine(W) == RightLine(W)
                absorptionEdge = (poptLeft[0] - poptRight[0]) / (poptRight[1] - poptLeft[1]);

                MFitGraphics = new FitGraphics(
                    leftLineXs: new[] { W_Min, W_IMax },
                    leftLineYs: new[] { line(W_Min, poptLeft), line(W_IMax, poptLeft) },
                    rightLineXs: new[] { lineInverseRight(0), lineInverseRight(intensitiesMax) },
                    rightLineYs: new[] { 0, intensitiesMax },
                    intersection: new PointF((float)absorptionEdge, (float)line(absorptionEdge, poptRight)),
                    markedLeft: Tuple.Create(iLeft, iRight - iLeft + 1),
                    markedRight: Tuple.Create(iLeft2, iRight2 - iLeft2 + 1));
            }
            else
            {
                // Next time search again the whole vector.
                IndexMax1D = -1;
            }
            return absorptionEdge;
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
            int length = iRight - iLeft + 1;  // Start with three points.
            // Fit line a subarray.
            // Subarrays, where fittting takes place.
            double[] xx = new double[length];  // Copy out.
            double[] yy = new double[length];  // Copy out.
            Array.Copy(Wavelengths, iLeft, xx, 0, length);
            Array.Copy(Intensities, iLeft, yy, 0, length);
            var popt = MathNet.Numerics.Fit.Line(xx, yy);

            // Tuple<double, double> to  double[2].
            var _popt = new double[] { popt.Item1, popt.Item2 };

            // Goodness of fit.
            double[] yFit = new double[length];
            for (int i = 0; i < length; i++)
            {
                yFit[i] = MathNet.Numerics.Polynomial.Evaluate(z: Wavelengths[i + iLeft], coefficients: _popt);
            }
            var _rSquared = MathNet.Numerics.GoodnessOfFit.RSquared(yFit, yy);

            return new FitData() { popt = _popt, rSquared = _rSquared };
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

            for (int i = 0; i < length; i++)  // Box shifting.
            {
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
            public DataReadyEventArgs(bool multipleSpectraLoaded, double temperature)
            {
                MultipleSpectraLoaded = multipleSpectraLoaded;
                Temperature = temperature;
            }
            public bool MultipleSpectraLoaded { get; private set; }
            public double Temperature { get; private set; }
        }
    }
}
