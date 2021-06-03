using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using RoslynCSharpCallGraph.Walker;

namespace RoslynCSharpCallGraph
{
    class Program
    {
        static void Main()
        {
            // workspaceの構築
            var projectName = "TestProject";
            var assemblyName = "TestProject";
            var files = new List<string> { @"HelloWorld/A.cs", @"HelloWorld/B.cs" };

            var projectId = ProjectId.CreateNewId(projectName);

            var metadataReferences = new List<MetadataReference> {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };

            var documentInfos = new List<DocumentInfo>();
            foreach (var file in files)
            {
                var debugName = file;
                var documentId = DocumentId.CreateNewId(projectId, debugName);

                var source = File.ReadAllText(file);

                var version = VersionStamp.Create();
                var textAndVersion = TextAndVersion.Create(SourceText.From(source), version);
                var loader = TextLoader.From(textAndVersion);
                var filePath = file;

                var documentName = file;
                var documentInfo = DocumentInfo.Create(
                    documentId,
                    documentName,
                    sourceCodeKind: SourceCodeKind.Regular,
                    loader: loader,
                    filePath: filePath
                );

                documentInfos.Add(documentInfo);
            }

            var workspace = new AdhocWorkspace();

            var solution = workspace.CurrentSolution
                .AddProject(projectId, projectName, assemblyName, LanguageNames.CSharp)
                .AddMetadataReferences(projectId, metadataReferences)
                .AddDocuments(documentInfos.ToImmutableArray());

            // 各Documentを解析してgraphvizを出力
            Console.WriteLine("digraph graph_name {");
            Console.WriteLine("\tgraph [ rankdir = LR ];");

            foreach (var documentInfo in documentInfos)
            {
                var document = solution.GetDocument(documentInfo.Id);
                var semanticModel = document.GetSemanticModelAsync().Result;
                var syntexTree = document.GetSyntaxTreeAsync().Result;
                var walker = new CallGraphWalker(semanticModel);
                walker.Visit(syntexTree.GetRoot());
            }

            Console.WriteLine("}");
        }
    }
}
