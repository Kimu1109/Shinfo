﻿using System;
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

namespace ShinfoServer
{
    internal static class Data
    {
        internal static IpcServerChannel Channel;
        

        internal static ObservableCollection<UserData> Users = new ObservableCollection<UserData>();
        internal static ObservableCollection<GroupData> Groups = new ObservableCollection<GroupData>();

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

            Channel = new IpcServerChannel("ShinfoServer");
            ChannelServices.RegisterChannel(Channel, true);
            RemotingServices.Marshal(tcp.clients, "TestObj", typeof(ObservableCollection<Client>));

            foreach (var file in Directory.GetFiles(AppPath + "\\data\\User"))
            {
                Users.Add(UserData.ParseFromFile(file));
            }
            if (File.Exists(AppPath + "\\data\\Group.xml")) Groups = new ObservableCollection<GroupData>(GroupData.ParseFromFile(AppPath + "\\data\\Group.xml"));
        }
    }
}
