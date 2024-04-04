using Microsoft.Win32;
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

            if (e.ClientData.user.IsSendMode)
            {
                Console.WriteLine($"GetRequest:{msg}");

                if (!e.ClientData.user.IsStart)
                {
                    e.ClientData.user.IsStart = true;
                    Console.WriteLine("set is start → true");
                }
                else
                {
                    e.ClientData.user.CatchRequest = msg;
                    e.ClientData.user.IsCatchBool = true;
                }
                return;
            }

            switch (arr[0])
            {
                case "QFPDST":
                    Console.WriteLine($"QFPDST Version→{arr[1]}");
                    e.ClientData.user.AllSize = int.Parse(arr[2]);
                    e.ClientData.user.IsGetMode = true;
                    Process.Send(e.ClientData, "QFPLET");
                    return;
                //args -> 1:ID, 2:Password
                //description:
                //ログインを行います。ユーザーの情報に変更を加えることができます。
                case "login":
                    string login_id = arr[1];
                    string login_password = arr[2];

                    foreach(var user in Data.Users)
                    {
                        if(user.ID == login_id && user.password == login_password)
                        {
                            e.ClientData.user = user;
                            e.ClientData.user.connect = DateTime.Now;
                            e.ClientData.user.IsLogin = true;
                            Process.Send(e.ClientData, "success");
                            return;
                        }
                    }
                    Process.Send(e.ClientData, "failed");

                    break;
                //args -> 1:ID, 2;Password
                //description:
                //ログインを行いますが、ユーザーの情報に変更を加えることはできません。
                case "login-sub":
                    string login_sub_id = arr[1];
                    string login_sub_password = arr[2];

                    foreach (var user in Data.Users)
                    {
                        if (user.ID == login_sub_id && user.password == login_sub_password)
                        {
                            e.ClientData.user = user.Clone();
                            e.ClientData.user.connect = DateTime.Now;
                            e.ClientData.user.IsLogin = true;
                            Process.Send(e.ClientData, "success");
                            return;
                        }
                    }
                    Process.Send(e.ClientData, "failed");

                    break;
                //description:
                //ユーザー情報を取得します。
                case "download-user-info":

                    if (e.ClientData.user.IsLogin)
                    {
                        var InfoXml = new StringBuilder(Data.XmlHeader);

                        InfoXml.AppendLine("<Root>");

                        foreach (var g in e.ClientData.user.Groups)
                        {
                            InfoXml.AppendLine(Data.Tab1 + "<Group>");

                            InfoXml.AppendLine(Data.Tab2 + "<Name>" + g.Name + "</Name>");
                            InfoXml.AppendLine(Data.Tab2 + "<ID>" + g.ID + "</ID>");
                            InfoXml.AppendLine(Data.Tab2 + "<Description>" + g.Description + "</Description>");

                            InfoXml.AppendLine(Data.Tab1 + "</Group>");
                            foreach(var u in g.Users)
                            {
                                InfoXml.AppendLine(Data.Tab1 + "<User>");

                                InfoXml.AppendLine(Data.Tab2 + "<Name>" + u.Name + "</Name>");
                                InfoXml.AppendLine(Data.Tab2 + "<ID>" + u.ID + "</ID>");

                                InfoXml.AppendLine(Data.Tab1 + "</User>");
                            }
                        }

                        InfoXml.AppendLine("</Root>");

                        Process.SendFile(e.ClientData, Encoding.UTF8.GetBytes(InfoXml.ToString()));
                    }

                    break;
            }

            foreach(var dll in Data.DllData)
            {
                if (dll.IndexOfCommand(arr))
                {
                    dll.RunMethod(e.ClientData.client, e.Message);
                }
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
                        selectUser.SelectedUser.Groups.Add(SelectedItem as GroupData);
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

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {//file -> create file
            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = Data.AppPath + "\\file";
            if (sfd.ShowDialog() == true)
            {
                Process.FileControl.Create(sfd.FileName.Replace(Data.AppPath + "\\file\\", ""), "Created by server.", Data.SystemAdminUser);
            }
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {//file -> create directory
            var sfd = new SaveFileDialog();
            sfd.InitialDirectory = Data.AppPath + "\\file";
            if (sfd.ShowDialog() == true)
            {
                Process.DirectoryControl.Create(sfd.FileName.Replace(Data.AppPath + "\\file\\", ""), "Created by server.", Data.SystemAdminUser);
            }
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {//file -> remove file
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = Data.AppPath + "\\file";
            if(ofd.ShowDialog() == true)
            {
                Process.FileControl.Delete(ofd.FileName.Replace(Data.AppPath + "\\file\\", ""), Data.SystemAdminUser);
            }
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {//file -> remove directory
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = Data.AppPath + "\\file";
            if (ofd.ShowDialog() == true)
            {
                Process.DirectoryControl.Delete(ofd.FileName.Replace(Data.AppPath + "\\file\\", ""), Data.SystemAdminUser);
            }
        }
    }
}
