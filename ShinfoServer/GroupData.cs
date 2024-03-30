using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ShinfoServer
{
    public class GroupData : INotifyPropertyChanged, UserAndGroupTree
    {
        // INotifyPropertyChanged impl --->
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // <---
        public bool IsGroup { get => true; }
        public BitmapImage Image { get => Data.GroupIcon; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        private string _id;
        public string ID
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
        }

        private UserData.UserLevel _groupLevel;
        public UserData.UserLevel Level
        {
            get => _groupLevel;
            set
            {
                _groupLevel = value;
                RaisePropertyChanged();
            }
        }

        public GroupData Parent { get; set; }
        public ObservableCollection<GroupData> Children { get; set; } = new ObservableCollection<GroupData>();
        public ObservableCollection<UserData> Users { get; set; } = new ObservableCollection<UserData>();
        public ObservableCollection<UserAndGroupTree> Nodes 
        { 
            get
            {
                var arr = new ObservableCollection<UserAndGroupTree>();
                foreach(var c in Children) arr.Add(c as UserAndGroupTree);
                foreach(var u in Users) arr.Add(u as UserAndGroupTree);
                return arr;
            }
            set
            {
                RaisePropertyChanged();
            }
        }
    }
}
