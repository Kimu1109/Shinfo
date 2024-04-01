using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

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
        public static string ToXml(GroupData[] groups)
        {
            var st = new StringBuilder(Data.XmlHeader);
            st.AppendLine("<Root>");
            foreach(var g in groups) st.Append(g._toXml(1));
            st.AppendLine("</Root>");
            return st.ToString();
        }
        private string _toXml(int tab = 0)
        {
            string tabs = "";
            for (int i = 0; i < tab; i++) tabs += "\t";

            var st = new StringBuilder();
            st.AppendLine($"{tabs}<Group Name=\"{Name}\" ID=\"{ID}\" Description=\"{Description}\" Level=\"{(int)Level}\">");
            foreach(var child in Children)
            {
                st.Append(child._toXml(tab + 1));
            }
            foreach(var user in Users)
            {
                st.AppendLine(tabs + "\t<User>" + user.ID + "</User>");
            }
            st.AppendLine($"{tabs}</Group>");
            return st.ToString();
        }
        public static GroupData[] ParseFromFile(string file)
        {
            var xml = XElement.Load(file);
            var groups = new List<GroupData>();

            foreach(var group in xml.Elements("Group"))
            {
                groups.Add(_parseFromElement(group));
            }
            return groups.ToArray();
        }
        public static GroupData _parseFromElement(XElement element)
        {
            var group = new GroupData();

            group.Name = element.Attribute("Name").Value;
            group.ID = element.Attribute("ID").Value;
            group.Description = element.Attribute("Description").Value;
            group.Level = (UserData.UserLevel)int.Parse(element.Attribute("Level").Value);

            foreach(var child in element.Elements("Group"))
            {
                group.Children.Add(_parseFromElement(child));
            }
            foreach(var user in element.Elements("User"))
            {
                foreach(var du in Data.Users)
                {
                    if(user.Value == du.ID)
                    {
                        group.Users.Add(du);
                        break;
                    }
                }
            }

            return group;
        }
        public GroupData Parent { get; set; }
        private ObservableCollection<GroupData> _children = new ObservableCollection<GroupData>();
        public ObservableCollection<GroupData> Children
        {
            get => _children;
            set
            {
                _children = value;
                RaisePropertyChanged();
                Nodes = null;
            }
        }
        private ObservableCollection<UserData> _users = new ObservableCollection<UserData>();
        public ObservableCollection<UserData> Users
        {
            get => _users;
            set
            {
                _users = value;
                RaisePropertyChanged();
                Nodes = null;
            }
        }
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
