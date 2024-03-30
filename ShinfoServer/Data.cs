using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace ShinfoServer
{
    internal static class Data
    {
        internal static ObservableCollection<UserData> Users = new ObservableCollection<UserData>();
        internal static ObservableCollection<GroupData> Groups = new ObservableCollection<GroupData>();

        internal static BitmapImage GroupIcon = new BitmapImage(new Uri(AppPath + "\\img\\Home.png"));
        internal static BitmapImage UserIcon = new BitmapImage(new Uri(AppPath + "\\img\\Account.png"));

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

        }
    }
}
