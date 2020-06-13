using Netch.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace Netch.Controllers
{
    public class NFController
    {
        /// <summary>
        ///     流量变动事件
        /// </summary>
        public event BandwidthUpdateHandler OnBandwidthUpdated;

        /// <summary>
        ///     流量变动处理器
        /// </summary>
        /// <param name="upload">上传</param>
        /// <param name="download">下载</param>
        public delegate void BandwidthUpdateHandler(long upload, long download);

        /// <summary>
        ///     进程实例
        /// </summary>
        public Process Instance;

        /// <summary>
        ///     UDP代理进程实例
        /// </summary>
        public Process UDPServerInstance;

        /// <summary>
        ///     当前状态
        /// </summary>
        public Models.State State = Models.State.Waiting;

        // 生成驱动文件路径
        public string driverPath = string.Format("{0}\\drivers\\netfilter2.sys", Environment.SystemDirectory);

        /// <summary>
        ///		启动
        /// </summary>
        /// <param name="server">服务器</param>
        /// <param name="mode">模式</param>
        /// <param name="StopServiceAndRestart">先停止驱动服务再重新启动</param>
        /// <returns>是否成功</returns>
        public bool Start(Models.Server server, Models.Mode mode, bool StopServiceAndRestart)
        {
            if (!StopServiceAndRestart)
                MainForm.Instance.StatusText($"{Utils.i18N.Translate("Status")}{Utils.i18N.Translate(": ")}{Utils.i18N.Translate("Starting Redirector")}");

            if (!File.Exists("bin\\Redirector.exe"))
            {
                return false;
            }

            // 检查驱动是否存在
            if (File.Exists(driverPath))
            {
                // 生成系统版本
                var version = $"{Environment.OSVersion.Version.Major.ToString()}.{Environment.OSVersion.Version.Minor.ToString()}";
                var driverName = "";

                switch (version)
                {
                    case "10.0":
                        driverName = "Win-10.sys";
                        break;
                    case "6.3":
                    case "6.2":
                        driverName = "Win-8.sys";
                        break;
                    case "6.1":
                    case "6.0":
                        driverName = "Win-7.sys";
                        break;
                    default:
                        Utils.Logging.Info($"不支持的系统版本：{version}");
                        return false;
                }

                // 检查驱动版本号
                FileVersionInfo SystemfileVerInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(driverPath);
                FileVersionInfo BinFileVerInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(string.Format("bin\\{0}", driverName));

                if (!SystemfileVerInfo.FileVersion.Equals(BinFileVerInfo.FileVersion))
                {
                    Utils.Logging.Info("开始更新驱动");
                    // 需要更新驱动
                    try
                    {
                        var service = new ServiceController("netfilter2");
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            service.Stop();
                            service.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                        nfapinet.NFAPI.nf_unRegisterDriver("netfilter2");

                        //删除老驱动
                        File.Delete(driverPath);
                        if (!InstallDriver())
                            return false;

                        Utils.Logging.Info($"驱动更新完毕，当前驱动版本:{BinFileVerInfo.FileVersion}");
                    }
                    catch (Exception)
                    {
                        Utils.Logging.Info($"更新驱动出错");
                    }

                }

            }
            else
            {
                if (!InstallDriver())
                {
                    return false;
                }
            }

            try
            {
                // 启动驱动服务
                var service = new ServiceController("netfilter2");
                if (service.Status == ServiceControllerStatus.Running && StopServiceAndRestart)
                {
                    // 防止其他程序占用 重置 NF 百万连接数限制
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    MainForm.Instance.StatusText($"{Utils.i18N.Translate("Status")}{Utils.i18N.Translate(": ")}{Utils.i18N.Translate("Starting netfilter2 Service")}");
                    service.Start();
                }
                else if (service.Status == ServiceControllerStatus.Stopped)
                {
                    MainForm.Instance.StatusText($"{Utils.i18N.Translate("Status")}{Utils.i18N.Translate(": ")}{Utils.i18N.Translate("Starting netfilter2 Service")}");
                    service.Start();
                }
            }
            catch (Exception e)
            {
                Utils.Logging.Info(e.ToString());

                var result = nfapinet.NFAPI.nf_registerDriver("netfilter2");
                if (result != nfapinet.NF_STATUS.NF_STATUS_SUCCESS)
                {
                    Utils.Logging.Info($"注册驱动失败，返回值：{result}");
                    return false;
                }
            }

            //代理进程
            var processes = "";
            //IP过滤
            var processesIPFillter = "";

            //开启进程白名单模式
            if (!Global.Settings.ProcessWhitelistMode)
            {
                processes += "NTT.exe,";
            }

            foreach (var proc in mode.Rule)
            {
                //添加进程代理
                if (proc.EndsWith(".exe"))
                {
                    processes += proc;
                    processes += ",";
                }
                else
                {
                    //添加IP过滤器
                    processesIPFillter += proc;
                    processesIPFillter += ",";
                }
            }
            processes = processes.Substring(0, processes.Length - 1);

            Instance = MainController.GetProcess();
            var fallback = "";


            if (!File.Exists("bin\\Redirector.exe"))
            {
                return false;
            }
            Instance.StartInfo.FileName = "bin\\Redirector.exe";

            if (server.Type != "Socks5")
            {
                fallback += $"-rtcp 127.0.0.1:{Global.Settings.Socks5LocalPort}";

                fallback = StartUDPServer(fallback);
            }
            else
            {
                var result = Utils.DNS.Lookup(server.Hostname);
                if (result == null)
                {
                    Utils.Logging.Info("无法解析服务器 IP 地址");
                    return false;
                }

                fallback += $"-rtcp {result}:{server.Port}";

                if (!string.IsNullOrWhiteSpace(server.Username) && !string.IsNullOrWhiteSpace(server.Password))
                {
                    fallback += $" -username \"{server.Username}\" -password \"{server.Password}\"";
                }

                if (Global.Settings.UDPServer)
                {
                    if (Global.Settings.UDPServerIndex == -1)
                    {
                        fallback += $" -rudp {result}:{server.Port}";
                    }
                    else
                    {
                        fallback = StartUDPServer(fallback);
                    }
                }
                else
                {
                    fallback += $" -rudp {result}:{server.Port}";
                }
            }

            //开启进程白名单模式
            if (Global.Settings.ProcessWhitelistMode)
            {
                processes += ",netch.exe";
                processes += ",Shadowsocks.exe,simple-obfs.exe";
                processes += ",ShadowsocksR.exe";
                processes += ",Privoxy.exe";
                processes += ",Trojan.exe";
                processes += ",v2ray.exe,v2ctl.exe,v2ray-plugin.exe";
                fallback += " -bypass true ";
            }
            else
            {
                fallback += " -bypass false";
            }

            fallback += $" -p \"{processes}\"";
            fallback += $" -fip \"{processesIPFillter}\"";

            // true  除规则内IP全走代理
            // false 仅代理规则内IP
            if (processesIPFillter.Length > 0)
            {
                if (mode.ProcesssIPFillter)
                {
                    fallback += $" -bypassip true";
                }
                else
                {
                    fallback += $" -bypassip false";
                }
            }
            else
            {
                fallback += $" -bypassip true";
            }

            //进程模式代理IP日志打印
            if (Global.Settings.ProcessProxyIPLog)
            {
                fallback += " -printProxyIP true";
            }
            else
            {
                fallback += " -printProxyIP false";
            }

            if (!Global.Settings.ProcessNoProxyForUdp)
            {
                //开启进程UDP代理
                fallback += " -udpEnable true";
            }
            else
            {
                fallback += " -udpEnable false";
            }

            Utils.Logging.Info($"Redirector : {fallback}");

            if (File.Exists("logging\\redirector.log"))
                File.Delete("logging\\redirector.log");

            Instance.StartInfo.Arguments = fallback;
            State = Models.State.Starting;
            Instance.Start();
            Instance.BeginOutputReadLine();
            Instance.BeginErrorReadLine();

            for (var i = 0; i < 1000; i++)
            {
                try
                {
                    if (File.Exists("logging\\redirector.log"))
                    {
                        FileStream fs = new FileStream("logging\\redirector.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);

                        if (sr.ReadToEnd().Contains("Redirect TCP to"))
                        {
                            State = Models.State.Started;
                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.Logging.Info(e.Message);
                }
                finally
                {
                    Thread.Sleep(10);
                }
            }

            Utils.Logging.Info("NF 进程启动超时");
            Stop();
            return false;
        }

        /// <summary>
        ///		停止
        /// </summary>
        public void Stop()
        {
            try
            {
                if (Instance != null && !Instance.HasExited)
                {
                    Instance.Kill();
                    Instance.WaitForExit();
                }
                if (UDPServerInstance != null && !UDPServerInstance.HasExited)
                {
                    UDPServerInstance.Kill();
                    UDPServerInstance.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Utils.Logging.Info(e.ToString());
            }
        }
        public bool InstallDriver()
        {

            Utils.Logging.Info("安装驱动中");
            // 生成系统版本
            var version = $"{Environment.OSVersion.Version.Major.ToString()}.{Environment.OSVersion.Version.Minor.ToString()}";

            // 检查系统版本并复制对应驱动
            try
            {
                switch (version)
                {
                    case "10.0":
                        File.Copy("bin\\Win-10.sys", driverPath);
                        Utils.Logging.Info("已复制 Win10 驱动");
                        break;
                    case "6.3":
                    case "6.2":
                        File.Copy("bin\\Win-8.sys", driverPath);
                        Utils.Logging.Info("已复制 Win8 驱动");
                        break;
                    case "6.1":
                    case "6.0":
                        File.Copy("bin\\Win-7.sys", driverPath);
                        Utils.Logging.Info("已复制 Win7 驱动");
                        break;
                    default:
                        Utils.Logging.Info($"不支持的系统版本：{version}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Utils.Logging.Info("复制驱动文件失败");
                Utils.Logging.Info(e.ToString());
                return false;
            }
            MainForm.Instance.StatusText($"{Utils.i18N.Translate("Status")}{Utils.i18N.Translate(": ")}{Utils.i18N.Translate("Register driver")}");
            // 注册驱动文件
            var result = nfapinet.NFAPI.nf_registerDriver("netfilter2");
            if (result != nfapinet.NF_STATUS.NF_STATUS_SUCCESS)
            {
                Utils.Logging.Info($"注册驱动失败，返回值：{result}");
                return false;
            }
            return true;
        }

        private string StartUDPServer(string fallback)
        {
            if (Global.Settings.UDPServer)
            {
                if (Global.Settings.UDPServerIndex == -1)
                {
                    fallback += $" -rudp 127.0.0.1:{Global.Settings.Socks5LocalPort}";
                }
                else
                {
                    Models.Server UDPServer = Global.Settings.Server.AsReadOnly()[Global.Settings.UDPServerIndex];

                    var result = Utils.DNS.Lookup(UDPServer.Hostname);
                    if (result == null)
                    {
                        Utils.Logging.Info("无法解析服务器 IP 地址");
                        return "error";
                    }
                    var UDPServerHostName = result.ToString();

                    if (UDPServer.Type != "Socks5")
                    {
                        //启动UDP分流服务支持SS/SSR/Trojan
                        UDPServerInstance = MainController.GetProcess();
                        if (UDPServer.Type == "SS")
                        {
                            UDPServerInstance.StartInfo.FileName = "bin\\Shadowsocks.exe";
                            UDPServerInstance.StartInfo.Arguments = $"-s {UDPServerHostName} -p {UDPServer.Port} -b {Global.Settings.LocalAddress} -l {Global.Settings.Socks5LocalPort + 1} -m {UDPServer.EncryptMethod} -k \"{UDPServer.Password}\" -u";
                        }

                        if (UDPServer.Type == "SSR")
                        {
                            UDPServerInstance.StartInfo.FileName = "bin\\ShadowsocksR.exe";

                            UDPServerInstance.StartInfo.Arguments = $"-s {UDPServerHostName} -p {UDPServer.Port} -k \"{UDPServer.Password}\" -m {UDPServer.EncryptMethod} -t 120";

                            if (!string.IsNullOrEmpty(UDPServer.Protocol))
                            {
                                UDPServerInstance.StartInfo.Arguments += $" -O {UDPServer.Protocol}";

                                if (!string.IsNullOrEmpty(UDPServer.ProtocolParam))
                                {
                                    UDPServerInstance.StartInfo.Arguments += $" -G \"{UDPServer.ProtocolParam}\"";
                                }
                            }

                            if (!string.IsNullOrEmpty(UDPServer.OBFS))
                            {
                                UDPServerInstance.StartInfo.Arguments += $" -o {UDPServer.OBFS}";

                                if (!string.IsNullOrEmpty(UDPServer.OBFSParam))
                                {
                                    UDPServerInstance.StartInfo.Arguments += $" -g \"{UDPServer.OBFSParam}\"";
                                }
                            }

                            UDPServerInstance.StartInfo.Arguments += $" -b {Global.Settings.LocalAddress} -l {Global.Settings.Socks5LocalPort + 1} -u";
                        }


                        if (UDPServer.Type == "TR")
                        {

                            File.WriteAllText("data\\UDPServerlast.json", Newtonsoft.Json.JsonConvert.SerializeObject(new Models.Trojan()
                            {
                                local_addr = Global.Settings.LocalAddress,
                                local_port = Global.Settings.Socks5LocalPort + 1,
                                remote_addr = UDPServerHostName,
                                remote_port = UDPServer.Port,
                                password = new List<string>()
                                    {
                                      UDPServer.Password
                                    }
                            }));

                            UDPServerInstance = MainController.GetProcess();
                            UDPServerInstance.StartInfo.FileName = "bin\\Trojan.exe";
                            UDPServerInstance.StartInfo.Arguments = "-c ..\\data\\UDPServerlast.json";

                        }

                        Utils.Logging.Info($"UDPServer : {UDPServerInstance.StartInfo.Arguments}");
                        File.Delete("logging\\UDPServer.log");
                        UDPServerInstance.OutputDataReceived += (sender, e) =>
                        {
                            try
                            {
                                File.AppendAllText("logging\\UDPServer.log", string.Format("{0}\r\n", e.Data));
                            }
                            catch (Exception)
                            {
                            }
                        };
                        UDPServerInstance.ErrorDataReceived += (sender, e) =>
                        {
                            try
                            {
                                File.AppendAllText("logging\\UDPServer.log", string.Format("{0}\r\n", e.Data));
                            }
                            catch (Exception)
                            {
                            }
                        };

                        UDPServerInstance.Start();
                        UDPServerInstance.BeginOutputReadLine();
                        UDPServerInstance.BeginErrorReadLine();


                        fallback += $" -rudp 127.0.0.1:{Global.Settings.Socks5LocalPort + 1}";
                    }
                    else
                    {
                        fallback += $" -rudp {UDPServerHostName}:{UDPServer.Port}";
                    }

                }
            }
            else
            {
                fallback += $" -rudp 127.0.0.1:{Global.Settings.Socks5LocalPort}";
            }
            return fallback;
        }
    }
}
