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
            _container.Dispose();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.cBoxSpect = new System.Windows.Forms.ComboBox();
            this.chBoxAdaptation = new System.Windows.Forms.CheckBox();
            this.chBoxSave = new System.Windows.Forms.CheckBox();
            this.chBoxRewrite = new System.Windows.Forms.CheckBox();
            this.tBoxIndex = new System.Windows.Forms.TextBox();
            this.tBoxFilename = new System.Windows.Forms.TextBox();
            this.lblAdaptation = new System.Windows.Forms.Label();
            this.tBoxAdaptation = new System.Windows.Forms.TextBox();
            this.lblSetFM = new System.Windows.Forms.Label();
            this.lblSpect = new System.Windows.Forms.Label();
            this.tboxSpect = new System.Windows.Forms.TextBox();
            this.tBoxAverage = new System.Windows.Forms.TextBox();
            this.lblSetSpect = new System.Windows.Forms.Label();
            this.tBoxExpTime = new System.Windows.Forms.TextBox();
            this.lblSetExp = new System.Windows.Forms.Label();
            this.tBoxPeriod = new System.Windows.Forms.TextBox();
            this.lblSave = new System.Windows.Forms.Label();
            this.lblRwt = new System.Windows.Forms.Label();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.lblAverage = new System.Windows.Forms.Label();
            this.btnConfig = new System.Windows.Forms.Button();
            this.tBoxLog = new System.Windows.Forms.TextBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnInit = new System.Windows.Forms.Button();
            this.btnMeas = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tBoxD = new System.Windows.Forms.TextBox();
            this.tBoxP = new System.Windows.Forms.TextBox();
            this.tBoxI = new System.Windows.Forms.TextBox();
            this.shapeContainer1 = new Microsoft.VisualBasic.PowerPacks.ShapeContainer();
            this.lineShape1 = new Microsoft.VisualBasic.PowerPacks.LineShape();
            this.pnl2 = new System.Windows.Forms.Panel();
            this.lblSwitch = new System.Windows.Forms.Label();
            this.tBoxVChange = new System.Windows.Forms.TextBox();
            this.tBoxSP = new System.Windows.Forms.TextBox();
            this.tBoxPIDAverage = new System.Windows.Forms.TextBox();
            this.tBoxVoltage = new System.Windows.Forms.TextBox();
            this.tBoxRamp = new System.Windows.Forms.TextBox();
            this.chBoxPID = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblTemp = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblPIDAverage = new System.Windows.Forms.Label();
            this.lblStep = new System.Windows.Forms.Label();
            this.lblVoltage = new System.Windows.Forms.Label();
            this.lblRamp = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.cBoxCalib = new System.Windows.Forms.ComboBox();
            this.btnSwitch = new System.Windows.Forms.Button();
            this.btnCalib = new System.Windows.Forms.Button();
            this.btnSize = new System.Windows.Forms.Button();
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.formsPlot2 = new ScottPlot.FormsPlot();
            this.pnlSettings.SuspendLayout();
            this.pnl2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSettings
            // 
            this.pnlSettings.Controls.Add(this.cBoxSpect);
            this.pnlSettings.Controls.Add(this.chBoxAdaptation);
            this.pnlSettings.Controls.Add(this.chBoxSave);
            this.pnlSettings.Controls.Add(this.chBoxRewrite);
            this.pnlSettings.Controls.Add(this.tBoxIndex);
            this.pnlSettings.Controls.Add(this.tBoxFilename);
            this.pnlSettings.Controls.Add(this.lblAdaptation);
            this.pnlSettings.Controls.Add(this.tBoxAdaptation);
            this.pnlSettings.Controls.Add(this.lblSetFM);
            this.pnlSettings.Controls.Add(this.lblSpect);
            this.pnlSettings.Controls.Add(this.tboxSpect);
            this.pnlSettings.Controls.Add(this.tBoxAverage);
            this.pnlSettings.Controls.Add(this.lblSetSpect);
            this.pnlSettings.Controls.Add(this.tBoxExpTime);
            this.pnlSettings.Controls.Add(this.lblSetExp);
            this.pnlSettings.Controls.Add(this.tBoxPeriod);
            this.pnlSettings.Controls.Add(this.lblSave);
            this.pnlSettings.Controls.Add(this.lblRwt);
            this.pnlSettings.Controls.Add(this.lblPeriod);
            this.pnlSettings.Controls.Add(this.lblAverage);
            this.pnlSettings.Location = new System.Drawing.Point(323, 12);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(313, 210);
            this.pnlSettings.TabIndex = 6;
            // 
            // cBoxSpect
            // 
            this.cBoxSpect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxSpect.FormattingEnabled = true;
            this.cBoxSpect.Location = new System.Drawing.Point(99, 1);
            this.cBoxSpect.Margin = new System.Windows.Forms.Padding(2);
            this.cBoxSpect.Name = "cBoxSpect";
            this.cBoxSpect.Size = new System.Drawing.Size(108, 21);
            this.cBoxSpect.TabIndex = 0;
            // 
            // chBoxAdaptation
            // 
            this.chBoxAdaptation.AutoSize = true;
            this.chBoxAdaptation.Location = new System.Drawing.Point(231, 81);
            this.chBoxAdaptation.Name = "chBoxAdaptation";
            this.chBoxAdaptation.Size = new System.Drawing.Size(15, 14);
            this.chBoxAdaptation.TabIndex = 8;
            this.toolTip1.SetToolTip(this.chBoxAdaptation, "Allow exposure time adaptation.");
            this.chBoxAdaptation.UseVisualStyleBackColor = true;
            this.chBoxAdaptation.CheckedChanged += new System.EventHandler(this.ChBoxAdaptation_CheckedChanged);
            // 
            // chBoxSave
            // 
            this.chBoxSave.AutoSize = true;
            this.chBoxSave.Checked = true;
            this.chBoxSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxSave.Location = new System.Drawing.Point(287, 6);
            this.chBoxSave.Name = "chBoxSave";
            this.chBoxSave.Size = new System.Drawing.Size(15, 14);
            this.chBoxSave.TabIndex = 4;
            this.chBoxSave.UseVisualStyleBackColor = true;
            this.chBoxSave.CheckedChanged += new System.EventHandler(this.ChBoxSave_CheckedChanged);
            // 
            // chBoxRewrite
            // 
            this.chBoxRewrite.AutoSize = true;
            this.chBoxRewrite.Checked = true;
            this.chBoxRewrite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chBoxRewrite.Location = new System.Drawing.Point(231, 30);
            this.chBoxRewrite.Name = "chBoxRewrite";
            this.chBoxRewrite.Size = new System.Drawing.Size(15, 14);
            this.chBoxRewrite.TabIndex = 5;
            this.chBoxRewrite.UseVisualStyleBackColor = true;
            this.chBoxRewrite.CheckedChanged += new System.EventHandler(this.ChBoxRwt_CheckedChanged);
            // 
            // tBoxIndex
            // 
            this.tBoxIndex.Enabled = false;
            this.tBoxIndex.Location = new System.Drawing.Point(252, 27);
            this.tBoxIndex.Name = "tBoxIndex";
            this.tBoxIndex.Size = new System.Drawing.Size(50, 20);
            this.tBoxIndex.TabIndex = 6;
            this.tBoxIndex.Text = "00000";
            // 
            // tBoxFilename
            // 
            this.tBoxFilename.Location = new System.Drawing.Point(99, 105);
            this.tBoxFilename.Name = "tBoxFilename";
            this.tBoxFilename.Size = new System.Drawing.Size(203, 20);
            this.tBoxFilename.TabIndex = 10;
            this.tBoxFilename.Text = "Data\\Spectrum";
            this.toolTip1.SetToolTip(this.tBoxFilename, "Double-click to open a folder browser dialog");
            this.tBoxFilename.DoubleClick += new System.EventHandler(this.TBoxFilename_DoubleClick);
            // 
            // lblAdaptation
            // 
            this.lblAdaptation.Location = new System.Drawing.Point(156, 78);
            this.lblAdaptation.Name = "lblAdaptation";
            this.lblAdaptation.Size = new System.Drawing.Size(69, 20);
            this.lblAdaptation.TabIndex = 12;
            this.lblAdaptation.Text = "...adaptation";
            this.lblAdaptation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip1.SetToolTip(this.lblAdaptation, "Exposure time adaptation");
            // 
            // tBoxAdaptation
            // 
            this.tBoxAdaptation.Enabled = false;
            this.tBoxAdaptation.Location = new System.Drawing.Point(252, 78);
            this.tBoxAdaptation.Name = "tBoxAdaptation";
            this.tBoxAdaptation.Size = new System.Drawing.Size(50, 20);
            this.tBoxAdaptation.TabIndex = 9;
            this.tBoxAdaptation.Tag = "";
            this.toolTip1.SetToolTip(this.tBoxAdaptation, "Maximal exposure time allowed.");
            // 
            // lblSetFM
            // 
            this.lblSetFM.Location = new System.Drawing.Point(3, 104);
            this.lblSetFM.Name = "lblSetFM";
            this.lblSetFM.Size = new System.Drawing.Size(91, 20);
            this.lblSetFM.TabIndex = 15;
            this.lblSetFM.Text = "Filename";
            this.lblSetFM.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSpect
            // 
            this.lblSpect.Location = new System.Drawing.Point(3, 2);
            this.lblSpect.Name = "lblSpect";
            this.lblSpect.Size = new System.Drawing.Size(91, 20);
            this.lblSpect.TabIndex = 17;
            this.lblSpect.Text = "Select device";
            this.lblSpect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tboxSpect
            // 
            this.tboxSpect.Location = new System.Drawing.Point(100, 27);
            this.tboxSpect.Name = "tboxSpect";
            this.tboxSpect.Size = new System.Drawing.Size(50, 20);
            this.tboxSpect.TabIndex = 1;
            this.tboxSpect.Text = "0";
            // 
            // tBoxAverage
            // 
            this.tBoxAverage.Location = new System.Drawing.Point(252, 53);
            this.tBoxAverage.Name = "tBoxAverage";
            this.tBoxAverage.Size = new System.Drawing.Size(50, 20);
            this.tBoxAverage.TabIndex = 7;
            this.tBoxAverage.Text = "1";
            this.toolTip1.SetToolTip(this.tBoxAverage, "Number of periods to average.");
            // 
            // lblSetSpect
            // 
            this.lblSetSpect.Location = new System.Drawing.Point(3, 26);
            this.lblSetSpect.Name = "lblSetSpect";
            this.lblSetSpect.Size = new System.Drawing.Size(91, 20);
            this.lblSetSpect.TabIndex = 0;
            this.lblSetSpect.Text = "Spectrometer";
            this.lblSetSpect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tBoxExpTime
            // 
            this.tBoxExpTime.Location = new System.Drawing.Point(99, 79);
            this.tBoxExpTime.Name = "tBoxExpTime";
            this.tBoxExpTime.Size = new System.Drawing.Size(50, 20);
            this.tBoxExpTime.TabIndex = 3;
            this.toolTip1.SetToolTip(this.tBoxExpTime, "Actual exposure time");
            // 
            // lblSetExp
            // 
            this.lblSetExp.Location = new System.Drawing.Point(3, 78);
            this.lblSetExp.Name = "lblSetExp";
            this.lblSetExp.Size = new System.Drawing.Size(91, 20);
            this.lblSetExp.TabIndex = 4;
            this.lblSetExp.Text = "Exposure time";
            this.lblSetExp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tBoxPeriod
            // 
            this.tBoxPeriod.Location = new System.Drawing.Point(99, 53);
            this.tBoxPeriod.Name = "tBoxPeriod";
            this.tBoxPeriod.Size = new System.Drawing.Size(50, 20);
            this.tBoxPeriod.TabIndex = 2;
            // 
            // lblSave
            // 
            this.lblSave.Location = new System.Drawing.Point(212, 2);
            this.lblSave.Name = "lblSave";
            this.lblSave.Size = new System.Drawing.Size(69, 20);
            this.lblSave.TabIndex = 5;
            this.lblSave.Text = "Save";
            this.lblSave.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRwt
            // 
            this.lblRwt.Location = new System.Drawing.Point(156, 26);
            this.lblRwt.Name = "lblRwt";
            this.lblRwt.Size = new System.Drawing.Size(69, 20);
            this.lblRwt.TabIndex = 6;
            this.lblRwt.Text = "Rewrite";
            this.lblRwt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblPeriod
            // 
            this.lblPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblPeriod.Location = new System.Drawing.Point(3, 52);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(91, 20);
            this.lblPeriod.TabIndex = 2;
            this.lblPeriod.Text = "Period read";
            this.lblPeriod.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAverage
            // 
            this.lblAverage.Location = new System.Drawing.Point(156, 52);
            this.lblAverage.Name = "lblAverage";
            this.lblAverage.Size = new System.Drawing.Size(69, 20);
            this.lblAverage.TabIndex = 9;
            this.lblAverage.Text = "Average";
            this.lblAverage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnConfig
            // 
            this.btnConfig.Location = new System.Drawing.Point(12, 156);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(100, 30);
            this.btnConfig.TabIndex = 11;
            this.btnConfig.Text = "&Reload config";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.BtnConfig_Click);
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
            // btnInit
            // 
            this.btnInit.Location = new System.Drawing.Point(12, 12);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(100, 30);
            this.btnInit.TabIndex = 0;
            this.btnInit.Text = "Init_default";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.BtnInit_Click);
            // 
            // btnMeas
            // 
            this.btnMeas.Location = new System.Drawing.Point(12, 48);
            this.btnMeas.Name = "btnMeas";
            this.btnMeas.Size = new System.Drawing.Size(100, 30);
            this.btnMeas.TabIndex = 1;
            this.btnMeas.Text = "Meas_default";
            this.btnMeas.UseVisualStyleBackColor = true;
            this.btnMeas.Click += new System.EventHandler(this.BtnMeas_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 200;
            this.toolTip1.ReshowDelay = 100;
            // 
            // tBoxD
            // 
            this.tBoxD.Location = new System.Drawing.Point(206, 27);
            this.tBoxD.Name = "tBoxD";
            this.tBoxD.Size = new System.Drawing.Size(50, 20);
            this.tBoxD.TabIndex = 8;
            this.tBoxD.Text = "10";
            this.toolTip1.SetToolTip(this.tBoxD, "D");
            // 
            // tBoxP
            // 
            this.tBoxP.Location = new System.Drawing.Point(112, 27);
            this.tBoxP.Name = "tBoxP";
            this.tBoxP.Size = new System.Drawing.Size(41, 20);
            this.tBoxP.TabIndex = 6;
            this.tBoxP.Text = "250";
            this.toolTip1.SetToolTip(this.tBoxP, "P");
            // 
            // tBoxI
            // 
            this.tBoxI.Location = new System.Drawing.Point(159, 27);
            this.tBoxI.Name = "tBoxI";
            this.tBoxI.Size = new System.Drawing.Size(41, 20);
            this.tBoxI.TabIndex = 7;
            this.tBoxI.Text = "300";
            this.toolTip1.SetToolTip(this.tBoxI, "I");
            // 
            // shapeContainer1
            // 
            this.shapeContainer1.Location = new System.Drawing.Point(0, 0);
            this.shapeContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.shapeContainer1.Name = "shapeContainer1";
            this.shapeContainer1.Shapes.AddRange(new Microsoft.VisualBasic.PowerPacks.Shape[] {
            this.lineShape1});
            this.shapeContainer1.Size = new System.Drawing.Size(913, 596);
            this.shapeContainer1.TabIndex = 6;
            this.shapeContainer1.TabStop = false;
            // 
            // lineShape1
            // 
            this.lineShape1.Name = "lineShape1";
            this.lineShape1.X1 = 12;
            this.lineShape1.X2 = 903;
            this.lineShape1.Y1 = 234;
            this.lineShape1.Y2 = 234;
            // 
            // pnl2
            // 
            this.pnl2.Controls.Add(this.lblSwitch);
            this.pnl2.Controls.Add(this.tBoxVChange);
            this.pnl2.Controls.Add(this.tBoxSP);
            this.pnl2.Controls.Add(this.tBoxPIDAverage);
            this.pnl2.Controls.Add(this.tBoxVoltage);
            this.pnl2.Controls.Add(this.tBoxRamp);
            this.pnl2.Controls.Add(this.tBoxD);
            this.pnl2.Controls.Add(this.chBoxPID);
            this.pnl2.Controls.Add(this.tBoxP);
            this.pnl2.Controls.Add(this.tBoxI);
            this.pnl2.Controls.Add(this.btnSave);
            this.pnl2.Controls.Add(this.lblTemp);
            this.pnl2.Controls.Add(this.lblInfo);
            this.pnl2.Controls.Add(this.lblPIDAverage);
            this.pnl2.Controls.Add(this.lblStep);
            this.pnl2.Controls.Add(this.lblVoltage);
            this.pnl2.Controls.Add(this.lblRamp);
            this.pnl2.Controls.Add(this.btnLoad);
            this.pnl2.Controls.Add(this.btnClear);
            this.pnl2.Controls.Add(this.cBoxCalib);
            this.pnl2.Controls.Add(this.btnSwitch);
            this.pnl2.Controls.Add(this.btnCalib);
            this.pnl2.Location = new System.Drawing.Point(642, 12);
            this.pnl2.Name = "pnl2";
            this.pnl2.Size = new System.Drawing.Size(262, 210);
            this.pnl2.TabIndex = 7;
            // 
            // lblSwitch
            // 
            this.lblSwitch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblSwitch.Location = new System.Drawing.Point(218, 180);
            this.lblSwitch.Name = "lblSwitch";
            this.lblSwitch.Size = new System.Drawing.Size(38, 30);
            this.lblSwitch.TabIndex = 12;
            // 
            // tBoxVChange
            // 
            this.tBoxVChange.Location = new System.Drawing.Point(149, 133);
            this.tBoxVChange.Name = "tBoxVChange";
            this.tBoxVChange.Size = new System.Drawing.Size(31, 20);
            this.tBoxVChange.TabIndex = 11;
            this.tBoxVChange.Text = "0.1";
            // 
            // tBoxSP
            // 
            this.tBoxSP.Location = new System.Drawing.Point(206, 53);
            this.tBoxSP.Name = "tBoxSP";
            this.tBoxSP.Size = new System.Drawing.Size(50, 20);
            this.tBoxSP.TabIndex = 9;
            this.tBoxSP.Text = "20";
            // 
            // tBoxPIDAverage
            // 
            this.tBoxPIDAverage.Location = new System.Drawing.Point(225, 133);
            this.tBoxPIDAverage.Name = "tBoxPIDAverage";
            this.tBoxPIDAverage.Size = new System.Drawing.Size(31, 20);
            this.tBoxPIDAverage.TabIndex = 11;
            this.tBoxPIDAverage.Text = "3";
            // 
            // tBoxVoltage
            // 
            this.tBoxVoltage.Location = new System.Drawing.Point(206, 105);
            this.tBoxVoltage.Name = "tBoxVoltage";
            this.tBoxVoltage.Size = new System.Drawing.Size(50, 20);
            this.tBoxVoltage.TabIndex = 11;
            this.tBoxVoltage.Text = "3";
            // 
            // tBoxRamp
            // 
            this.tBoxRamp.Location = new System.Drawing.Point(206, 78);
            this.tBoxRamp.Name = "tBoxRamp";
            this.tBoxRamp.Size = new System.Drawing.Size(50, 20);
            this.tBoxRamp.TabIndex = 10;
            this.tBoxRamp.Text = "10";
            // 
            // chBoxPID
            // 
            this.chBoxPID.Appearance = System.Windows.Forms.Appearance.Button;
            this.chBoxPID.Location = new System.Drawing.Point(112, 0);
            this.chBoxPID.Name = "chBoxPID";
            this.chBoxPID.Size = new System.Drawing.Size(144, 22);
            this.chBoxPID.TabIndex = 5;
            this.chBoxPID.Text = "&PID control";
            this.chBoxPID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chBoxPID.UseVisualStyleBackColor = true;
            this.chBoxPID.CheckedChanged += new System.EventHandler(this.ChBoxPID_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(3, 0);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "&Save temps";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // lblTemp
            // 
            this.lblTemp.Location = new System.Drawing.Point(109, 52);
            this.lblTemp.Name = "lblTemp";
            this.lblTemp.Size = new System.Drawing.Size(91, 20);
            this.lblTemp.TabIndex = 0;
            this.lblTemp.Text = "Temp setpoint";
            this.lblTemp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(109, 154);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(147, 20);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = "<info>";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPIDAverage
            // 
            this.lblPIDAverage.Location = new System.Drawing.Point(185, 132);
            this.lblPIDAverage.Name = "lblPIDAverage";
            this.lblPIDAverage.Size = new System.Drawing.Size(34, 20);
            this.lblPIDAverage.TabIndex = 0;
            this.lblPIDAverage.Text = "#Avg";
            this.lblPIDAverage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblStep
            // 
            this.lblStep.Location = new System.Drawing.Point(109, 131);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(34, 20);
            this.lblStep.TabIndex = 0;
            this.lblStep.Text = "MΔV";
            this.lblStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblVoltage
            // 
            this.lblVoltage.Location = new System.Drawing.Point(109, 104);
            this.lblVoltage.Name = "lblVoltage";
            this.lblVoltage.Size = new System.Drawing.Size(91, 20);
            this.lblVoltage.TabIndex = 0;
            this.lblVoltage.Text = "Output voltage";
            this.lblVoltage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblRamp
            // 
            this.lblRamp.Location = new System.Drawing.Point(109, 78);
            this.lblRamp.Name = "lblRamp";
            this.lblRamp.Size = new System.Drawing.Size(91, 20);
            this.lblRamp.TabIndex = 0;
            this.lblRamp.Text = "Ramp (°C / min)";
            this.lblRamp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(3, 36);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(100, 30);
            this.btnLoad.TabIndex = 1;
            this.btnLoad.Text = "&Load spectra";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
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
            // cBoxCalib
            // 
            this.cBoxCalib.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cBoxCalib.FormattingEnabled = true;
            this.cBoxCalib.Location = new System.Drawing.Point(3, 112);
            this.cBoxCalib.Margin = new System.Windows.Forms.Padding(2);
            this.cBoxCalib.Name = "cBoxCalib";
            this.cBoxCalib.Size = new System.Drawing.Size(100, 21);
            this.cBoxCalib.TabIndex = 3;
            this.cBoxCalib.SelectedIndexChanged += new System.EventHandler(this.CBoxCalib_SelectedIndexChanged);
            // 
            // btnSwitch
            // 
            this.btnSwitch.Location = new System.Drawing.Point(112, 180);
            this.btnSwitch.Name = "btnSwitch";
            this.btnSwitch.Size = new System.Drawing.Size(100, 30);
            this.btnSwitch.TabIndex = 4;
            this.btnSwitch.Text = "Swit_default";
            this.btnSwitch.UseVisualStyleBackColor = true;
            this.btnSwitch.Click += new System.EventHandler(this.BtnSwitch_Click);
            // 
            // btnCalib
            // 
            this.btnCalib.Location = new System.Drawing.Point(3, 144);
            this.btnCalib.Name = "btnCalib";
            this.btnCalib.Size = new System.Drawing.Size(100, 30);
            this.btnCalib.TabIndex = 4;
            this.btnCalib.Text = "Sho&w calibration";
            this.btnCalib.UseVisualStyleBackColor = true;
            this.btnCalib.Click += new System.EventHandler(this.BtnCalib_Click);
            // 
            // btnSize
            // 
            this.btnSize.Location = new System.Drawing.Point(12, 120);
            this.btnSize.Name = "btnSize";
            this.btnSize.Size = new System.Drawing.Size(100, 30);
            this.btnSize.TabIndex = 3;
            this.btnSize.Text = "&Default size";
            this.btnSize.UseVisualStyleBackColor = true;
            this.btnSize.Click += new System.EventHandler(this.BtnSize_Click);
            // 
            // formsPlot1
            // 
            this.formsPlot1.BackColor = System.Drawing.Color.Transparent;
            this.formsPlot1.Location = new System.Drawing.Point(12, 246);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(446, 446);
            this.formsPlot1.TabIndex = 12;
            // 
            // formsPlot2
            // 
            this.formsPlot2.BackColor = System.Drawing.Color.Transparent;
            this.formsPlot2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.formsPlot2.Location = new System.Drawing.Point(458, 246);
            this.formsPlot2.Name = "formsPlot2";
            this.formsPlot2.Size = new System.Drawing.Size(446, 446);
            this.formsPlot2.TabIndex = 13;
            // 
            // Form_main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 596);
            this.Controls.Add(this.formsPlot2);
            this.Controls.Add(this.formsPlot1);
            this.Controls.Add(this.btnConfig);
            this.Controls.Add(this.btnSize);
            this.Controls.Add(this.pnl2);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.tBoxLog);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnMeas);
            this.Controls.Add(this.shapeContainer1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(646, 273);
            this.Name = "Form_main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "spectrometric_thermometer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_main_FormClosed);
            this.Load += new System.EventHandler(this.Form_main_Load);
            this.SizeChanged += new System.EventHandler(this.Form_main_SizeChanged);
            this.pnlSettings.ResumeLayout(false);
            this.pnlSettings.PerformLayout();
            this.pnl2.ResumeLayout(false);
            this.pnl2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Panel pnlSettings;
        internal System.Windows.Forms.CheckBox chBoxRewrite;
        internal System.Windows.Forms.TextBox tBoxFilename;
        internal System.Windows.Forms.TextBox tBoxPeriod;
        internal System.Windows.Forms.TextBox tBoxIndex;
        internal System.Windows.Forms.TextBox tBoxExpTime;
        internal System.Windows.Forms.TextBox tboxSpect;
        internal System.Windows.Forms.Label lblSetFM;
        internal System.Windows.Forms.Label lblPeriod;
        internal System.Windows.Forms.Label lblRwt;
        internal System.Windows.Forms.Label lblSetExp;
        internal System.Windows.Forms.Label lblSetSpect;
        internal System.Windows.Forms.TextBox tBoxLog;
        internal System.Windows.Forms.Button btnHelp;
        internal System.Windows.Forms.Button btnExit;
        internal System.Windows.Forms.Button btnInit;
        internal System.Windows.Forms.Button btnMeas;
        internal System.Windows.Forms.CheckBox chBoxAdaptation;
        internal System.Windows.Forms.Label lblAdaptation;
        private System.Windows.Forms.ComboBox cBoxSpect;
        internal System.Windows.Forms.Label lblSpect;
        internal System.Windows.Forms.Label lblAverage;
        internal System.Windows.Forms.TextBox tBoxAverage;
        internal System.Windows.Forms.TextBox tBoxAdaptation;
        private System.Windows.Forms.ToolTip toolTip1;
        private Microsoft.VisualBasic.PowerPacks.ShapeContainer shapeContainer1;
        private Microsoft.VisualBasic.PowerPacks.LineShape lineShape1;
        internal System.Windows.Forms.CheckBox chBoxSave;
        internal System.Windows.Forms.Label lblSave;
        private System.Windows.Forms.Panel pnl2;
        internal System.Windows.Forms.Button btnSize;
        internal System.Windows.Forms.Button btnCalib;
        private System.Windows.Forms.ComboBox cBoxCalib;
        internal System.Windows.Forms.Button btnClear;
        internal System.Windows.Forms.Button btnLoad;
        internal System.Windows.Forms.Button btnSave;
        internal System.Windows.Forms.TextBox tBoxP;
        internal System.Windows.Forms.TextBox tBoxD;
        internal System.Windows.Forms.TextBox tBoxI;
        internal System.Windows.Forms.CheckBox chBoxPID;
        internal System.Windows.Forms.TextBox tBoxRamp;
        internal System.Windows.Forms.Label lblRamp;
        internal System.Windows.Forms.TextBox tBoxSP;
        internal System.Windows.Forms.Label lblTemp;
        internal System.Windows.Forms.Label lblInfo;
        internal System.Windows.Forms.TextBox tBoxVoltage;
        internal System.Windows.Forms.Label lblVoltage;
        internal System.Windows.Forms.Button btnConfig;
        internal System.Windows.Forms.TextBox tBoxVChange;
        internal System.Windows.Forms.Label lblStep;
        internal System.Windows.Forms.TextBox tBoxPIDAverage;
        internal System.Windows.Forms.Label lblPIDAverage;
        internal System.Windows.Forms.Button btnSwitch;
        private System.Windows.Forms.Label lblSwitch;
        private ScottPlot.FormsPlot formsPlot1;
        private ScottPlot.FormsPlot formsPlot2;
    }
}

