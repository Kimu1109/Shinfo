using IPC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat
{
    public static class TcpForServer
    {
        public static void UploadBytes(TcpClient client, byte[] bytes)
        {
            Task.Run(() =>
            {
                var Stopwatch = new Stopwatch();
                double BytesPerMillisecond = -1;
                double BytesPerMillisecond_Buf;

                int CHUNK_DATA_SIZE = 1024;

                const int CHUNK_DATA_START = 6;
                const int CHUNK_DATA_SELECT = 1;
                const int CHUNK_DATA_LENGTH = 4;
                const int CHUNK_HEADER = CHUNK_DATA_SELECT + CHUNK_DATA_LENGTH + CHUNK_DATA_START;
                const int CHUNK_FOOTER = 6;

                bytes = bytes.Compress();

                var ns = client.GetStream();

                //前提情報の送信
                SendBytes(client, Encoding.UTF8.GetBytes($"QFPDST/1000/{bytes.Length}"));
                GetBytes(client);

                for (int count = 0; count < bytes.Length;)
                {
                    Stopwatch.Restart();

                    int CHUNK_SIZE = CHUNK_DATA_SIZE + CHUNK_HEADER + CHUNK_FOOTER;

                    int chunkSize = Math.Min(CHUNK_SIZE, bytes.Length - count + CHUNK_HEADER + CHUNK_FOOTER);
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
                    Array.Copy(bytes, count, OneChunk, CHUNK_HEADER, datSize);

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
                        SendBytes(client, OneChunk);

                        switch (Encoding.UTF8.GetString(GetBytes(client)))
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
            });
        }
        public static void SendBytes(TcpClient client, byte[] bytes)
        {
            var ns = client.GetStream();
            ns.Write(bytes, 0, bytes.Length);
        }
        public static byte[] GetBytes(TcpClient client)
        {
            var ns = client.GetStream();
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
            return ms.ToArray();
        }

    }
}
