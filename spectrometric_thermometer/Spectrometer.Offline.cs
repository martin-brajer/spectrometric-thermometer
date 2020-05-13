using System;
using System.Windows.Forms;

// using no dll.

namespace spectrometric_thermometer
{
    public abstract partial class Spectrometer
    {
        /// <summary>
        /// Class simulating spectrometer
        /// for offline testing.
        /// Inherits from base class <see cref="Spectrometer"/>.
        /// </summary>
        private class Offline : Spectrometer
        {
            private bool selected = false;
            private bool openned = false;
            private bool exposure = false;

            // Here, initializer, start measurement, cancel measurement
            private Timer timer = new Timer();
            Random rnd = new Random();

            public Offline()
            {
                timer.Tick += new EventHandler(Timer_Tick);
            }

            ~Offline()
            {
                Dispose(false);
            }

            public override string ModelName => "Test spectrometer";

            public override string SerialNo => "007";

            public override float ExposureTime { get; protected set; }

            public override float MaxExposureTime => 1e3f;

            public override float MinExposureTime => 1e-6f;


            public override void CancelExposure()
            {
                exposure = false;
                timer.Stop();
            }

            public override bool CheckDeviceRemoved()
            {
                return !selected;
            }

            public override void Close()
            {
                openned = false;
            }

            public override void SelectDevice()
            {
                selected = false;
            }

            public override void EraceDeviceList()
            { }

            public override bool IsOpen()
            {
                return openned;
            }

            public override bool IsSelected()
            {
                return selected;
            }

            public override void Open()
            {
                openned = true;
                //Wavelengths = new double[] { 300, 350, 400, 450, 500, 550, 600, 650, 700 };
                Wavelengths = new double[] {
                340.76, 348.23, 355.69, 363.14, 370.58, 378.01, 385.42, 392.82, 400.21, 407.58,
                414.94, 422.29, 429.63, 436.95, 444.26, 451.56, 458.84, 466.11, 473.37, 480.61,
                487.84, 495.06, 502.26, 509.45, 516.62, 523.78, 530.92, 538.05, 545.17, 552.27,
                559.35, 566.43, 573.48, 580.52, 587.55, 594.56, 601.55, 608.53, 615.49, 622.44,
            };
                ExposureTime = 1.5f;  // Loading the default one.

                Period = ExposureTime + timeReserve;
                saturationLevel = 0;  // Reset to zero.
            }

            public override void SaveSpectrum()
            {
                //Intensities = new float[] { 1.0f, 1.1f, 1.2f, 1.8f, 4.3f, 4.0f, 3.0f, 2.5f, 2.0f };
                Intensities = new float[] {
                472.73f, 763.36f, 957.06f, 937.53f, 948.37f, 945.34f, 945.97f, 966.77f, 968.28f, 995.38f,
                1051.58f, 1074.4f, 1130.6f, 1190.09f, 1267.47f, 1337.04f, 1478.57f, 1622.12f, 1919.29f, 2336.95f,
                2802.5f, 3144.17f, 3502.72f, 3757.55f, 4106.9f, 4538.17f, 4785.19f, 5327.49f, 8148.4f, 12310.5f,
                17653.87f, 19535.23f, 20436.84f, 21045.81f, 21719.43f, 22521.1f, 23532.74f, 24594.15f, 25292.6f, 25587.89f,
            };
                float noiseAmplitude = 1000f;
                for (int i = 0; i < Intensities.Length; i++)
                {
                    Intensities[i] = Intensities[i] + noiseAmplitude * (float)rnd.NextDouble();
                }
            }

            public override void SearchDevices()
            {
                NumberOfDevicesFound = 2;
            }

            public override void SelectDevice(int index)
            {
                if (index < NumberOfDevicesFound && index >= 0)
                {
                    selected = true;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public override void StartExposure()
            {
                exposure = true;
                timer.Interval = (int)(ExposureTime * 1000);
                timer.Start();
            }

            public override string Status()
            {
                return "Test " + selected.ToString();
            }

            public override bool StatusIsTakingSpectrum()
            {
                return exposure;
            }

            /// <summary>
            /// Timer tick = spectrometer.OnExposureFinished.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void Timer_Tick(object sender, EventArgs e)
            {
                timer.Stop();
                exposure = false;
                OnExposureFinished(this, EventArgs.Empty);
            }

            protected sealed override void Dispose(bool disposing)
            {
                timer.Dispose();
            }
        }
    }
}