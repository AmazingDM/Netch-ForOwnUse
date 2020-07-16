﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Netch.Forms;
using Netch.Models;
using Netch.Utils;
using nfapinet;

namespace Netch.Controllers
{
    public class NFController : ModeController
    {
        private static readonly ServiceController NFService = new ServiceController("netfilter2");

        private static readonly string BinDriver = "";
        private static readonly string SystemDriver = $"{Environment.SystemDirectory}\\drivers\\netfilter2.sys";

        public static string DriverVersion(string file)
        {
            return File.Exists(file) ? FileVersionInfo.GetVersionInfo(file).FileVersion : "";
        }
        static NFController()
        {
            // 生成系统版本
            var winNTver = $"{Environment.OSVersion.Version.Major.ToString()}.{Environment.OSVersion.Version.Minor.ToString()}";
            switch (winNTver)
            {
                case "10.0":
                    BinDriver = "Win-10.sys";
                    break;
                case "6.3":
                case "6.2":
                    BinDriver = "Win-8.sys";
                    break;
                case "6.1":
                case "6.0":
                    BinDriver = "Win-7.sys";
                    break;
                default:
                    Logging.Error($"不支持的系统版本：{winNTver}");
                    return;
            }

            BinDriver = "bin\\" + BinDriver;
        }

        public NFController()
        {
            MainFile = "Redirector";
            ExtFiles = new[] { Path.GetFileName(BinDriver) };
            InitCheck();

            if (!File.Exists(SystemDriver))
            {
                InstallDriver();
            }
        }

        /*public override bool Start(Server server, Mode mode)
        {
            if (!CheckDriverReady())
            {
                if (File.Exists(SystemDriver))
                    UninstallDriver();
                if (!InstallDriver())
                    return false;
            }

            var processList = "";
            foreach (var proc in mode.Rule)
                processList += proc + ",";
            processList += "NTT.exe";

            Instance = GetProcess("bin\\Redirector.exe");
            if (server.Type != "Socks5")
            {
                Instance.StartInfo.Arguments += $"-r 127.0.0.1:{Global.Settings.Socks5LocalPort} -p \"{processList}\"";
            }

            else
            {
                var result = DNS.Lookup(server.Hostname);
                if (result == null)
                {
                    Logging.Info("无法解析服务器 IP 地址");
                    return false;
                }

                Instance.StartInfo.Arguments += $"-r {result}:{server.Port} -p \"{processList}\"";
                if (!string.IsNullOrWhiteSpace(server.Username) && !string.IsNullOrWhiteSpace(server.Password)) Instance.StartInfo.Arguments += $" -username \"{server.Username}\" -password \"{server.Password}\"";
            }

            Instance.StartInfo.Arguments += $" -t {Global.Settings.RedirectorTCPPort}";
            Instance.OutputDataReceived += OnOutputDataReceived;
            Instance.ErrorDataReceived += OnOutputDataReceived;

            for (var i = 0; i < 2; i++)
            {
                State = State.Starting;
                Instance.Start();
                Instance.BeginOutputReadLine();
                Instance.BeginErrorReadLine();

                for (var j = 0; j < 40; j++)
                {
                    Thread.Sleep(250);

                    if (State == State.Started) return true;
                }

                Logging.Error("NF 进程启动超时");
                Stop();
                if (!RestartService()) return false;
            }

            return false;
        }*/


        private static string[] _sysDns = { };
        public override bool Start(Server server, Mode mode)
        {
            if (!CheckDriverReady())
            {
                if (File.Exists(SystemDriver))
                    UninstallDriver();
                if (!InstallDriver())
                    return false;
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

            Instance = GetProcess("bin\\Redirector.exe");
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
                var result = DNS.Lookup(server.Hostname);
                if (result == null)
                {
                    Logging.Info("无法解析服务器 IP 地址");
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
                processes += ",ck-client.exe,Privoxy.exe,Redirector.exe,Shadowsocks.exe,ShadowsocksR.exe,simple-obfs.exe,Trojan.exe,tun2socks.exe,unbound.exe,v2ctl.exe,v2ray-plugin.exe,v2ray.exe,wv2ray.exe,tapinstall.exe,Netch.exe";
                fallback += " -bypass true ";
            }
            else
            {
                fallback += " -bypass false";
            }

            fallback += $" -p \"{processes}\"";

            // true  除规则内IP全走代理
            // false 仅代理规则内IP
            if (processesIPFillter.Length > 0)
            {
                if (mode.ProcesssIPFillter())
                {
                    fallback += $" -bypassip true";
                }
                else
                {
                    fallback += $" -bypassip false";
                }
                fallback += $" -fip \"{processesIPFillter}\"";
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

            Logging.Info($"Redirector : {fallback}");

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

                            State = State.Started;

                            //备份并替换系统DNS
                            _sysDns = DNS.getSystemDns();
                            string[] dns = { "1.1.1.1", "8.8.8.8" };
                            DNS.SetDNS(dns);

                            return true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logging.Error(e.Message);
                    return true;
                }
                finally
                {
                    Thread.Sleep(10);
                }
            }

            Logging.Info("NF 进程启动超时");
            Stop();
            return false;
        }
        private bool RestartService()
        {
            try
            {
                switch (NFService.Status)
                {
                    // 启动驱动服务
                    case ServiceControllerStatus.Running:
                        // 防止其他程序占用 重置 NF 百万连接数限制
                        NFService.Stop();
                        NFService.WaitForStatus(ServiceControllerStatus.Stopped);
                        MainForm.Instance.StatusText(i18N.Translate("Starting netfilter2 Service"));
                        NFService.Start();
                        break;
                    case ServiceControllerStatus.Stopped:
                        MainForm.Instance.StatusText(i18N.Translate("Starting netfilter2 Service"));
                        NFService.Start();
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Error("启动驱动服务失败：\n" + e);

                var result = NFAPI.nf_registerDriver("netfilter2");
                if (result != NF_STATUS.NF_STATUS_SUCCESS)
                {
                    Logging.Error($"注册驱动失败，返回值：{result}");
                    return false;
                }

                Logging.Info("注册驱动成功");
            }

            return true;
        }

        private bool CheckDriverReady()
        {
            // 检查驱动是否存在
            if (!File.Exists(SystemDriver)) return false;

            // 检查驱动版本号
            return DriverVersion(SystemDriver) == DriverVersion(BinDriver);
        }

        public static bool UninstallDriver()
        {
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
            try
            {
                NFAPI.nf_unRegisterDriver("netfilter2");

                File.Delete(SystemDriver);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool InstallDriver()
        {
            Logging.Info("安装驱动中");
            try
            {
                File.Copy(BinDriver, SystemDriver);
            }
            catch (Exception e)
            {
                Logging.Error("驱动复制失败\n" + e);
                return false;
            }

            MainForm.Instance.StatusText(i18N.Translate("Register driver"));
            // 注册驱动文件
            var result = NFAPI.nf_registerDriver("netfilter2");
            if (result == NF_STATUS.NF_STATUS_SUCCESS)
            {
                Logging.Info($"驱动安装成功，当前驱动版本:{DriverVersion(DriverVersion(SystemDriver))}");
            }
            else
            {
                Logging.Error($"注册驱动失败，返回值：{result}");
                return false;
            }

            return true;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!WriteLog(e)) return;
            if (State == State.Starting)
            {
                if (Instance.HasExited)
                    State = State.Stopped;
                else if (e.Data.Contains("Started"))
                    State = State.Started;
                else if (e.Data.Contains("Failed") || e.Data.Contains("Unable")) State = State.Stopped;
            }
            else if (State == State.Started)
            {
                if (e.Data.StartsWith("[APP][Bandwidth]"))
                {
                    var splited = e.Data.Replace("[APP][Bandwidth]", "").Trim().Split(',');
                    if (splited.Length == 2)
                    {
                        var uploadSplited = splited[0].Split(':');
                        var downloadSplited = splited[1].Split(':');

                        if (uploadSplited.Length == 2 && downloadSplited.Length == 2)
                            if (long.TryParse(uploadSplited[1], out var upload) && long.TryParse(downloadSplited[1], out var download))
                                Task.Run(() => OnBandwidthUpdated(upload, download));
                    }
                }
            }
        }

        public override void Stop()
        {
            StopInstance();
            //恢复系统DNS
            DNS.SetDNS(_sysDns);
            try
            {
                if (UDPServerInstance == null || UDPServerInstance.HasExited) return;
                UDPServerInstance.Kill();
                UDPServerInstance.WaitForExit();
            }
            catch (Exception e)
            {
                Logging.Error($"停止 {MainFile}.exe 错误：\n" + e);
            }
        }

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
        ///     UDP代理进程实例
        /// </summary>
        public Process UDPServerInstance;

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
                        if (UDPServer.Type == "SS")
                        {
                            UDPServerInstance = GetProcess("bin\\Shadowsocks.exe");
                            UDPServerInstance.StartInfo.Arguments = $"-s {UDPServerHostName} -p {UDPServer.Port} -b {Global.Settings.LocalAddress} -l {Global.Settings.Socks5LocalPort + 1} -m {UDPServer.EncryptMethod} -k \"{UDPServer.Password}\" -u";
                        }

                        if (UDPServer.Type == "SSR")
                        {
                            UDPServerInstance = GetProcess("bin\\ShadowsocksR.exe");
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

                            UDPServerInstance = GetProcess("bin\\Trojan.exe");
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