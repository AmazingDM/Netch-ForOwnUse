using System;
using System.Collections;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace Netch.Utils
{
    public static class DNS
    {
        /// <summary>
        ///     缓存
        /// </summary>
        public static Hashtable Cache = new Hashtable();

        /// <summary>
        ///     查询
        /// </summary>
        /// <param name="hostname">主机名</param>
        /// <returns></returns>
        public static IPAddress Lookup(string hostname)
        {
            try
            {
                if (Cache.Contains(hostname))
                {
                    return Cache[hostname] as IPAddress;
                }

                var task = Dns.GetHostAddressesAsync(hostname);
                if (!task.Wait(1000))
                {
                    return null;
                }

                if (task.Result.Length == 0)
                {
                    return null;
                }

                Cache.Add(hostname, task.Result[0]);

                return task.Result[0];
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 设置DNS
        /// </summary>
        /// <param name="dns"></param>
        public static void SetDNS(string[] dns)
        {
            ManagementClass wmi = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = wmi.GetInstances();
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;
            foreach (ManagementObject mo in moc)
            {
                //如果没有启用IP设置的网络设备则跳过
                if (!(bool)mo["IPEnabled"])
                    continue;

                //设置DNS地址
                if (dns != null)
                {
                    inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                    inPar["DNSServerSearchOrder"] = dns;
                    outPar = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);
                }
            }
        }
        /// <summary>
        /// 从网卡获取ip设置信息
        /// </summary>
        public static string[] getSystemDns()
        {
            string[] dns = { };
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                bool Pd1 = (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet); //判断是否是以太网连接
                if (Pd1)
                {
                    IPInterfaceProperties ip = adapter.GetIPProperties();     //IP配置信
                    int DnsCount = ip.DnsAddresses.Count;
                    Console.WriteLine("DNS服务器地址：");
                    if (DnsCount > 0)
                    {
                        try
                        {
                            return new string[] { ip.DnsAddresses[0].ToString(), ip.DnsAddresses[1].ToString() };
                        }
                        catch (Exception er)
                        {
                            throw er;
                        }
                    }
                }
            }
            return new string[] { "223.5.5.5", "1.1.1.1" };
        }
    }
}
