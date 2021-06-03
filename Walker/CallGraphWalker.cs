using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynCSharpCallGraph.Walker
{
    public class CallGraphWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel semanticModel;

        public CallGraphWalker(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        private static string ExtractCallerID(SemanticModel semanticModel, SyntaxNode node)
        {
            while (node != null)
            {
                if (node is MethodDeclarationSyntax methodDeclarationNode)
                {
                    var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationNode);
                    return methodSymbol.ToDisplayString();
                }

                node = node.Parent;
            }

            throw new ArgumentException("");
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var calleeMethodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            string callerMethodID = ExtractCallerID(semanticModel, node);
            string calleeMethodID = calleeMethodSymbol.ToDisplayString();
            Console.WriteLine("\t\"{0}\" -> \"{1}\";", callerMethodID, calleeMethodID);

            base.VisitInvocationExpression(node);
        }
    }
}