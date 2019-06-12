// Copyright MyScript. All right reserved.

using System.Windows;
using System;
using System.IO;

namespace MyScriptBatchRecognizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs args)
        {
            if (args.Args.Length != 1)
            {
                Console.WriteLine("Start application with a working directory argument");
                Current.Shutdown();
            } else {
                working_directory = args.Args[0];
                DirectoryInfo d = new DirectoryInfo(working_directory);
                if (!d.Exists)
                {
                    Console.WriteLine(working_directory + " does not exist");
                    Current.Shutdown();
                }
            }
        }

        private String working_directory;

        public String WorkingDirectory()
        {
            return working_directory;
        }
    }
}
