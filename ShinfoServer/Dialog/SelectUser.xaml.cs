﻿using System;
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
    /// SelectUser.xaml の相互作用ロジック
    /// </summary>
    public partial class SelectUser : Window
    {
        public UserData SelectedUser;
        public SelectUser()
        {
            InitializeComponent();
            UserList.ItemsSource = Data.Users;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//OK
            SelectedUser = UserList.SelectedItem as UserData;
            this.DialogResult = SelectedUser != null ? true : false;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//キャンセル
            this.DialogResult = false;
            this.Close();
        }
    }
}
