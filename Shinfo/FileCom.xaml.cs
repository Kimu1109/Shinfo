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
using System.Windows.Shapes;

namespace Shinfo
{
    /// <summary>
    /// FileCom.xaml の相互作用ロジック
    /// </summary>
    public partial class FileCom : Window
    {
        public double ProgressMax
        {
            set
            {
                Progress.Maximum = value;
            }
        }
        public void SetData(double Num, string Persent, string Speed, string Fine)
        {
            Progress.Value = Num;
            this.Persent.Text = Persent;
            this.Speed.Text = Speed;
            this.Fine.Text = Fine;
        }
        public FileCom()
        {
            InitializeComponent();
        }
    }
}
