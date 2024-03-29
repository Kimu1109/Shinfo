using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ShinfoServer.Data;

namespace ShinfoServer
{
    internal static partial class Process
    {
        internal static void GetProcess(Client client_data, byte[] bytes)
        {
            Console.WriteLine($"GetRequest:{Encoding.UTF8.GetString(bytes)}");

            if (!client_data.user.IsStart)
            {
                client_data.user.IsStart = true;
                Console.WriteLine("set is start → true");
            }
            else
            {
                client_data.user.CatchRequest = Encoding.UTF8.GetString(bytes);
                client_data.user.IsCatchBool = true;
            }
        }
        internal static void SendProcess(Client client_data, byte[] bytes)
        {
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
                Send(client_data, "More");
                return;
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
                Send(client_data, "More");
                return;
            }

            char switchChar = (char)bytes[6]; //識別文字
            int chunkSize = BitConverter.ToInt32(new byte[] { bytes[7], bytes[8], bytes[9], bytes[10] }, 0); //サイズ
            var mainData = new byte[bytes.Length - 6 - 6 - 1 - 4]; //6 = QFPDED分, 6 = QFPDST分, 1 = 識別文字分, 4 = チャンクサイズ分

            //サイズの確認
            if (chunkSize != bytes.Length)
            {
                Send(client_data, "More");
                return;
            }

            Array.Copy(bytes, 6 + 1 + 4, mainData, 0, mainData.Length);

            if (client_data.user.getBytes == null) client_data.user.getBytes = new byte[0];
            client_data.user.getBytes = client_data.user.getBytes.Concat(mainData).ToArray();
            switch (switchChar)
            {
                case 'n'://normal
                    Send(client_data, "Next");
                    break;
                case 'f'://final
                    if (client_data.user.getBytes.Length == client_data.user.AllSize)
                    {
                        Send(client_data, "Thx");
                        client_data.user.getBytes = Decompress(client_data.user.getBytes);
                        client_data.user.FinishedGet = true;
                        Console.WriteLine("FINISH-GET!!!!!!!!!!!!!!!!!!!!!");
                    }
                    else
                        Send(client_data, "Cancel");
                    break;
            }
        }
        //クライアントに文字列送信
        internal static void Send(Client client_data, string message)
        {
            byte[] message_byte = Encoding.UTF8.GetBytes(message);

            TcpClient client = client_data.client;

            try
            {
                Console.WriteLine("send -> " + message);
                var ns = client.GetStream();
                ns.Write(message_byte, 0, message_byte.Length);
            }
            catch (System.ObjectDisposedException)
            {
                //切断されたオブジェクトにアクセスするとこうなる
            }
            catch (System.InvalidOperationException)
            {
                //切断と同時に送信した場合この例外が発生する
            }

        }
        internal static void SendFile(Client client_data, byte[] byteArr)
        {
            Task.Run(() =>
            {
                client_data.user.IsSendMode = true;

                var Stopwatch = new Stopwatch();
                double BytesPerMillisecond = -1;
                double BytesPerMillisecond_Buf;

                int CHUNK_DATA_SIZE = 1024;

                const int CHUNK_DATA_START = 6;
                const int CHUNK_DATA_SELECT = 1;
                const int CHUNK_DATA_LENGTH = 4;
                const int CHUNK_HEADER = CHUNK_DATA_SELECT + CHUNK_DATA_LENGTH + CHUNK_DATA_START;
                const int CHUNK_FOOTER = 6;

                byteArr = Compress(byteArr);

                var ns = client_data.client.GetStream();

                //前提情報の送信
                Send(client_data, $"QFPDST/1000/{byteArr.Length}");
                while (!client_data.user.IsStart) { Thread.Sleep(50); }

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
                        SendBytes(client_data, OneChunk);

                        client_data.user.IsCatchBool = false;
                        while (!client_data.user.IsCatchBool) { Thread.Sleep(1); }

                        Console.WriteLine($"caught : {client_data.user.CatchRequest}");

                        switch (client_data.user.CatchRequest)
                        {
                            case "Thx":
                            case "Next":
                                IsNext = true;
                                break;
                            case "More":
                                break;
                            case "Cancel":
                                return;
                        }

                    }

                    count += CHUNK_DATA_SIZE;

                    Stopwatch.Stop();

                    BytesPerMillisecond_Buf = chunkSize / (Stopwatch.ElapsedMilliseconds / 1000d);
                    if (BytesPerMillisecond == -1) BytesPerMillisecond = BytesPerMillisecond_Buf;
                    CHUNK_DATA_SIZE = Math.Max(1024, CHUNK_DATA_SIZE + (int)(BytesPerMillisecond - BytesPerMillisecond_Buf) + 256);

                }

                Console.WriteLine("データの転送完了!");
                client_data.isRemove = true;
            });
        }
        internal static void SendBytes(Client client_data, byte[] bytes)
        {
            Console.WriteLine("バイトを送ります! 長さ:" + bytes.Length.ToString());
            TcpClient client = client_data.client;

            try
            {
                var ns = client.GetStream();
                do
                {
                    ns.Write(bytes, 0, bytes.Length);
                } while (ns.DataAvailable);

            }
            catch (System.InvalidOperationException)
            {
                //切断と同時に送信した場合この例外が発生する
            }
        }
        internal static async Task<byte[]> ReadBytes(Client client_data)
        {
            var task = Task.Run(() =>
            {
                var TimeoutTcpStart = DateTime.Now.AddSeconds(3);
                while (!client_data.user.IsGetMode)
                {
                    if (TimeoutTcpStart < DateTime.Now) return null;
                    Thread.Sleep(20);
                }
                while (client_data.user.IsGetMode)
                {
                    if (client_data.user.FinishedGet)
                    {
                        return client_data.user.getBytes;
                    }
                    Thread.Sleep(20);
                }
                return null;
            });
            await task;
            return task.Result;
        }
        internal static Task<bool> WriteFileByTcp(string path, UserData user, Client client_data)
        {
            var task = Task.Run(() =>
            {
                var TimeoutTcpStart = DateTime.Now.AddSeconds(3);
                while (!client_data.user.IsGetMode)
                {
                    if (TimeoutTcpStart < DateTime.Now) return false;
                    Thread.Sleep(20);
                }
                while (client_data.user.IsGetMode)
                {
                    if (client_data.user.FinishedGet)
                    {
                        FileControl.Write(path, user, client_data.user.getBytes);
                        return true;
                    }
                    Thread.Sleep(20);
                }
                return false;
            });
            return task;
        }
    }
}
