using System;
using System.Collections.Generic;
using System.IO;
using IO = System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using IniParser;
using static ShinfoServer.Data;

namespace ShinfoServer
{
    public static partial class Process
    {
        public static FileIniDataParser parser = new FileIniDataParser();
        public static string[] GetDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                DirectoryControl.Create(path, "made by server system.", SystemAdminUser);
            }
            return Directory.GetDirectories(AppPath + "\\file\\" + path);
        }
        public static string GetRelativePath(string FullPath)
        {
            return FullPath.Replace(AppPath + "\\file\\", "");
        }
        public static string GetOnlyIniKey(string IniPath, string Section, string Key)
        {
            var dat = parser.ReadFile(IniPath, Encoding.UTF8);
            return dat[Section][Key];
        }
        /// <summary>
        /// 親フォルダを配列形式で返します。
        /// </summary>
        /// <param name="PCPath">パス</param>
        /// <returns></returns>
        private static List<string> GetParentsDirectoryFromPath(string Path)
        {
            var Parents = new List<string>(Path.Split('\\'));
            if (Parents[Parents.Count - 1].IndexOf('.') != -1) Parents.RemoveAt(Parents.Count - 1);

            for (int i = Parents.Count - 1; i >= 0; i--)
            {
                string buf = "";
                for (int j = 0; j <= i; j++)
                {
                    buf += Parents[j] + "\\";
                }
                Parents[i] = buf.TrimEnd('\\');
            }

            return Parents;
        }
        /// <summary>
        /// 最初の親フォルダを返します。
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static string GetFirstParentFromPath(string Path)
        {
            return System.IO.Path.GetDirectoryName(Path);
        }
        /// <summary>
        /// UserLevelを取得する。ただ、それだけ。
        /// </summary>
        /// <param name="iniFile">iniファイルのパス</param>
        /// <param name="Section">せくしょん</param>
        /// <param name="Key">きい</param>
        /// <returns></returns>
        private static UserData.UserLevel GetOnlyUserLevelByIniFile(string iniFile, string Section, string Key)
        {
            var data = parser.ReadFile(iniFile, Encoding.UTF8);
            return (UserData.UserLevel)Enum.Parse(typeof(UserData.UserLevel), data[Section][Key]);
        }
        /// <summary>
        /// 文字列からユーザーレベルへ変換
        /// </summary>
        /// <param name="Str">文字列</param>
        /// <returns></returns>
        public static UserData.UserLevel StringToUserLevel(string Str)
        {
            return (UserData.UserLevel)Enum.Parse(typeof(UserData.UserLevel), Str);
        }
        /// <summary>
        /// iniファイルのある場所に変換します。
        /// </summary>
        /// <param name="Path">フォルダパス(file以下を省略された形式)</param>
        /// <param name="FileOrDirectory">ファイル(false)かディレクトリ(true)か</param>
        /// <returns></returns>
        private static string GetInfoPathFromPath(string Path, bool FileOrDirectory)
        {
            return AppPath + @"\file-info\" + Path + "." + (FileOrDirectory == false ? "file" : "directory") + ".ini";
        }
        /// <summary>
        /// メインデータのあるフォルダに変換して返します。
        /// </summary>
        /// <param name="Path">フォルダパス(file以下を省略された形式)</param>
        /// <returns></returns>
        private static string GetDataPathFromPath(string Path)
        {
            if (Path == "")
            {
                return AppPath + @"\file";
            }
            else return AppPath + @"\file\" + Path;
        }
        /// <summary>
        /// プロパティデータのあるフォルダの、ファイルのためのフォルダに変換して返します。
        /// </summary>
        /// <param name="Path">フォルダパス(file以下を省略された形式)</param>
        /// <returns></returns>
        private static string GetInfoDirPathFromPath(string Path)
        {
            return AppPath + @"\file-info\" + Path;
        }
        public enum IOResult
        {
            Finished = 0,
            LevelMissOfWrite = 1,
            LevelMissOfRead = 2,
            LevelMissOfView = 3,
            ExistError = 4,
            NamingError = 5,
            NotExistError = 6,
            LevelMissOfDelete = 7,
            CanNotSetUpperThanYours = 8,
            YoursIsUpperThanParent = 9,
            ArgumentMiss = 10
        }
        public class FileAndDirectoryInfer
        {
            public bool IsFile { get; set; }
            public string Name { get; set; }
            public ulong Size { get; set; }
            public DateTime LastWroteTime { get; set; }
            public string LastWroteUser { get; set; }
            public FileAndDirectoryInfer(bool IsFile, string Name, ulong Size, DateTime LastWroteTime, string LastWroteUser)
            {
                this.IsFile = IsFile;
                this.Name = Name;
                this.Size = Size;
                this.LastWroteTime = LastWroteTime;
                this.LastWroteUser = LastWroteUser;
            }
        }
        /// <summary>
        /// Qweek専用のファイルのコントロール
        /// </summary>
        internal static class FileControl
        {
            /// <summary>
            /// ファイルを作成します。
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="Description">説明</param>
            /// <param name="UserData">ログを残す者(任意)</param>
            /// <returns></returns>
            public static IOResult Create(string Path, string Description = "なし", UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileCreate", Path);

                string DataPath = GetDataPathFromPath(Path);
                string InfoPath = GetInfoPathFromPath(Path, false);

                string ParentInfoPath = GetInfoPathFromPath(GetFirstParentFromPath(Path), true);

                if (System.IO.Path.GetFileName(DataPath).IndexOf('.') == -1) return IOResult.NamingError;
                if (File.Exists(DataPath)) return IOResult.ExistError;
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(DataPath))) return IOResult.NotExistError;
                if (GetOnlyUserLevelByIniFile(ParentInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfWrite;

                var st = new StringBuilder();
                st.AppendLine("[Info]");
                st.AppendLine("Description = " + Description);

                st.AppendLine("[User]");
                st.AppendLine("LastWrote = " + UserData.userID);
                st.AppendLine("LastRead = " + UserData.userID);
                st.AppendLine("MakeUser = " + UserData.userID);

                st.AppendLine("[Time]");
                st.AppendLine("LastWrote = " + DateTime.Now.ToString());
                st.AppendLine("LastRead = " + DateTime.Now.ToString());
                st.AppendLine("MakeTime = " + DateTime.Now.ToString());

                st.AppendLine("[Access]");
                st.AppendLine("Read = " + UserData.Level.ToString());
                st.AppendLine("Write = " + UserData.Level.ToString());
                st.AppendLine("View = " + UserData.Level.ToString());
                st.AppendLine("Delete = " + UserData.Level.ToString());

                File.Create(DataPath);
                File.WriteAllText(InfoPath, st.ToString());

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイルを削除
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者(任意)</param>
            /// <returns></returns>
            public static IOResult Delete(string Path, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileDelete", Path);

                string DataPath = GetDataPathFromPath(Path);
                string InfoPath = GetInfoPathFromPath(Path, false);

                if (!File.Exists(DataPath)) return IOResult.NotExistError;

                if (GetOnlyUserLevelByIniFile(InfoPath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;

                File.Delete(InfoPath);
                File.Delete(DataPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイルを移動
            /// </summary>
            /// <param name="BeforePath">移動前のパス</param>
            /// <param name="AfterPath">移動後のパス</param>
            /// <param name="UserData">ログを残す者(任意)</param>
            /// <returns></returns>
            public static IOResult Move(string BeforePath, string AfterPath, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileMove", BeforePath + " | " + AfterPath);

                string BeforeDataPath = GetDataPathFromPath(BeforePath);
                string BeforeInfoPath = GetInfoPathFromPath(BeforePath, false);

                string AfterDataPath = GetDataPathFromPath(AfterPath);
                string AfterInfoPath = GetInfoPathFromPath(AfterPath, false);

                string AfterDirInfoPath = GetInfoPathFromPath(GetFirstParentFromPath(AfterPath), true);

                if (GetOnlyUserLevelByIniFile(AfterDirInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfWrite;
                if (GetOnlyUserLevelByIniFile(BeforeInfoPath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;
                if (File.Exists(AfterDataPath)) return IOResult.ExistError;
                if (!File.Exists(BeforeDataPath)) return IOResult.NotExistError;
                if (!Directory.Exists(Path.GetDirectoryName(AfterDataPath))) return IOResult.NotExistError;

                File.Move(BeforeDataPath, AfterDataPath);
                File.Move(BeforeInfoPath, AfterInfoPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイルをコピー
            /// </summary>
            /// <param name="SourcePath">コピーさせるファイル</param>
            /// <param name="PastePath">コピーするファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <returns></returns>
            public static IOResult Copy(string SourcePath, string PastePath, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileCopy", SourcePath + " | " + PastePath);

                string SourceDataPath = GetDataPathFromPath(SourcePath);
                string SourceInfoPath = GetInfoPathFromPath(SourcePath, false);

                string PasteDataPath = GetDataPathFromPath(PastePath);
                string PasteInfoPath = GetInfoPathFromPath(PastePath, false);

                string PasteDirInfoPath = GetInfoPathFromPath(GetFirstParentFromPath(PastePath), true);

                if (GetOnlyUserLevelByIniFile(PasteDirInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfWrite;
                if (GetOnlyUserLevelByIniFile(SourceInfoPath, "Access", "Read") > UserData.Level) return IOResult.LevelMissOfRead;
                if (File.Exists(PasteDataPath)) return IOResult.ExistError;
                if (!File.Exists(SourceDataPath)) return IOResult.NotExistError;
                if (!Directory.Exists(Path.GetDirectoryName(PasteDataPath))) return IOResult.NotExistError;

                File.Copy(SourceDataPath, PasteDataPath);
                File.Copy(SourceInfoPath, PasteInfoPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイルに書き込む(ない場合は、ファイルを作成する)
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="content">内容</param>
            /// <param name="UserData">ログを残すもの(任意)</param>
            /// <returns></returns>
            public static IOResult WriteOver(string Path, string content, UserData UserData = null)
            {
                var result = Write(Path, UserData, null, content);
                if (result == IOResult.NotExistError)
                {
                    result = Create(Path, UserData: UserData);
                    if (result == IOResult.Finished) result = Write(Path, UserData, null, content);
                }
                return result;
            }
            /// <summary>
            /// ファイルに書き込む
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <param name="bytes">書き込むバイト配列</param>
            /// <param name="str">書き込む文字列</param>
            public static IOResult Write(string Path, UserData UserData = null, byte[] bytes = null, string str = null, bool IsCheck = false)
            {
                if (IsCheck == false && bytes == null && str == null) return IOResult.ArgumentMiss;
                if (IsCheck == false && bytes != null && str != null) return IOResult.ArgumentMiss;

                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileWrite", Path);

                string DataPath = GetDataPathFromPath(Path);
                string InfoPath = GetInfoPathFromPath(Path, false);

                if (!File.Exists(DataPath))
                {
                    var create = Create(Path, "なし", UserData);
                    if (create != IOResult.Finished) return create;
                }

                var data = parser.ReadFile(InfoPath, Encoding.UTF8);

                if (StringToUserLevel(data["Access"]["Write"]) > UserData.Level) return IOResult.LevelMissOfWrite;

                data["Time"]["LastWrote"] = DateTime.Now.ToString();
                data["User"]["LastWrote"] = UserData.userID;

                parser.WriteFile(InfoPath, data, Encoding.UTF8);

                if (!IsCheck)
                {
                    if (bytes != null)
                        File.WriteAllBytes(DataPath, bytes);
                    else
                        File.WriteAllText(DataPath, str);
                }

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイルをバイト配列を読み込む
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <returns></returns>
            public static (byte[] content, IOResult result) ReadByBytes(string Path, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileRead", Path);

                string DataPath = GetDataPathFromPath(Path);
                string IniPath = GetInfoPathFromPath(Path, false);

                if (!File.Exists(DataPath)) return (null, IOResult.NotExistError);

                var data = parser.ReadFile(IniPath, Encoding.UTF8);

                if (StringToUserLevel(data["Access"]["Read"]) > UserData.Level) return (null, IOResult.LevelMissOfRead);

                data["Time"]["LastRead"] = DateTime.Now.ToString();
                data["User"]["LastRead"] = UserData.userID;

                parser.WriteFile(IniPath, data, Encoding.UTF8);

                return (File.ReadAllBytes(DataPath), IOResult.Finished);
            }
            /// <summary>
            /// ファイルを文字列で読み込む(文字化け率が高い)
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <returns></returns>
            public static (string content, IOResult result) ReadByString(string Path, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileRead", Path);

                string DataPath = GetDataPathFromPath(Path);

                if (!File.Exists(DataPath)) return (null, IOResult.NotExistError);

                var data = parser.ReadFile(DataPath, Encoding.UTF8);

                if (StringToUserLevel(data["Access"]["Read"]) > UserData.Level) return (null, IOResult.LevelMissOfRead);

                data["Time"]["LastRead"] = DateTime.Now.ToString();
                data["User"]["LastRead"] = UserData.userID;

                parser.WriteFile(DataPath, data, Encoding.UTF8);

                return (File.ReadAllText(DataPath), IOResult.Finished);
            }
            /// <summary>
            /// 説明を変える
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="Description">説明</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            public static IOResult SetDescription(string Path, string Description, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileDescription", Path);

                string WritePath = GetInfoPathFromPath(Path, false);

                if (!File.Exists(WritePath)) return IOResult.NotExistError;

                if (GetOnlyUserLevelByIniFile(WritePath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfWrite;

                var data = parser.ReadFile(WritePath, Encoding.UTF8);

                data["Info"]["Description"] = Description;

                parser.WriteFile(WritePath, data, Encoding.UTF8);

                return IOResult.Finished;
            }
            /// <summary>
            /// アクセス権を変更
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <param name="Read">読み込みの権限(任意)</param>
            /// <param name="Write">書き込みの権限(任意)</param>
            /// <param name="View">表示の権限(任意)</param>
            /// <param name="Delete">削除の権限(任意)</param>
            public static IOResult SetAccess(string Path, UserData UserData = null, UserData.UserLevel Read = UserData.UserLevel.Nothing, UserData.UserLevel Write = UserData.UserLevel.Nothing, UserData.UserLevel View = UserData.UserLevel.Nothing, UserData.UserLevel Delete = UserData.UserLevel.Nothing)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "FileAccess", Path);

                string WritePath = GetInfoPathFromPath(Path, false);

                if (!File.Exists(WritePath)) return IOResult.NotExistError;

                if (GetOnlyUserLevelByIniFile(WritePath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;

                if ((Read > UserData.Level) ||
                    (Write > UserData.Level) ||
                    (View > UserData.Level) ||
                    (Delete > UserData.Level))
                {
                    return IOResult.CanNotSetUpperThanYours;
                }

                foreach (var parent in GetParentsDirectoryFromPath(Path))
                {
                    var parentData = parser.ReadFile(GetInfoPathFromPath(parent, true), Encoding.UTF8);
                    var parentRead = StringToUserLevel(parentData["Access"]["Read"]);
                    var parentWrite = StringToUserLevel(parentData["Access"]["Write"]);
                    var parentView = StringToUserLevel(parentData["Access"]["View"]);
                    var parentDelete = StringToUserLevel(parentData["Access"]["Delete"]);

                    if (parentRead < Read || parentWrite < Write || parentView < View || parentDelete < Delete) return IOResult.YoursIsUpperThanParent;
                }

                var data = parser.ReadFile(WritePath, Encoding.UTF8);

                if (Read != UserData.UserLevel.Nothing) data["Access"]["Read"] = Read.ToString();
                if (Write != UserData.UserLevel.Nothing) data["Access"]["Write"] = Write.ToString();
                if (View != UserData.UserLevel.Nothing) data["Access"]["View"] = View.ToString();
                if (Delete != UserData.UserLevel.Nothing) data["Access"]["Delete"] = Delete.ToString();

                parser.WriteFile(WritePath, data, Encoding.UTF8);

                return IOResult.Finished;
            }
        }
        /// <summary>
        /// ディレクトリの操作関係
        /// </summary>
        internal static class DirectoryControl
        {
            /// <summary>
            /// フォルダを作成
            /// </summary>
            /// <param name="Path">作成するパス</param>
            /// <param name="Description">説明</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <returns></returns>
            public static IOResult Create(string Path, string Description = "なし", UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryCreate", Path);

                string DataPath = GetDataPathFromPath(Path);
                string InfoDirPath = GetInfoDirPathFromPath(Path);
                string InfoPath = GetInfoPathFromPath(Path, true);

                string ParentInfoPath = GetInfoPathFromPath(GetFirstParentFromPath(Path), true);

                if (DataPath.IndexOf('.') != -1) return IOResult.NamingError;
                if (Directory.Exists(DataPath)) return IOResult.ExistError;
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(DataPath))) return IOResult.NotExistError;
                if (GetOnlyUserLevelByIniFile(ParentInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfWrite;

                Directory.CreateDirectory(DataPath);
                Directory.CreateDirectory(InfoDirPath);

                var st = new StringBuilder();
                st.AppendLine("[Info]");
                st.AppendLine("Description = " + Description);

                st.AppendLine("[Access]");
                st.AppendLine("Read = " + UserData.Level.ToString());
                st.AppendLine("Write = " + UserData.Level.ToString());
                st.AppendLine("View = " + UserData.Level.ToString());
                st.AppendLine("Delete = " + UserData.Level.ToString());

                File.WriteAllText(InfoPath, st.ToString());

                return IOResult.Finished;
            }
            /// <summary>
            /// 削除
            /// </summary>
            /// <param name="Path">ディレクトリパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <returns></returns>
            public static IOResult Delete(string Path, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryDelete", Path);

                string DataPath = GetDataPathFromPath(Path);
                string InfoDirPath = GetInfoDirPathFromPath(Path);
                string InfoPath = GetInfoPathFromPath(Path, true);

                if (!Directory.Exists(DataPath)) return IOResult.NotExistError;
                if (GetOnlyUserLevelByIniFile(InfoPath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;

                File.Delete(InfoPath);
                Directory.Delete(DataPath);
                Directory.Delete(InfoDirPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// フォルダの移動
            /// </summary>
            /// <param name="BeforePath">移動させるフォルダのフォルダパス</param>
            /// <param name="AfterPath">移動するフォルダのパス</param>
            /// <param name="UserData">ユーザーデータ(ログを残す者)(任意)</param>
            /// <returns></returns>
            public static IOResult Move(string BeforePath, string AfterPath, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryMove", BeforePath + " | " + AfterPath);

                string BeforeDataPath = GetDataPathFromPath(BeforePath);
                string BeforeInfoDirPath = GetInfoDirPathFromPath(BeforePath);
                string BeforeInfoPath = GetInfoPathFromPath(BeforePath, true);

                string AfterDataPath = GetDataPathFromPath(AfterPath);
                string AfterInfoPath = GetInfoPathFromPath(AfterPath, true);

                string MovePath = AfterDataPath + @"\" + Path.GetFileName(BeforeDataPath);
                string MoveInfoDirPath = GetInfoDirPathFromPath(AfterPath) + @"\" + Path.GetFileName(BeforePath);
                string MoveInfoPath = GetInfoDirPathFromPath(AfterPath) + @"\" + GetInfoPathFromPath(BeforePath, true);

                if (!Directory.Exists(BeforeDataPath)) return IOResult.NotExistError;
                if (!Directory.Exists(AfterDataPath)) return IOResult.NotExistError;
                if (Directory.Exists(MovePath)) return IOResult.ExistError;

                if (GetOnlyUserLevelByIniFile(BeforeInfoPath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;
                if (GetOnlyUserLevelByIniFile(AfterInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfDelete;

                Directory.Move(BeforeDataPath, MovePath);
                Directory.Move(BeforeInfoDirPath, MoveInfoDirPath);
                File.Move(BeforeInfoPath, MoveInfoPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// フォルダのコピー
            /// </summary>
            /// <param name="SourcePath">コピーするフォルダパス</param>
            /// <param name="PastePath">貼り付けするフォルダのパス(注意:貼り付けるフォルダのパスです。)</param>
            /// <param name="UserData"></param>
            /// <returns></returns>
            public static IOResult Copy(string SourcePath, string PastePath, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryCopy", SourcePath + " | " + PastePath);

                string SourceDataPath = GetDataPathFromPath(SourcePath);
                string SourceInfoDirPath = GetInfoDirPathFromPath(SourcePath);
                string SourceInfoPath = GetInfoPathFromPath(SourcePath, true);

                string PasteDataPath = GetDataPathFromPath(PastePath);
                string PasteInfoPath = GetInfoPathFromPath(PastePath, true);

                string CopyPath = PasteDataPath + @"\" + Path.GetFileName(SourceDataPath);
                string CopyInfoDirPath = GetInfoDirPathFromPath(PastePath) + @"\" + Path.GetFileName(SourcePath);
                string CopyInfoPath = GetInfoDirPathFromPath(PastePath) + @"\" + GetInfoPathFromPath(SourcePath, true);

                if (!Directory.Exists(SourcePath)) return IOResult.NotExistError;
                if (!Directory.Exists(PastePath)) return IOResult.NotExistError;
                if (Directory.Exists(CopyPath)) return IOResult.ExistError;

                if (GetOnlyUserLevelByIniFile(SourceInfoPath, "Access", "Read") > UserData.Level) return IOResult.LevelMissOfDelete;
                if (GetOnlyUserLevelByIniFile(PasteInfoPath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfDelete;

                CopyDirectory(SourceDataPath, CopyPath);
                CopyDirectory(SourceInfoDirPath, CopyInfoDirPath);
                File.Copy(SourceInfoPath, CopyInfoPath);

                return IOResult.Finished;
            }
            /// <summary>
            /// ファイル一覧取得
            /// </summary>
            /// <param name="Path">フォルダパス</param>
            /// <param name="UserData">ログを残すもの(任意)</param>
            /// <returns></returns>
            public static (string XML, IOResult Result) Read(string Path, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryRead", Path);

                string ReadDataPath = GetDataPathFromPath(Path);
                string ReadInfoPath = GetInfoPathFromPath(Path, true);

                if (!Directory.Exists(ReadDataPath)) return (null, IOResult.NotExistError);
                if (GetOnlyUserLevelByIniFile(ReadInfoPath, "Access", "Read") > UserData.Level) return (null, IOResult.LevelMissOfRead);

                var files = Directory.GetFiles(ReadDataPath, "*", SearchOption.TopDirectoryOnly);
                var dirs = Directory.GetDirectories(ReadDataPath, "*", SearchOption.TopDirectoryOnly);

                var st = new StringBuilder(XmlHeader());
                st.AppendLine("<Root>");

                foreach (var file in files)
                {
                    string fileIni = GetInfoPathFromPath(GetRelativePath(file), false);
                    var fileDat = parser.ReadFile(fileIni, Encoding.UTF8);
                    if (!(GetOnlyUserLevelByIniFile(fileIni, "Access", "View") > UserData.Level))
                    {
                        st.AppendLine("<File>");
                        st.AppendLine($"<Name>{IO.Path.GetFileName(file)}</Name>");
                        st.AppendLine($"<Description>{fileDat["Info"]["Description"]}</Description>");
                        st.AppendLine($"<Size>{new IO.FileInfo(file).Length}</Size>");
                        st.AppendLine($"<LastWroteUser>{fileDat["User"]["LastWrote"]}</LastWroteUser>");
                        st.AppendLine($"<LastReadUser>{fileDat["User"]["LastRead"]}</LastReadUser>");
                        st.AppendLine($"<MakeUser>{fileDat["User"]["MakeUser"]}</MakeUser>");
                        st.AppendLine($"<LastWroteTime>{DateTime.Parse(fileDat["Time"]["LastWrote"])}</LastWroteTime>");
                        st.AppendLine($"<LastReadTime>{DateTime.Parse(fileDat["Time"]["LastRead"])}</LastReadTime>");
                        st.AppendLine($"<MakeTime>{DateTime.Parse(fileDat["Time"]["MakeTime"])}</MakeTime>");
                        st.AppendLine($"<ReadAccess>{fileDat["Access"]["Read"]}</ReadAccess>");
                        st.AppendLine($"<WriteAccess>{fileDat["Access"]["Write"]}</WriteAccess>");
                        st.AppendLine($"<ViewAccess>{fileDat["Access"]["View"]}</ViewAccess>");
                        st.AppendLine($"<DeleteAccess>{fileDat["Access"]["Delete"]}</DeleteAccess>");
                        st.AppendLine("</File>");
                    }
                }
                foreach (var dir in dirs)
                {
                    string dirIni = GetInfoPathFromPath(GetRelativePath(dir), true);
                    var dirDat = parser.ReadFile(dirIni, Encoding.UTF8);
                    if (!(GetOnlyUserLevelByIniFile(dirIni, "Access", "View") > UserData.Level))
                    {
                        st.AppendLine("<Directory>");
                        st.AppendLine($"<Name>{IO.Path.GetFileName(dir)}</Name>");
                        st.AppendLine($"<Description>{dirDat["Info"]["Description"]}</Description>");
                        st.AppendLine($"<Size>{GetDirectorySize(new DirectoryInfo(dir))}</Size>");
                        st.AppendLine($"<ReadAccess>{dirDat["Access"]["Read"]}</ReadAccess>");
                        st.AppendLine($"<WriteAccess>{dirDat["Access"]["Write"]}</WriteAccess>");
                        st.AppendLine($"<ViewAccess>{dirDat["Access"]["View"]}</ViewAccess>");
                        st.AppendLine($"<DeleteAccess>{dirDat["Access"]["Delete"]}</DeleteAccess>");
                        st.AppendLine("</Directory>");
                    }
                }

                st.AppendLine("</Root>");

                return (st.ToString(), IOResult.Finished);
            }
            /// <summary>
            /// 説明変更
            /// </summary>
            /// <param name="Path">パス</param>
            /// <param name="Description">説明</param>
            /// <param name="UserData">ログを残すもの(任意)</param>
            /// <returns></returns>
            public static IOResult SetDescription(string Path, string Description, UserData UserData = null)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryDescription", Path);

                string WritePath = GetInfoPathFromPath(Path, true);

                if (!Directory.Exists(WritePath)) return IOResult.NotExistError;
                if (GetOnlyUserLevelByIniFile(WritePath, "Access", "Write") > UserData.Level) return IOResult.LevelMissOfDelete;

                var data = parser.ReadFile(WritePath, Encoding.UTF8);

                data["Info"]["Description"] = Description;

                parser.WriteFile(WritePath, data, Encoding.UTF8);

                return IOResult.Finished;
            }
            /// <summary>
            /// アクセス権を変更
            /// </summary>
            /// <param name="Path">ファイルパス</param>
            /// <param name="UserData">ログを残す者（任意）</param>
            /// <param name="Read">読み込みの権限(任意)</param>
            /// <param name="Write">書き込みの権限(任意)</param>
            /// <param name="View">表示の権限(任意)</param>
            /// <param name="Delete">削除の権限(任意)</param>
            public static IOResult SetAccess(string Path, UserData UserData = null, UserData.UserLevel Read = UserData.UserLevel.Nothing, UserData.UserLevel Write = UserData.UserLevel.Nothing, UserData.UserLevel View = UserData.UserLevel.Nothing, UserData.UserLevel Delete = UserData.UserLevel.Nothing)
            {
                if (UserData == null) UserData = SystemGeneralUser;
                LogWriteLineByBase(UserData, "DirectoryAccess", Path);

                string WritePath = GetInfoPathFromPath(Path, true);

                if (!Directory.Exists(WritePath)) return IOResult.NotExistError;

                var parents = GetParentsDirectoryFromPath(WritePath);
                foreach (var pl in parents)
                {
                    string parent = GetInfoPathFromPath(pl, true);

                    var parentData = parser.ReadFile(parent, Encoding.UTF8);

                    var parentRead = StringToUserLevel(parentData["Access"]["Read"]);
                    var parentWrite = StringToUserLevel(parentData["Access"]["Write"]);
                    var parentView = StringToUserLevel(parentData["Access"]["View"]);
                    var parentDelete = StringToUserLevel(parentData["Access"]["Delete"]);

                    if (parentRead > Read || parentWrite > Write || parentView > View || parentDelete > Delete) return IOResult.YoursIsUpperThanParent;
                }

                if (GetOnlyUserLevelByIniFile(WritePath, "Access", "Delete") > UserData.Level) return IOResult.LevelMissOfDelete;

                if ((Read > UserData.Level) ||
                    (Write > UserData.Level) ||
                    (View > UserData.Level) ||
                    (Delete > UserData.Level))
                {
                    return IOResult.CanNotSetUpperThanYours;
                }

                var data = parser.ReadFile(WritePath, Encoding.UTF8);

                if (Read != UserData.UserLevel.Nothing) data["Access"]["Read"] = Read.ToString();
                if (Write != UserData.UserLevel.Nothing) data["Access"]["Write"] = Write.ToString();
                if (View != UserData.UserLevel.Nothing) data["Access"]["View"] = View.ToString();
                if (Delete != UserData.UserLevel.Nothing) data["Access"]["Delete"] = Delete.ToString();

                parser.WriteFile(WritePath, data, Encoding.UTF8);

                return IOResult.Finished;
            }
        }
    }
}
