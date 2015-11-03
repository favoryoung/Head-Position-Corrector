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
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace BlinkV0._01
{
    /// <summary>
    /// SubWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SubWindow : Window
    {
        public delegate void ChangArgs();
        public event ChangArgs ChangArgEvent;

        MainWindow mainWindow;

        public SubWindow(MainWindow w)
        {
            this.mainWindow = w;
            InitializeComponent();
            DataFile TimeArg = new DataFile();
            //ComboBoxItem TempItem = new ComboBoxItem();
            //TempItem.Content = TimeArg.CorrectMinute.ToString();
            this.ComboBox1.SelectedValue = TimeArg.CorrectMinute;
            //TempItem.Content = TimeArg.CorrectSecond.ToString();
            this.ComboBox2.SelectedValue = TimeArg.CorrectSecond;
            //TempItem.Content = TimeArg.RelaxMinute.ToString();
            this.ComboBox3.SelectedValue = TimeArg.RelaxMinute;
            //TempItem.Content = TimeArg.RelaxSecond.ToString();
            this.ComboBox4.SelectedValue = TimeArg.RelaxSecond;
            //((ComboBoxItem)this.ComboBox1.SelectedItem).Content = TimeArg.CorrectMinute;
            //((ComboBoxItem)this.ComboBox2.SelectedItem).Content = TimeArg.CorrectSecond;
            //((ComboBoxItem)this.ComboBox3.SelectedItem).Content = TimeArg.RelaxMinute;
            //((ComboBoxItem)this.ComboBox4.SelectedItem).Content = TimeArg.RelaxSecond;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string stringData = "";
             //System.Environment.CurrentDirectory +
            string strDir = "TimeData.txt";
            try
            {
                FileStream TimeData = new FileStream(strDir, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                StreamWriter sw = new StreamWriter(TimeData);
                TimeData.SetLength(0);
                stringData = ((ComboBoxItem)this.ComboBox1.SelectedItem).Content.ToString();
                sw.WriteLine(stringData);
                stringData = ((ComboBoxItem)this.ComboBox2.SelectedItem).Content.ToString();
                sw.WriteLine(stringData);
                stringData = ((ComboBoxItem)this.ComboBox3.SelectedItem).Content.ToString();
                sw.WriteLine(stringData);
                stringData = ((ComboBoxItem)this.ComboBox4.SelectedItem).Content.ToString();
                sw.WriteLine(stringData);
                sw.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("出现了一个IO异常！","异常");
                MessageBox.Show(ex.ToString());
                return;
            }
            ChangArgEvent();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
