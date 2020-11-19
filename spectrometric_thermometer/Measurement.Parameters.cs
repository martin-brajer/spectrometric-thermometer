using System;

namespace spectrometric_thermometer
{

    public partial class Measurement
    {
        /// <summary>
        /// Store empirical parameters used in <see cref="FindAbsorptionEdge"/>.
        /// </summary>
        public class Parameters
        {
            /// <summary>
            /// Defaults.
            /// </summary>
            public Parameters()
            {
                PointsToSkip = 5;
                EpsilonLimit = 1.2;
                SmoothingIntensities = 10;
                SmoothingDerivatives = 10;
                SearchHalfWidth = 20;
                SliderLimit = 0;
                Absorbtion_edge = "const";
            }
            
            public Parameters(int pointsToSkip, double epsilonLimit, int smoothingIntensities,
                int smoothingDerivatives, int searchHalfWidth, double sliderLimit,
                string absorbtion_edge)
            {
                PointsToSkip = pointsToSkip;
                EpsilonLimit = epsilonLimit;
                SmoothingIntensities = smoothingIntensities;
                SmoothingDerivatives = smoothingDerivatives;
                SearchHalfWidth = searchHalfWidth;
                SliderLimit = sliderLimit;
                Absorbtion_edge = absorbtion_edge ?? throw new ArgumentNullException(nameof(absorbtion_edge));
            }

            /// <summary>
            /// FindAbsorbtionEdge => skipToLeft.
            /// </summary>
            public int PointsToSkip { get; set; }
            /// <summary>
            /// Inchworm => rSquared minimising forgiveness.
            /// </summary>
            public double EpsilonLimit { get; set; }
            /// <summary>
            /// FindAbsorbtionEdge => Intensities smooth window width.
            /// </summary>
            public int SmoothingIntensities { get; set; }
            /// <summary>
            /// FindAbsorbtionEdge => derivatives smooth window width.
            /// </summary>
            public int SmoothingDerivatives { get; set; }
            /// <summary>
            /// FindAbsorbtionEdge => search around last point half-width.
            /// </summary>
            public int SearchHalfWidth { get; set; }
            /// <summary>
            /// How much the slider is forgiving.
            /// </summary>
            public double SliderLimit { get; set; }
            /// <summary>
            /// Which method to use in search for the two lines.
            /// </summary>
            public string Absorbtion_edge { get; set; }
        }
    }
}
