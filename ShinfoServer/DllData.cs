using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShinfoServer
{
    internal class DllData
    {
        private Type _class;
        private string _method;
        private string[] _commands;

        public DllData(Assembly assembly, string dllClass, string dllMethod, string Command)
        {
            _class = assembly.GetType(dllClass);
            _method = dllMethod;
            _commands = Command.Split('/');
        }
        public bool IndexOfCommand(string[] arr)
        {
            if (arr.Length < _commands.Length) return false;
            for(int i = 0; i < _commands.Length; i++)
            {
                if (_commands[i] != arr[i]) return false;
            }
            return true;
        }
        public void RunMethod(TcpClient client, byte[] message)
        {
            _class.GetMethod(_method).Invoke(null, new object[] {client, message});
        }
    }
}
