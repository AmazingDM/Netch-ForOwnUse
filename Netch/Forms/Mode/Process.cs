﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using Netch.Utils;

namespace Netch.Forms.Mode
{
    public partial class Process : Form
    {
        //用于判断当前窗口是否为编辑模式
        private bool EditMode;
        //被编辑模式坐标
        private Models.Mode EditMode_Old;
        /// <summary>
		///		编辑模式
		/// </summary>
		/// <param name="mode">模式</param>
        public Process(Models.Mode mode)
        {

            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            EditMode_Old = mode;
            Text = "Edit Process Mode";
            //循环填充已有规则
            mode.Rule.ForEach(i => RuleListBox.Items.Add(i));

            EditMode = true;
            StaySameButton.Enabled = false;
            TimeDataButton.Enabled = false;
            FilenameTextBox.Enabled = false;
            FilenameLabel.Enabled = false;
            UseCustomFilenameBox.Enabled = false;

            FilenameTextBox.Text = mode.FileName;
            RemarkTextBox.Text = mode.Remark;

        }
        public Process()
        {
            InitializeComponent();

            CheckForIllegalCrossThreadCalls = false;

            EditMode = false;
            EditMode_Old = null;
        }

        /// <summary>
		///		扫描目录
		/// </summary>
		/// <param name="DirName">路径</param>
		public void ScanDirectory(string DirName)
        {
            try
            {
                var RDirInfo = new DirectoryInfo(DirName);
                if (!RDirInfo.Exists)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            var DirStack = new Stack<string>();
            DirStack.Push(DirName);

            while (DirStack.Count > 0)
            {
                var DirInfo = new DirectoryInfo(DirStack.Pop());
                foreach (var DirChildInfo in DirInfo.GetDirectories())
                {
                    DirStack.Push(DirChildInfo.FullName);
                }
                foreach (var FileChildInfo in DirInfo.GetFiles())
                {
                    if (FileChildInfo.Name.EndsWith(".exe") && !RuleListBox.Items.Contains(FileChildInfo.Name))
                    {
                        RuleListBox.Items.Add(FileChildInfo.Name);
                    }
                }
            }
        }

        private void ModeForm_Load(object sender, EventArgs e)
        {
            Text = i18N.Translate(Text);
            ConfigurationGroupBox.Text = i18N.Translate(ConfigurationGroupBox.Text);
            RemarkLabel.Text = i18N.Translate(RemarkLabel.Text);
            FilenameLabel.Text = i18N.Translate(FilenameLabel.Text);
            UseCustomFilenameBox.Text = i18N.Translate(UseCustomFilenameBox.Text);
            StaySameButton.Text = i18N.Translate(StaySameButton.Text);
            TimeDataButton.Text = i18N.Translate(TimeDataButton.Text);
            AddButton.Text = i18N.Translate(AddButton.Text);
            ScanButton.Text = i18N.Translate(ScanButton.Text);
            ControlButton.Text = i18N.Translate(ControlButton.Text);

            if (Global.Settings.ModeFileNameType == 0)
            {
                UseCustomFilenameBox.Checked = true;
                StaySameButton.Enabled = false;
                TimeDataButton.Enabled = false;
            }
            else if (Global.Settings.ModeFileNameType == 1)
            {
                FilenameTextBox.Enabled = false;
                FilenameLabel.Enabled = false;
                StaySameButton.Checked = true;
            }
            else
            {
                FilenameTextBox.Enabled = false;
                FilenameLabel.Enabled = false;
                TimeDataButton.Checked = true;
            }
        }

        private void ModeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.MainForm.Show();
        }

        /// <summary>
        /// listBox右键菜单
        /// </summary>
        private void RuleListBox_MouseUp(object sender, MouseEventArgs e)
        {
            var strip = new ContextMenuStrip();
            strip.Items.Add(i18N.Translate("Delete"));
            if (e.Button == MouseButtons.Right)
            {
                strip.Show(RuleListBox, e.Location);//鼠标右键按下弹出菜单
                strip.MouseClick += deleteRule_Click;
            }
        }
        void deleteRule_Click(object sender, EventArgs e)
        {
            if (RuleListBox.SelectedIndex != -1)
            {
                RuleListBox.Items.RemoveAt(RuleListBox.SelectedIndex);
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ProcessNameTextBox.Text))
            {
                var process = ProcessNameTextBox.Text;
                if (!process.EndsWith(".exe"))
                {
                    process += ".exe";
                }

                if (!RuleListBox.Items.Contains(process))
                {
                    RuleListBox.Items.Add(process);
                }

                ProcessNameTextBox.Text = string.Empty;
            }
            else
            {
                MessageBoxX.Show(i18N.Translate("Please enter an process name (xxx.exe)"));
            }
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = i18N.Translate("Select a folder"),
                AddToMostRecentlyUsedList = false,
                EnsurePathExists = true,
                NavigateToShortcut = true
            };
            if (dialog.ShowDialog(Win32Native.GetForegroundWindow()) == CommonFileDialogResult.Ok)
            {
                ScanDirectory(dialog.FileName);
                MessageBoxX.Show(i18N.Translate("Scan completed"));
            }
        }

        private void ControlButton_Click(object sender, EventArgs e)
        {
            if (EditMode)
            {
                // 编辑模式

                if (RuleListBox.Items.Count != 0)
                {
                    var mode = new Models.Mode
                    {
                        BypassChina = false,
                        FileName = FilenameTextBox.Text,
                        Type = 0,
                        Remark = RemarkTextBox.Text
                    };

                    var text = $"# {RemarkTextBox.Text}, 0\r\n";
                    foreach (var item in RuleListBox.Items)
                    {
                        var process = item as string;
                        mode.Rule.Add(process);
                        text += process + "\r\n";
                    }

                    text = text.Substring(0, text.Length - 2);

                    if (!Directory.Exists("mode"))
                    {
                        Directory.CreateDirectory("mode");
                    }

                    File.WriteAllText(Path.Combine("mode", FilenameTextBox.Text) + ".txt", text);

                    MessageBoxX.Show(i18N.Translate("Mode updated successfully"));

                    Global.MainForm.UpdateMode(mode, EditMode_Old);
                    Close();
                }
                else
                {
                    MessageBoxX.Show(i18N.Translate("Unable to add empty rule"));
                }
            }
            else
            {
                // 自定义文件名
                if (UseCustomFilenameBox.Checked)
                {
                    Global.Settings.ModeFileNameType = 0;
                }
                // 使用和备注一致的文件名
                else if (StaySameButton.Checked)
                {
                    Global.Settings.ModeFileNameType = 1;
                    FilenameTextBox.Text = RemarkTextBox.Text;
                }
                // 使用时间数据作为文件名
                else
                {
                    Global.Settings.ModeFileNameType = 2;
                    FilenameTextBox.Text = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
                }

                Configuration.Save();

                if (!string.IsNullOrWhiteSpace(RemarkTextBox.Text))
                {
                    if (Global.Settings.ModeFileNameType == 0 && string.IsNullOrWhiteSpace(FilenameTextBox.Text))
                    {
                        MessageBoxX.Show(i18N.Translate("Please enter a mode filename"));
                        return;
                    }
                    var ModeFilename = Path.Combine("mode", FilenameTextBox.Text);

                    // 如果文件已存在，返回
                    if (File.Exists(ModeFilename + ".txt"))
                    {
                        MessageBoxX.Show(i18N.Translate("File already exists.\n Please Change the filename"));
                        return;
                    }

                    if (RuleListBox.Items.Count != 0)
                    {
                        var mode = new Models.Mode
                        {
                            BypassChina = false,
                            FileName = FilenameTextBox.Text,
                            Type = 0,
                            Remark = RemarkTextBox.Text
                        };

                        var text = $"# {RemarkTextBox.Text}, 0\r\n";
                        foreach (var item in RuleListBox.Items)
                        {
                            var process = item as string;
                            mode.Rule.Add(process);
                            text += process + "\r\n";
                        }

                        text = text.Substring(0, text.Length - 2);

                        if (!Directory.Exists("mode"))
                        {
                            Directory.CreateDirectory("mode");
                        }

                        File.WriteAllText(ModeFilename + ".txt", text);

                        MessageBoxX.Show(i18N.Translate("Mode added successfully"));

                        Global.MainForm.AddMode(mode);
                        Close();
                    }
                    else
                    {
                        MessageBoxX.Show(i18N.Translate("Unable to add empty rule"));
                    }
                }
                else
                {
                    MessageBoxX.Show(i18N.Translate("Please enter a mode remark"));
                }
            }
        }

        private void UseCustomFileNameBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UseCustomFilenameBox.Checked)
            {
                StaySameButton.Enabled = false;
                TimeDataButton.Enabled = false;
                FilenameTextBox.Enabled = true;
                FilenameLabel.Enabled = true;
            }
            else
            {
                StaySameButton.Enabled = true;
                TimeDataButton.Enabled = true;
                FilenameTextBox.Enabled = false;
                FilenameLabel.Enabled = false;
            }
        }
    }
}
