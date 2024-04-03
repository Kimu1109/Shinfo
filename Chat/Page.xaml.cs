using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;

namespace Chat
{
    /// <summary>
    /// Page.xaml の相互作用ロジック
    /// </summary>
    public partial class Page : System.Windows.Controls.Page
    {
        private IpcClientChannel Channel;
        private IPC.UserInfo UserData;
        public Page()
        {
            InitializeComponent();
            Init_WebView2();

            Channel = new IpcClientChannel();
            // 作成したチャネルを登録
            ChannelServices.RegisterChannel(Channel, true);

            UserData = Activator.GetObject(typeof(IPC.UserInfo), "ipc://Shinfo/UserInfo") as IPC.UserInfo;
        }
        private async void Init_WebView2()
        {
            await WebView2.EnsureCoreWebView2Async(null);
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//送信
            using(var tcp = new TCP(UserData.IP,UserData.Port,UserData.Password, UserData.ID))
            {
                tcp.ComLogin();

            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
