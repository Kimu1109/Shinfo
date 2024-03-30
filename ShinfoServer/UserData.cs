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
    public partial class UserData : INotifyPropertyChanged, UserAndGroupTree
    {//一般
        // INotifyPropertyChanged impl --->
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // <---
        public bool IsGroup { get => false; }
        public BitmapImage Image { get => Data.UserIcon; }
        public GroupData Parent { get; set; }

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
        private string _password;
        public string password
        {
            get => _password;
            set
            {
                _password = value; 
                RaisePropertyChanged();
            }
        }

        private DateTime _connect;
        public DateTime connect
        {
            get => _connect;
            set
            {
                _connect = value;
                RaisePropertyChanged();
            }
        }

        private string _userID;
        public string userID
        {
            get => _userID;
            set
            {
                _userID = value;
                RaisePropertyChanged();
            }
        }

        private UserLevel _userLevel = UserLevel.Guest;
        public UserLevel Level
        {
            get => _userLevel;
            set
            {
                _userLevel = value;
                RaisePropertyChanged();
            }
        }
        public ObservableCollection<UserAndGroupTree> Nodes { get; set; }

        public enum UserLevel
        {
            Nothing,
            Guest,
            General,
            Power,
            Admin
        }
    }
    public partial class UserData
    {//送信時
        public bool IsSendMode { get; set; }
        public bool IsStart { get; set; }
        public bool IsCatchBool { get; set; }
        public string CatchRequest { get; set; }
    }
    public partial class UserData
    {//取得時
        public bool IsGetMode { get; set; }
        public byte[] getBytes { set; get; }
        public bool FinishedGet { get; set; }
        public int AllSize { get; set; }
    }
}
