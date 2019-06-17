using MyScript.IInk;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using MyScriptBatchRecognizer;
using System.Windows.Threading;
using System.Windows;

[DataContract]
internal class Cmd
{
    [DataMember(Name = "cmd", IsRequired = true)]
    internal String cmd { get; set; } = "";
    [DataMember(Name = "input", IsRequired = false)]
    internal String input_file { get; set; } = "";
    [DataMember(Name = "result", IsRequired = false)]
    internal String output_file { get; set; } = "";
    [DataMember(Name = "progress", IsRequired = false)]
    internal String prg_file { get; set; } = "";
    [DataMember(Name = "abort", IsRequired = false)]
    internal String abort_file { get; set; } = "";
}

[DataContract]
internal class StrokePoint
{
    [DataMember(Name = "x", IsRequired = true)]
    internal float[] x { get; set; } = null;
    [DataMember(Name = "y", IsRequired = true)]
    internal float[] y { get; set; } = null;
}

[DataContract]
internal class StrokesInput
{
    [DataMember(Name = "type", IsRequired = true)]
    internal String type { get; set; } = "";
    [DataMember(Name = "lang", IsRequired = true)]
    internal String lang { get; set; } = "";
    [DataMember(Name = "strokes", IsRequired = true)]
    internal StrokePoint[] strokes { get; set; } = null;
}

[DataContract]
internal class Result
{
    [DataMember(Name = "result_type", IsRequired = true)]
    internal string type { get; set; } = "";
    [DataMember(Name = "result", IsRequired = true)]
    internal String result { get; set; } = "";
}

[DataContract]
internal class Progress
{
    [DataMember(Name = "info", IsRequired = true)]
    internal string info { get; set; } = "";
    [DataMember(Name = "percentage", IsRequired = true)]
    internal int percentage { get; set; } = -1;
}

internal class Listener : IEditorListener
{
    private ILogMessage logger;
    private bool _inError;
    private String _lastError;

    public Listener()
    {
        logger = new LogToConsole();
        _inError = false;
    }

    public void setLogger(ILogMessage l)
    {
        logger = l;
    }

    public void ContentChanged(Editor editor, string[] blockIds)
    {
        String ids = "";
        String comma  = "";
        int i, N;
        for(i = 0, N = blockIds.Length; i < N; i++)
        {
            ids += comma;
            ids += blockIds[i];
            comma = ", ";
        }
        logger.logDebug("Content Changed: " + ids);
    }

    public void OnError(Editor editor, string blockId, string message)
    {
        String msg = "Block Id(" + blockId + "): " + message;
        _lastError = msg;
        _inError = true;
        logger.logError(msg);
    }

    public void PartChanged(Editor editor)
    {
        logger.logDebug("Part Changed");
    }

    public bool inError()
    {
        return _inError;
    }

    public String lastError()
    {
        return _lastError;
    }

    public void resetError()
    {
        _inError = false;
    }
}

namespace MyScriptRecognizer
{
    public class zcRecognizer 
    {
        String _path;
        String _tmp_path;

        String _lang;

        Editor _editor;
        Engine _engine;
        Renderer _renderer;
        ContentPackage _package;
        ContentPart _part;

        Listener _listener;
        ILogMessage _logger;

        float _dpiX;
        float _dpiY;

        private String mkFileName(String basedir, String name)
        {
            return basedir + "\\" + name;
        }

        public zcRecognizer(String _monitor_directory, String language)
        {
            _lang = language;
            _path = _monitor_directory;
            _tmp_path = _path + "\\" + "tmp";
            DirectoryInfo d = new DirectoryInfo(_tmp_path);
            d.Create();
		}
				
	    public void initDefault()
		{
            // Dispose if not null
            if (_package != null) { _package.Dispose();_package = null; }
            if (_part != null) { _part.Dispose(); _part = null; }
            if (_editor != null) { _editor.Dispose(); _editor = null;  }
            if (_renderer != null) { _renderer.Dispose(); _renderer = null; }
            if (_engine != null) { _engine.Dispose(); _engine = null; }

            _engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);
            var confDirs = new string[1];
            confDirs[0] = "conf";
            _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

            var localFolder = _path;
            var tempFolder = _tmp_path;
            _engine.Configuration.SetString("content-package.temp-folder", tempFolder);

            _engine.Configuration.SetString("lang", _lang);

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

            _editor.Configuration.SetBoolean("text.guides.enable", false);
            _engine.Configuration.SetBoolean("text.guides.enable", false);

            initListener();
        }

        public void setLanguage(String lang)
        {
            if (lang != _lang)
            {
                _lang = lang;
                initDefault();
            }
        }

        private void initListener()
        {
            if (_logger == null)
            {
                _logger = new LogToConsole();
            }

            _listener = new Listener();
            _listener.setLogger(_logger);
            _editor.AddListener(_listener);
        }

        public void setLogger(ILogMessage logger)
        {
            _logger = logger;
            _listener.setLogger(logger);
        }

        public ILogMessage logger()
        {
            return _logger;
        }

        public async void Run()
        {
            _logger.logInfo("MyScript Batch Recognizer started");
            _logger.logInfo("Listening to directory " + _path);
            _logger.logInfo("");
            _logger.logRemaining();

            bool _stop = false;
            while(!_stop)
            {
                _stop = Convert();
                System.Threading.Thread.Sleep(10);
                App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            App app = (App)(Application.Current);
            app.Shutdown();
        }

        public bool Convert()
        {
            var cmd_file = _path + "/cmd.json";
            cmd_file = cmd_file.Replace('/', '\\');
            FileInfo cmd_f = null;  

            bool cmd_exists = false;
            try
            {
                cmd_f = new FileInfo(cmd_file);
                cmd_exists = cmd_f.Exists;
            }
            catch (Exception e)
            {
                _logger.logDebug(e.Message);
            }

            if (cmd_exists)
            {
                bool read = false;
                Cmd c = null;
                while (!read)
                {
                    try
                    {
                        FileStream f_in = cmd_f.OpenRead();
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Cmd));
                        c = (Cmd)ser.ReadObject(f_in);
                        f_in.Dispose();
                        read = true;
                    }
                    catch (Exception e)
                    {
                        _logger.logDebug(e.Message);
                    }
                }

                if (c.cmd == "quit")
                {
                    _logger.logInfo("Quit received, closing Recognizer");
                    cmd_f.Delete();
                    return true;    // Quit
                }
                else if (c.cmd == "recognize")
                {
                    _logger.logInfo("Recognition job received");
                    _editor.Clear();

                    var progress_file = c.prg_file;
                    var abort_file = c.abort_file;

                    var input_file = c.input_file;
                    FileInfo in_f = new FileInfo(input_file);
                    _logger.logInfo("Size of job: " + in_f.Length / 1024 + " Kb");

                    DataContractJsonSerializer rec_ser = new DataContractJsonSerializer(typeof(StrokesInput));
                    FileStream in_f_s = in_f.OpenRead();
                    StrokesInput si = (StrokesInput)rec_ser.ReadObject(in_f_s);
                    in_f_s.Dispose();

                    String lng = si.lang;
                    if (lng != _lang) {
                        _logger.logInfo("Resetting language to: " + lng);
                        setLanguage(lng);
                    }

                    _logger.logDebug("Assembling Pointer Events...");
                    List<PointerEvent> events = new List<PointerEvent>();

                    int i, N;
                    for(i = 0, N = si.strokes.Length;i < N; i++)
                    {
                        var stroke = si.strokes[i];
                        int j, M;
                        for(j = 0, M = stroke.x.Length; j < M; j++)
                        {
                            float x = stroke.x[j] / (300.0f / _dpiX);
                            float y = stroke.y[j] / (300.0f / _dpiY);

                            PointerEvent evt;

                            if (j == 0 && j == M - 1)
                            {
                                evt = new PointerEvent().Down(x, y);
                                events.Add(evt);
                                evt = new PointerEvent().Up(x, y);
                            }
                            else if (j == 0)
                            {
                                evt = new PointerEvent().Down(x, y);
                            } else if (j == M - 1)
                            {
                                evt = new PointerEvent().Up(x, y);
                            } else
                            {
                                evt = new PointerEvent().Move(x, y);
                            }

                            events.Add(evt);
                        }
                    }

                    _logger.logDebug("Feeding Pointer Events to Editor");
                    _editor.PointerEvents(events.ToArray(), false);

                    _logger.logDebug("Waiting for recognizer to finish");
                    DateTime tm_start = DateTime.Now;
                    int s = -1;
                    bool cancel = false;
                    while (!_editor.IsIdle() && !cancel && !_listener.inError())
                    {
                        App.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
                        System.Threading.Thread.Sleep(20);
                        double seconds = DateTime.Now.Subtract(tm_start).TotalSeconds;
                        int ss = (int)(seconds * 2);
                        if (ss != s)
                        {
                            String lang = _lang.Substring(0, 2);
                            String info;
                            if (lang == "nl") {
                                info = String.Format("Handschrift herkenning bezig: {0,0:F1} seconden...", seconds);
                            } else {
                                info = String.Format("Recognition process busy: {0,0:F1} seconds...", seconds);
                            }

                            _logger.logInfo(info);
                            s = ss;

                            FileInfo f_a = new FileInfo(abort_file);
                            if (f_a.Exists)
                            {
                                _logger.logDebug("Abort file " + abort_file + " exists");
                                cancel = true;
                                f_a.Delete();
                            }
                            else
                            {
                                FileInfo ff = new FileInfo(progress_file);
                                if (!ff.Exists)
                                {
                                    try
                                    {
                                        FileInfo f = new FileInfo(progress_file + "_ff");
                                        FileStream fout = f.OpenWrite();
                                        Progress p = new Progress();
                                        p.info = String.Format(info);
                                        DataContractJsonSerializer f_ser = new DataContractJsonSerializer(typeof(Progress));
                                        f_ser.WriteObject(fout, p);
                                        fout.Dispose();
                                        f.MoveTo(progress_file);
                                    } catch(Exception e)
                                    {
                                        _logger.logError(e.Message);
                                    }
                                }
                            }
                        }
                        _logger.logRemaining();
                    }
                    //_editor.WaitForIdle();

                    if (cancel)
                    {
                        _logger.logInfo("Operation Cancelled");
                    }
                    else
                    {
                        _logger.logDebug("Getting result...");
                        String result = _editor.Export_(_editor.GetRootBlock(), MimeType.TEXT);

                        if (_logger.debug())
                        {
                            String rr = result.Replace("\n", "\\n");
                            if (rr.Length < 40) { _logger.logDebug("Result: " + rr); }
                            else { _logger.logDebug("Result: " + rr.Substring(0, 40) + "..."); }
                        }

                        in_f.Delete();

                        _logger.logDebug("Writing back result to " + c.output_file);
                        var result_file = c.output_file + "_rr";
                        var result_ren = c.output_file;
                        FileInfo result_f = new FileInfo(result_file);
                        FileStream f_out = result_f.OpenWrite();
                        Result r = new Result();

                        if (_listener.inError()) {
                            r.type = "error";
                            r.result = _listener.lastError();
                            _listener.resetError();
                        } else {
                            r.type = "text";
                            r.result = result;
                        }

                        DataContractJsonSerializer res_ser = new DataContractJsonSerializer(typeof(Result));
                        res_ser.WriteObject(f_out, r);
                        f_out.Dispose();
                        result_f.MoveTo(result_ren);

                        _logger.logInfo("done.");
                    }

                    _logger.logRemaining();
                }

                cmd_f.Delete();
            }

            return false; // Don't quit
        }
    }
}
