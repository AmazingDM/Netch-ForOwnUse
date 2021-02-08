using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using Netch.Forms;
using Netch.Models;
using Netch.Servers.Shadowsocks;
using Netch.Servers.Socks5;
using Netch.Utils;
using nfapinet;

namespace Netch.Controllers
{
    public class NFController : IModeController
    {
        private static readonly ServiceController NFService = new("netfilter2");

        private static readonly string BinDriver = string.Empty;
        private static readonly string SystemDriver = $"{Environment.SystemDirectory}\\drivers\\netfilter2.sys";
        private static string _sysDns;

        static NFController()
        {
            string fileName;
            switch ($"{Environment.OSVersion.Version.Major}.{Environment.OSVersion.Version.Minor}")
            {
                case "10.0":
                    fileName = "Win-10.sys";
                    break;
                case "6.3":
                case "6.2":
                    fileName = "Win-8.sys";
                    break;
                case "6.1":
                case "6.0":
                    fileName = "Win-7.sys";
                    break;
                default:
                    Logging.Error($"不支持的系统版本：{Environment.OSVersion.Version}");
                    return;
            }

            BinDriver = "bin\\" + fileName;
        }

        public string Name { get; } = "Redirector";

        public bool Start(in Mode mode)
        {
            if (!CheckDriver())
                return false;

            #region aio_dial

            if (Global.Settings.ProcessNoProxyForUdp && Global.Settings.ProcessNoProxyForTcp) MessageBoxX.Show("？");

            aio_dial((int)NameList.TYPE_FILTERLOOPBACK, "false");

            //UDP
            if (Global.Settings.ProcessNoProxyForUdp)
            {
                aio_dial((int)NameList.TYPE_FILTERUDP, "false");
            }
            else
            {
                aio_dial((int)NameList.TYPE_FILTERUDP, "true");
            }
            //TCP
            if (Global.Settings.ProcessNoProxyForTcp)
            {
                aio_dial((int)NameList.TYPE_FILTERTCP, "false");
            }
            else
            {
                aio_dial((int)NameList.TYPE_FILTERTCP, "true");
            }

            SetServer();

            if (!CheckRule(mode.FullRule, out var list))
            {
                MessageBoxX.Show($"\"{string.Join("", list.Select(s => s + "\n"))}\" does not conform to C++ regular expression syntax");
                return false;
            }

            SetName(mode);
            SetFip(mode);

            #endregion

            if (Global.Settings.ModifySystemDNS)
            {
                // 备份并替换系统 DNS
                _sysDns = DNS.OutboundDNS;
                if (string.IsNullOrWhiteSpace(Global.Settings.ModifiedDNS))
                    Global.Settings.ModifiedDNS = "1.1.1.1,8.8.8.8";
                DNS.OutboundDNS = Global.Settings.ModifiedDNS;
            }

            return aio_init();
        }

        public void Stop()
        {
            Task.Run(() =>
            {
                MainController.UdpServerController.Stop();
                if (Global.Settings.ModifySystemDNS)
                    //恢复系统DNS
                    DNS.OutboundDNS = _sysDns;
            });

            aio_free();
        }

        /// <summary>
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="incompatibleRule"></param>
        /// <returns>No Problem true</returns>
        public static bool CheckRule(IEnumerable<string> rules, out IEnumerable<string> incompatibleRule)
        {
            incompatibleRule = rules.Where(r => !CheckCppRegex(r, false));
            aio_dial((int)NameList.TYPE_CLRNAME, "");
            return !incompatibleRule.Any();
        }

        /// <summary>
        /// </summary>
        /// <param name="r"></param>
        /// <param name="clear"></param>
        /// <returns>No Problem true</returns>
        public static bool CheckCppRegex(string r, bool clear = true)
        {
            try
            {
                if (r.StartsWith("!"))
                    return aio_dial((int)NameList.TYPE_ADDNAME, r.Substring(1));
                return aio_dial((int)NameList.TYPE_ADDNAME, r);
            }
            finally
            {
                if (clear)
                    aio_dial((int)NameList.TYPE_CLRNAME, "");
            }
        }

        private static bool CheckDriver()
        {
            var binFileVersion = Utils.Utils.GetFileVersion(BinDriver);
            var systemFileVersion = Utils.Utils.GetFileVersion(SystemDriver);

            Logging.Info("内置驱动版本: " + binFileVersion);
            Logging.Info("系统驱动版本: " + systemFileVersion);

            if (!File.Exists(BinDriver))
            {
                Logging.Warning("内置驱动不存在");
                if (File.Exists(SystemDriver))
                {
                    Logging.Warning("使用系统驱动");
                    return true;
                }

                Logging.Error("未安装驱动");
                return false;
            }

            if (!File.Exists(SystemDriver))
                return InstallDriver();

            var updateFlag = false;

            if (Version.TryParse(binFileVersion, out var binResult) && Version.TryParse(systemFileVersion, out var systemResult))
            {
                if (binResult.CompareTo(systemResult) > 0)
                {
                    // Bin greater than Installed
                    updateFlag = true;
                }
                else
                {
                    // Installed greater than Bin
                    if (systemResult.Major != binResult.Major)
                        // API breaking changes
                        updateFlag = true;
                }
            }
            else
            {
                if (!systemFileVersion.Equals(binFileVersion))
                    updateFlag = true;
            }

            if (!updateFlag) return true;

            Logging.Info("更新驱动");
            UninstallDriver();
            return InstallDriver();
        }

        private void SetServer()
        {
            aio_dial((int)NameList.TYPE_TCPLISN, Global.Settings.RedirectorTCPPort.ToString());

            Server server = MainController.Server;
            IServerController controller = MainController.ServerController;

            if (server is Socks5 socks5)
            {
                aio_dial((int)NameList.TYPE_TCPTYPE, "Socks5");
                aio_dial((int)NameList.TYPE_TCPHOST, $"{socks5.AutoResolveHostname()}:{socks5.Port}");
                aio_dial((int)NameList.TYPE_TCPUSER, socks5.Username ?? string.Empty);
                aio_dial((int)NameList.TYPE_TCPPASS, socks5.Password ?? string.Empty);
                aio_dial((int)NameList.TYPE_TCPMETH, string.Empty);

                aio_dial((int)NameList.TYPE_UDPTYPE, "Socks5");
                aio_dial((int)NameList.TYPE_UDPHOST, $"{socks5.AutoResolveHostname()}:{socks5.Port}");
                aio_dial((int)NameList.TYPE_UDPUSER, socks5.Username ?? string.Empty);
                aio_dial((int)NameList.TYPE_UDPPASS, socks5.Password ?? string.Empty);
                aio_dial((int)NameList.TYPE_UDPMETH, string.Empty);
            }
            else if (server is Shadowsocks shadowsocks && !shadowsocks.HasPlugin() && Global.Settings.RedirectorSS)
            {
                aio_dial((int)NameList.TYPE_TCPTYPE, "Shadowsocks");
                aio_dial((int)NameList.TYPE_TCPHOST, $"{shadowsocks.AutoResolveHostname()}:{shadowsocks.Port}");
                aio_dial((int)NameList.TYPE_TCPMETH, shadowsocks.EncryptMethod ?? string.Empty);
                aio_dial((int)NameList.TYPE_TCPPASS, shadowsocks.Password ?? string.Empty);

                aio_dial((int)NameList.TYPE_UDPTYPE, "Shadowsocks");
                aio_dial((int)NameList.TYPE_UDPHOST, $"{shadowsocks.AutoResolveHostname()}:{shadowsocks.Port}");
                aio_dial((int)NameList.TYPE_UDPMETH, shadowsocks.EncryptMethod ?? string.Empty);
                aio_dial((int)NameList.TYPE_UDPPASS, shadowsocks.Password ?? string.Empty);
            }
            else
            {
                aio_dial((int)NameList.TYPE_TCPTYPE, "Socks5");
                aio_dial((int)NameList.TYPE_TCPHOST, $"127.0.0.1:{controller.Socks5LocalPort()}");
                aio_dial((int)NameList.TYPE_TCPUSER, string.Empty);
                aio_dial((int)NameList.TYPE_TCPPASS, string.Empty);
                aio_dial((int)NameList.TYPE_TCPMETH, string.Empty);

                aio_dial((int)NameList.TYPE_UDPTYPE, "Socks5");
                aio_dial((int)NameList.TYPE_UDPHOST, $"127.0.0.1:{controller.Socks5LocalPort()}");
                aio_dial((int)NameList.TYPE_UDPUSER, string.Empty);
                aio_dial((int)NameList.TYPE_UDPPASS, string.Empty);
                aio_dial((int)NameList.TYPE_UDPMETH, string.Empty);
            }

            //UDP 分流
            if (!Global.Settings.ProcessNoProxyForUdp && Global.Settings.UDPServer && Global.Settings.UDPServerIndex > -1)
            {
                Server udpServer = Global.Settings.Server[Global.Settings.UDPServerIndex];
                MainController.UdpServerController = ServerHelper.GetUtilByTypeName(udpServer.Type).GetController();
                MainController.UdpServerController.Socks5LocalPort = Convert.ToUInt16(Utils.Utils.GetRandomPort());

                Mode udpMode = new Mode();
                udpMode.Type = -1;
                udpMode.BypassChina = false;
                MainController.UdpServerController.Start(udpServer, udpMode);

                aio_dial((int)NameList.TYPE_UDPTYPE, "Socks5");
                aio_dial((int)NameList.TYPE_UDPHOST, $"127.0.0.1:{MainController.UdpServerController.Socks5LocalPort}");
                aio_dial((int)NameList.TYPE_UDPUSER, string.Empty);
                aio_dial((int)NameList.TYPE_UDPPASS, string.Empty);
                aio_dial((int)NameList.TYPE_UDPMETH, string.Empty);
            }

        }

        private void SetName(Mode mode)
        {
            aio_dial((int)NameList.TYPE_CLRNAME, "");
            foreach (var rule in mode.FullRule)
            {
                if (rule.StartsWith("!"))
                {
                    aio_dial((int)NameList.TYPE_BYPNAME, rule.Substring(1));
                }

                aio_dial((int)NameList.TYPE_ADDNAME, rule);
            }

            aio_dial((int)NameList.TYPE_ADDNAME, @"NTT.exe");
        }

        private void SetFip(Mode mode)
        {
            aio_dial((int)NameList.TYPE_CLRFIP, "");
            if (mode.ProcesssIPFillter)
            {
                aio_dial((int)NameList.TYPE_FILTERIP, "true");
            }
            else
            {
                aio_dial((int)NameList.TYPE_FILTERIP, "false");

                if (Global.Settings.STUN_Server == "stun.stunprotocol.org")
                    aio_dial((int)NameList.TYPE_ADDFIP, Dns.GetHostAddresses(Global.Settings.STUN_Server)[0].ToString());
            }

            foreach (var rule in mode.FullRule)
            {
                aio_dial((int)NameList.TYPE_ADDFIP, rule);
            }
        }

        #region NativeMethods

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool aio_dial(int name, [MarshalAs(UnmanagedType.LPWStr)] string value);

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool aio_init();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool aio_free();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong aio_getUP();

        [DllImport("Redirector.bin", CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong aio_getDL();


        public enum NameList
        {
            TYPE_FILTERLOOPBACK,
            TYPE_FILTERTCP,
            TYPE_FILTERUDP,
            TYPE_FILTERIP,

            TYPE_TCPLISN,
            TYPE_TCPTYPE,
            TYPE_TCPHOST,
            TYPE_TCPUSER,
            TYPE_TCPPASS,
            TYPE_TCPMETH,

            TYPE_UDPTYPE,
            TYPE_UDPHOST,
            TYPE_UDPUSER,
            TYPE_UDPPASS,
            TYPE_UDPMETH,

            TYPE_ADDNAME,
            TYPE_ADDFIP,

            TYPE_BYPNAME,

            TYPE_CLRNAME,
            TYPE_CLRFIP,

            TYPE_REDIRCTOR_DNS
        }

        #endregion

        #region Utils

        /// <summary>
        ///     安装 NF 驱动
        /// </summary>
        /// <returns>驱动是否安装成功</returns>
        public static bool InstallDriver()
        {
            Logging.Info("安装 NF 驱动");
            try
            {
                File.Copy(BinDriver, SystemDriver);
            }
            catch (Exception e)
            {
                Logging.Error("驱动复制失败\n" + e);
                return false;
            }

            Global.MainForm.StatusText(i18N.Translate("Register driver"));
            // 注册驱动文件
            var result = NFAPI.nf_registerDriver("netfilter2");
            if (result == NF_STATUS.NF_STATUS_SUCCESS)
            {
                Logging.Info("驱动安装成功");
            }
            else
            {
                Logging.Error($"注册驱动失败，返回值：{result}");
                return false;
            }

            return true;
        }

        /// <summary>
        ///     卸载 NF 驱动
        /// </summary>
        /// <returns>是否成功卸载</returns>
        public static bool UninstallDriver()
        {
            Logging.Info("卸载 NF 驱动");
            try
            {
                if (NFService.Status == ServiceControllerStatus.Running)
                {
                    NFService.Stop();
                    NFService.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (!File.Exists(SystemDriver)) return true;
            NFAPI.nf_unRegisterDriver("netfilter2");
            File.Delete(SystemDriver);

            return true;
        }

        #endregion
    }
}