using System.Drawing;

namespace spectrometric_thermometer
{
    public partial class FormMain
    {
        /// <summary>
        /// GUI constants - like labels or default size.
        /// </summary>
        public class Constants
        {
            private readonly string _initialMessage;

            /// <summary>
            /// Initial message including version.
            /// </summary>
            public string Version { get; }
            /// <summary>
            /// First line printed in <see cref="tBoxLog"/> at program start.
            /// </summary>
            public string InitialMessage => string.Format(_initialMessage, Version);
            public string HelpFileName { get; }
            // Button labels.
            public string[] BtnInitializeText { get; }
            public string[] BtnMeasureText { get; }
            public string[] BtnSwitchText { get; }
            /// <summary>
            /// Default size of the <see cref="FormMain"/> window.
            /// </summary>
            public Size DefaultSize { get; }
            /// <summary>
            /// Default size of <see cref="plotLeft"/> etc figures.
            /// </summary>
            public int FormsPlotSize { get; }
            // Plotting - left figure.
            public string PlotLeft_XLabel { get; }
            public string PlotLeft_YLabel { get; }
            public string PlotLeft_Title { get; }
            // Plotting - right figure.
            public string PlotRight_XLabel { get; }
            public string PlotRight_YLabel { get; }
            public string PlotRight_Title { get; }

            /// <summary>
            /// General values used in all languages. See <see cref="Constants_EN"/>.
            /// </summary>
            private Constants()
            {
                Version = "3.5";
                DefaultSize = new Size(width: 929, height: 738);
                FormsPlotSize = 446;
            }

            public Constants(string initialMessage, string helpFileName,
                string[] btnInitializeText, string[] btnMeasureText, string[] btnSwitchText,
                string plotLeft_Title, string plotLeft_XLabel, string plotLeft_YLabel,
                string plotRight_Title, string plotRight_XLabel, string plotRight_YLabel) : this()
            {
                _initialMessage = initialMessage;
                HelpFileName = helpFileName;
                BtnInitializeText = btnInitializeText;
                BtnMeasureText = btnMeasureText;
                BtnSwitchText = btnSwitchText;
                PlotLeft_Title = plotLeft_Title;
                PlotLeft_XLabel = plotLeft_XLabel;
                PlotLeft_YLabel = plotLeft_YLabel;
                PlotRight_Title = plotRight_Title;
                PlotRight_XLabel = plotRight_XLabel;
                PlotRight_YLabel = plotRight_YLabel;
            }

            public static Constants Constants_EN => new Constants(
                initialMessage: "Spectrometric Thermometer (version {0})",
                helpFileName: "Help.pdf",
                btnInitializeText: new string[] { "&Initialize", "Choose dev&ice", "Disc&onnect" },
                btnMeasureText: new string[] { "&Measure", "S&top" },
                btnSwitchText: new string[] { "Pr&epare to switch", "Ab&ort" },
                plotLeft_Title: "Spectrum",
                plotLeft_XLabel: "Wavelength (nm)",
                plotLeft_YLabel: "Intensity(a.u.)",
                plotRight_Title: "T: ? °C",
                plotRight_XLabel: "Time (sec)",
                plotRight_YLabel: "Temperature (°C)");
        }
    }
}