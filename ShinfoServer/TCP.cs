using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShinfoServer
{
    internal class TCP
    {
        public delegate void TCPEventHandler(object sender, TCPEventArgs e);

        //イベントデリゲートの宣言
        public event TCPEventHandler Get;
        public event EventHandler NewClient;
        public event EventHandler RemoveClient;

        public TcpListener server;
        public int port;

        public List<Client> clients;

        public Random rnd = new Random();

        /// <summary>
        /// サーバーを起動します。
        /// </summary>
        /// <param name="port">ポート番号</param>
        public TCP(int port)
        {
            this.port = port;

            this.clients = new List<Client>();

            server = new TcpListener(IPAddress.Any, port);
        }
        public void ListenStart()
        {
            server.Start();
            BackGroundProcess();

            _ = Acceptwait_Async();
        }
        private async Task Acceptwait_Async()
        {
            while (true)
            {
                Debug.WriteLine($"Acceptwaitスレッド:{Thread.CurrentThread.ManagedThreadId}");

                TcpClient client;

                try
                {
                    //接続待ち (非同期実行)
                    client = await server.AcceptTcpClientAsync();
                }
                catch (System.ObjectDisposedException)
                {
                    return;
                }


                if (true)//Keepaliveを使う場合
                {
                    //Keepaliveを使う場合
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    byte[] tcp_keepalive = new byte[12];
                    BitConverter.GetBytes((Int32)1).CopyTo(tcp_keepalive, 0);//onoffスイッチ.
                    BitConverter.GetBytes((Int32)2000).CopyTo(tcp_keepalive, 4);//wait time.(ms)
                    BitConverter.GetBytes((Int32)500).CopyTo(tcp_keepalive, 8);//interval.(ms)
                                                                               // keep-aliveのパラメータ設定
                    client.Client.IOControl(IOControlCode.KeepAliveValues, tcp_keepalive, null);
                }

                //クライアントの追加
                Client client_data = new Client();
                client_data.client = client;
                client_data.user = new UserData() { username = "NULL", password = "NULL", connect = DateTime.Now };
                clients.Add(client_data);

                NewClient?.Invoke(this, new EventArgs());
                Console.WriteLine("new client!");

                //受信タスクを開始
                _ = Recievewait_Async(client, client_data);

            }
        }
        //非同期でクライアントから文字列受信を待ち受ける
        private async Task Recievewait_Async(TcpClient client, Client client_data)
        {
            var ns = client.GetStream();
            while (true)
            {
                var ms = new MemoryStream();
                byte[] result_bytes = new byte[16];

                Debug.WriteLine($"Receiveスレッド:{Thread.CurrentThread.ManagedThreadId}");

                do
                {
                    int result_size = 0;

                    try
                    {
                        //受信 (非同期実行)
                        result_size = await ns.ReadAsync(result_bytes, 0, result_bytes.Length);
                    }
                    catch (System.IO.IOException)
                    {
                        //LANケーブルが抜けたときKeepaliveによってこの例外が発生する
                    }

                    if (result_size == 0)
                    {
                        //受信サイズが0のとき切断とみなし クライアントの削除
                        //リストから削除する
                        clients.Remove(client_data);
                        //Clientを閉じる
                        client.Close();

                        //一応メモリ破棄
                        ms.Close();
                        ms.Dispose();

                        RemoveClient?.Invoke(this, new EventArgs());

                        //受信待ちをやめるため、関数を抜ける
                        Debug.WriteLine($"Receiveスレッド:{Thread.CurrentThread.ManagedThreadId}:抜ける");
                        return;
                    }

                    ms.Write(result_bytes, 0, result_size);

                } while (ns.DataAvailable);

                Get?.Invoke(this, new TCPEventArgs() { ClientData = client_data, Client = client, Message = ms.ToArray() });
            }
        }
        public void BackGroundProcess()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    //明示的な削除対象のクライアントを削除
                    //接続してからログインせずに1分経過したクライアントを削除(暗黙的)
                    var cl = new List<Client>();
                    var oneMinute = new TimeSpan(0, 1, 0);
                    foreach (var c in clients)
                    {
                        if (c.isRemove) cl.Add(c);
                        else if (c.user.username == "NULL" && c.user.password == "NULL" && DateTime.Now - c.user.connect >= oneMinute) cl.Add(c);
                    }
                    foreach (var c in cl) clients.Remove(c);

                    Thread.Sleep(1000);
                }
            });
        }
        public class TCPEventArgs : EventArgs
        {
            public byte[] Message;
            public Client ClientData;
            public TcpClient Client;
        }
    }
    internal class Client
    {
        public TcpClient client;
        public UserData user;

        public bool isRemove;
    }
}
