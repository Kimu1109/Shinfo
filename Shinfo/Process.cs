using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;

namespace Shinfo
{
    internal static class Process
    {
        internal static PackIconKind IndexOfKindFromString(string kind)
        {
            PackIconKind result;
            if (Enum.TryParse(kind, out result))
                return result;
            else
                return PackIconKind.QuestionMark;
        }
        internal static void ShowErrorMessageBox(string errorMessage, string message)
        {
            MessageBox.Show(errorMessage + "\n" + message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void CreateFile(string Path)
        {
            string dir;
            var arr = Path.Split('\\');
            if (arr.Last().IndexOf('.') != -1)
            {
                dir = Path.Replace(arr.Last(), "");
            }
            else
            {
                dir = Path;
            }

            if (!Directory.Exists($"{Data.AppPath}\\data\\file\\{dir}")) Directory.CreateDirectory($"{Data.AppPath}\\data\\file\\{dir}");
            if (arr.Last().IndexOf('.') != -1 && File.Exists($"{Data.AppPath}\\data\\file\\{Path}") == false) File.Create($"{Data.AppPath}\\data\\file\\{Path}");
        }
    }
    public static class Extension
    {
        public static string SizeToString(this ulong Size)
        {
            if (Size < 1024)
            {
                return $"{Size}B";
            }
            else if (Size < 1024 * 1024)
            {
                return $"{(int)(Size / 1024)}KB";
            }
            else if (Size < 1024 * 1024 * 1024)
            {
                return $"{(int)(Size / 1024 / 1024)}MB";
            }
            else if (Size < 1024ul * 1024 * 1024 * 1024)
            {
                return $"{(int)(Size / 1024 / 1024 / 1024)}GB";
            }
            else if (Size < 1024ul * 1024 * 1024 * 1024 * 1024)
            {
                return $"{(int)(Size / 1024 / 1024 / 1024 / 1024)}TB";
            }
            else
            {
                return $"{(int)(Size / 1024 / 1024 / 1024 / 1024 / 1024)}PB";
            }
        }
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
        public static string ToMsg(this string str)
        {
            return str.Replace("&sr;", "/").Replace("&lf;", "\n").Replace("&dl;", "$").Replace("&per;", "%").Replace("&lab;", "<").Replace("&rab;", ">").Replace("&bsr;", "\\");
        }
        public static string ToStr(this string message)
        {
            return message.Replace("/", "&sr;").Replace("\n", "&lf;").Replace("$", "&dl;").Replace("%", "&per;").Replace("<", "&lab;").Replace(">", "&rab;").Replace("\\", "&bsr;");
        }
    }
}
