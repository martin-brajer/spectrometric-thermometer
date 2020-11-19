using System.Drawing;

namespace spectrometric_thermometer
{
    public partial class Form_main
    {
        /// <summary>
        /// GUI constants - like labels or default size.
        /// </summary>
        public class Constants
        {
            public readonly string version;
            /// <summary>
            /// First line printed in <see cref="tBoxLog"/> at program start.
            /// </summary>
            public readonly string initialMessage;
            public readonly string helpFileName;
            // Buttons text.
            public readonly string[] btnInitializeText;
            public readonly string[] btnMeasureText;
            public readonly string[] btnSwitchText;
            /// <summary>
            /// Default size of the <see cref="Form_main"/> window.
            /// </summary>
            public readonly Size defaultSize;
            /// <summary>
            /// Default size of <see cref="plotLeft"/> etc figures.
            /// </summary>
            public readonly int formsPlotSize;
            // Plotting - Figure 1.
            public readonly string fig1LabelX;
            public readonly string fig1LabelY;
            public readonly string fig1Title;
            // Plotting - Figure 2.
            public readonly string fig2LabelX;
            public readonly string fig2LabelY;
            public readonly string fig2Title;

            public Constants()
            {
                version = "3.5";
                defaultSize = new Size(width: 929, height: 738);
                formsPlotSize = 446;
            }

            public Constants(string initialMessage, string helpFileName,
                string[] btnInitializeText, string[] btnMeasureText, string[] btnSwitchText,
                string fig1Title, string fig1LabelX, string fig1LabelY, string fig2Title,
                string fig2LabelX, string fig2LabelY) : this()
            {
                this.initialMessage = string.Format(initialMessage, version);
                this.helpFileName = helpFileName;
                this.btnInitializeText = btnInitializeText;
                this.btnMeasureText = btnMeasureText;
                this.btnSwitchText = btnSwitchText;
                this.fig1Title = fig1Title;
                this.fig1LabelX = fig1LabelX;
                this.fig1LabelY = fig1LabelY;
                this.fig2Title = fig2Title;
                this.fig2LabelX = fig2LabelX;
                this.fig2LabelY = fig2LabelY;
            }

            public static Constants Constants_EN => new Constants(
                initialMessage: "Spectrometric Thermometer (version {0})",
                helpFileName: "Help.pdf",
                btnInitializeText: new string[] { "&Initialize", "Choose dev&ice", "Disc&onnect" },
                btnMeasureText: new string[] { "&Measure", "S&top" },
                btnSwitchText: new string[] { "Pr&epare to switch", "Ab&ort" },
                fig1Title: "Spectrum",
                fig1LabelX: "Wavelength (nm)",
                fig1LabelY: "Intensity(a.u.)",
                fig2Title: "T: ?? °C",
                fig2LabelX: "Time (sec)",
                fig2LabelY: "Temperature (°C)");
        }
    }
}