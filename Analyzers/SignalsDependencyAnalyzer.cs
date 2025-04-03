using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Signals.Dependency.Analyzer.Atoms;

namespace Signals.Dependency.Analyzer.Analyzers
{
    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="solution"></param>
    public class SignalsDependencyAnalyzer(string solution)
    {
        /// <summary>
        /// Points to the location of the solution
        /// </summary>
        public string Solution { get; private set; } = solution;

        /// <summary>
        /// MSBuild workspace setup
        /// </summary>
        MSBuildWorkspace _workspace;
        Solution _solution;
        bool isSetupCompleted = false;

        /// <summary>
        /// Get all references for the provided class symbol
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns></returns>
        private List<ReferencedSymbol> FindClassReferences(INamedTypeSymbol classSymbol)
        {
            var references = new List<ReferencedSymbol>();
            var referenceLocations = SymbolFinder.FindReferencesAsync(classSymbol, _solution).Result;
            references.AddRange(referenceLocations);
            return references;
        }

        /// <summary>
        /// Get all references of the provided process
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        private SignalsProcess? Analyze(SignalsProcess process)
        {
            if (process == null) return null;
            var subprocess = Analyze(process.FilePath);
            if(subprocess != null) subprocess.Line = process.Line;
            return subprocess;
        }

        /// <summary>
        /// Strip the generic <> from the type name and return the class name w/out the generics
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private string StripGenericParameters(string typeName)
        {
            var indexOfGenericStart = typeName.IndexOf('<');
            // return the type name as is
            if (indexOfGenericStart <= 0) return typeName;

            // if there are generics, take the part before the generic arguments
            return typeName.Substring(0, indexOfGenericStart);
        }

        /// <summary>
        /// Check if the provided type inherits from the Signals process clasess.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        private bool IsBaseProcessType(string baseType)
        {
            // strip out generic parameters like <int, VoidResult> from the type name
            var strippedType = StripGenericParameters(baseType);
            return SignalsProcessType.ProcessTypes.Any(x => string.Compare(x.Value, strippedType, StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        /// <summary>
        /// Get the process type fo the provided class declaration syntax of a Signals process
        /// </summary>
        /// <param name="cls"></param>
        /// <returns></returns>
        private SignalsProcessType GetProcessType(ClassDeclarationSyntax cls)
        {
            var signalsClass = cls.BaseList?.Types.FirstOrDefault(baseType => IsBaseProcessType(baseType.Type.ToString()));
            if (signalsClass == null) return null;

            var strippedType = StripGenericParameters(signalsClass.Type.ToString());
            var type = SignalsProcessType.ProcessTypes.FirstOrDefault(x => string.Compare(x.Value, strippedType, StringComparison.InvariantCultureIgnoreCase) == 0);
            return type;
        }

        /// <summary>
        /// Setup MSBuild and solution workspace
        /// </summary>
        public void Setup()
        {
            if (isSetupCompleted) return;
            _workspace = MSBuildWorkspace.Create();
            _solution = _workspace.OpenSolutionAsync(Solution).Result;
            isSetupCompleted = true;
        }

        /// <summary>
        /// Get all references of the provided class(where this class is in use)
        /// </summary>
        /// <param name="pathToClass"></param>
        public SignalsProcess? Analyze(string pathToClass)
        {           
            // get the file path of the class
            var filePath = Path.GetFullPath(pathToClass);
            var document = _solution.Projects
                .SelectMany(p => p.Documents)
                .FirstOrDefault(d => d.FilePath == filePath);

            if (document == null) return null;

            // parse the document and get the class symbol
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            var root = syntaxTree.GetRootAsync().Result;
            var classDeclaration = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(cls =>
                    cls.BaseList?.Types.Any(
                        baseType => IsBaseProcessType(baseType.Type.ToString()
                    )
                ) == true);

            if (classDeclaration == null) return null;

            // get the class symbol (e.g., the type of the class)
            var semanticModel = document.GetSemanticModelAsync().Result;
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null) return null;

            var signalsProcess = new SignalsProcess(classSymbol.Name, filePath: pathToClass);
            signalsProcess.Type = GetProcessType(classDeclaration);

            // find references to the class in the entire solution
            var references = FindClassReferences(classSymbol);
            foreach (var reference in references)
            {
                Parallel.ForEach(reference.Locations, location =>
                {
                    var subProcess = new SignalsProcess(
                        reference.Definition.Name,
                        null,
                        location.Document?.FilePath,
                        (location.Location?.GetLineSpan().StartLinePosition.Line + 1) ?? 0
                    );
                    subProcess = Analyze(subProcess);
                    signalsProcess.AddSubprocess(subProcess);
                });
            }

            return signalsProcess;
        }
    }
}
