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
using System.Xml.Linq;
using System.Reflection;
using static Shinfo.Data;
using static Shinfo.Process;

namespace Shinfo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Load();
        }
        public void Load()
        {
            var xml = XElement.Load(AppPath + "\\data\\config.xml");
            foreach (var dll in xml.Elements("DLL"))
            {
                //必要な部分を読み込む
                string PackIcon = dll.Element("PackIcon")?.Value;
                string TabItemText = dll.Element("TabItemText")?.Value;
                string RelativePath = dll.Element("RelativePath")?.Value;
                string DllPath = AppPath + "\\data\\dll\\" + RelativePath;
                string PageNameSpace = dll.Element("PageNameSpace")?.Value;


                //コントロールを宣言
                var TabItem = new TabItem() { Height = 50 };
                var StackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
                var Frame = new Frame();

                //ページを読み込んで、設定
                try
                {
                    Assembly assembly = Assembly.LoadFile(DllPath);
                    Type pageType = assembly.GetType(PageNameSpace);
                    if (pageType != null)
                    {
                        //アイコンを設定
                        StackPanel.Children.Add(new MaterialDesignThemes.Wpf.PackIcon()
                        {
                            Kind = IndexOfKindFromString(PackIcon),
                            Width = 30,
                            Height = 30,
                            Margin = new Thickness(0, 0, 5, 0),
                            VerticalAlignment = VerticalAlignment.Center
                        });

                        //テキストを設定
                        StackPanel.Children.Add(new TextBlock()
                        {
                            Text = TabItemText,
                            FontSize = 24,
                            FontWeight = FontWeights.Regular,
                            VerticalAlignment = VerticalAlignment.Center
                        });

                        //コントロールに代入
                        Frame.Content = (Page)Activator.CreateInstance(pageType);
                        TabItem.Header = StackPanel;
                        TabItem.Content = Frame;

                        this.tabControl.Items.Add(TabItem);
                    }
                    else
                    {
                        ShowErrorMessageBox("not exist page error", $"DLL({DllPath})に{PageNameSpace}が存在しませんでした。");
                    }
                }
                catch (System.IO.FileNotFoundException e)
                {
                    ShowErrorMessageBox(e.Message, $"DLL({DllPath})が存在しませんでした。");
                }
            }
        }
    }
}
