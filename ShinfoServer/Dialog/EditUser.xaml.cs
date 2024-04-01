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
    /// EditUser.xaml の相互作用ロジック
    /// </summary>
    public partial class EditUser : Window
    {
        public UserData User;
        public EditUser(UserData user)
        {
            this.DataContext = user;
            this.User = user;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//OK
            this.User.Name = NameBox.Text;
            this.User.ID = IdBox.Text;
            this.User.Level = (UserData.UserLevel)LevelCombo.SelectedIndex;
            this.DialogResult = true;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//Cancel
            this.DialogResult = false;
            this.Close();
        }
    }
}
