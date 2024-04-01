using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// AddGroup.xaml の相互作用ロジック
    /// </summary>
    public partial class AddGroup : Window
    {
        public GroupData Group;
        public AddGroup()
        {
            InitializeComponent();
            IdBox.Text = Process.GeneratePassword(13);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//IDを自動生成
            IdBox.Text = Process.GeneratePassword(13);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//OK
            if(Group == null)
            {
                Data.Groups.Add(new GroupData()
                {
                    Name = NameBox.Text,
                    ID = IdBox.Text,
                    Description = Description.Text,
                    Level = (UserData.UserLevel)Enum.Parse(typeof(UserData.UserLevel), LevelCombo.Text)
                });
            }
            else
            {
                Group.Children.Add(new GroupData()
                {
                    Name = NameBox.Text,
                    ID = IdBox.Text,
                    Description = Description.Text,
                    Level = (UserData.UserLevel)Enum.Parse(typeof(UserData.UserLevel), LevelCombo.Text)
                });
                Group.Nodes = null;
            }
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {//キャンセル
            this.Close();
        }
    }
}
