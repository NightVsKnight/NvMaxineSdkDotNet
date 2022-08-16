namespace NvAfxDotNetTestApp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelHeaderEnableCaptureRender = new System.Windows.Forms.Panel();
            this.checkBoxCaptureRender = new System.Windows.Forms.CheckBox();
            this.comboBoxRender = new System.Windows.Forms.ComboBox();
            this.labelRender = new System.Windows.Forms.Label();
            this.comboBoxCapture = new System.Windows.Forms.ComboBox();
            this.labelCapture = new System.Windows.Forms.Label();
            this.groupBoxNoiseSuppression = new System.Windows.Forms.GroupBox();
            this.checkBoxNoiseSuppression = new System.Windows.Forms.CheckBox();
            this.panelNoiseSuppressionIntensity = new System.Windows.Forms.Panel();
            this.trackBarNoiseSuppressionIntensity = new System.Windows.Forms.TrackBar();
            this.labelNoiseSuppressionIntensity = new System.Windows.Forms.Label();
            this.textBoxNoiseSuppressionIntensity = new System.Windows.Forms.TextBox();
            this.panelHeaderEnableCaptureRender.SuspendLayout();
            this.groupBoxNoiseSuppression.SuspendLayout();
            this.panelNoiseSuppressionIntensity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarNoiseSuppressionIntensity)).BeginInit();
            this.SuspendLayout();
            // 
            // panelHeaderEnableCaptureRender
            // 
            this.panelHeaderEnableCaptureRender.AutoSize = true;
            this.panelHeaderEnableCaptureRender.Controls.Add(this.checkBoxCaptureRender);
            this.panelHeaderEnableCaptureRender.Controls.Add(this.comboBoxRender);
            this.panelHeaderEnableCaptureRender.Controls.Add(this.labelRender);
            this.panelHeaderEnableCaptureRender.Controls.Add(this.comboBoxCapture);
            this.panelHeaderEnableCaptureRender.Controls.Add(this.labelCapture);
            this.panelHeaderEnableCaptureRender.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeaderEnableCaptureRender.Location = new System.Drawing.Point(0, 0);
            this.panelHeaderEnableCaptureRender.Name = "panelHeaderEnableCaptureRender";
            this.panelHeaderEnableCaptureRender.Size = new System.Drawing.Size(492, 71);
            this.panelHeaderEnableCaptureRender.TabIndex = 3;
            // 
            // checkBoxCaptureRender
            // 
            this.checkBoxCaptureRender.AutoSize = true;
            this.checkBoxCaptureRender.Location = new System.Drawing.Point(3, 3);
            this.checkBoxCaptureRender.Name = "checkBoxCaptureRender";
            this.checkBoxCaptureRender.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.checkBoxCaptureRender.Size = new System.Drawing.Size(70, 17);
            this.checkBoxCaptureRender.TabIndex = 0;
            this.checkBoxCaptureRender.Text = "Enabled";
            this.checkBoxCaptureRender.UseVisualStyleBackColor = true;
            this.checkBoxCaptureRender.CheckedChanged += new System.EventHandler(this.checkBoxCaptureRender_CheckedChanged);
            // 
            // comboBoxRender
            // 
            this.comboBoxRender.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxRender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRender.FormattingEnabled = true;
            this.comboBoxRender.Location = new System.Drawing.Point(74, 47);
            this.comboBoxRender.Name = "comboBoxRender";
            this.comboBoxRender.Size = new System.Drawing.Size(412, 21);
            this.comboBoxRender.Sorted = true;
            this.comboBoxRender.TabIndex = 4;
            this.comboBoxRender.SelectedValueChanged += new System.EventHandler(this.comboBoxRender_SelectedValueChanged);
            // 
            // labelRender
            // 
            this.labelRender.Location = new System.Drawing.Point(3, 50);
            this.labelRender.Name = "labelRender";
            this.labelRender.Size = new System.Drawing.Size(65, 13);
            this.labelRender.TabIndex = 3;
            this.labelRender.Text = "Render";
            this.labelRender.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelRender.DoubleClick += new System.EventHandler(this.labelRender_DoubleClick);
            // 
            // comboBoxCapture
            // 
            this.comboBoxCapture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxCapture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCapture.FormattingEnabled = true;
            this.comboBoxCapture.Location = new System.Drawing.Point(74, 20);
            this.comboBoxCapture.Name = "comboBoxCapture";
            this.comboBoxCapture.Size = new System.Drawing.Size(412, 21);
            this.comboBoxCapture.Sorted = true;
            this.comboBoxCapture.TabIndex = 2;
            this.comboBoxCapture.SelectedValueChanged += new System.EventHandler(this.comboBoxCapture_SelectedValueChanged);
            // 
            // labelCapture
            // 
            this.labelCapture.Location = new System.Drawing.Point(3, 23);
            this.labelCapture.Name = "labelCapture";
            this.labelCapture.Size = new System.Drawing.Size(65, 13);
            this.labelCapture.TabIndex = 1;
            this.labelCapture.Text = "Capture";
            this.labelCapture.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.labelCapture.DoubleClick += new System.EventHandler(this.labelCapture_DoubleClick);
            // 
            // groupBoxNoiseSuppression
            // 
            this.groupBoxNoiseSuppression.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxNoiseSuppression.Controls.Add(this.checkBoxNoiseSuppression);
            this.groupBoxNoiseSuppression.Controls.Add(this.panelNoiseSuppressionIntensity);
            this.groupBoxNoiseSuppression.Location = new System.Drawing.Point(12, 77);
            this.groupBoxNoiseSuppression.Name = "groupBoxNoiseSuppression";
            this.groupBoxNoiseSuppression.Size = new System.Drawing.Size(468, 66);
            this.groupBoxNoiseSuppression.TabIndex = 18;
            this.groupBoxNoiseSuppression.TabStop = false;
            // 
            // checkBoxNoiseSuppression
            // 
            this.checkBoxNoiseSuppression.AutoSize = true;
            this.checkBoxNoiseSuppression.Location = new System.Drawing.Point(6, 6);
            this.checkBoxNoiseSuppression.Name = "checkBoxNoiseSuppression";
            this.checkBoxNoiseSuppression.Size = new System.Drawing.Size(114, 17);
            this.checkBoxNoiseSuppression.TabIndex = 7;
            this.checkBoxNoiseSuppression.Text = "Noise Suppression";
            this.checkBoxNoiseSuppression.UseVisualStyleBackColor = true;
            this.checkBoxNoiseSuppression.CheckedChanged += new System.EventHandler(this.checkBoxNoiseSuppression_CheckedChanged);
            // 
            // panelNoiseSuppressionIntensity
            // 
            this.panelNoiseSuppressionIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelNoiseSuppressionIntensity.Controls.Add(this.trackBarNoiseSuppressionIntensity);
            this.panelNoiseSuppressionIntensity.Controls.Add(this.labelNoiseSuppressionIntensity);
            this.panelNoiseSuppressionIntensity.Controls.Add(this.textBoxNoiseSuppressionIntensity);
            this.panelNoiseSuppressionIntensity.Location = new System.Drawing.Point(6, 24);
            this.panelNoiseSuppressionIntensity.Name = "panelNoiseSuppressionIntensity";
            this.panelNoiseSuppressionIntensity.Size = new System.Drawing.Size(456, 35);
            this.panelNoiseSuppressionIntensity.TabIndex = 5;
            // 
            // trackBarNoiseSuppressionIntensity
            // 
            this.trackBarNoiseSuppressionIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarNoiseSuppressionIntensity.Location = new System.Drawing.Point(79, 0);
            this.trackBarNoiseSuppressionIntensity.Name = "trackBarNoiseSuppressionIntensity";
            this.trackBarNoiseSuppressionIntensity.Size = new System.Drawing.Size(296, 45);
            this.trackBarNoiseSuppressionIntensity.TabIndex = 1;
            this.trackBarNoiseSuppressionIntensity.ValueChanged += new System.EventHandler(this.trackBarNoiseSuppressionIntensity_ValueChanged);
            // 
            // labelNoiseSuppressionIntensity
            // 
            this.labelNoiseSuppressionIntensity.Location = new System.Drawing.Point(3, 6);
            this.labelNoiseSuppressionIntensity.Name = "labelNoiseSuppressionIntensity";
            this.labelNoiseSuppressionIntensity.Size = new System.Drawing.Size(70, 13);
            this.labelNoiseSuppressionIntensity.TabIndex = 0;
            this.labelNoiseSuppressionIntensity.Text = "Intensity";
            this.labelNoiseSuppressionIntensity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxNoiseSuppressionIntensity
            // 
            this.textBoxNoiseSuppressionIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxNoiseSuppressionIntensity.Location = new System.Drawing.Point(381, 3);
            this.textBoxNoiseSuppressionIntensity.Name = "textBoxNoiseSuppressionIntensity";
            this.textBoxNoiseSuppressionIntensity.ReadOnly = true;
            this.textBoxNoiseSuppressionIntensity.Size = new System.Drawing.Size(46, 20);
            this.textBoxNoiseSuppressionIntensity.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(492, 159);
            this.Controls.Add(this.groupBoxNoiseSuppression);
            this.Controls.Add(this.panelHeaderEnableCaptureRender);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panelHeaderEnableCaptureRender.ResumeLayout(false);
            this.panelHeaderEnableCaptureRender.PerformLayout();
            this.groupBoxNoiseSuppression.ResumeLayout(false);
            this.groupBoxNoiseSuppression.PerformLayout();
            this.panelNoiseSuppressionIntensity.ResumeLayout(false);
            this.panelNoiseSuppressionIntensity.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarNoiseSuppressionIntensity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panelHeaderEnableCaptureRender;
        private System.Windows.Forms.CheckBox checkBoxCaptureRender;
        private System.Windows.Forms.ComboBox comboBoxRender;
        private System.Windows.Forms.Label labelRender;
        private System.Windows.Forms.ComboBox comboBoxCapture;
        private System.Windows.Forms.Label labelCapture;
        private System.Windows.Forms.GroupBox groupBoxNoiseSuppression;
        private System.Windows.Forms.CheckBox checkBoxNoiseSuppression;
        private System.Windows.Forms.Panel panelNoiseSuppressionIntensity;
        private System.Windows.Forms.Label labelNoiseSuppressionIntensity;
        private System.Windows.Forms.TrackBar trackBarNoiseSuppressionIntensity;
        private System.Windows.Forms.TextBox textBoxNoiseSuppressionIntensity;
    }
}

