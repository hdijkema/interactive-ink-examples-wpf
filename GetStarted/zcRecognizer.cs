using MyScript.IInk;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MyScript.IInk.UIReferenceImplementation;

[DataContract]
internal class Cmd
{
    [DataMember(Name = "cmd", IsRequired = true)]
    internal String cmd { get; set; } = "";
    [DataMember(Name = "input", IsRequired = false)]
    internal String input_file { get; set; } = "";
    [DataMember(Name = "result", IsRequired = false)]
    internal String output_file { get; set; } = "";
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

internal class Listener : IEditorListener
{
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
        Console.WriteLine("Content Changed: " + ids);
    }

    public void OnError(Editor editor, string blockId, string message)
    {
        Console.WriteLine("Error(" + blockId + "): " + message);
    }

    public void PartChanged(Editor editor)
    {
        Console.WriteLine("Part Changed");
    }
}

namespace MyScriptRecognizer
{
    public class zcRecognizer 
    {
        String _path;
        String _tmp_path;

        Editor _editor;
        Engine _engine;
        Renderer _renderer;
        ContentPackage _package;
        ContentPart _part;

        float _dpiX;
        float _dpiY;

        private String mkFileName(String basedir, String name)
        {
            return basedir + "\\" + name;
        }

        public zcRecognizer(String _monitor_directory)
        {
            _path = _monitor_directory;
            _tmp_path = _path + "\\" + "tmp";
            DirectoryInfo d = new DirectoryInfo(_tmp_path);
            d.Create();
				}
				
	    public void initDefault()
		{
            _engine = Engine.Create(MyScript.Certificate.MyCertificate.Bytes);
            var confDirs = new string[1];
            confDirs[0] = "conf";
            _engine.Configuration.SetStringArray("configuration-manager.search-path", confDirs);

            var localFolder = _path;
            var tempFolder = _tmp_path;
            _engine.Configuration.SetString("content-package.temp-folder", tempFolder);

            _engine.Configuration.SetString("lang", "nl_NL");

            // Initialize the editor with the engine
            //UcEditor.Engine = _engine;
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

            initListener();
        }
				
		public void initFromVars(Engine e, Editor ed, Renderer r, float dpix, float dpiy, ContentPackage pk, ContentPart part)
		{
			_dpiX = dpix;
			_dpiY = dpiy;
			_engine = e;
			_editor = ed;
			_part = part;
			_package = pk;
			_renderer = r;
            initListener();
		}

        private void initListener()
        {
            _editor.AddListener(new Listener());
        }

        public bool Convert()
        {
            float dpix = _dpiX;
            float dpiy = _dpiY;
            Editor editor = _editor;

            var cmd_file = _path + "/cmd.json";
            cmd_file = cmd_file.Replace('/', '\\');
            FileInfo cmd_f = new FileInfo(cmd_file);
            if (cmd_f.Exists)
            {
                FileStream f_in = cmd_f.OpenRead();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Cmd));
                Cmd c = (Cmd)ser.ReadObject(f_in);
                f_in.Dispose();

                if (c.cmd == "quit")
                {
                    cmd_f.Delete();
                    return true;    // Quit
                }
                else if (c.cmd == "recognize")
                {
                    editor.Clear();

                    var input_file = c.input_file;
                    FileInfo in_f = new FileInfo(input_file);
                    DataContractJsonSerializer rec_ser = new DataContractJsonSerializer(typeof(StrokesInput));
                    FileStream in_f_s = in_f.OpenRead();
                    StrokesInput si = (StrokesInput)rec_ser.ReadObject(in_f_s);
                    in_f_s.Dispose();

                    Debug.WriteLine("Recognize received");

                    List<PointerEvent> events = new List<PointerEvent>();

                    int i, N;
                    for(i = 0, N = si.strokes.Length;i < N; i++)
                    {
                        var stroke = si.strokes[i];
                        int j, M;
                        for(j = 0, M = stroke.x.Length; j < M; j++)
                        {
                            float x = stroke.x[j] / (300.0f / dpix);
                            float y = stroke.y[j] / (300.0f / dpiy);

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

                    editor.PointerEvents(events.ToArray(), false);
                            
                    editor.WaitForIdle();
                    String result = editor.Export_(editor.GetRootBlock(), MimeType.TEXT);

                    in_f.Delete();

                    var result_file = c.output_file + "_ff";
                    var result_ren = c.output_file;
                    FileInfo result_f = new FileInfo(result_file);
                    FileStream f_out = result_f.OpenWrite();
                    Result r = new Result();
                    r.type = "text";
                    r.result = result;
                    DataContractJsonSerializer res_ser = new DataContractJsonSerializer(typeof(Result));
                    res_ser.WriteObject(f_out, r);
                    f_out.Dispose();
                    result_f.MoveTo(result_ren);
                }

                cmd_f.Delete();
            }

            return false; // Don't quit
        }
    }
}
