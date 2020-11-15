namespace spectrometric_thermometer
{
    partial class Form_main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.chBoxAutoExposureTime = new System.Windows.Forms.CheckBox();
            this.lblAutoExposureTime = new System.Windows.Forms.Label();
            this.tBoxAverage = new System.Windows.Forms.TextBox();
            this.tBoxExposureTime = new System.Windows.Forms.TextBox();
            this.lblExposureTime = new System.Windows.Forms.Label();
            this.tBoxPeriod = new System.Windows.Forms.TextBox();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.lblAverage = new System.Windows.Forms.Label();
            this.tBoxFilename = new System.Windows.Forms.TextBox();
            this.tBoxFilenameIndex = new System.Windows.Forms.TextBox();
            this.chBoxSave = new System.Windows.Forms.CheckBox();
            this.chBoxRewrite = new System.Windows.Forms.CheckBox();
            this.lblFilename = new System.Windows.Forms.Label();
            this.lblFilenameIndex = new System.Windows.Forms.Label();
            this.lblRewrite = new System.Windows.Forms.Label();
            this.chBoxPlot = new System.Windows.Forms.CheckBox();
            this.lblPlot = new System.Windows.Forms.Label();
            this.cBoxDeviceType = new System.Windows.Forms.ComboBox();
            this.lblDeviceType = new System.Windows.Forms.Label();
            this.tboxDeviceID = new System.Windows.Forms.TextBox();
            this.lblDeviceID = new System.Windows.Forms.Label();
            this.shapeContainer2 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape4 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape3 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lineShape2 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.lblSave = new System.Windows.Forms.Label();
            this.btnReloadConfig = new System.Windows.Forms.Button();
            this.tBoxLog = new System.Windows.Forms.TextBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnMeasure = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tBoxPID_D = new System.Windows.Forms.TextBox();
            this.tBoxPID_P = new System.Windows.Forms.TextBox();
            this.tBoxPID_I = new System.Windows.Forms.TextBox();
            this.lblLED = new System.Windows.Forms.Label();
            this.lblPIDAverage = new System.Windows.Forms.Label();
            this.lblVoltageStep = new System.Windows.Forms.Label();
            this.pnlTemp = new System.Windows.Forms.Panel();
            this.btnSaveTemperatures = new System.Windows.Forms.Button();
            this.btnLoadSpectra = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cBoxCalibration = new System.Windows.Forms.ComboBox();
            this.btnPlotCalibration = new System.Windows.Forms.Button();
            this.tBoxVoltageStep = new System.Windows.Forms.TextBox();
            this.tBoxSetpoint = new System.Windows.Forms.TextBox();
            this.tBoxPIDAverage = new System.Windows.Forms.TextBox();
            this.tBoxOutputVoltage = new System.Windows.Forms.TextBox();
            this.tBoxRamp = new System.Windows.Forms.TextBox();
            this.chBoxPID = new System.Windows.Forms.CheckBox();
            this.lblSetpoint = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblOutputVoltage = new System.Windows.Forms.Label();
            this.lblRamp = new System.Windows.Forms.Label();
            this.btnSwitch = new System.Windows.Forms.Button();
            this.btnDefaultSize = new System.Windows.Forms.Button();
            this.formsPlotLeft = new ScottPlot.FormsPlot();
            this.formsPlotRight = new ScottPlot.FormsPlot();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShapePlot = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.pnlPID = new System.Windows.Forms.Panel();
            this.pnlMain.SuspendLayout();
            this.pnlTemp.SuspendLayout();
            this.pnlPID.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.chBoxAutoExposureTime);
            this.pnlMain.Controls.Add(this.lblAutoExposureTime);
            this.pnlMain.Controls.Add(this.tBoxAverage);
            this.pnlMain.Controls.Add(this.tBoxExposureTime);
            this.pnlMain.Controls.Add(this.lblExposureTime);
            this.pnlMain.Controls.Add(this.tBoxPeriod);
            this.pnlMain.Controls.Add(this.lblPeriod);
            this.pnlMain.Controls.Add(this.lblAverage);
            this.pnlMain.Controls.Add(this.tBoxFilename);
            this.pnlMain.Controls.Add(this.tBoxFilenameIndex);
            this.pnlMain.Controls.Add(this.chBoxSave);
            this.pnlMain.Controls.Add(this.chBoxRewrite);
            this.pnlMain.Controls.Add(this.lblFilename);
            this.pnlMain.Controls.Add(this.lblFilenameIndex);
            this.pnlMain.Controls.Add(this.lblRewrite);
            this.pnlMain.Controls.Add(this.cBoxDeviceType);
            this.pnlMain.Controls.Add(this.lblDeviceType);
            this.pnlMain.Controls.Add(this.tboxDeviceID);
            this.pnlMain.Controls.Add(this.lblDeviceID);
            this.pnlMain.Controls.Add(this.shapeContainer2);
            this.pnlMain.Controls.Add(this.lblSave);
            this.pnlMain.Location = new System.Drawing.Point(323, 12);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(308, 156);
            this.pnlMain.TabIndex = 6;
            // 
            // chBoxAutoExposureTime
            // 
            this.chBoxAutoExposureTime.AutoSize = true;
            this.chBoxAutoExposureTime.Location = new System.Drawing.Point(253, 56);
            this.chBoxAutoExposureTime.Name = "chBoxAutoExposureTime";
            this.chBoxAutoExposureTime.Size = new System.Drawing.Size(15, 14);
            this.chBoxAutoExposureTime.TabIndex = 8;
            this.chBoxAutoExposureTime.UseVisualStyleBackColor = true;
            // 
            // lblAutoExposureTime
            // 
            this.lblAutoExposureTime.Location = new System.Drawing.Point(156, 52);
            this.lblAutoExposureTime.Name = "lblAutoExposureTime";
            this.lblAutoExposureTime.Size = new System.Drawing.Size(91, 20);
            this.lblAutoExposureTime.TabIndex = 12;
            this.lblAutoExposureTime.Text = "Autoexposure";
            this.lblAutoExposureTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAutoExposureTime, "Automatic exposure adaptation");
            // 
            // tBoxAverage
            // 
            this.tBoxAverage.Location = new System.Drawing.Point(253, 27);
            this.tBoxAverage.Name = "tBoxAverage";
            this.tBoxAverage.Size = new System.Drawing.Size(50, 20);
            this.tBoxAverage.TabIndex = 7;
            this.tBoxAverage.Text = "1";
            // 
            // tBoxExposureTime
            // 
            this.tBoxExposureTime.Location = new System.Drawing.Point(99, 53);
            this.tBoxExposureTime.Name = "tBoxExposureTime";
            this.tBoxExposureTime.Size = new System.Drawing.Size(50, 20);
            this.tBoxExposureTime.TabIndex = 3;
            // 
            // lblExposureTime
            // 
            this.lblExposureTime.Location = new System.Drawing.Point(3, 52);
            this.lblExposureTime.Name = "lblExposureTime";
            this.lblExposureTime.Size = new System.Drawing.Size(91, 20);
            this.lblExposureTime.TabIndex = 4;
            this.lblExposureTime.Text = "Exposure time";
            this.lblExposureTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tBoxPeriod
            // 
            this.tBoxPeriod.Location = new System.Drawing.Point(99, 27);
            this.tBoxPeriod.Name = "tBoxPeriod";
            this.tBoxPeriod.Size = new System.Drawing.Size(50, 20);
            this.tBoxPeriod.TabIndex = 2;
            // 
            // lblPeriod
            // 
            this.lblPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPeriod.Location = new System.Drawing.Point(3, 26);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(91, 20);
            this.lblPeriod.TabIndex = 2;
            this.lblPeriod.Text = "Period";
            this.lblPeriod.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblPeriod, "One measurement per period");
            // 
            // lblAverage
            // 
            this.lblAverage.Location = new System.Drawing.Point(156, 26);
            this.lblAverage.Name = "lblAverage";
            this.lblAverage.Size = new System.Drawing.Size(91, 20);
            this.lblAverage.TabIndex = 9;
            this.lblAverage.Text = "Average";
            this.lblAverage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAverage, "Number of periods to average");
            // 
            // tBoxFilename
            // 
            this.tBoxFilename.Location = new System.Drawing.Point(100, 131);
            this.tBoxFilename.Name = "tBoxFilename";
            this.tBoxFilename.Size = new System.Drawing.Size(203, 20);
            this.tBoxFilename.TabIndex = 10;
            this.tBoxFilename.Text = "Data\\Spectrum";
            this.toolTip1.SetToolTip(this.tBoxFilename, "Double-click to open a folder browser");
            this.tBoxFilename.DoubleClick += new System.EventHandler(this.TBoxFilename_DoubleClick);
            // 
            // tBoxFilenameIndex
            // 
            this.tBoxFilenameIndex.Location = new System.Drawing.Point(253, 105);
            this.tBoxFilenameIndex.Name = "tBoxFilenameIndex";
            this.tBoxFilenameIndex.Size = new System.Drawing.Size(50, 20);
            this.tBoxFilenameIndex.TabIndex = 6;
            this.tBoxFilenameIndex.Text = "00000";
            // 
            // chBoxSave
            // 
            this.chBoxSave.AutoSize = true;
            this.chBoxSave.Checked = true;
            this.chBoxSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxSave.Location = new System.Drawing.Point(99, 82);
            this.chBoxSave.Name = "chBoxSave";
            this.chBoxSave.Size = new System.Drawing.Size(15, 14);
            this.chBoxSave.TabIndex = 4;
            this.chBoxSave.UseVisualStyleBackColor = true;
            this.chBoxSave.CheckedChanged += new System.EventHandler(this.ChBoxSave_CheckedChanged);
            // 
            // chBoxRewrite
            // 
            this.chBoxRewrite.AutoSize = true;
            this.chBoxRewrite.Location = new System.Drawing.Point(100, 108);
            this.chBoxRewrite.Name = "chBoxRewrite";
            this.chBoxRewrite.Size = new System.Drawing.Size(15, 14);
            this.chBoxRewrite.TabIndex = 5;
            this.chBoxRewrite.UseVisualStyleBackColor = true;
            this.chBoxRewrite.CheckedChanged += new System.EventHandler(this.ChBoxRwt_CheckedChanged);
            // 
            // lblFilename
            // 
            this.lblFilename.Location = new System.Drawing.Point(3, 130);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(91, 20);
            this.lblFilename.TabIndex = 15;
            this.lblFilename.Text = "Filename";
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFilenameIndex
            // 
            this.lblFilenameIndex.Location = new System.Drawing.Point(156, 104);
            this.lblFilenameIndex.Name = "lblFilenameIndex";
            this.lblFilenameIndex.Size = new System.Drawing.Size(91, 20);
            this.lblFilenameIndex.TabIndex = 4;
            this.lblFilenameIndex.Text = "Filename index";
            this.lblFilenameIndex.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRewrite
            // 
            this.lblRewrite.Location = new System.Drawing.Point(3, 104);
            this.lblRewrite.Name = "lblRewrite";
            this.lblRewrite.Size = new System.Drawing.Size(91, 20);
            this.lblRewrite.TabIndex = 6;
            this.lblRewrite.Text = "Rewrite";
            this.lblRewrite.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chBoxPlot
            // 
            this.chBoxPlot.AutoSize = true;
            this.chBoxPlot.Checked = true;
            this.chBoxPlot.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxPlot.Location = new System.Drawing.Point(423, 172);
            this.chBoxPlot.Name = "chBoxPlot";
            this.chBoxPlot.Size = new System.Drawing.Size(15, 14);
            this.chBoxPlot.TabIndex = 18;
            this.chBoxPlot.UseVisualStyleBackColor = true;
            // 
            // lblPlot
            // 
            this.lblPlot.Location = new System.Drawing.Point(326, 168);
            this.lblPlot.Name = "lblPlot";
            this.lblPlot.Size = new System.Drawing.Size(91, 20);
            this.lblPlot.TabIndex = 19;
            this.lblPlot.Text = "Plot";
            this.lblPlot.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cBoxDeviceType
            // 
            this.cBoxDeviceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxDeviceType.FormattingEnabled = true;
            this.cBoxDeviceType.Location = new System.Drawing.Point(99, 1);
            this.cBoxDeviceType.Margin = new System.Windows.Forms.Padding(2);
            this.cBoxDeviceType.Name = "cBoxDeviceType";
            this.cBoxDeviceType.Size = new System.Drawing.Size(108, 21);
            this.cBoxDeviceType.TabIndex = 0;
            // 
            // lblDeviceType
            // 
            this.lblDeviceType.Location = new System.Drawing.Point(3, 1);
            this.lblDeviceType.Name = "lblDeviceType";
            this.lblDeviceType.Size = new System.Drawing.Size(91, 20);
            this.lblDeviceType.TabIndex = 17;
            this.lblDeviceType.Text = "Device type";
            this.lblDeviceType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tboxDeviceID
            // 
            this.tboxDeviceID.Location = new System.Drawing.Point(253, 1);
            this.tboxDeviceID.Name = "tboxDeviceID";
            this.tboxDeviceID.Size = new System.Drawing.Size(50, 20);
            this.tboxDeviceID.TabIndex = 1;
            this.tboxDeviceID.Text = "0";
            // 
            // lblDeviceID
            // 
            this.lblDeviceID.Location = new System.Drawing.Point(212, 2);
            this.lblDeviceID.Name = "lblDeviceID";
            this.lblDeviceID.Size = new System.Drawing.Size(35, 20);
            this.lblDeviceID.TabIndex = 0;
            this.lblDeviceID.Text = "ID";
            this.lblDeviceID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // shapeContainer2
            // 
            this.shapeContainer2.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer2.Name = "shapeContainer2";
            this.shapeContainer2.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape4,
            this.lineShape3,
            this.lineShape2});
            this.shapeContainer2.Size = new System.Drawing.Size(308, 156);
            this.shapeContainer2.TabIndex = 20;
            this.shapeContainer2.TabStop = false;
            // 
            // lineShape4
            // 
            this.lineShape4.Enabled = false;
            this.lineShape4.Name = "lineShape4";
            this.lineShape4.X1 = 0;
            this.lineShape4.X2 = 308;
            this.lineShape4.Y1 = 154;
            this.lineShape4.Y2 = 154;
            // 
            // lineShape3
            // 
            this.lineShape3.Enabled = false;
            this.lineShape3.Name = "lineShape3";
            this.lineShape3.X1 = 0;
            this.lineShape3.X2 = 308;
            this.lineShape3.Y1 = 24;
            this.lineShape3.Y2 = 24;
            // 
            // lineShape2
            // 
            this.lineShape2.Enabled = false;
            this.lineShape2.Name = "lineShape2";
            this.lineShape2.X1 = 0;
            this.lineShape2.X2 = 308;
            this.lineShape2.Y1 = 76;
            this.lineShape2.Y2 = 76;
            // 
            // lblSave
            // 
            this.lblSave.Location = new System.Drawing.Point(3, 77);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(91, 20);
            this.lblSave.TabIndex = 5;
            this.lblSave.Text = "Save";
            this.lblSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnReloadConfig
            // 
            this.btnReloadConfig.Location = new System.Drawing.Point(12, 156);
            this.btnReloadConfig.Name = "btnReloadConfig";
            this.btnReloadConfig.Size = new System.Drawing.Size(100, 30);
            this.btnReloadConfig.TabIndex = 11;
            this.btnReloadConfig.Text = "&Reload config";
            this.btnReloadConfig.UseVisualStyleBackColor = true;
            this.btnReloadConfig.Click += new System.EventHandler(this.BtnReloadConfig_Click);
            // 
            // tBoxLog
            // 
            this.tBoxLog.Location = new System.Drawing.Point(118, 12);
            this.tBoxLog.Multiline = true;
            this.tBoxLog.Name = "tBoxLog";
            this.tBoxLog.ReadOnly = true;
            this.tBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tBoxLog.Size = new System.Drawing.Size(199, 210);
            this.tBoxLog.TabIndex = 5;
            this.tBoxLog.TabStop = false;
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(12, 84);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(100, 30);
            this.btnHelp.TabIndex = 2;
            this.btnHelp.Text = "&Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.BtnHelp_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(12, 192);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(100, 30);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(12, 12);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(100, 30);
            this.btnInitialize.TabIndex = 0;
            this.btnInitialize.Text = "Initialize_default";
            this.btnInitialize.UseVisualStyleBackColor = true;
            this.btnInitialize.Click += new System.EventHandler(this.BtnInitialize_Click);
            // 
            // btnMeasure
            // 
            this.btnMeasure.Location = new System.Drawing.Point(12, 48);
            this.btnMeasure.Name = "btnMeasure";
            this.btnMeasure.Size = new System.Drawing.Size(100, 30);
            this.btnMeasure.TabIndex = 1;
            this.btnMeasure.Text = "Measure_default";
            this.btnMeasure.UseVisualStyleBackColor = true;
            this.btnMeasure.Click += new System.EventHandler(this.BtnMeasure_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 200;
            this.toolTip1.ReshowDelay = 100;
            // 
            // tBoxPID_D
            // 
            this.tBoxPID_D.Location = new System.Drawing.Point(105, 27);
            this.tBoxPID_D.Name = "tBoxPID_D";
            this.tBoxPID_D.Size = new System.Drawing.Size(45, 20);
            this.tBoxPID_D.TabIndex = 8;
            this.tBoxPID_D.Text = "10";
            this.toolTip1.SetToolTip(this.tBoxPID_D, "D");
            // 
            // tBoxPID_P
            // 
            this.tBoxPID_P.Location = new System.Drawing.Point(3, 27);
            this.tBoxPID_P.Name = "tBoxPID_P";
            this.tBoxPID_P.Size = new System.Drawing.Size(45, 20);
            this.tBoxPID_P.TabIndex = 6;
            this.tBoxPID_P.Text = "250";
            this.toolTip1.SetToolTip(this.tBoxPID_P, "P");
            // 
            // tBoxPID_I
            // 
            this.tBoxPID_I.Location = new System.Drawing.Point(54, 27);
            this.tBoxPID_I.Name = "tBoxPID_I";
            this.tBoxPID_I.Size = new System.Drawing.Size(45, 20);
            this.tBoxPID_I.TabIndex = 7;
            this.tBoxPID_I.Text = "300";
            this.toolTip1.SetToolTip(this.tBoxPID_I, "I");
            // 
            // lblLED
            // 
            this.lblLED.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblLED.Location = new System.Drawing.Point(109, 180);
            this.lblLED.Name = "lblLED";
            this.lblLED.Size = new System.Drawing.Size(41, 29);
            this.lblLED.TabIndex = 12;
            this.toolTip1.SetToolTip(this.lblLED, "LED");
            // 
            // lblPIDAverage
            // 
            this.lblPIDAverage.Location = new System.Drawing.Point(79, 130);
            this.lblPIDAverage.Name = "lblPIDAverage";
            this.lblPIDAverage.Size = new System.Drawing.Size(34, 20);
            this.lblPIDAverage.TabIndex = 0;
            this.lblPIDAverage.Text = "#Avg";
            this.lblPIDAverage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblPIDAverage, "Averaging");
            // 
            // lblVoltageStep
            // 
            this.lblVoltageStep.Location = new System.Drawing.Point(3, 130);
            this.lblVoltageStep.Name = "lblVoltageStep";
            this.lblVoltageStep.Size = new System.Drawing.Size(34, 20);
            this.lblVoltageStep.TabIndex = 0;
            this.lblVoltageStep.Text = "MΔV";
            this.lblVoltageStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblVoltageStep, "Maximal voltage step");
            // 
            // pnlTemp
            // 
            this.pnlTemp.Controls.Add(this.btnSaveTemperatures);
            this.pnlTemp.Controls.Add(this.btnLoadSpectra);
            this.pnlTemp.Controls.Add(this.btnClear);
            this.pnlTemp.Controls.Add(this.cBoxCalibration);
            this.pnlTemp.Controls.Add(this.btnPlotCalibration);
            this.pnlTemp.Location = new System.Drawing.Point(637, 12);
            this.pnlTemp.Name = "pnlTemp";
            this.pnlTemp.Size = new System.Drawing.Size(106, 210);
            this.pnlTemp.TabIndex = 7;
            // 
            // btnSaveTemperatures
            // 
            this.btnSaveTemperatures.Location = new System.Drawing.Point(3, 0);
            this.btnSaveTemperatures.Name = "btnSaveTemperatures";
            this.btnSaveTemperatures.Size = new System.Drawing.Size(100, 30);
            this.btnSaveTemperatures.TabIndex = 0;
            this.btnSaveTemperatures.Text = "&Save temps";
            this.btnSaveTemperatures.UseVisualStyleBackColor = true;
            this.btnSaveTemperatures.Click += new System.EventHandler(this.BtnSaveTemperatures_Click);
            // 
            // btnLoadSpectra
            // 
            this.btnLoadSpectra.Location = new System.Drawing.Point(3, 36);
            this.btnLoadSpectra.Name = "btnLoadSpectra";
            this.btnLoadSpectra.Size = new System.Drawing.Size(100, 30);
            this.btnLoadSpectra.TabIndex = 1;
            this.btnLoadSpectra.Text = "&Load spectra";
            this.btnLoadSpectra.UseVisualStyleBackColor = true;
            this.btnLoadSpectra.Click += new System.EventHandler(this.BtnLoadSpectra_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(3, 72);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 30);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "&Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // cBoxCalibration
            // 
            this.cBoxCalibration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxCalibration.FormattingEnabled = true;
            this.cBoxCalibration.Location = new System.Drawing.Point(3, 112);
            this.cBoxCalibration.Margin = new System.Windows.Forms.Padding(2);
            this.cBoxCalibration.Name = "cBoxCalibration";
            this.cBoxCalibration.Size = new System.Drawing.Size(100, 21);
            this.cBoxCalibration.TabIndex = 3;
            this.cBoxCalibration.SelectedIndexChanged += new System.EventHandler(this.CBoxCalibration_SelectedIndexChanged);
            // 
            // btnPlotCalibration
            // 
            this.btnPlotCalibration.Location = new System.Drawing.Point(3, 144);
            this.btnPlotCalibration.Name = "btnPlotCalibration";
            this.btnPlotCalibration.Size = new System.Drawing.Size(100, 30);
            this.btnPlotCalibration.TabIndex = 4;
            this.btnPlotCalibration.Text = "Plot c&alibration";
            this.btnPlotCalibration.UseVisualStyleBackColor = true;
            this.btnPlotCalibration.Click += new System.EventHandler(this.BtnPlotCalibration_Click);
            // 
            // tBoxVoltageStep
            // 
            this.tBoxVoltageStep.Location = new System.Drawing.Point(42, 131);
            this.tBoxVoltageStep.Name = "tBoxVoltageStep";
            this.tBoxVoltageStep.Size = new System.Drawing.Size(31, 20);
            this.tBoxVoltageStep.TabIndex = 11;
            this.tBoxVoltageStep.Text = "0.1";
            // 
            // tBoxSetpoint
            // 
            this.tBoxSetpoint.Location = new System.Drawing.Point(100, 53);
            this.tBoxSetpoint.Name = "tBoxSetpoint";
            this.tBoxSetpoint.Size = new System.Drawing.Size(50, 20);
            this.tBoxSetpoint.TabIndex = 9;
            this.tBoxSetpoint.Text = "20";
            // 
            // tBoxPIDAverage
            // 
            this.tBoxPIDAverage.Location = new System.Drawing.Point(119, 131);
            this.tBoxPIDAverage.Name = "tBoxPIDAverage";
            this.tBoxPIDAverage.Size = new System.Drawing.Size(31, 20);
            this.tBoxPIDAverage.TabIndex = 11;
            this.tBoxPIDAverage.Text = "3";
            // 
            // tBoxOutputVoltage
            // 
            this.tBoxOutputVoltage.Location = new System.Drawing.Point(100, 105);
            this.tBoxOutputVoltage.Name = "tBoxOutputVoltage";
            this.tBoxOutputVoltage.Size = new System.Drawing.Size(50, 20);
            this.tBoxOutputVoltage.TabIndex = 11;
            this.tBoxOutputVoltage.Text = "3";
            // 
            // tBoxRamp
            // 
            this.tBoxRamp.Location = new System.Drawing.Point(100, 79);
            this.tBoxRamp.Name = "tBoxRamp";
            this.tBoxRamp.Size = new System.Drawing.Size(50, 20);
            this.tBoxRamp.TabIndex = 10;
            this.tBoxRamp.Text = "10";
            // 
            // chBoxPID
            // 
            this.chBoxPID.Appearance = System.Windows.Forms.Appearance.Button;
            this.chBoxPID.Location = new System.Drawing.Point(3, 0);
            this.chBoxPID.Name = "chBoxPID";
            this.chBoxPID.Size = new System.Drawing.Size(147, 22);
            this.chBoxPID.TabIndex = 5;
            this.chBoxPID.Text = "&PID control";
            this.chBoxPID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chBoxPID.UseVisualStyleBackColor = true;
            this.chBoxPID.CheckedChanged += new System.EventHandler(this.ChBoxPID_CheckedChanged);
            // 
            // lblSetpoint
            // 
            this.lblSetpoint.Location = new System.Drawing.Point(3, 52);
            this.lblSetpoint.Name = "lblSetpoint";
            this.lblSetpoint.Size = new System.Drawing.Size(91, 20);
            this.lblSetpoint.TabIndex = 0;
            this.lblSetpoint.Text = "Temp setpoint";
            this.lblSetpoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(3, 156);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(147, 20);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "<info>";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOutputVoltage
            // 
            this.lblOutputVoltage.Location = new System.Drawing.Point(3, 104);
            this.lblOutputVoltage.Name = "lblOutputVoltage";
            this.lblOutputVoltage.Size = new System.Drawing.Size(91, 20);
            this.lblOutputVoltage.TabIndex = 0;
            this.lblOutputVoltage.Text = "Output voltage";
            this.lblOutputVoltage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRamp
            // 
            this.lblRamp.Location = new System.Drawing.Point(3, 78);
            this.lblRamp.Name = "lblRamp";
            this.lblRamp.Size = new System.Drawing.Size(91, 20);
            this.lblRamp.TabIndex = 0;
            this.lblRamp.Text = "Ramp (°C / min)";
            this.lblRamp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnSwitch
            // 
            this.btnSwitch.Location = new System.Drawing.Point(3, 180);
            this.btnSwitch.Name = "btnSwitch";
            this.btnSwitch.Size = new System.Drawing.Size(100, 30);
            this.btnSwitch.TabIndex = 4;
            this.btnSwitch.Text = "Switch_default";
            this.btnSwitch.UseVisualStyleBackColor = true;
            this.btnSwitch.Click += new System.EventHandler(this.BtnSwitch_Click);
            // 
            // btnDefaultSize
            // 
            this.btnDefaultSize.Location = new System.Drawing.Point(12, 120);
            this.btnDefaultSize.Name = "btnDefaultSize";
            this.btnDefaultSize.Size = new System.Drawing.Size(100, 30);
            this.btnDefaultSize.TabIndex = 3;
            this.btnDefaultSize.Text = "&Default size";
            this.toolTip1.SetToolTip(this.btnDefaultSize, "Ctrl to shrink");
            this.btnDefaultSize.UseVisualStyleBackColor = true;
            this.btnDefaultSize.Click += new System.EventHandler(this.BtnDefaultSize_Click);
            // 
            // formsPlotLeft
            // 
            this.formsPlotLeft.BackColor = System.Drawing.Color.Transparent;
            this.formsPlotLeft.Location = new System.Drawing.Point(12, 246);
            this.formsPlotLeft.Name = "formsPlotLeft";
            this.formsPlotLeft.Size = new System.Drawing.Size(446, 446);
            this.formsPlotLeft.TabIndex = 12;
            // 
            // formsPlotRight
            // 
            this.formsPlotRight.BackColor = System.Drawing.Color.Transparent;
            this.formsPlotRight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.formsPlotRight.Location = new System.Drawing.Point(458, 246);
            this.formsPlotRight.Name = "formsPlotRight";
            this.formsPlotRight.Size = new System.Drawing.Size(446, 446);
            this.formsPlotRight.TabIndex = 13;
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShapePlot});
            this.shapeContainer1.Size = new System.Drawing.Size(913, 699);
            this.shapeContainer1.TabIndex = 14;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShapePlot
            // 
            this.lineShapePlot.Name = "lineShapePlot";
            this.lineShapePlot.X1 = 12;
            this.lineShapePlot.X2 = 904;
            this.lineShapePlot.Y1 = 234;
            this.lineShapePlot.Y2 = 234;
            // 
            // pnlPID
            // 
            this.pnlPID.Controls.Add(this.lblLED);
            this.pnlPID.Controls.Add(this.chBoxPID);
            this.pnlPID.Controls.Add(this.tBoxVoltageStep);
            this.pnlPID.Controls.Add(this.btnSwitch);
            this.pnlPID.Controls.Add(this.tBoxSetpoint);
            this.pnlPID.Controls.Add(this.lblRamp);
            this.pnlPID.Controls.Add(this.tBoxPIDAverage);
            this.pnlPID.Controls.Add(this.lblOutputVoltage);
            this.pnlPID.Controls.Add(this.tBoxOutputVoltage);
            this.pnlPID.Controls.Add(this.lblVoltageStep);
            this.pnlPID.Controls.Add(this.tBoxRamp);
            this.pnlPID.Controls.Add(this.lblPIDAverage);
            this.pnlPID.Controls.Add(this.tBoxPID_D);
            this.pnlPID.Controls.Add(this.lblInfo);
            this.pnlPID.Controls.Add(this.lblSetpoint);
            this.pnlPID.Controls.Add(this.tBoxPID_P);
            this.pnlPID.Controls.Add(this.tBoxPID_I);
            this.pnlPID.Location = new System.Drawing.Point(749, 12);
            this.pnlPID.Name = "pnlPID";
            this.pnlPID.Size = new System.Drawing.Size(155, 209);
            this.pnlPID.TabIndex = 15;
            // 
            // Form_main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 699);
            this.Controls.Add(this.lblPlot);
            this.Controls.Add(this.chBoxPlot);
            this.Controls.Add(this.pnlPID);
            this.Controls.Add(this.formsPlotLeft);
            this.Controls.Add(this.formsPlotRight);
            this.Controls.Add(this.btnReloadConfig);
            this.Controls.Add(this.btnDefaultSize);
            this.Controls.Add(this.pnlTemp);
            this.Controls.Add(this.tBoxLog);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnInitialize);
            this.Controls.Add(this.btnMeasure);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.shapeContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(652, 273);
            this.Name = "Form_main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spectrometric Thermometer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_main_FormClosed);
            this.Load += new System.EventHandler(this.Form_main_Load);
            this.SizeChanged += new System.EventHandler(this.Form_main_SizeChanged);
            this.pnlMain.ResumeLayout(false);
            this.pnlMain.PerformLayout();
            this.pnlTemp.ResumeLayout(false);
            this.pnlPID.ResumeLayout(false);
            this.pnlPID.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Panel pnlMain;
        internal System.Windows.Forms.CheckBox chBoxRewrite;
        internal System.Windows.Forms.TextBox tBoxFilename;
        internal System.Windows.Forms.TextBox tBoxPeriod;
        internal System.Windows.Forms.TextBox tBoxFilenameIndex;
        internal System.Windows.Forms.TextBox tBoxExposureTime;
        internal System.Windows.Forms.TextBox tboxDeviceID;
        internal System.Windows.Forms.Label lblFilename;
        internal System.Windows.Forms.Label lblPeriod;
        internal System.Windows.Forms.Label lblRewrite;
        internal System.Windows.Forms.Label lblExposureTime;
        internal System.Windows.Forms.Label lblDeviceID;
        internal System.Windows.Forms.TextBox tBoxLog;
        internal System.Windows.Forms.Button btnHelp;
        internal System.Windows.Forms.Button btnExit;
        internal System.Windows.Forms.Button btnInitialize;
        internal System.Windows.Forms.Button btnMeasure;
        internal System.Windows.Forms.CheckBox chBoxAutoExposureTime;
        internal System.Windows.Forms.Label lblAutoExposureTime;
        private System.Windows.Forms.ComboBox cBoxDeviceType;
        internal System.Windows.Forms.Label lblDeviceType;
        internal System.Windows.Forms.Label lblAverage;
        internal System.Windows.Forms.TextBox tBoxAverage;
        private System.Windows.Forms.ToolTip toolTip1;
        internal System.Windows.Forms.CheckBox chBoxSave;
        internal System.Windows.Forms.Label lblSave;
        private System.Windows.Forms.Panel pnlTemp;
        internal System.Windows.Forms.Button btnDefaultSize;
        internal System.Windows.Forms.Button btnPlotCalibration;
        internal System.Windows.Forms.ComboBox cBoxCalibration;
        internal System.Windows.Forms.Button btnClear;
        internal System.Windows.Forms.Button btnLoadSpectra;
        internal System.Windows.Forms.Button btnSaveTemperatures;
        internal System.Windows.Forms.TextBox tBoxPID_P;
        internal System.Windows.Forms.TextBox tBoxPID_D;
        internal System.Windows.Forms.TextBox tBoxPID_I;
        internal System.Windows.Forms.CheckBox chBoxPID;
        internal System.Windows.Forms.TextBox tBoxRamp;
        internal System.Windows.Forms.Label lblRamp;
        internal System.Windows.Forms.TextBox tBoxSetpoint;
        internal System.Windows.Forms.Label lblSetpoint;
        internal System.Windows.Forms.Label lblInfo;
        internal System.Windows.Forms.TextBox tBoxOutputVoltage;
        internal System.Windows.Forms.Label lblOutputVoltage;
        internal System.Windows.Forms.Button btnReloadConfig;
        internal System.Windows.Forms.TextBox tBoxVoltageStep;
        internal System.Windows.Forms.Label lblVoltageStep;
        internal System.Windows.Forms.TextBox tBoxPIDAverage;
        internal System.Windows.Forms.Label lblPIDAverage;
        internal System.Windows.Forms.Button btnSwitch;
        private System.Windows.Forms.Label lblLED;
        private ScottPlot.FormsPlot formsPlotLeft;
        private ScottPlot.FormsPlot formsPlotRight;
        internal System.Windows.Forms.CheckBox chBoxPlot;
        internal System.Windows.Forms.Label lblPlot;
        internal System.Windows.Forms.Label lblFilenameIndex;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShapePlot;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape2;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape3;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape4;
        private System.Windows.Forms.Panel pnlPID;
    }
}

