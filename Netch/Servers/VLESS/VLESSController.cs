using System.IO;
using Netch.Controllers;
using Netch.Models;
using Netch.Servers.VMess.Utils;

namespace Netch.Servers.VLESS
{
    public class VLESSController : Guard, IServerController
    {
        public override string Name { get; protected set; } = "VLESS";
        public override string MainFile { get; protected set; } = "v2ray.exe";

        public ushort? Socks5LocalPort { get; set; }

        public string LocalAddress { get; set; }

        public  bool Start(in Server s,in Mode mode)
        {
            if (mode.Type == -1)
            {
                Name += "_UDP";
            }
            File.WriteAllText("data\\last.json", V2rayConfigUtils.GenerateClientConfig(s, mode));
            return StartInstanceAuto("-config ..\\data\\last.json");
        }

        public override void Stop()
        {
            StopInstance();
        }
    }
}