﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using TaskScheduler;

namespace Netch.Forms
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        private void TUNTAPUseCustomDNSCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!UseCustomDNSCheckBox.Checked)
            {
                TUNTAPDNSTextBox.Enabled = false;
            }
            else
            {
                TUNTAPDNSTextBox.Enabled = true;
            }

            if (Global.Settings.TUNTAP.DNS.Count > 0)
            {
                var dns = "";
                foreach (var ip in Global.Settings.TUNTAP.DNS)
                {
                    dns += ip;
                    dns += ',';
                }
                dns = dns.Trim();
                TUNTAPDNSTextBox.Text = dns.Substring(0, dns.Length - 1);
            }
            // 如果 DNS 为空，设置为默认 DNS 1.1.1.1
            else
            {
                Global.Settings.TUNTAP.DNS.Add("1.1.1.1");
                TUNTAPDNSTextBox.Text = "1.1.1.1";
            }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            Text = Utils.i18N.Translate(Text);
            PortGroupBox.Text = Utils.i18N.Translate(PortGroupBox.Text);
            AllowDevicesCheckBox.Text = Utils.i18N.Translate(AllowDevicesCheckBox.Text);
            TUNTAPAddressLabel.Text = Utils.i18N.Translate(TUNTAPAddressLabel.Text);
            TUNTAPNetmaskLabel.Text = Utils.i18N.Translate(TUNTAPNetmaskLabel.Text);
            TUNTAPGatewayLabel.Text = Utils.i18N.Translate(TUNTAPGatewayLabel.Text);
            UseCustomDNSCheckBox.Text = Utils.i18N.Translate(UseCustomDNSCheckBox.Text);
            ProxyDNSCheckBox.Text = Utils.i18N.Translate(ProxyDNSCheckBox.Text);
            UseFakeDNSCheckBox.Text = Utils.i18N.Translate(UseFakeDNSCheckBox.Text);
            GlobalBypassIPsButton.Text = Utils.i18N.Translate(GlobalBypassIPsButton.Text);
            ControlButton.Text = Utils.i18N.Translate(ControlButton.Text);
            BootShadowsocksFromDLLCheckBox.Text = Utils.i18N.Translate(BootShadowsocksFromDLLCheckBox.Text);

            ExitWhenClosedCheckBox.Checked = Global.Settings.ExitWhenClosed;
            StopWhenExitedCheckBox.Checked = Global.Settings.StopWhenExited;
            StartWhenOpenedCheckBox.Checked = Global.Settings.StartWhenOpened;
            CheckUpdateWhenOpenedCheckBox.Checked = Global.Settings.CheckUpdateWhenOpened;
            MinimizeWhenStartedCheckBox.Checked = Global.Settings.MinimizeWhenStarted;
            RunAtStartup.Checked = Global.Settings.RunAtStartup;
            EnableStartedTcping_CheckBox.Checked = Global.Settings.StartedTcping;
            DetectionInterval_TextBox.Text = Global.Settings.StartedTcping_Interval.ToString();
            BootShadowsocksFromDLLCheckBox.Checked = Global.Settings.BootShadowsocksFromDLL;

            Socks5PortTextBox.Text = Global.Settings.Socks5LocalPort.ToString();
            HTTPPortTextBox.Text = Global.Settings.HTTPLocalPort.ToString();
            RedirectorTextBox.Text = Global.Settings.RedirectorTCPPort.ToString();

            TUNTAPAddressTextBox.Text = Global.Settings.TUNTAP.Address;
            TUNTAPNetmaskTextBox.Text = Global.Settings.TUNTAP.Netmask;
            TUNTAPGatewayTextBox.Text = Global.Settings.TUNTAP.Gateway;

            UseCustomDNSCheckBox.Checked = Global.Settings.TUNTAP.UseCustomDNS;
            ProxyDNSCheckBox.Checked = Global.Settings.TUNTAP.ProxyDNS;
            UseFakeDNSCheckBox.Checked = Global.Settings.TUNTAP.UseFakeDNS;

            BehaviorGroupBox.Text = Utils.i18N.Translate(BehaviorGroupBox.Text);
            ExitWhenClosedCheckBox.Text = Utils.i18N.Translate(ExitWhenClosedCheckBox.Text);
            StopWhenExitedCheckBox.Text = Utils.i18N.Translate(StopWhenExitedCheckBox.Text);
            StartWhenOpenedCheckBox.Text = Utils.i18N.Translate(StartWhenOpenedCheckBox.Text);
            MinimizeWhenStartedCheckBox.Text = Utils.i18N.Translate(MinimizeWhenStartedCheckBox.Text);
            RunAtStartup.Text = Utils.i18N.Translate(RunAtStartup.Text);
            CheckUpdateWhenOpenedCheckBox.Text = Utils.i18N.Translate(CheckUpdateWhenOpenedCheckBox.Text);
            ProfileCount_Label.Text = Utils.i18N.Translate(ProfileCount_Label.Text);
            DelayTestAfterStartup_Label.Text = Utils.i18N.Translate(DelayTestAfterStartup_Label.Text);
            EnableStartedTcping_CheckBox.Text = Utils.i18N.Translate(EnableStartedTcping_CheckBox.Text);
            DetectionInterval_Label.Text = Utils.i18N.Translate(DetectionInterval_Label.Text);
            DelayTestAfterStartup_Label.Text = Utils.i18N.Translate(DelayTestAfterStartup_Label.Text);
            STUNServerLabel.Text = Utils.i18N.Translate(STUNServerLabel.Text);
            STUNServerPortLabel.Text = Utils.i18N.Translate(STUNServerPortLabel.Text);

            ProfileCount_TextBox.Text = Global.Settings.ProfileCount.ToString();
            STUN_ServerTextBox.Text = Global.Settings.STUN_Server.ToString();
            STUN_ServerPortTextBox.Text = Global.Settings.STUN_Server_Port.ToString();

            AclLabel.Text = Utils.i18N.Translate(AclLabel.Text);
            AclAddr.Text = Global.Settings.ACL.ToString();

            ProcessWhitelistModeCheckbox.Text = Utils.i18N.Translate(ProcessWhitelistModeCheckbox.Text);
            ProcessWhitelistModeCheckbox.Checked = Global.Settings.ProcessWhitelistMode;

            ProcessNoProxyForUdpcheckBox.Checked = Global.Settings.ProcessNoProxyForUdp;
            PrintProxyIPCheckBox.Checked = Global.Settings.ProcessProxyIPLog;

            UDPServerCheckBox.Checked = Global.Settings.UDPServer;
            if (Global.Settings.UDPServer)
            {
                ShowUDPServerComboBox();
            }
            else
            {
                UDPServerComboBox.Visible = false;
            }

            LanguageLabel.Text = Utils.i18N.Translate(LanguageLabel.Text);
            LanguageComboBox.Items.AddRange(Utils.i18N.GetTranslateList().ToArray());
            LanguageComboBox.SelectedItem = Global.Settings.Language;

            if (Global.Settings.TUNTAP.DNS.Count > 0)
            {
                var dns = "";
                foreach (var ip in Global.Settings.TUNTAP.DNS)
                {
                    dns += ip;
                    dns += ',';
                }
                dns = dns.Trim();
                TUNTAPDNSTextBox.Text = dns.Substring(0, dns.Length - 1);
            }
            // 如果 DNS 为空，设置为默认 DNS 1.1.1.1
            else
            {
                Global.Settings.TUNTAP.DNS.Add("1.1.1.1");
                TUNTAPDNSTextBox.Text = "1.1.1.1";
            }

            if (!UseCustomDNSCheckBox.Checked)
            {
                TUNTAPDNSTextBox.Enabled = false;
            }

            // 设置本地代理是否允许其他设备连接
            if (Global.Settings.LocalAddress == "127.0.0.1")
            {
                AllowDevicesCheckBox.Checked = false;
            }
            else if (Global.Settings.LocalAddress == "0.0.0.0")
            {
                AllowDevicesCheckBox.Checked = true;
            }
            else
            {
                Global.Settings.LocalAddress = "127.0.0.1";
                AllowDevicesCheckBox.Checked = false;
            }
        }

        private void SettingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.MainForm.Show();
        }

        private void GlobalBypassIPsButton_Click(object sender, EventArgs e)
        {
            new GlobalBypassIPForm().Show();
            Hide();
        }

        private void ControlButton_Click(object sender, EventArgs e)
        {
            Global.Settings.ExitWhenClosed = ExitWhenClosedCheckBox.Checked;
            Global.Settings.StopWhenExited = StopWhenExitedCheckBox.Checked;
            Global.Settings.StartWhenOpened = StartWhenOpenedCheckBox.Checked;
            Global.Settings.CheckUpdateWhenOpened = CheckUpdateWhenOpenedCheckBox.Checked;
            Global.Settings.MinimizeWhenStarted = MinimizeWhenStartedCheckBox.Checked;
            Global.Settings.RunAtStartup = RunAtStartup.Checked;
            Global.Settings.BootShadowsocksFromDLL = BootShadowsocksFromDLLCheckBox.Checked;
            Global.Settings.ProcessWhitelistMode = ProcessWhitelistModeCheckbox.Checked;
            Global.Settings.UDPServer = UDPServerCheckBox.Checked;
            Global.Settings.UDPServerIndex = UDPServerComboBox.SelectedIndex - 1;
            Global.Settings.Language = LanguageComboBox.SelectedItem.ToString();
            Global.Settings.ProcessNoProxyForUdp = ProcessNoProxyForUdpcheckBox.Checked;
            Global.Settings.ProcessProxyIPLog = PrintProxyIPCheckBox.Checked;

            // 加载系统语言
            if (Global.Settings.Language.Equals("System"))
            {
                // 得到当前线程语言代码
                var culture = CultureInfo.CurrentCulture.Name;

                // 尝试加载内置中文语言
                if (culture == "zh-CN")
                {
                    // 加载语言
                    Utils.i18N.Load(Encoding.UTF8.GetString(Properties.Resources.zh_CN));
                }

                // 从外置文件中加载语言
                if (File.Exists($"i18n\\{culture}"))
                {
                    // 加载语言
                    Utils.i18N.Load(File.ReadAllText($"i18n\\{culture}"));
                }
            }

            if (Global.Settings.Language.Equals("zh-CN"))
            {
                // 加载内置中文
                Utils.i18N.Load(Encoding.UTF8.GetString(Properties.Resources.zh_CN));
            }
            else if (Global.Settings.Language.Equals("en-US"))
            {
                // 加载内置英文
                Utils.i18N.Load(Global.Settings.Language);
            }
            else if (File.Exists($"i18n\\{Global.Settings.Language}"))
            {
                // 从外置文件中加载语言
                Utils.i18N.Load(File.ReadAllText($"i18n\\{Global.Settings.Language}"));
            }

            // 开机自启判断
            TaskSchedulerClass scheduler = new TaskSchedulerClass();
            scheduler.Connect(null, null, null, null);
            ITaskFolder folder = scheduler.GetFolder("\\");
            bool taskIsExists = false;
            try
            {
                folder.GetTask("Netch Startup");
                taskIsExists = true;
            }
            catch (Exception) { }

            if (RunAtStartup.Checked)
            {
                if (taskIsExists)
                    folder.DeleteTask("Netch Startup", 0);

                ITaskDefinition task = scheduler.NewTask(0);
                task.RegistrationInfo.Author = "Netch";
                task.RegistrationInfo.Description = "Netch run at startup.";
                task.Principal.RunLevel = _TASK_RUNLEVEL.TASK_RUNLEVEL_HIGHEST;

                task.Triggers.Create(_TASK_TRIGGER_TYPE2.TASK_TRIGGER_LOGON);
                IExecAction action = (IExecAction)task.Actions.Create(_TASK_ACTION_TYPE.TASK_ACTION_EXEC);
                action.Path = System.Windows.Forms.Application.ExecutablePath;


                task.Settings.ExecutionTimeLimit = "PT0S";
                task.Settings.DisallowStartIfOnBatteries = false;
                task.Settings.RunOnlyIfIdle = false;

                folder.RegisterTaskDefinition("Netch Startup", task, (int)_TASK_CREATION.TASK_CREATE, null, null, _TASK_LOGON_TYPE.TASK_LOGON_INTERACTIVE_TOKEN, "");
            }
            else
            {
                if (taskIsExists)
                    folder.DeleteTask("Netch Startup", 0);
            }

            try
            {
                var Socks5Port = int.Parse(Socks5PortTextBox.Text);

                if (Socks5Port > 0 && Socks5Port < 65536)
                {
                    Global.Settings.Socks5LocalPort = Socks5Port;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                Socks5PortTextBox.Text = Global.Settings.Socks5LocalPort.ToString();
                MessageBox.Show(Utils.i18N.Translate("Port value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            try
            {
                var HTTPPort = int.Parse(HTTPPortTextBox.Text);

                if (HTTPPort > 0 && HTTPPort < 65536)
                {
                    Global.Settings.HTTPLocalPort = HTTPPort;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                HTTPPortTextBox.Text = Global.Settings.HTTPLocalPort.ToString();
                MessageBox.Show(Utils.i18N.Translate("Port value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            try
            {
                var RedirectorPort = int.Parse(RedirectorTextBox.Text);

                if (RedirectorPort > 0 && RedirectorPort < 65536)
                {
                    Global.Settings.RedirectorTCPPort = RedirectorPort;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                RedirectorTextBox.Text = Global.Settings.RedirectorTCPPort.ToString();
                MessageBox.Show(Utils.i18N.Translate("Port value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            if (AllowDevicesCheckBox.Checked)
            {
                Global.Settings.LocalAddress = "0.0.0.0";
            }
            else
            {
                Global.Settings.LocalAddress = "127.0.0.1";
            }

            try
            {
                var Address = IPAddress.Parse(TUNTAPAddressTextBox.Text);
                var Netmask = IPAddress.Parse(TUNTAPNetmaskTextBox.Text);
                var Gateway = IPAddress.Parse(TUNTAPGatewayTextBox.Text);

                var DNS = new System.Collections.Generic.List<IPAddress>();
                foreach (var ip in TUNTAPDNSTextBox.Text.Split(','))
                {
                    DNS.Add(IPAddress.Parse(ip));
                }
            }
            catch (FormatException)
            {
                MessageBox.Show(Utils.i18N.Translate("IP address format illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                TUNTAPAddressTextBox.Text = Global.Settings.TUNTAP.Address;
                TUNTAPNetmaskTextBox.Text = Global.Settings.TUNTAP.Netmask;
                TUNTAPGatewayTextBox.Text = Global.Settings.TUNTAP.Gateway;

                var DNS = "";
                foreach (var ip in Global.Settings.TUNTAP.DNS)
                {
                    DNS += ip;
                    DNS += ',';
                }
                DNS = DNS.Trim();
                TUNTAPDNSTextBox.Text = DNS.Substring(0, DNS.Length - 1);
                UseCustomDNSCheckBox.Checked = Global.Settings.TUNTAP.UseCustomDNS;

                return;
            }
            try
            {
                var ProfileCount = int.Parse(ProfileCount_TextBox.Text);

                if (ProfileCount > -1)
                {
                    Global.Settings.ProfileCount = ProfileCount;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                ProfileCount_TextBox.Text = Global.Settings.ProfileCount.ToString();
                MessageBox.Show(Utils.i18N.Translate("ProfileCount value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }
            try
            {
                var STUN_Server = STUN_ServerTextBox.Text;
                Global.Settings.STUN_Server = STUN_Server;

                var STUN_ServerPort = int.Parse(STUN_ServerPortTextBox.Text);

                if (STUN_ServerPort > 0)
                {
                    Global.Settings.STUN_Server_Port = STUN_ServerPort;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                ProfileCount_TextBox.Text = Global.Settings.ProfileCount.ToString();
                MessageBox.Show(Utils.i18N.Translate("STUN_ServerPort value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }
            try
            {
                Global.Settings.StartedTcping = EnableStartedTcping_CheckBox.Checked;

                var DetectionInterval = int.Parse(DetectionInterval_TextBox.Text);

                if (DetectionInterval > 0)
                {
                    Global.Settings.StartedTcping_Interval = DetectionInterval;
                }
                else
                {
                    throw new FormatException();
                }
            }
            catch (FormatException)
            {
                ProfileCount_TextBox.Text = Global.Settings.ProfileCount.ToString();
                MessageBox.Show(Utils.i18N.Translate("Detection interval value illegal. Try again."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            Global.Settings.ACL = AclAddr.Text;

            Global.Settings.TUNTAP.Address = TUNTAPAddressTextBox.Text;
            Global.Settings.TUNTAP.Netmask = TUNTAPNetmaskTextBox.Text;
            Global.Settings.TUNTAP.Gateway = TUNTAPGatewayTextBox.Text;

            Global.Settings.TUNTAP.DNS.Clear();
            foreach (var ip in TUNTAPDNSTextBox.Text.Split(','))
            {
                Global.Settings.TUNTAP.DNS.Add(ip);
            }

            Global.Settings.TUNTAP.UseCustomDNS = UseCustomDNSCheckBox.Checked;
            Global.Settings.TUNTAP.ProxyDNS = ProxyDNSCheckBox.Checked;
            Global.Settings.TUNTAP.UseFakeDNS = UseFakeDNSCheckBox.Checked;

            Utils.Configuration.Save();
            MessageBox.Show(Utils.i18N.Translate("Saved."), Utils.i18N.Translate("Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }

        private void UDPServerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UDPServerComboBox.Visible = UDPServerCheckBox.Checked;
            if (UDPServerCheckBox.Checked)
            {
                ShowUDPServerComboBox();
            }
        }
        private void ShowUDPServerComboBox()
        {

            UDPServerComboBox.Items.Clear();
            UDPServerComboBox.Items.Add(new Models.Server()
            {
                Hostname = "FakeServer",
                Type = "FakeServer"
            });
            UDPServerComboBox.Items.AddRange(Global.Settings.Server.ToArray());
            UDPServerComboBox.SelectedIndex = Global.Settings.UDPServerIndex + 1;
        }
    }
}
