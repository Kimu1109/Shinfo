using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting.Channels.Ipc;

namespace Shinfo
{
    internal static class Data
    {
        internal static string AppPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        internal static IpcServerChannel ipc;
        internal static UserInfo userInfo;
    }
    internal class UserInfo : MarshalByRefObject
    {
        internal string IP { get; set; }
        internal int Port { get; set; }
        internal string AES { get; set; }

        internal string ID { get; set; }
        internal string Password { get; set; }

        internal List<(string Name, string ID)> Users { get; set; } = new List<(string Name, string ID)>();
    }
}
