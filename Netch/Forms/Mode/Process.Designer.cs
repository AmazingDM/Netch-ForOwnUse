﻿namespace Netch.Forms.Mode
{
    partial class Process
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Process));
            this.ConfigurationGroupBox = new System.Windows.Forms.GroupBox();
            this.UseCustomFilenameBox = new System.Windows.Forms.CheckBox();
            this.TimeDataButton = new System.Windows.Forms.RadioButton();
            this.StaySameButton = new System.Windows.Forms.RadioButton();
            this.FilenameLabel = new System.Windows.Forms.Label();
            this.FilenameTextBox = new System.Windows.Forms.TextBox();
            this.ScanButton = new System.Windows.Forms.Button();
            this.ProcessGroupBox = new System.Windows.Forms.GroupBox();
            this.AddButton = new System.Windows.Forms.Button();
            this.ProcessNameTextBox = new System.Windows.Forms.TextBox();
            this.RuleListBox = new System.Windows.Forms.ListBox();
            this.RemarkTextBox = new System.Windows.Forms.TextBox();
            this.RemarkLabel = new System.Windows.Forms.Label();
            this.ControlButton = new System.Windows.Forms.Button();
            this.ConfigurationGroupBox.SuspendLayout();
            this.ProcessGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ConfigurationGroupBox
            // 
            this.ConfigurationGroupBox.Controls.Add(this.UseCustomFilenameBox);
            this.ConfigurationGroupBox.Controls.Add(this.TimeDataButton);
            this.ConfigurationGroupBox.Controls.Add(this.StaySameButton);
            this.ConfigurationGroupBox.Controls.Add(this.FilenameLabel);
            this.ConfigurationGroupBox.Controls.Add(this.FilenameTextBox);
            this.ConfigurationGroupBox.Controls.Add(this.ScanButton);
            this.ConfigurationGroupBox.Controls.Add(this.ProcessGroupBox);
            this.ConfigurationGroupBox.Controls.Add(this.RuleListBox);
            this.ConfigurationGroupBox.Controls.Add(this.RemarkTextBox);
            this.ConfigurationGroupBox.Controls.Add(this.RemarkLabel);
            this.ConfigurationGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
            this.ConfigurationGroupBox.Size = new System.Drawing.Size(340, 344);
            this.ConfigurationGroupBox.TabIndex = 0;
            this.ConfigurationGroupBox.TabStop = false;
            this.ConfigurationGroupBox.Text = "Configuration";
            // 
            // UseCustomFilenameBox
            // 
            this.UseCustomFilenameBox.AutoSize = true;
            this.UseCustomFilenameBox.Location = new System.Drawing.Point(84, 82);
            this.UseCustomFilenameBox.Name = "UseCustomFilenameBox";
            this.UseCustomFilenameBox.Size = new System.Drawing.Size(152, 21);
            this.UseCustomFilenameBox.TabIndex = 9;
            this.UseCustomFilenameBox.Text = "Use Custom Filename";
            this.UseCustomFilenameBox.UseVisualStyleBackColor = true;
            this.UseCustomFilenameBox.CheckedChanged += new System.EventHandler(this.UseCustomFileNameBox_CheckedChanged);
            // 
            // TimeDataButton
            // 
            this.TimeDataButton.AutoSize = true;
            this.TimeDataButton.Location = new System.Drawing.Point(197, 106);
            this.TimeDataButton.Name = "TimeDataButton";
            this.TimeDataButton.Size = new System.Drawing.Size(84, 21);
            this.TimeDataButton.TabIndex = 8;
            this.TimeDataButton.TabStop = true;
            this.TimeDataButton.Text = "Time data";
            this.TimeDataButton.UseVisualStyleBackColor = true;
            // 
            // StaySameButton
            // 
            this.StaySameButton.AutoSize = true;
            this.StaySameButton.Location = new System.Drawing.Point(84, 106);
            this.StaySameButton.Name = "StaySameButton";
            this.StaySameButton.Size = new System.Drawing.Size(107, 21);
            this.StaySameButton.TabIndex = 7;
            this.StaySameButton.TabStop = true;
            this.StaySameButton.Text = "Stay the same";
            this.StaySameButton.UseVisualStyleBackColor = true;
            // 
            // FilenameLabel
            // 
            this.FilenameLabel.AutoSize = true;
            this.FilenameLabel.Location = new System.Drawing.Point(12, 55);
            this.FilenameLabel.Name = "FilenameLabel";
            this.FilenameLabel.Size = new System.Drawing.Size(59, 17);
            this.FilenameLabel.TabIndex = 6;
            this.FilenameLabel.Text = "Filename";
            // 
            // FilenameTextBox
            // 
            this.FilenameTextBox.Location = new System.Drawing.Point(84, 52);
            this.FilenameTextBox.Name = "FilenameTextBox";
            this.FilenameTextBox.Size = new System.Drawing.Size(250, 23);
            this.FilenameTextBox.TabIndex = 5;
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(6, 315);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(75, 23);
            this.ScanButton.TabIndex = 4;
            this.ScanButton.Text = "Scan";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // ProcessGroupBox
            // 
            this.ProcessGroupBox.Controls.Add(this.AddButton);
            this.ProcessGroupBox.Controls.Add(this.ProcessNameTextBox);
            this.ProcessGroupBox.Location = new System.Drawing.Point(6, 263);
            this.ProcessGroupBox.Name = "ProcessGroupBox";
            this.ProcessGroupBox.Size = new System.Drawing.Size(328, 46);
            this.ProcessGroupBox.TabIndex = 3;
            this.ProcessGroupBox.TabStop = false;
            // 
            // AddButton
            // 
            this.AddButton.Location = new System.Drawing.Point(247, 15);
            this.AddButton.Name = "AddButton";
            this.AddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButton.TabIndex = 1;
            this.AddButton.Text = "Add";
            this.AddButton.UseVisualStyleBackColor = true;
            this.AddButton.Click += new System.EventHandler(this.AddButton_Click);
            // 
            // ProcessNameTextBox
            // 
            this.ProcessNameTextBox.Location = new System.Drawing.Point(6, 15);
            this.ProcessNameTextBox.Name = "ProcessNameTextBox";
            this.ProcessNameTextBox.Size = new System.Drawing.Size(222, 23);
            this.ProcessNameTextBox.TabIndex = 0;
            // 
            // RuleListBox
            // 
            this.RuleListBox.FormattingEnabled = true;
            this.RuleListBox.ItemHeight = 17;
            this.RuleListBox.Location = new System.Drawing.Point(6, 134);
            this.RuleListBox.Name = "RuleListBox";
            this.RuleListBox.Size = new System.Drawing.Size(328, 123);
            this.RuleListBox.TabIndex = 2;
            this.RuleListBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RuleListBox_MouseUp);
            // 
            // RemarkTextBox
            // 
            this.RemarkTextBox.Location = new System.Drawing.Point(84, 22);
            this.RemarkTextBox.Name = "RemarkTextBox";
            this.RemarkTextBox.Size = new System.Drawing.Size(250, 23);
            this.RemarkTextBox.TabIndex = 1;
            // 
            // RemarkLabel
            // 
            this.RemarkLabel.AutoSize = true;
            this.RemarkLabel.Location = new System.Drawing.Point(12, 25);
            this.RemarkLabel.Name = "RemarkLabel";
            this.RemarkLabel.Size = new System.Drawing.Size(53, 17);
            this.RemarkLabel.TabIndex = 0;
            this.RemarkLabel.Text = "Remark";
            // 
            // ControlButton
            // 
            this.ControlButton.Location = new System.Drawing.Point(277, 362);
            this.ControlButton.Name = "ControlButton";
            this.ControlButton.Size = new System.Drawing.Size(75, 23);
            this.ControlButton.TabIndex = 1;
            this.ControlButton.Text = "Save";
            this.ControlButton.UseVisualStyleBackColor = true;
            this.ControlButton.Click += new System.EventHandler(this.ControlButton_Click);
            // 
            // Process
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(364, 397);
            this.Controls.Add(this.ControlButton);
            this.Controls.Add(this.ConfigurationGroupBox);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "Process";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Create Process Mode";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ModeForm_FormClosing);
            this.Load += new System.EventHandler(this.ModeForm_Load);
            this.ConfigurationGroupBox.ResumeLayout(false);
            this.ConfigurationGroupBox.PerformLayout();
            this.ProcessGroupBox.ResumeLayout(false);
            this.ProcessGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ConfigurationGroupBox;
        private System.Windows.Forms.Label RemarkLabel;
        private System.Windows.Forms.GroupBox ProcessGroupBox;
        private System.Windows.Forms.ListBox RuleListBox;
        private System.Windows.Forms.TextBox RemarkTextBox;
        private System.Windows.Forms.TextBox ProcessNameTextBox;
        private System.Windows.Forms.Button AddButton;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.Button ControlButton;
        private System.Windows.Forms.Label FilenameLabel;
        private System.Windows.Forms.TextBox FilenameTextBox;
        private System.Windows.Forms.RadioButton StaySameButton;
        private System.Windows.Forms.RadioButton TimeDataButton;
        private System.Windows.Forms.CheckBox UseCustomFilenameBox;
    }
}