using ShinfoServer.Dialog;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace ShinfoServer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Data.InitData();
            Data.tcp.Get += ProcessOfGeneral;

            ClientList.ItemsSource = Data.tcp.clients;
            GroupAndUserTree.ItemsSource = Data.Groups;
            UsersList.ItemsSource = Data.Users;
        }
        private void ProcessOfGeneral(object sender, TCP.TCPEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.Message);
            string[] arr = msg.Split('/');

            switch(arr[0])
            {
                //args -> 1:ID, 2:Password
                case "login":
                    string login_id = arr[1];
                    string login_password = arr[2];

                    foreach(var user in Data.Users)
                    {
                        if(user.ID == login_id && user.password == login_password)
                        {
                            e.ClientData.user = user;
                            Process.Send(e.ClientData, "success");
                            return;
                        }
                    }
                    Process.Send(e.ClientData, "failed");

                    break;
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = GroupAndUserTree.SelectedItem as UserAndGroupTree;
            if (SelectedItem == null)
            {
                new AddGroup() { Group = null }.Show();
            }
            else
            {
                if (SelectedItem.IsGroup)
                {
                    new AddGroup() { Group = SelectedItem as GroupData }.Show();
                }
                else
                {
                    MessageBox.Show("Please select group.");
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = GroupAndUserTree.SelectedItem as UserAndGroupTree;
            if (SelectedItem == null)
            {
                MessageBox.Show("Please select group.");
            }
            else
            {
                var selectUser = new SelectUser();
                if (selectUser.ShowDialog() == true)
                {
                    if (SelectedItem.IsGroup)
                    {
                        (SelectedItem as GroupData).Users.Add(selectUser.SelectedUser);
                        (SelectedItem as GroupData).Nodes = null;
                    }
                    else
                    {
                        MessageBox.Show("Please select group.");
                    }
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {//Add User
            new AddUserAtUsers().Show();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {//Remove User
            var selectUser = UsersList.SelectedItem as UserData;
            if(selectUser != null)
            {
                var msgResult = MessageBox.Show($"Are you sure you want to delete this user(Name:{selectUser.Name}, ID:{selectUser.ID})?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(msgResult == MessageBoxResult.OK)
                {
                    Data.Users.Remove(selectUser);
                }                
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {//Edit User
            var selectUser = UsersList.SelectedItem as UserData;
            if(selectUser != null)
            {
                new EditUser(selectUser).Show();
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {//Data -> Save
            File.WriteAllText(Data.AppPath + "\\data\\Group.xml", GroupData.ToXml(Data.Groups.ToArray()));
            foreach(var user in Data.Users)
            {
                File.WriteAllText(Data.AppPath + "\\data\\User\\" + user.ID + ".xml", user.ToXml());
            }
        }
    }
}
