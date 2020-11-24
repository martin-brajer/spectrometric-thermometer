using MathNet.Numerics.Interpolation;

namespace spectrometric_thermometer
{
    /// <summary>
    /// Store calibration data and use it.
    /// Abstract class.
    /// </summary>
    public interface ICalibration
    {
        /// <summary>
        /// Recelculate absorption edge wavelength
        /// to the sample temperature.
        /// </summary>
        /// <param name="wavelength">Input wavelength.</param>
        /// <returns>Temperature.</returns>
        double? Use(double? wavelength);
    }

    /// <summary>
    /// Calibration using multiple points and interpolate.
    /// Inherits from abstract Calibration class.
    /// </summary>
    public class Calibration_Points : ICalibration
    {
        private IInterpolation interpolation = null;

        public double[] EdgeWavelength { get; set; }
        public double[] Temperature { get; set; }

        /// <summary>
        /// Experimental constructor with preset calibration data.
        /// </summary>
        /// <returns>Constructed class.</returns>
        public static Calibration_Points Experimental()
        {
            double[] edgeWavelength = new double[] { 0, 1, 2, 3, 4, 5 };
            double[] temperature = new double[] { 4, 6, 5, 10, 12, 10 };

            return new Calibration_Points(edgeWavelength, temperature);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="edgeWavelength">Absorbtion edge wavelength
        /// calibration array.</param>
        /// <param name="tempetature">Temperature calibration array.</param>
        public Calibration_Points(double[] edgeWavelength, double[] tempetature)
        {
            EdgeWavelength = edgeWavelength;
            Temperature = tempetature;

            // interpolation = MathNet.Numerics.Interpolate.Linear(EdgeWavelength, Temperature);
            interpolation = MathNet.Numerics.Interpolate.CubicSpline(EdgeWavelength, Temperature);
        }

        /// <summary>
        /// Interpolation.
        /// </summary>
        /// <param name="wavelength">Input.</param>
        /// <returns>Temperature.</returns>
        public double? Use(double? wavelength)
        {
            return wavelength == null ? null : (double?)interpolation.Interpolate((double)wavelength);
        }
    }

    /// <summary>
    /// Calibration using polynomial coefficients.
    /// Inherits from abstract Calibration class.
    /// </summary>
    public class Calibration_Polynom : ICalibration
    {
        MathNet.Numerics.Polynomial polynom;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="coefs">Polynomial coefficients.</param>
        public Calibration_Polynom(double[] coefs)
        {
            polynom = new MathNet.Numerics.Polynomial(coefs);
        }

        /// <summary>
        /// Recelculate absorption edge wavelength
        /// to the sample temperature.
        /// </summary>
        /// <param name="wavelength">Input wavelength.</param>
        /// <returns>Temperature.</returns>
        public double? Use(double? wavelength)
        {
            return wavelength == null ? null : (double?)polynom.Evaluate((double)wavelength);
        }
    }
}
