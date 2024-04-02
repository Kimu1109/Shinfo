using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Shinfo
{
    internal partial class TCP : IDisposable
    {
        public class GetEventArgs : EventArgs
        {
            public string msg;
        }

        public event EventHandler<GetEventArgs> GetMsgFromAsync;

        public TcpClient client;
        private NetworkStream ns;

        public string ip;
        public int port;

        public string password;
        public string ID;

        private const int TIME_OUT = 10;

        private Thread getTask;

        /// <summary>
        /// クラスを作成すると同時に、サーバーに接続します。
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="password"></param>
        /// <param name="ID"></param>
        public TCP(string ip, int port, string password, string ID)
        {
            this.ip = ip;
            this.port = port;

            this.password = password;
            this.ID = ID;

            client = new TcpClient(ip, port);
            client.SendTimeout = TIME_OUT;
            ns = client.GetStream();
        }
        /// <summary>
        /// COMタイプのログイン
        /// </summary>
        /// <param name="source"></param>
        /// <exception cref="Exception">comタイプのログイン失敗</exception>
        public TCP(TCP source)
        {
            this.ip = source.ip;
            this.port = source.port;

            this.password = source.password;
            this.ID = source.ID;

            client = new TcpClient(ip, port);
            client.SendTimeout = TIME_OUT;
            ns = client.GetStream();

            if (!ComLogin()) throw new Exception("エラー:comタイプでログインできませんでした。");
        }
        /// <summary>
        /// ログインします。
        /// </summary>
        /// <returns></returns>
        public bool Login()
        {
            Send("login/" + ID + "/" + password);
            string log = Get();
            var logArr = log.Split('/');
            switch (logArr[0])
            {
                case "success":
                    return true;
                case "failed":
                default:
                    return false;
            }
        }
        /// <summary>
        /// 通信を終了します。
        /// サーバーサイドに終了のメッセージも行います。
        /// 主にCOMモードの終了及び、その他処理の終了につかわれます。
        /// </summary>
        public void Close()
        {
            Send("close");
            StopGet();
            client.Close();
            ns.Dispose();
        }
        /// <summary>
        /// 破棄
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        /// <summary>
        /// 特定の文字列をUTF8形式に変更して送信します。
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="ns"></param>
        public void Send(string msg, NetworkStream ns = null)
        {
            if (ns == null) ns = this.ns;
            var bytes = Encoding.UTF8.GetBytes(msg);
            ns.Write(bytes, 0, bytes.Length);

            Console.WriteLine("Client-Send:" + msg);
        }
        /// <summary>
        /// 文字列を受信します。
        /// これらの処理はスレッドをブロックします。
        /// </summary>
        /// <param name="ns"></param>
        /// <returns></returns>
        public string Get(NetworkStream ns = null)
        {
            try
            {
                if (ns == null) ns = this.ns;
                //サーバーから送られたデータを受信する
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                byte[] resBytes = new byte[256];
                int resSize = 0;
                do
                {
                    //データの一部を受信する
                    resSize = ns.Read(resBytes, 0, resBytes.Length);
                    //Readが0を返した時はサーバーが切断したと判断
                    if (resSize == 0)
                    {
                        Console.WriteLine("サーバーが切断しました。");
                        throw new Exception("Disconnected from server.");
                    }
                    //受信したデータを蓄積する
                    ms.Write(resBytes, 0, resSize);
                    //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                    // 受信を続ける
                } while (ns.DataAvailable);
                //受信したデータを文字列に変換
                string resMsg = Encoding.UTF8.GetString(ms.ToArray(), 0, (int)ms.Length);

                ms.Close();
                return resMsg;
            }
            catch
            {
                throw new Exception("Disconnected from server.");
            }
        }
        public byte[] GetToBytes(NetworkStream ns = null)
        {
            if (ns == null) ns = this.ns;
            var ms = new MemoryStream();
            byte[] result_bytes = new byte[16];

            do
            {
                int result_size = 0;

                try
                {
                    //受信 (非同期実行)
                    result_size = ns.ReadAsync(result_bytes, 0, result_bytes.Length).Result;
                }
                catch (System.IO.IOException)
                {
                    //LANケーブルが抜けたときKeepaliveによってこの例外が発生する
                }

                if (result_size == 0)
                {
                    //Clientを閉じる
                    client.Close();

                    //一応メモリ破棄
                    ms.Close();
                    ms.Dispose();
                    break;
                }

                ms.Write(result_bytes, 0, result_size);

            } while (ns.DataAvailable);
            return ms.ToArray();
        }
        /// <summary>
        /// 非同期で実行される受信を強制的に終了します。
        /// </summary>
        public void StopGet()
        {
            if (getTask != null)
            {
                getTask.Abort();
                getTask = null;
            }
        }
        /// <summary>
        /// 非同期で受信を開始します。
        /// 受信すると、GetMsgFromAsyncイベントを返します。
        /// </summary>
        public void GetAsync()
        {
            if (getTask != null) return;
            getTask = new Thread(() =>
            {
                while (true)
                {
                    var ret = Get();
                    GetMsgFromAsync?.Invoke(this, new GetEventArgs() { msg = ret });
                }
            });
            getTask.Start();
        }

    }
    internal partial class TCP
    {
        /// <summary>
        /// COMモードでログインします。
        /// 基本的にファイルの通信などに用います。
        /// </summary>
        /// <returns></returns>
        public bool ComLogin()
        {
            Send("com-login/" + password + "/" + ID);
            string log = Get();
            switch (log)
            {
                case "success":
                    return true;
                case "failed":
                default:
                    return false;
            }
        }

        /// <summary>
        /// ファイルをアップロードします。
        /// これらの処理はSendFile関数を使用します。
        /// </summary>
        /// <param name="fileName">送信するファイルパスです。</param>
        /// <returns></returns>
        public bool Upload(string fileName)
        {
            var com = new TCP(ip, port, password, ID);
            com.ComLogin();
            bool ret = com.SendFile(System.IO.File.ReadAllBytes(fileName)).Result;
            com.Close();
            return ret;

        }
        /// <summary>
        /// ファイルをダウンロードします。
        /// これらの処理はGet_Process関数を使用します。
        /// </summary>
        /// <param name="downloadPath"></param>
        /// <returns></returns>
        public async Task Download(string downloadPath)
        {
            var com = new TCP(ip, port, password, ID);
            com.ComLogin();
            com.Send("file/read/" + downloadPath.ToStr());
            var arr = com.Get().Split('/');
            if (arr[0] == "file" && arr[1] == "read")
            {
                switch (arr[2])
                {
                    case "success":
                        Process.CreateFile(downloadPath);
                        using (var fs = new FileStream(Data.AppPath + "\\data\\file\\" + downloadPath, FileMode.Create, FileAccess.Write))
                        {
                            var bytes = await com.Download(true);
                            fs.Write(bytes, 0, bytes.Length);
                        }
                        break;
                    case "failed":
                        MessageBox.Show("ダウンロードに失敗しました。\n" + arr[3]);
                        break;
                }
            }
            else MessageBox.Show("ダウンロードに失敗しました。\nサーバーからの応答がありません。");
        }
        public async Task<byte[]> Download(bool IsShowDialog = false)
        {
            ulong _AllSize;
            byte[] getBytes = null;

            string header = Get();
            var headerArr = header.Split('/');
            if (headerArr[0] == "QFPDST")
            {
                Console.WriteLine($"QFPDST ver → {headerArr[1]}");
                _AllSize = ulong.Parse(headerArr[2]);
            }
            else
            {
                throw new Exception("ヘッダーエラー NOT QFPDST");
            }
            Send("QFPLET");

            var fileCom = new FileCom();
            if (IsShowDialog)
            {
                fileCom.ProgressMax = _AllSize;
                fileCom.Show();
            }

            var task = Task.Run(() =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                while (true)
                {
                    var bytes = GetToBytes();

                    //チャンクの最初が正しいかどうか
                    if (!(
                        bytes[0] == (byte)'Q' &&
                        bytes[1] == (byte)'F' &&
                        bytes[2] == (byte)'P' &&
                        bytes[3] == (byte)'D' &&
                        bytes[4] == (byte)'S' &&
                        bytes[5] == (byte)'T'
                        ))
                    {
                        Send("More");
                    }

                    //チャンクの最後が正しいかどうか
                    if (!(
                        bytes[bytes.Length - 6] == (byte)'Q' &&
                        bytes[bytes.Length - 5] == (byte)'F' &&
                        bytes[bytes.Length - 4] == (byte)'P' &&
                        bytes[bytes.Length - 3] == (byte)'D' &&
                        bytes[bytes.Length - 2] == (byte)'E' &&
                        bytes[bytes.Length - 1] == (byte)'D'
                        ))
                    {
                        Send("More");
                        goto SKIP;
                    }

                    char switchChar = (char)bytes[6]; //識別文字
                    int chunkSize = BitConverter.ToInt32(new byte[] { bytes[7], bytes[8], bytes[9], bytes[10] }, 0); //サイズ
                    var mainData = new byte[bytes.Length - 6 - 6 - 1 - 4]; //6 = QFPDED分, 6 = QFPDST分, 1 = 識別文字分, 4 = チャンクサイズ分

                    //サイズの確認
                    if (chunkSize != bytes.Length)
                    {
                        Send("More");
                        goto SKIP;
                    }

                    Array.Copy(bytes, 6 + 1 + 4, mainData, 0, mainData.Length);

                    if (getBytes == null) getBytes = new byte[0];
                    getBytes = getBytes.Concat(mainData).ToArray();
                    switch (switchChar)
                    {
                        case 'n'://normal
                            Send("Next");
                            break;
                        case 'f'://final
                            if ((ulong)getBytes.Length == _AllSize)
                            {
                                Send("Thx");
                                getBytes = getBytes.Decompress();
                                Console.WriteLine("FINISH-GET!!!!!!!!!!!!!!!!!!!!!");

                                stopWatch.Stop();

                                if (IsShowDialog)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        fileCom.Close();
                                    });
                                }

                                return getBytes;
                            }
                            else
                            {
                                Send("Cancel");

                                stopWatch.Stop();

                                if (IsShowDialog)
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        fileCom.Close();
                                    });
                                }

                                throw new Exception("ダウンロードエラー\n正しくダウンロードできませんでした。");
                            }
                    }

                    if (IsShowDialog)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            fileCom.SetData(getBytes.Length, (ulong)getBytes.Length * 100Lu / _AllSize + "%完了", ((ulong)((double)getBytes.Length / ((double)stopWatch.ElapsedMilliseconds / 1000D))).SizeToString() + "/s", "残り" + ((ulong)(_AllSize - (ulong)getBytes.Length)).SizeToString());
                        });
                    }

                SKIP:;
                }
            });
            await task;

            return task.Result;
        }
        /// <summary>
        /// バイト配列を送信します。
        /// </summary>
        /// <param name="byteArr"></param>
        /// <returns></returns>
        public async Task<bool> SendFile(byte[] byteArr, bool IsShowDialog = true)
        {
            var start = DateTime.Now;

            var Stopwatch = new Stopwatch();
            double BytesPerMillisecond = -1;
            double BytesPerMillisecond_Buf;

            int CHUNK_DATA_SIZE = 1024;

            const int CHUNK_DATA_START = 6;
            const int CHUNK_DATA_SELECT = 1;
            const int CHUNK_DATA_LENGTH = 4;
            const int CHUNK_HEADER = CHUNK_DATA_SELECT + CHUNK_DATA_LENGTH + CHUNK_DATA_START;
            const int CHUNK_FOOTER = 6;

            byteArr = byteArr.Compress();

            FileCom filecom = null;
            if (IsShowDialog)
            {
                filecom = new FileCom();
                filecom.ProgressMax = byteArr.Length;
                filecom.Title = "アップロード";
                filecom.Show();
            }

            var task = Task.Run(() =>
            {
                //前提情報の送信
                Send($"QFPDST/1000/{byteArr.Length}");
                Get();

                for (int count = 0; count < byteArr.Length;)
                {
                    Stopwatch.Restart();

                    int CHUNK_SIZE = CHUNK_DATA_SIZE + CHUNK_HEADER + CHUNK_FOOTER;

                    int chunkSize = Math.Min(CHUNK_SIZE, byteArr.Length - count + CHUNK_HEADER + CHUNK_FOOTER);
                    int datSize = chunkSize - CHUNK_HEADER - CHUNK_FOOTER;

                    var OneChunk = new byte[chunkSize];

                    //header - 識別文字(QweekFileProtocolDataSTart)
                    OneChunk[0] = (byte)'Q';
                    OneChunk[1] = (byte)'F';
                    OneChunk[2] = (byte)'P';
                    OneChunk[3] = (byte)'D';
                    OneChunk[4] = (byte)'S';
                    OneChunk[5] = (byte)'T';

                    //header - 識別文字
                    if (chunkSize == CHUNK_SIZE)
                        OneChunk[6] = (byte)'n'; //normal
                    else
                        OneChunk[6] = (byte)'f'; //final

                    //header - データの長さ
                    var datLength = BitConverter.GetBytes(chunkSize);
                    OneChunk[7] = datLength[0];
                    OneChunk[8] = datLength[1];
                    OneChunk[9] = datLength[2];
                    OneChunk[10] = datLength[3];

                    //body - データのコピー
                    //for (int i = 0; i < datSize; i++) OneChunk[i + CHUNK_HEADER] = byteArr[count + i];
                    Array.Copy(byteArr, count, OneChunk, CHUNK_HEADER, datSize);

                    //footer - 識別文字(QweekFileProtocolDataEnD)
                    OneChunk[OneChunk.Length - 6] = (byte)'Q';
                    OneChunk[OneChunk.Length - 5] = (byte)'F';
                    OneChunk[OneChunk.Length - 4] = (byte)'P';
                    OneChunk[OneChunk.Length - 3] = (byte)'D';
                    OneChunk[OneChunk.Length - 2] = (byte)'E';
                    OneChunk[OneChunk.Length - 1] = (byte)'D';

                    //送信
                    bool IsNext = false;
                    while (!IsNext)
                    {
                        SendBytes(OneChunk);

                        switch (Get())
                        {
                            case "Thx":
                            case "Next":
                                IsNext = true;
                                break;
                            case "More":
                                break;
                            case "Cancel":
                                return false;
                        }
                    }

                    count += CHUNK_DATA_SIZE;

                    Stopwatch.Stop();

                    BytesPerMillisecond_Buf = chunkSize / (Stopwatch.ElapsedMilliseconds / 1000d);
                    if (BytesPerMillisecond == -1) BytesPerMillisecond = BytesPerMillisecond_Buf;
                    CHUNK_DATA_SIZE = Math.Max(1024, CHUNK_DATA_SIZE + (int)(BytesPerMillisecond - BytesPerMillisecond_Buf) + 256);

                    if (IsShowDialog)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            filecom.SetData(count, count * 100 / byteArr.Length + "%完了", ((ulong)BytesPerMillisecond_Buf).SizeToString() + "/s", "残り" + ((ulong)(byteArr.Length - count)).SizeToString());
                        });
                    }
                }

                Console.WriteLine($"FINISHED → {DateTime.Now - start}");
                Console.WriteLine($"送信速度:{byteArr.Length / (DateTime.Now - start).TotalSeconds}");

                if (IsShowDialog)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        filecom.Close();
                    });
                }

                return true;
            });

            await task;

            return task.Result;
        }
        /// <summary>
        /// 特定のバイト配列を送信します。
        /// これらは、直接使うことはあまりありません。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="ns"></param>
        public void SendBytes(byte[] bytes, NetworkStream ns = null)
        {
            if (ns == null) ns = this.ns;
            ns.Write(bytes, 0, bytes.Length); ;
            Console.WriteLine("バイトを送ります! " + bytes.Length);
        }
    }
}
