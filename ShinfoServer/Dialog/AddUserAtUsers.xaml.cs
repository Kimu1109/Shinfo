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
using System.Windows.Shapes;

namespace ShinfoServer.Dialog
{
    /// <summary>
    /// AddUserAtUsers.xaml の相互作用ロジック
    /// </summary>
    public partial class AddUserAtUsers : Window
    {
        public AddUserAtUsers()
        {
            InitializeComponent();
            PasswordBox.Text = Process.GeneratePassword(13);
            IdBox.Text = Process.GeneratePassword(13);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//IDを自動生成
            IdBox.Text = Process.GeneratePassword(13);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//OK
            Data.Users.Add(new UserData()
            {
                Name = NameBox.Text,
                password = PasswordBox.Text,
                ID = IdBox.Text,
                Level = (UserData.UserLevel)Enum.Parse(typeof(UserData.UserLevel), LevelCombo.Text)
            });
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {//キャンセル
            this.Close();
        }
    }
}
