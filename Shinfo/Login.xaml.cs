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

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Data.userInfo = new UserInfo();
            Data.userInfo.IP = TextBox_IP.Text;
            Data.userInfo.Port = int.Parse(TextBox_Port.Text);
            Data.userInfo.AES = TextBox_AESKey.Text;

            Data.userInfo.ID = TextBox_UserID.Text;
            Data.userInfo.Password = TextBox_Password.Text;

            var tcp = new TCP(Data.userInfo.IP, Data.userInfo.Port, Data.userInfo.Password, Data.userInfo.ID);
            if (tcp.Login())
            {
                Data.ipc = new System.Runtime.Remoting.Channels.Ipc.IpcServerChannel("Shinfo");
                ChannelServices.RegisterChannel(Data.ipc, true);

                RemotingServices.Marshal(Data.userInfo, "UserInfo", typeof(UserInfo));

                new MainWindow().Show();
                this.Close();
            }
            else
            {
                Process.ShowErrorMessageBox("ログインできませんでした。", "IPまたはポートまたはAESキーまたはIDまたはパスワードが間違っている可能性があります :)");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
