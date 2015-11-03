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
using System.Timers;
using System.Runtime.InteropServices;

namespace BlinkV0._01
{
    //时间文件读取
    public class DataFile
    {
        private int intCorrectMinute;
        private int intCorrectSecond;
        private int intRelaxMinute;
        private int intRelaxSecond;
        private string strDir;

        public DataFile()
        {
            string strLine;
            strDir = AppDomain.CurrentDomain.BaseDirectory + "TimeData.txt";
            try
            {
                FileStream TimeData = new FileStream(strDir, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader sr = new StreamReader(TimeData);
                strLine = sr.ReadLine();
                intCorrectMinute = int.Parse(strLine);
                strLine = sr.ReadLine();
                intCorrectSecond = int.Parse(strLine);
                strLine = sr.ReadLine();
                intRelaxMinute = int.Parse(strLine);
                strLine = sr.ReadLine();
                intRelaxSecond = int.Parse(strLine);
                sr.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show("出现了一个IO异常！");
                MessageBox.Show(ex.ToString());
                return;
            }
        }

        public int CorrectMinute
        {
            get
            { return intCorrectMinute; }
        }
        public int CorrectSecond
        {
            get
            { return intCorrectSecond; }
        }
        public int CorrectSeconds
        {
            get
            { return intCorrectMinute * 60 + intCorrectSecond; }
        }

        public int RelaxMinute
        {
            get
            { return intRelaxMinute; }
        }
        public int RelaxSecond
        {
            get
            { return intRelaxSecond; }
        }
        public int RelaxSeconds
        {
            get
            { return intRelaxMinute * 60 + intRelaxSecond; }
        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport(@"BlinkDll.dll", EntryPoint = "dllmain", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern int dllmain(ref int EyeX, ref int EyeY);
        [DllImport(@"BlinkDll.dll", EntryPoint = "init", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        public static extern void init();

        public static string StrTime;
        public static int CorrectCount = 0;
        public delegate void Correction(string message);
        public delegate void Detection();
        public delegate void AnalyseArg();

        public static class Args
        {
            public static int longMilliseconds = 10000;
            public static int shortMilliseconds = 5000;
            public static int preAnaMilliseconds = 6000;
            public static int Threshold = 7;
            public static string Message1 = "提醒";
        }

        private static class Flag
        {
            public static bool Message = false;
            public static MessageBoxResult MessageResult = MessageBoxResult.None;
            public static bool Foucus = false;
        }

        private static class Coord
        {
            public static int EyeX = -1;
            public static int EyeY = -1;
            public static int EyeW = -1;
            public static int EyeH = -1;
            public static int lastEyeX = -1;
            public static int lastEyeY = -1;
            public static int stableX = -1;
            public static int stableY = -1;
        }

        private static class Counter
        {
            public static int sTimeCount = 0;
            public static int msTimeCount = 0;
            public static int longTimeCount = 0;
            public static int shortTimeCount = 0;
        }

        //“检测”事件
        public class Events
        {
            public event Detection TimeToDetect;
            public event AnalyseArg TimeToAnalyse;
            public event Correction LongTimeDetected;
            public event Correction ShortTimeDetected;

            private Timer TdTimer;

            MainWindow mainWindow;

            public Events(MainWindow w)
            {
                this.mainWindow = w;
                TdTimer = new Timer(100);
                TdTimer.Elapsed += new ElapsedEventHandler(CheckForDetectSignal);
                TdTimer.Elapsed += new ElapsedEventHandler(CheckForAnalyseSignal);
                TdTimer.Elapsed += new ElapsedEventHandler(CheckForLongTimeSignal);
                TdTimer.Elapsed += new ElapsedEventHandler(CheckForShortTimeSignal);
            }

            public void Detect()
            {
                TdTimer.Start();
            }

            public void DetectFinished()
            {
                TdTimer.Stop();
            }

            private void CheckForDetectSignal(Object source, ElapsedEventArgs e)
            {
                if (TimeToDetect != null)
                    TimeToDetect();
            }

            private void CheckForAnalyseSignal(Object source, ElapsedEventArgs e)
            {
                if (TimeToAnalyse != null)
                    TimeToAnalyse();
            }

            private void CheckForLongTimeSignal(Object source, ElapsedEventArgs e)
            {
                if (LongTimeDetected != null && Flag.Message == false && Counter.longTimeCount > Args.longMilliseconds)
                {
                    Counter.longTimeCount = 0;
                    Flag.MessageResult = MessageBoxResult.None;
                    CorrectCount++;
                    LongTimeDetected("保持同一坐姿时间太长了，休息一下吧:)");
                    this.mainWindow.TimeLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.CountLabel.Content = CorrectCount.ToString();
                        }));
                }
            }

            private void CheckForShortTimeSignal(Object source, ElapsedEventArgs e)
            {
                if (ShortTimeDetected != null && Flag.Message == false && Counter.shortTimeCount > Args.shortMilliseconds)
                {
                    Counter.shortTimeCount = 0;
                    Flag.MessageResult = MessageBoxResult.None;
                    CorrectCount++;
                    ShortTimeDetected("坐姿不端正哦，调整一下吧:)");
                    this.mainWindow.TimeLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.CountLabel.Content = CorrectCount.ToString();
                        }));
                }
            }
        }

        //各种事件处理方法
        public class Handlers
        {
            MainWindow mainWindow;
            public Handlers(MainWindow w)
            {
                this.mainWindow = w;
            }

            //坐标检测函数
            public void EyeDetect()
            {
                Coord.lastEyeX = Coord.EyeX;
                Coord.lastEyeY = Coord.EyeY;
                dllmain(ref Coord.EyeX, ref Coord.EyeY);
            }

            //前期分析函数
            public void PreAnalyse()
            {
                if (Coord.stableX > 0)
                    return;
                if (Coord.EyeX <= 0)
                {
                    Flag.Foucus = false;
                    this.mainWindow.StatusLabel.Dispatcher.Invoke(new Action(
                      delegate
                      {
                          mainWindow.StatusLabel.Content = "正在检测坐姿，请以舒服的姿势坐端正，并眨眼";
                      }));
                    return;
                }
                else
                    Flag.Foucus = true;
                if (Counter.longTimeCount > Args.preAnaMilliseconds)
                {
                    Coord.stableX = Coord.EyeX;
                    Coord.stableY = Coord.EyeY;
                    this.mainWindow.StatusLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.StatusLabel.Content = "坐姿保持很好！";
                        }));
                    return;
                }
                else if (Math.Sqrt(Math.Pow(Coord.EyeX - Coord.lastEyeX, 2) + Math.Pow(Coord.EyeY - Coord.lastEyeY, 2)) < 5)
                    Counter.longTimeCount += 100;
                else
                    Counter.longTimeCount = 0;
            }

            //坐标分析函数
            public void Analyse()
            {
                if (Coord.EyeX <= 0)
                {
                    Flag.Foucus = false;
                    return;
                }
                else
                    Flag.Foucus = true;
                if (Flag.Message == true || Coord.stableX <= 0)
                    return;

                if (Math.Sqrt(Math.Pow(Coord.EyeX - Coord.lastEyeX, 2) + Math.Pow(Coord.EyeY - Coord.lastEyeY, 2)) < Args.Threshold
                    && Math.Sqrt(Math.Pow(Coord.EyeX - Coord.stableX, 2) + Math.Pow(Coord.EyeY - Coord.stableY, 2)) <= Args.Threshold)
                    Counter.longTimeCount += 100;
                else
                    Counter.longTimeCount = 0;
                if (Math.Sqrt(Math.Pow(Coord.EyeX - Coord.lastEyeX, 2) + Math.Pow(Coord.EyeY - Coord.lastEyeY, 2)) < Args.Threshold
                    && Math.Sqrt(Math.Pow(Coord.EyeX - Coord.stableX, 2) + Math.Pow(Coord.EyeY - Coord.stableY, 2)) > Args.Threshold)
                {
                    Counter.longTimeCount = 0;
                    Counter.shortTimeCount += 100;
                    this.mainWindow.StatusLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.StatusLabel.Content = "坐姿不端正哦！请注意调整！";
                        }));
                    return;
                }
                else
                {
                    Counter.shortTimeCount = 0;
                    this.mainWindow.StatusLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.StatusLabel.Content = "坐姿保持很好！";
                        }));
                    return;
                }

            }

            //运行时间
            public void RunTime()
            {
                Counter.msTimeCount += 100;
                if (Counter.msTimeCount / 1000 == 1)
                {
                    Counter.sTimeCount++;
                    TimeSpan time = new TimeSpan(0, 0, Counter.sTimeCount);
                    StrTime = time.ToString();
                    this.mainWindow.TimeLabel.Dispatcher.Invoke(new Action(
                        delegate
                        {
                            mainWindow.TimeLabel.Content = StrTime;
                        }));
                    Counter.msTimeCount = 0;
                }
            }

            //提醒界面
            public void DisplayMessage(string message)
            {
                Flag.Message = true;
                Flag.MessageResult = MessageBox.Show(message, Args.Message1);
                if (Flag.MessageResult != MessageBoxResult.OK)
                    Flag.Message = true;
                else if (Flag.MessageResult == MessageBoxResult.OK)
                    Flag.Message = false;
            }
        }

        //主逻辑
        public MainWindow()
        {
            InitializeComponent();
            init();

            DataFile TimeArg = new DataFile();
            Args.longMilliseconds = TimeArg.RelaxSeconds * 1000;
            Args.shortMilliseconds = TimeArg.CorrectSeconds * 1000;
            var w = (MainWindow)Window.GetWindow(this);

            Events myDetection = new Events(w);
            Handlers myHandlers = new Handlers(w);
            myDetection.TimeToDetect += new Detection(myHandlers.EyeDetect);
            myDetection.TimeToDetect += new Detection(myHandlers.RunTime);
            myDetection.TimeToAnalyse += new AnalyseArg(myHandlers.PreAnalyse);
            myDetection.TimeToAnalyse += new AnalyseArg(myHandlers.Analyse);
            myDetection.LongTimeDetected += new Correction(myHandlers.DisplayMessage);
            myDetection.ShortTimeDetected += new Correction(myHandlers.DisplayMessage);
            myDetection.Detect();
        }

        //“设置”修改主窗口参数
        public void ChangeArgs()
        {
            DataFile TimeArg = new DataFile();
            Args.longMilliseconds = TimeArg.RelaxSeconds * 1000;
            Args.shortMilliseconds = TimeArg.CorrectSeconds * 1000;
        }

        //设置键
        private void Preference_Click(object sender, RoutedEventArgs e)
        {
            var w = (MainWindow)Window.GetWindow(this);
            SubWindow sw = new SubWindow(w);
            sw.ChangArgEvent += new SubWindow.ChangArgs(ChangeArgs);
            sw.Show();
            return;
        }
    }
}
