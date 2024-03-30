using ShinfoServer.Dialog;
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
            Data.tcp = new TCP(2001);
            Data.tcp.Get += ProcessOfGeneral;

            GroupAndUserTree.ItemsSource = Data.Groups;
        }
        private void ProcessOfGeneral(object sender, TCP.TCPEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GroupAndUserTree_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var SelectedItem = GroupAndUserTree.SelectedItem as UserAndGroupTree;
            if (SelectedItem != null)
            {
                if (SelectedItem.IsGroup)
                {

                }
                else
                {

                }
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = GroupAndUserTree.SelectedItem as UserAndGroupTree;
            if(SelectedItem == null)
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
                    new AddGroup() { Group = (SelectedItem as UserData).Parent }.Show();
                }
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var SelectedItem = GroupAndUserTree.SelectedItem as UserAndGroupTree;
            if(SelectedItem == null)
            {
                MessageBox.Show("ユーザーまたは、グループを選択してください。");
            }
            else
            {
                if (SelectedItem.IsGroup)
                {
                    
                }
                else
                {
                    
                }
            }
        }
    }
}
