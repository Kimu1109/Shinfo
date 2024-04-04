using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using ShinfoServer;
using System.Net.Sockets;
using System.Xml.Linq;
using System.Reflection;

namespace ShinfoServer
{
    internal static class Data
    {        
        internal static ObservableCollection<UserData> Users = new ObservableCollection<UserData>();
        internal static ObservableCollection<GroupData> Groups = new ObservableCollection<GroupData>();

        internal static List<DllData> DllData = new List<DllData> ();

        internal static BitmapImage GroupIcon = new BitmapImage(new Uri(AppPath + "\\img\\Home.png"));
        internal static BitmapImage UserIcon = new BitmapImage(new Uri(AppPath + "\\img\\Account.png"));

        internal const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        internal const string Tab1 = "\t";
        internal const string Tab2 = "\t\t";
        internal const string Tab3 = "\t\t\t";

        internal static TCP tcp;
        internal static string AppPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        internal static UserData SystemAdminUser = new UserData() { Name = "admin", password = "admin", Level = UserData.UserLevel.Admin };
        internal static UserData SystemGeneralUser = new UserData() { Name = "general", password = "general", Level = UserData.UserLevel.General };

        internal static void InitData()
        {
            tcp = new TCP(2001);
            tcp.ListenStart();
            tcp.BackGroundProcess();

            foreach (var file in Directory.GetFiles(AppPath + "\\data\\User"))
            {
                Users.Add(UserData.ParseFromFile(file));
            }
            if (File.Exists(AppPath + "\\data\\Group.xml")) Groups = new ObservableCollection<GroupData>(GroupData.ParseFromFile(AppPath + "\\data\\Group.xml"));

            var xml = XElement.Load(AppPath + "\\plugin\\config.xml");

            foreach(var dll in xml.Elements("DLL"))
            {
                var assembly = Assembly.LoadFile(AppPath + "\\plugin\\DLL\\" + xml.Element("RelativePath").Value);
                foreach(var command in dll.Element("Commands").Elements("Command"))
                {
                    string From = command.Element("From").Value;
                    string Class = command.Element("Class").Value;
                    string Method = command.Element("Method").Value;

                    DllData.Add(new DllData(assembly, Class, Method, From));
                }
            }

        }
    }
}
