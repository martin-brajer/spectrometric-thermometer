using System;

namespace spectrometric_thermometer
{

    public partial class SpectraProcessor
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
                InchwormMethodLeft = InchwormMethod.Old;
                InchwormMethodRight = InchwormMethod.Constant;
            }
            
            public Parameters(int pointsToSkip, double epsilonLimit, int smoothingIntensities,
                int smoothingDerivatives, int searchHalfWidth, double sliderLimit,
                InchwormMethod inchwormMethodLeft, InchwormMethod inchwormMethodRight)
            {
                PointsToSkip = pointsToSkip;
                EpsilonLimit = epsilonLimit;
                SmoothingIntensities = smoothingIntensities;
                SmoothingDerivatives = smoothingDerivatives;
                SearchHalfWidth = searchHalfWidth;
                SliderLimit = sliderLimit;
                InchwormMethodLeft = inchwormMethodLeft;
                InchwormMethodRight = inchwormMethodRight;
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
            /// Which method to use in search for the left line.
            /// </summary>
            public InchwormMethod InchwormMethodLeft { get; set; }
            /// <summary>
            /// Which method to use in search for the right line.
            /// </summary>
            public InchwormMethod InchwormMethodRight { get; set; }
        }
    }
}
