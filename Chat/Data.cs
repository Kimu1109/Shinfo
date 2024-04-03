using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public static class Extension
    {
        public static byte[] Compress(this byte[] src)
        {
            using (var ms = new MemoryStream())
            {
                using (var ds = new DeflateStream(ms, CompressionMode.Compress, true/*msは*/))
                {
                    ds.Write(src, 0, src.Length);
                }

                // 圧縮した内容をbyte配列にして取り出す
                ms.Position = 0;
                byte[] comp = new byte[ms.Length];
                ms.Read(comp, 0, comp.Length);
                return comp;
            }
        }
        public static byte[] Decompress(this byte[] src)
        {
            using (var ms = new MemoryStream(src))
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                using (var dest = new MemoryStream())
                {
                    ds.CopyTo(dest);

                    dest.Position = 0;
                    byte[] decomp = new byte[dest.Length];
                    dest.Read(decomp, 0, decomp.Length);
                    return decomp;
                }
            }
        }
    }
}
