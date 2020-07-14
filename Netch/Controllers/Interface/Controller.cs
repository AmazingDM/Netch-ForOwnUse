﻿using System;
using System.Diagnostics;
using System.IO;
using Netch.Models;
using Netch.Utils;

namespace Netch.Controllers
{
    public abstract class Controller
    {
        /// <summary>
        ///     控制器名
        ///     <param />
        ///     未赋值会在 <see cref="InitCheck" /> 赋值为 <see cref="MainFile" />
        /// </summary>
        public string AkaName;

        /// <summary>
        ///     其他需要文件
        /// </summary>
        protected string[] ExtFiles;

        /// <summary>
        ///     进程实例
        /// </summary>
        public Process Instance;

        /// <summary>
        ///     主程序名(不含扩展名)
        /// </summary>
        public string MainFile;

        /// <summary>
        ///     运行检查, 由 <see cref="InitCheck()" />  赋值
        /// </summary>
        public bool Ready;

        /// <summary>
        ///     当前状态
        /// </summary>
        protected State State = State.Waiting;

        public abstract void Stop();

        /// <summary>
        ///     停止
        /// </summary>
        protected void StopInstance()
        {
            try
            {
                if (Instance == null || Instance.HasExited) return;
                Instance.Kill();
                Instance.WaitForExit();
            }
            catch (Exception e)
            {
                Logging.Error($"停止 {MainFile}.exe 错误：\n" + e);
            }
        }


        /// <summary>
        ///     杀残留进程，清除日志，检查文件
        /// </summary>
        /// <returns></returns>
        protected void InitCheck()
        {
            if (string.IsNullOrEmpty(AkaName)) AkaName = MainFile;

            var result = false;
            // 杀残留
            MainController.KillProcessByName(MainFile);
            // 清日志
            try
            {
                if (File.Exists($"logging\\{AkaName}.log")) File.Delete($"logging\\{AkaName}.log");
            }
            catch (Exception)
            {
                // ignored
            }

            // 检查文件
            var mainResult = true;
            var extResult = true;
            if (!string.IsNullOrEmpty(MainFile) && !File.Exists($"bin\\{MainFile}.exe"))
            {
                mainResult = false;
                Logging.Error($"主程序 bin\\{MainFile}.exe 不存在");
            }

            if (ExtFiles == null)
                extResult = true;
            else
                foreach (var f in ExtFiles)
                    if (!File.Exists($"bin\\{f}"))
                    {
                        extResult = false;
                        Logging.Error($"附加文件 bin\\{f} 不存在");
                    }

            result = extResult && mainResult;
            if (!result)
                Logging.Error(AkaName + " 未就绪");
            Ready = result;
        }

        /// <summary>
        ///     写日志
        /// </summary>
        /// <param name="std"></param>
        /// <returns><see cref="std" />是否为空</returns>
        protected bool WriteLog(DataReceivedEventArgs std)
        {
            if (string.IsNullOrWhiteSpace(std.Data)) return false;
            try

            {
                File.AppendAllText($"logging\\{AkaName}.log", $@"{std.Data}{Global.EOF}");
            }
            catch (Exception e)
            {
                Logging.Error($"写入{AkaName}日志错误：\n" + e);
            }

            return true;
        }

        public static Process GetProcess(string path = null, bool redirectStd = true)
        {
            var p = new Process
            {
                StartInfo =
                {
                    Arguments = "",
                    WorkingDirectory = $"{Global.NetchDir}\\bin",
                    CreateNoWindow = true,
                    RedirectStandardError = redirectStd,
                    RedirectStandardInput = redirectStd,
                    RedirectStandardOutput = redirectStd,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };
            if (path != null) p.StartInfo.FileName = Path.GetFullPath(path);
            return p;
        }
    }
}