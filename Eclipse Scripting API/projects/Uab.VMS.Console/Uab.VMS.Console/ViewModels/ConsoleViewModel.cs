using Cardan.CodeCompletion;
using CodeCompleteTests;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using ScriptCs;
using ScriptCs.Contracts;
using ScriptCs.Engine.Roslyn;
using ScriptCs.Hosting;
using Uab.VMS.Console.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
//using Uab.RO.ESAPIX.Proxies;
using Uab.VMS.Console.Helper;
using Uab.VMS.Console.Views;
using Uab.VMS.Console.ViewModels;
using Uab.VMS.Console.Properties;
using VMS.TPS.Common.Model.API;
using System.Text.RegularExpressions;
using System.Collections;


namespace Uab.VMS.Console.ViewModels
{
    public class ConsoleViewModel
    {
        private DateTime _completionOpen;
        private ScriptServices _service;
        private TextArea _area;
        private JsonSerializerSettings _settings;
        private int lastEvalLine = 0;
        private CompletionProvider _compProvider;
        private CompletionWindow _completionWindow;
        private InsightWindow _insightWindow;
        private int recallLineOffset = 0;
        private TextEditor _edit;
        private TextArea _history;

        public ConsoleViewModel()
        {
            _completionOpen = DateTime.Now.AddDays(-1);
            _settings = new JsonSerializerSettings();
            XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(Resources.Highlight));
            reader.Read();
            HighlightManager = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            SubscribeTextAreaCommand = new DelegateCommand<TextEditor>((edit) =>
            {
                var it = new InteractiveContainerHolder(edit);
                _edit = edit;
                //_manager.SetDocument(it);
                _compProvider = new CompletionProvider(_service, edit.Document);
                _compProvider.SetDocument(it);
                edit.TextArea.TextEntering += TextArea_TextEntering;
                edit.TextArea.TextEntered += TextArea_TextEntered;
                edit.TextArea.Focus();
                _area = edit.TextArea;
                edit.TextArea.ReadOnlySectionProvider = new ReadOnlySectionProvider(_area);
                _service.Executor.ExecuteScript("Console.WriteLine(\"UAB Medicine VMS Console v.1.0\")");
            });
            StartScriptCs();
        }

        //TODO
        void edit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                var codeLines = _area.Document.GetCodeLinesBefore().ToList();
                if (codeLines.Any())
                {
                    var offset = codeLines.Count > recallLineOffset ? codeLines[codeLines.Count - recallLineOffset - 1] : codeLines.First();
                    _area.PerformTextInput(_area.Document.GetText(offset));
                }
                e.Handled = true;
            }
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }


        void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            var a = sender as TextArea;
            var task = ShowCompletion(controlSpace: false);

            //EXECUTE REPL
            if (a.Document.GetText(a.Document.Lines.Last()).Length == 0 && ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
            {
                var lastLine = a.Document.Lines.Last(l => l.TotalLength != 0);
                var index = a.Document.Lines.IndexOf(lastLine);
                if (index > lastEvalLine)
                {
                    var startCommandLine = a.Document.Lines.Reverse().SkipWhile(l => !a.Document.GetText(l).StartsWith(">")).FirstOrDefault();
                    if (startCommandLine == null) { return; }
                    var commandLines = a.Document.Lines
                        .SkipWhile(l => l != startCommandLine)
                        .TakeWhile(l => l != lastLine)
                        .Concat(new DocumentLine[] { lastLine })
                        .Select(l => a.Document.GetText(l))
                        .ToArray();

                    string command = string.Join("\n", commandLines);

                    command = command.TrimStart('>');

                    var result = _service.Executor.ExecuteScript(command);
                    if (result.ReturnValue != null && !command.EndsWith(";"))
                    {
                        try
                        {
                            NewLine();
                            if (LinqExtensionProvider.IsIEnumerable(result.ReturnValue) && !(result.ReturnValue is string))
                            {
                                IEnumerable ie = ((dynamic)result.ReturnValue);
                                int i = 0;
                                foreach (var l in ie)
                                {
                                    a.Document.Text += string.Format("[{0}]", i++);
                                    NewLine();
                                    var json = JsonConvert.SerializeObject(l, _settings);
                                    var formatted = JsonHelper.FormatJson(json);
                                    a.Document.Text += formatted;
                                    NewLine();
                                }
                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(result.ReturnValue, _settings);
                                var formatted = JsonHelper.FormatJson(json);
                                a.Document.Text += formatted;
                                NewLine();
                            }
                        }
                        catch (Exception ex)
                        {
                            a.Document.Text += ex.Message;
                            NewLine();
                        }
                    }
                    if (result.CompileExceptionInfo != null)
                    {
                        a.Document.Text += (result.CompileExceptionInfo.SourceException.Message); NewLine();
                    }
                    if (result.ExecuteExceptionInfo != null)
                    {
                        a.Document.Text += result.ExecuteExceptionInfo.SourceException.Message; NewLine();
                    }
                    a.Document.Text += ">";
                    lastEvalLine = index;
                    recallLineOffset = 0;
                    _edit.ScrollToEnd();
                }
            }

        }

        private bool IsIEnumerable(object p)
        {
            return p.GetType()
              .GetInterfaces()
              .Any(t => t.IsGenericType
                     && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private void NewLine()
        {
            _area.Document.Text += Environment.NewLine;
        }


        void completion_Closed(object sender, EventArgs e)
        {
            var c = sender as CompletionWindow;
            c.Closed -= completion_Closed;
            _completionOpen = DateTime.Now;
        }

        private void StartScriptCs()
        {
            var name = "WPFScript.csx";
            var console = new WPFConsoleRelay();

            var configurator = new LoggerConfigurator(LogLevel.Info);
            configurator.Configure(console);
            var logger = configurator.GetLogger();

            var init = new InitializationServices(logger);
            init.GetAppDomainAssemblyResolver().Initialize();

            var builder = new ScriptServicesBuilder(console, logger, null, null, init)
            .Cache()
            .Debug(false)
            .LogLevel(LogLevel.Info)
            .ScriptName(name)
            .Repl();

            var modules = new string[0];
            var extension = Path.GetExtension(name);

            //OVERRIDES
            builder.ScriptHostFactory<WPFScriptHostFactory>();
            builder.ScriptEngine<RoslynScriptEngine>();
            builder.LoadModules(extension, modules);

            //BUILD SERVICE
            _service = builder.Build();
            _service.Executor.Initialize(Enumerable.Empty<string>(), _service.ScriptPackResolver.GetPacks(), new string[0]);
            var types = new Type[]{
                typeof(IConsole),
                typeof(ScriptContext),
                typeof(Newtonsoft.Json.Converters.BinaryConverter)
            };


            _service.Executor.AddReferenceAndImportNamespaces(types);



            EventAggr.Instance.GetEvent<WriteLineEvent>().Subscribe((text) =>
               {
                   string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                   foreach (var line in lines.Where(l => !string.IsNullOrEmpty(l)))
                   {
                       _area.Document.Text += line;
                       NewLine();
                       _area.Document.Text += ">";
                   }
               });

            EventAggr.Instance.GetEvent<WriteEvent>().Subscribe((text) =>
                    {

                        _area.Document.Text += text;
                    }
               );

        }

        private async Task ShowCompletion(bool controlSpace)
        {
            if (_completionWindow == null)
            {
                int offset = _area.Caret.Offset;
                var completionChar = controlSpace ? (char?)null : _area.Document.GetCharAt(offset - 1);

                var codeLines = _area.Document.GetCodeLinesBefore();
                var currentLineOffset = offset - _area.Document.GetCharactersBeforeCurrentLine();
                offset = codeLines.Sum(l => l.TotalLength - 1) + currentLineOffset - 1;

                var results = await _compProvider.GetCompletionData(offset, completionChar).ConfigureAwait(true);

                if (_insightWindow == null && results.OverloadProvider != null)
                {
                    _insightWindow = new MyOverloadInsightWindow(_area)
                    {
                        Provider = results.OverloadProvider
                    };
                    _insightWindow.Show();
                    _insightWindow.Closed += (o, args) => _insightWindow = null;
                    return;
                }

                if (_completionWindow == null && results.CompletionData.Any())
                {
                    // Open code completion after the user has pressed dot:
                    _completionWindow = new CompletionWindow(_area)
                    {
                        // CloseWhenCaretAtBeginning = controlSpace
                    };
                    if (completionChar != null && char.IsLetterOrDigit(completionChar.Value))
                    {
                        _completionWindow.StartOffset -= 1;
                    }

                    var data = _completionWindow.CompletionList.CompletionData;
                    ISelectedCompletionData selected = null;
                    foreach (var completion in results.CompletionData) //.OrderBy(item => item.SortText))
                    {
                        if (completion.IsSelected)
                        {
                            selected = completion;
                        }
                        data.Add(completion);
                    }

                    if (selected != null)
                    {
                        _completionWindow.CompletionList.SelectedItem = selected;
                    }
                    _completionWindow.Show();
                    _completionWindow.Closed += (o, args) =>
                    {
                        _completionWindow = null;
                    };
                }
            }
        }



        public DelegateCommand<KeyEventArgs> RunCurrentCommand { get; set; }
        public DelegateCommand<TextBox> Focus { get; set; }
        public DelegateCommand<ScrollViewer> SetViewer { get; set; }
        public DelegateCommand<TextEditor> SubscribeTextAreaCommand { get; set; }
        public IHighlightingDefinition HighlightManager { get; set; }

    }
}
