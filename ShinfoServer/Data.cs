using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ShinfoServer
{
    internal static class Data
    {
        internal static ObservableCollection<UserData> Users = new ObservableCollection<UserData>();
        internal static ObservableCollection<GroupData> Groups = new ObservableCollection<GroupData>();

        internal static TCP tcp;
        internal static string AppPath
        {
            get
            {
                return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }
        internal static UserData SystemAdminUser = new UserData() { username = "admin", password = "admin", userLevel = UserData.UserLevel.Admin };
        internal static UserData SystemGeneralUser = new UserData() { username = "general", password = "general", userLevel = UserData.UserLevel.General };

        internal static void InitData()
        {

        }
    }
}
