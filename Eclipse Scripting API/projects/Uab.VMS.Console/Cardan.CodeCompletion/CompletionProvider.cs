using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeCompleteTests;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Cardan.CodeCompletion.Properties;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Cardan.CodeCompletion
{
    public class CompletionProvider : MarshalByRefObject
    {
        private static readonly Type[] _assemblyTypes =
        {
            typeof (object),
            typeof (Task),
            typeof (List<>),
            typeof (Regex),
            typeof (StringBuilder),
            typeof (Uri),
            typeof (Enumerable),
            typeof(Application),
            typeof(DoseValue)
        };

        private Document _doc;
        private MefHostServices _host;
        private InteractiveWorkspace _ws;
        private Solution _sol;
        private Project _proj;
        private ISignatureHelpProvider[] _signatureHelpProviders;
        private MetadataReference[] _references;
        private CSharpParseOptions _parseOptions;
        private CSharpCompilationOptions _compilationOptions;
        private DocumentId _currentDocumenId;
        private SimpleCompletionProvider _simp;

        public CompletionProvider(ScriptCs.ScriptServices service,  ICSharpCode.AvalonEdit.Document.TextDocument  text)
        {
            _simp = new SimpleCompletionProvider(service,text);

            var currentPath = Assembly.GetExecutingAssembly().Location;
            currentPath = Path.GetDirectoryName(currentPath);
            var f1 = Path.Combine(currentPath, "Microsoft.CodeAnalysis.Features.dll");
            var f2 = Path.Combine(currentPath, "Microsoft.CodeAnalysis.CSharp.Features");
            var f3 = Path.Combine(currentPath, "Microsoft.CodeAnalysis.Workspaces.Desktop");

            if (!File.Exists(f1)) { File.WriteAllBytes(f1, Resources.Microsoft_CodeAnalysis_Features); }
            if (!File.Exists(f2)) { File.WriteAllBytes(f2, Resources.Microsoft_CodeAnalysis_CSharp_Features); }
            if (!File.Exists(f3)) { File.WriteAllBytes(f3, Resources.Microsoft_CodeAnalysis_Workspaces_Desktop); }

            _host = MefHostServices.Create(MefHostServices.DefaultAssemblies.Concat(new[]
            {               
                Assembly.LoadFrom(f1),
                Assembly.LoadFrom(f2)
            }));

            _ws = new InteractiveWorkspace(_host);


            _parseOptions = new CSharpParseOptions(kind: SourceCodeKind.Interactive);

            _references = _assemblyTypes.Select(t =>
                MetadataReference.CreateFromAssembly(t.Assembly)).ToArray();
            _compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                usings: _assemblyTypes.Select(x => x.Namespace).ToImmutableArray());

            var container = new CompositionContainer(new AssemblyCatalog(typeof(ISignatureHelpProvider).Assembly),
                CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe);
            _signatureHelpProviders = container.GetExportedValues<ISignatureHelpProvider>().ToArray();
        }


        #region Documents

        public void SetDocument(InteractiveContainerHolder textContainer)
        {
            var currentSolution = _ws.CurrentSolution;
            var project = CreateSubmissionProject(currentSolution);
            var currentDocument = SetSubmissionDocument(textContainer.GetContainer(), project);
        }

        private Document SetSubmissionDocument(SourceTextContainer textContainer, Project project)
        {
            var id = DocumentId.CreateNewId(project.Id);
            var solution = project.Solution.AddDocument(id, project.Name, textContainer.CurrentText);
            _ws.SetCurrentSolution(solution);
            _ws.OpenDocument(id, textContainer);
            _currentDocumenId = id;
            return solution.GetDocument(id);
        }

        private Project CreateSubmissionProject(Solution solution)
        {
            string name = "Program" + 1;
            ProjectId id = ProjectId.CreateNewId(name);
            solution = solution.AddProject(ProjectInfo.Create(id, VersionStamp.Create(), name, name, LanguageNames.CSharp,
                parseOptions: _parseOptions,
                compilationOptions: _compilationOptions.WithScriptClassName(name),
                metadataReferences: _references));

            return solution.GetProject(id);
        }

        #endregion

        #region Completion

        private Document GetCurrentDocument()
        {
            return _ws.CurrentSolution.GetDocument(_currentDocumenId);
        }

        public Task<bool> IsCompletionTriggerCharacter(int position)
        {
            return CompletionService.IsCompletionTriggerCharacterAsync(GetCurrentDocument(), position);
        }

        #endregion

        #region Signature Help

        public async Task<bool> IsSignatureHelpTriggerCharacter(int position)
        {
            var text = await GetCurrentDocument().GetTextAsync().ConfigureAwait(false);
            var character = text.GetSubText(new TextSpan(position, 1))[0];
            return _signatureHelpProviders.Any(p => p.IsTriggerCharacter(character));
        }

        public async Task<SignatureHelpItems> GetSignatureHelp(SignatureHelpTriggerInfo trigger, int position)
        {
            var document = GetCurrentDocument();
            foreach (var provider in _signatureHelpProviders)
            {
                var items = await provider.GetItemsAsync(document, position, trigger, CancellationToken.None)
                            .ConfigureAwait(false);
                if (items != null)
                {
                    return items;
                }
            }
            return null;
        }

        public Workspace GetWorkspace()
        {
            return _ws;
        }

        #endregion

        public async Task<CompletionResult> GetCompletionData(int position, char? triggerChar)
        {
            IList<ISelectedCompletionData> completionData = null;
            IOverloadProvider overloadProvider = null;
            bool? isCompletion = null;

            if (triggerChar != null)
            {
                var isSignatureHelp = await IsSignatureHelpTriggerCharacter(position - 1).ConfigureAwait(false);
                if (isSignatureHelp)
                {
                    var signatureHelp = await GetSignatureHelp(
                        new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.TypeCharCommand, triggerChar.Value), position)
                        .ConfigureAwait(false);
                    if (signatureHelp != null)
                    {
                        overloadProvider = new OverloadProvider(signatureHelp);
                    }
                }
                else
                {
                    isCompletion = await IsCompletionTriggerCharacter(position - 1).ConfigureAwait(false);
                }
            }

            if (overloadProvider == null && isCompletion != false)
            {
                var items = GetCompletion(
                    triggerChar != null
                        ? CompletionTriggerInfo.CreateTypeCharTriggerInfo(triggerChar.Value)
                        : CompletionTriggerInfo.CreateInvokeCompletionTriggerInfo(),
                    position);
                completionData = items.ToList();
            }

            return new CompletionResult(completionData, overloadProvider);
        }

        public  IEnumerable<ISelectedCompletionData> GetCompletion(CompletionTriggerInfo trigger, int position)
        {
            if (trigger.TriggerCharacter == '.')
            {
                return _simp.GetCompletionData();
            }
            return null;
        }
    }

}
