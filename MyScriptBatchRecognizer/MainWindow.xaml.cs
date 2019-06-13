// Copyright MyScript. All right reserved.

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using MyScriptRecognizer;
using System.Windows.Interop;
using System.Collections.Generic;

namespace MyScriptBatchRecognizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    internal class Logger : ILogMessage
    {
        private bool log_debug = false;
        private TextBox text_box;
        private ScrollViewer scroll_view;
        private System.Threading.Thread my_thread;
        private List<String> _msg_queue;

        public Logger(TextBox tb, ScrollViewer sc)
        {
            text_box = tb;
            scroll_view = sc;
            my_thread = System.Threading.Thread.CurrentThread;
            _msg_queue = new List<String>();
            ComponentDispatcher.ThreadIdle += new EventHandler(logAsync);
        }

        private void logAsync(object sender, EventArgs e)
        {
            logRemaining();
        }

        public void logRemaining()
        {
            String[] msgs = _msg_queue.ToArray();
            _msg_queue.Clear();
            foreach (String msg in msgs)
            {
                text_box.AppendText(msg);
            }
            scroll_view.ScrollToBottom();
        }

        public bool debug()
        {
            return log_debug;
        }

        private String now()
        {
            DateTime dt = DateTime.Now;
            String tm = String.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}.{3,3:D3}", dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            //String tm = dt.Hour + ":" + dt.Minute + ":" + dt.Second + "." + dt.Millisecond;
            return tm;
        }

        private void doLog(String type, String message)
        {
            String tm = now();
            String msg = tm + " " + type + " " + message + "\n";
            _msg_queue.Add(msg);
        }

        public void logDebug(string message)
        {
            if (log_debug) doLog(" debug ", message);
        }

        public void logError(string message)
        {
            doLog(" ERROR ", message);
        }

        public void logInfo(string message)
        {
            doLog(" info  ", message);
        }

        public void setLogDebug(bool yes)
        {
            log_debug = yes;
        }
    }

    public partial class MainWindow : Window
    {
        private zcRecognizer _recognizer;
        private ILogMessage  _logger;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;
            ShowHideApp obj = (ShowHideApp)this.WindowGrid.Resources["ShowHide"];
            obj.setWindow(this);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            App app = (App)Application.Current;
            var localFolder = app.WorkingDirectory();

            DirectoryInfo d = new DirectoryInfo(localFolder);
            FileInfo[] files = d.GetFiles();
            foreach(FileInfo file in files) {
                file.Delete();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App app = (App)Application.Current;
            var localFolder = app.WorkingDirectory(); 
            var tempFolder = localFolder + "\\tmp"; 

            _recognizer = new zcRecognizer(localFolder, "en_US");
            _recognizer.initDefault();
            _logger = new Logger(this.Log, this.Scroll);
            _recognizer.setLogger(_logger);
            _recognizer.logger().setLogDebug(true);

            //ComponentDispatcher.ThreadIdle += new EventHandler(OnProcessWork);

            ShowHideApp obj = (ShowHideApp)this.WindowGrid.Resources["ShowHide"];
            obj.hide();

            _recognizer.Run();
        }

        private void Exit_App(object sender, RoutedEventArgs e)
        {
            this.Close();
            App app = (App)Application.Current;
            app.Shutdown();
        }

        private void OnProcessWork(object sender, EventArgs e)
        {
            /*bool quit = _recognizer.Convert();
            if (quit)
            {
                App app = (App)Application.Current;
                app.Shutdown();
            }*/
        }
    }
}
