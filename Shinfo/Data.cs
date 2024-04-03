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
        internal static IPC.UserInfo userInfo;
    }
}
namespace IPC
{
    public class UserInfo : MarshalByRefObject
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string AES { get; set; }

        public string ID { get; set; }
        public string Password { get; set; }

        public List<User> Users { get; set; } = new List<User>();
        public List<Group> Groups { get; set; } = new List<Group>();

        public class User : MarshalByRefObject
        {
            public string Name { get; set; }
            public string ID { get; set; }
        }
        public class Group : MarshalByRefObject
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public string Description { get; set; }
        }
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}