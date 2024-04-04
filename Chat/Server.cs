using IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chat
{
    public static class Server
    {
        public const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n";
        public const string Tab1 = "\t";
        public const string Tab2 = "\t\t";
        public const string Tab3 = "\t\t\t";

        public class ChatData
        {
            public string GroupID;
            public List<OneChatData> Chats;
        }
        public class OneChatData
        {
            public string ID;
            public DateTime Time;
            public string Msg;
        }

        public static List<ChatData> Chats = new List<ChatData>();

        //args -> 0:"Chat", 1:"Send", 2:groupID, 3:userID, 4:msg
        public static void Send(TcpClient client, byte[] message)
        {
            string Message = Encoding.UTF8.GetString(message);
            var MsgArr = Message.Split('/');

            if (MsgArr[0] == "Chat" && MsgArr[1] == "Send")
            {
                string groupID = MsgArr[2];
                string userID = MsgArr[3];
                string msg = MsgArr[4].ToMsg();

                foreach(var c in Chats)
                {
                    if(c.GroupID == groupID)
                    {
                        c.Chats.Add(new OneChatData()
                        {
                            ID = userID,
                            Time = DateTime.Now,
                            Msg = msg
                        });
                        break;
                    }
                }
            }
        }
        //args -> 0:"Chat", 1:"Reload", 2:groupID
        public static void Reload(TcpClient client, byte[] message)
        {
            string Message = Encoding.UTF8.GetString(message);
            var MsgArr = Message.Split('/');

            if (MsgArr[0] == "Chat" && MsgArr[1] == "Reload")
            {
                string groupID = MsgArr[2];

                foreach(var c in Chats)
                {
                    if(c.GroupID == groupID)
                    {
                        var st = new StringBuilder(XmlHeader);
                        st.AppendLine("<Root>");
                        foreach(var cd in c.Chats)
                        {
                            st.AppendLine(Tab1 + "<Chat>");

                            st.AppendLine(Tab2 + "<ID>" + cd.ID + "</ID>");
                            st.AppendLine(Tab2 + "<Time>" + cd.Time.ToString().ToXML() + "</Time>");
                            st.AppendLine(Tab2 + "<Msg>" + cd.Msg.ToXML() + "</Msg>");

                            st.AppendLine(Tab1 + "</Chat>");
                        }
                        st.AppendLine("</Root>");

                        TcpForServer.UploadBytes(client, Encoding.UTF8.GetBytes(st.ToString()));
                        break;
                    }
                }
            }
        }
    }
}
