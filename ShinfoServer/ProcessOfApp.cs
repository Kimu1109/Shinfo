using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using static ShinfoServer.Data;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace ShinfoServer
{
    public interface UserAndGroupTree
    {
        string Name { get; set; }
        ObservableCollection<UserAndGroupTree> Nodes { get; set; }
        bool IsGroup { get; }
        BitmapImage Image { get; }
    }
    public static partial class Process
    {
        //パスワードに使用する文字
        private static readonly string passwordChars = "0123456789abcdefghijklmnopqrstuvwxyz";
        private static int passwordSeed = Environment.TickCount;

        /// <summary>
        /// ランダムな文字列を生成する
        /// </summary>
        /// <param name="length">生成する文字列の長さ</param>
        /// <returns>生成された文字列</returns>
        public static string GeneratePassword(int length)
        {
            unchecked
            {
                passwordSeed++;
            }
            StringBuilder sb = new StringBuilder(length);
            Random r = new Random(passwordSeed);

            for (int i = 0; i < length; i++)
            {
                //文字の位置をランダムに選択
                int pos = r.Next(passwordChars.Length);
                //選択された位置の文字を取得
                char c = passwordChars[pos];
                //パスワードに追加
                sb.Append(c);
            }

            return sb.ToString();
        }
        public static void LogWriteLineByBase(UserData UserData, string message, string info)
        {
            File.AppendAllText(AppPath + "\\server.log", UserData.ID + " | " + message + " | " + info + "\n");
        }
        public static string XmlHeader()
        {
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine;
        }
        /// <summary>
        /// ディレクトリをコピーする
        /// </summary>
        /// <param name="sourceDirName">コピーするディレクトリ</param>
        /// <param name="destDirName">コピー先のディレクトリ</param>
        public static void CopyDirectory(
            string sourceDirName, string destDirName)
        {
            if (!System.IO.Directory.Exists(destDirName))
            {
                System.IO.Directory.CreateDirectory(destDirName);
                System.IO.File.SetAttributes(destDirName,
                    System.IO.File.GetAttributes(sourceDirName));
            }

            if (destDirName[destDirName.Length - 1] !=
                    System.IO.Path.DirectorySeparatorChar)
                destDirName = destDirName + System.IO.Path.DirectorySeparatorChar;

            string[] files = System.IO.Directory.GetFiles(sourceDirName);
            foreach (string file in files)
                System.IO.File.Copy(file,
                    destDirName + System.IO.Path.GetFileName(file), true);

            string[] dirs = System.IO.Directory.GetDirectories(sourceDirName);
            foreach (string dir in dirs)
                CopyDirectory(dir, destDirName + System.IO.Path.GetFileName(dir));
        }
        /// <summary>
        /// フォルダのサイズを取得する
        /// </summary>
        /// <param name="dirInfo">サイズを取得するフォルダ</param>
        /// <returns>フォルダのサイズ（バイト）</returns>
        public static long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;

            foreach (FileInfo fi in dirInfo.GetFiles())
                size += fi.Length;

            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                size += GetDirectorySize(di);

            return size;
        }
        public static byte[] Compress(byte[] src)
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
        public static byte[] Decompress(byte[] src)
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
