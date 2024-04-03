using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Shinfo
{
    /// <summary>
    /// Login.xaml の相互作用ロジック
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();

            if(File.Exists(Data.AppPath + "\\data\\LocalUser.xml"))
            {
                var xml = XElement.Load(Data.AppPath + "\\data\\LocalUser.xml");

                var server = xml.Element("Server");
                var user = xml.Element("User");

                TextBox_IP.Text = server.Element("IP").Value;
                TextBox_Port.Text = server.Element("Port").Value;
                TextBox_AESKey.Text = server.Element("AES").Value;

                TextBox_UserID.Text = user.Element("ID").Value;
                TextBox_Password.Text = user.Element("Password").Value;
            }
        }
        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            Data.userInfo = new IPC.UserInfo();
            Data.userInfo.IP = TextBox_IP.Text;
            Data.userInfo.Port = int.Parse(TextBox_Port.Text);
            Data.userInfo.AES = TextBox_AESKey.Text;

            Data.userInfo.ID = TextBox_UserID.Text;
            Data.userInfo.Password = TextBox_Password.Text;

            using(var tcp = new TCP(Data.userInfo.IP, Data.userInfo.Port, Data.userInfo.Password, Data.userInfo.ID))
            {
                if (tcp.Login())
                {
                    tcp.Send("download-user-info");
                    var xml = XElement.Parse(Encoding.UTF8.GetString(await tcp.Download()));

                    foreach(var group in xml.Elements("Group"))
                    {
                        var groupObj = new IPC.UserInfo.Group();
                        groupObj.Name = group.Element("Name").Value;
                        groupObj.ID = group.Element("ID").Value;
                        groupObj.Description = group.Element("Description").Value;
                    }
                    foreach(var user in xml.Elements("User"))
                    {
                        var userObj = new IPC.UserInfo.User();
                        userObj.Name = user.Element("Name").Value;
                        userObj.ID = user.Element("ID").Value;
                    }

                    Data.ipc = new System.Runtime.Remoting.Channels.Ipc.IpcServerChannel("Shinfo");
                    ChannelServices.RegisterChannel(Data.ipc, true);

                    RemotingServices.Marshal(Data.userInfo, "UserInfo", typeof(IPC.UserInfo));

                    new MainWindow().Show();
                    this.Close();
                }
                else
                {
                    Process.ShowErrorMessageBox("ログインできませんでした。", "IPまたはポートまたはAESキーまたはIDまたはパスワードが間違っている可能性があります :)");
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
