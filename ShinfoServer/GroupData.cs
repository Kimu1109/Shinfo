using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShinfoServer
{
    internal class GroupData : INotifyPropertyChanged, UserAndGroupTree
    {
        // INotifyPropertyChanged impl --->
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // <---
        public bool IsGroup { get => true; }

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
        internal string ID
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        private string _description;
        internal string Description
        {
            get => _description;
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
        }

        private UserData.UserLevel _groupLevel;
        internal UserData.UserLevel GroupLevel
        {
            get => _groupLevel;
            set
            {
                _groupLevel = value;
                RaisePropertyChanged();
            }
        }

        internal GroupData Parent { get; set; }
        internal ObservableCollection<GroupData> Children { get; set; }
        internal ObservableCollection<UserData> Users { get; set; }
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
