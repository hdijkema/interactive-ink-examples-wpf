// Copyright MyScript. All right reserved.

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using MyScript.IInk.UIReferenceImplementation;
using MyScriptRecognizer;
using System.Windows.Interop;

namespace MyScript.IInk.GetStarted
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Defines the type of content (possible values are: "Text Document", "Text", "Diagram", "Math", and "Drawing")
        private const string PART_TYPE = "Text";

        private Engine _engine;
        private Editor _editor; // => UcEditor.Editor;
        private zcRecognizer _recognizer;
        private float _dpiX, _dpiY;
        private Renderer _renderer;
        private ContentPackage _package;
        private ContentPart _part;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += Window_Closing;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_editor != null)
            {
                var part = _editor.Part;
                var package = part?.Package;

                _editor.Part = null;

                part?.Dispose();
                package?.Dispose();

                _editor.Dispose();
            }

            //UcEditor?.Closing();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MyScriptBatchRecognizer.App app = (MyScriptBatchRecognizer.App)Application.Current;
            var localFolder = app.WorkingDirectory(); // Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var tempFolder = localFolder + "\\tmp"; //Path.Combine(localFolder, "MyScript", "tmp");

            /*try
             {
                 // Initialize Interactive Ink runtime environment
                 _engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                 Close();
                 return;
             }

             // Folders "conf" and "resources" are currently parts of the layout
             // (for each conf/res file of the project => properties => "Build Action = content")
             string[] confDirs = new string[1];
             confDirs[0] = "conf";
             _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

             _engine.Configuration.SetString("content-package.temp-folder", tempFolder);

             // Initialize the editor with the engine
             UcEditor.Engine = _engine;
             UcEditor.Initialize(this);

             ContentPackage package = _engine.CreatePackage("text.iink");
             ContentPart part = package.CreatePart("Text");
             UcEditor.Editor.Part = part;

             _recognizer = new zcRecognizer(localFolder);
             Renderer r = UcEditor.Renderer;
             _recognizer.initFromVars(_engine, UcEditor.Editor, r, r.DpiX, r.DpiY, package, part);
             */
             /*
            try {
                // Initialize Interactive Ink runtime environment
                _engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);
            }
             catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Folders "conf" and "resources" are currently parts of the layout
            // (for each conf/res file of the project => properties => "Build Action = content")
            string[] confDirs = new string[1];
            confDirs[0] = "conf";
            _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

            _engine.Configuration.SetString("content-package.temp-folder", tempFolder);

            //_engine.Configuration.SetString("lang", "nl_NL");

            // Initialize the editor with the engine

            _dpiX = 300;
            _dpiY = 300;
            _renderer = _engine.CreateRenderer(_dpiX, _dpiY, null);
            _package = _engine.CreatePackage("text.iink");
            _part = _package.CreatePart("Text");

            _editor = _engine.CreateEditor(_renderer);
            _editor.Theme = ".text { font-size: 7.8;line-height: 1.0; }";
            _editor.SetViewSize(30000, 30000);
            var fmp = new FontMetricsProvider(_dpiX, _dpiY);
            _editor.SetFontMetricsProvider(fmp);

            _editor.Part = _part;
            */
            /*
            UcEditor.Engine = _engine;
            UcEditor.Initialize(this);

            ContentPackage package = _engine.CreatePackage("text.iink");
            ContentPart part = package.CreatePart("Text");
            UcEditor.Editor.Part = part;

            _recognizer = new zcRecognizer(localFolder);
            Renderer r = UcEditor.Renderer;
            */

            /*Renderer r = _renderer;
            _recognizer = new zcRecognizer(localFolder);
            _recognizer.initFromVars(_engine, _editor, r, r.DpiX, r.DpiY, _package, _part);
            */

            _recognizer = new zcRecognizer(localFolder);
            _recognizer.initDefault();

            // Force pointer to be a pen, for an automatic detection, set InputMode to AUTO
            //SetInputMode(InputMode.PEN);

            //NewFile();

            ComponentDispatcher.ThreadIdle += new EventHandler(OnProcessWork);
         }

        private void OnProcessWork(object sender, EventArgs e)
        {
            _recognizer.Convert();
        }

        private void EditUndo_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //_editor.Undo();
        }

        private void EditRedo_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //_editor.Redo();
        }

        private void EditClear_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //_editor.Clear();
        }

        private void EditConvert_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            /*try
            {
                var supportedStates = _editor.GetSupportedTargetConversionStates(null);

                if ( (supportedStates != null) && (supportedStates.Count() > 0) )
                  _editor.Convert(null, supportedStates[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/

            try
            {
                _recognizer.Convert();
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.ToString());
            }
        }

        public void NewFile()
        {
            /*
            // Close current package
            if (_editor.Part != null)
            {
                var part = _editor.Part;
                var package = part?.Package;
                _editor.Part = null;
                part?.Dispose();
                package?.Dispose();
            }

            // Create package and part
            {
                var packageName = MakeUntitledFilename();
                var package = _engine.CreatePackage(packageName);
                var part = package.CreatePart(PART_TYPE);
                _editor.Part = part;
                Type.Text = "Type: " + PART_TYPE;
            }
            */
        }

        private string MakeUntitledFilename()
        {
            /*
            var localFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            int num = 0;
            string name;

            do
            {
                string baseName = "File" + (++num) + ".iink";
                name = System.IO.Path.Combine(localFolder, "MyScript", baseName);
            }
            while (System.IO.File.Exists(name));

            return name;
            */
            return "";
        }

        private void SetInputMode(InputMode inputMode)
        {
            /*
            UcEditor.InputMode = inputMode;
            Auto.IsChecked = (inputMode == InputMode.AUTO);
            Touch.IsChecked = (inputMode == InputMode.TOUCH);
            Pen.IsChecked = (inputMode == InputMode.PEN);
            */
        }

        private void Pen_Click(object sender, RoutedEventArgs e)
        {
            /*
            ToggleButton toggleButton = sender as ToggleButton;

            if ((bool)toggleButton.IsChecked)
            {
                SetInputMode(InputMode.PEN);
            }
            */
        }

        private void Touch_Click(object sender, RoutedEventArgs e)
        {
            /*
            ToggleButton toggleButton = sender as ToggleButton;

            if ((bool)toggleButton.IsChecked)
            {
                SetInputMode(InputMode.TOUCH);
            }
            */
        }

        private void Auto_Click(object sender, RoutedEventArgs e)
        {
            /*
            ToggleButton toggleButton = sender as ToggleButton;

            if ((bool)toggleButton.IsChecked)
            {
                SetInputMode(InputMode.AUTO);
            }
            */
        }
    }
}
