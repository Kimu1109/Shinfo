using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShinfoServer
{
    internal partial class UserData : INotifyPropertyChanged
    {//一般
        // INotifyPropertyChanged impl --->
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // <---

        private string _username;
        internal string username
        {
            get => _username;
            set
            {
                _username = value;
                RaisePropertyChanged();
            }
        }
        private string _password;
        internal string password
        {
            get => _password;
            set
            {
                _password = value; 
                RaisePropertyChanged();
            }
        }

        private DateTime _connect;
        internal DateTime connect
        {
            get => _connect;
            set
            {
                _connect = value;
                RaisePropertyChanged();
            }
        }

        private string _userID;
        internal string userID
        {
            get => _userID;
            set
            {
                _userID = value;
                RaisePropertyChanged();
            }
        }

        private UserLevel _userLevel = UserLevel.Guest;
        internal UserLevel userLevel
        {
            get => _userLevel;
            set
            {
                _userLevel = value;
                RaisePropertyChanged();
            }
        }

        internal enum UserLevel
        {
            Nothing,
            Guest,
            General,
            Power,
            Admin
        }
    }
    internal partial class UserData
    {//送信時
        internal bool IsSendMode { get; set; }
        internal bool IsStart { get; set; }
        internal bool IsCatchBool { get; set; }
        internal string CatchRequest { get; set; }
    }
    internal partial class UserData
    {//取得時
        internal bool IsGetMode { get; set; }
        internal byte[] getBytes { set; get; }
        internal bool FinishedGet { get; set; }
        internal int AllSize { get; set; }
    }
}
