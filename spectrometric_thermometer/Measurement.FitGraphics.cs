﻿using System;
using System.Drawing;

namespace spectrometric_thermometer
{
    public partial class Measurement
    {
        /// <summary>
        /// Data-structure used in Measurements class.
        /// Store information about fitting graphics.
        /// </summary>
        public class FitGraphics
        {
            /// <summary>
            /// Empty.
            /// </summary>
            private FitGraphics()
            {
                IsEmpty = true;
                LL = Point.Empty;
                LR = Point.Empty;
                RL = Point.Empty;
                RR = Point.Empty;
                Crossing = Point.Empty;
                MarkedLeft = null;
                MarkedRight = null;
            }

            public FitGraphics(PointF lL, PointF lR, PointF rL, PointF rR,
                PointF crossing, int[] lIndexes, int[] rIndexes)
            {
                IsEmpty = false;
                LL = lL;
                LR = lR;
                RL = rL;
                RR = rR;
                Crossing = crossing;
                MarkedLeft = lIndexes ?? throw new ArgumentNullException(nameof(lIndexes));
                MarkedRight = rIndexes ?? throw new ArgumentNullException(nameof(rIndexes));
            }

            /// <summary>
            /// Represents a <see cref="FitGraphics"/> not meant to be plotted.
            /// </summary>
            public FitGraphics Empty => new FitGraphics();

            /// <summary>
            /// Empty <see cref="FitGraphics"/> should not be plotted.
            /// </summary>
            public bool IsEmpty { get; }
            /// <summary>
            /// Left line, left end.
            /// </summary>
            public PointF LL { get; }
            /// <summary>
            /// Left line, right end.
            /// </summary>
            public PointF LR { get; }
            /// <summary>
            /// Right line, left end.
            /// </summary>
            public PointF RL { get; }
            /// <summary>
            /// Right line, right end.
            /// </summary>
            public PointF RR { get; }
            /// <summary>
            /// Crossing point of the left and the right line.
            /// </summary>
            public PointF Crossing { get; }

            /// <summary>
            /// Left line indexes, where fitting was done.
            /// Start index and length.
            /// </summary>
            public int[] MarkedLeft { get; }
            /// <summary>
            /// Right line indexes, where fitting was done.
            /// Start index and length.
            /// </summary>
            public int[] MarkedRight { get; }

            public double[] LeftLineXs => new double[2] { LL.X, LR.X };
            public double[] LeftLineYs => new double[2] { LL.Y, LR.Y };
            public double[] RightLineXs => new double[2] { RL.X, RR.X };
            public double[] RightLineYs => new double[2] { RL.Y, RR.Y };

            public Tuple<double[], double[]> MarkedPlotLeft(
                double[] x, double[] y)
            {
                return MarkedPlot(x, y, MarkedLeft);
            }

            public Tuple<double[], double[]> MarkedPlotRight(
                double[] x, double[] y)
            {
                return MarkedPlot(x, y, MarkedRight);
            }

            private Tuple<double[], double[]> MarkedPlot(
                double[] x, double[] y, int[] marked)
            {
                double[] xMarked = new double[marked[1]];
                double[] yMarked = new double[marked[1]];

                Array.Copy(x, marked[0], xMarked, 0, marked[1]);
                Array.Copy(y, marked[0], yMarked, 0, marked[1]);

                return Tuple.Create(xMarked, yMarked);
            }
        }
    }
}
