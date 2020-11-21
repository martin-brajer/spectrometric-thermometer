using System;
using System.Drawing;

namespace spectrometric_thermometer
{
    public partial class SpectraProcessor
    {
        /// <summary>
        /// Data-structure storing information about fitting graphics.
        /// Two fitting lines (left & right), their intersection and
        /// points used for fitting (to be marked) for both lines.
        /// </summary>
        public class FitGraphics
        {
            /// <summary>
            /// Empty.
            /// </summary>
            private FitGraphics()
            {
                IsEmpty = true;

                LeftLineXs = null;
                LeftLineYs = null;
                RightLineXs = null;
                RightLineYs = null;

                Intersection = Point.Empty;
                MarkedLeft = null;
                MarkedRight = null;
            }

            public FitGraphics(
                double[] leftLineXs, double[] leftLineYs,
                double[] rightLineXs, double[] rightLineYs,
                PointF intersection,
                Tuple<int, int> markedLeft,
                Tuple<int, int> markedRight)
            {
                IsEmpty = false;

                LeftLineXs = leftLineXs;
                LeftLineYs = leftLineYs;
                RightLineXs = rightLineXs;
                RightLineYs = rightLineYs;
                
                Intersection = intersection;
                MarkedLeft = markedLeft;
                MarkedRight = markedRight;
            }

            /// <summary>
            /// Indicates whether the specified fitGraphics object is
            /// <see langword="null"/> or a <see cref="Empty"/>.
            /// </summary>
            /// <param name="fitGraphics"></param>
            /// <returns></returns>
            public static bool IsNullOrEmpty(FitGraphics fitGraphics)
            {
                return fitGraphics == null || fitGraphics.IsEmpty;
            }
                
            /// <summary>
            /// Represents a <see cref="FitGraphics"/> not meant to be plotted.
            /// </summary>
            public FitGraphics Empty => new FitGraphics();

            /// <summary>
            /// Empty <see cref="FitGraphics"/> should not be plotted.
            /// </summary>
            public bool IsEmpty { get; }

            public double[] LeftLineXs { get; }
            public double[] LeftLineYs { get; }
            public double[] RightLineXs { get; }
            public double[] RightLineYs { get; }

            /// <summary>
            /// Intersection point of the left and the right line.
            /// </summary>
            public PointF Intersection { get; }
            /// <summary>
            /// Left line indexes used for fitting.
            /// Start index and length.
            /// </summary>
            public Tuple<int, int> MarkedLeft { get; }
            /// <summary>
            /// Right line indexes used for fitting
            /// Start index and length.
            /// </summary>
            public Tuple<int, int> MarkedRight { get; }


            /// <summary>
            /// Copy subarrays based on marked parameter.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public Tuple<double[], double[]> MarkedPlotLeft(
                double[] x, double[] y)
            {
                return MarkedPlot(x, y, MarkedLeft);
            }
            /// <summary>
            /// Copy subarrays based on marked parameter.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public Tuple<double[], double[]> MarkedPlotRight(
                double[] x, double[] y)
            {
                return MarkedPlot(x, y, MarkedRight);
            }

            /// <summary>
            /// Copy subarrays based on marked parameter.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <param name="marked"></param>
            /// <returns></returns>
            private Tuple<double[], double[]> MarkedPlot(
                double[] x, double[] y, Tuple<int, int> marked)
            {
                double[] xMarked = new double[marked.Item2];
                double[] yMarked = new double[marked.Item2];

                Array.Copy(x, marked.Item1, xMarked, 0, marked.Item2);
                Array.Copy(y, marked.Item1, yMarked, 0, marked.Item2);

                return Tuple.Create(xMarked, yMarked);
            }
        }
    }
}
