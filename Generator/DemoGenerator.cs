using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Generator;

[Generator]
public class DemoGenerator : IIncrementalGenerator
{
    public const string DemoAttributeName = "Generator.DemoAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static postInitializationContext =>
            postInitializationContext.AddSource("DemoAttribute.g.cs", SourceText.From("""
                using System;

                namespace Generator;

                [AttributeUsage(AttributeTargets.Method)]
                public class DemoAttribute : Attribute
                {
                }
                """, Encoding.UTF8)));

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: DemoAttributeName,
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (attr, _) => attr)
            .Where((attr) => attr.TargetSymbol is IMethodSymbol)
            .Select((attr, ctx) =>
            {
                var containingType = attr.TargetSymbol.ContainingType;
                var methodSymbol = (IMethodSymbol)attr.TargetSymbol;
                var methodSyntax = (MethodDeclarationSyntax)attr.TargetNode;

                return new Model
                {
                    Namespace = containingType.ContainingNamespace?
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat
                        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
                    ClassName = containingType.Name,
                    MethodName = methodSymbol.Name,
                    MethodLocation = methodSymbol.Locations[0],
                    IsPartialDefinition = methodSymbol.IsPartialDefinition,
                    EmptyMethodBody = methodSyntax.Body is null
                };
            });

        context.RegisterSourceOutput(provider, static (context, model) =>
        {
            if (!(model.IsPartialDefinition && model.EmptyMethodBody))
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.MethodMustBePartial,
                    model.MethodLocation, model.MethodName));
            }

            context.AddSource($"{model.ClassName}.{model.MethodName}.g.cs", SourceText.From($$"""
                using System;

                namespace {{model.Namespace}}
                {
                    partial class {{model.ClassName}}
                    {
                        static partial void {{model.MethodName}}()
                        {
                            Console.WriteLine("From the generator!");
                        }
                    }
                }
                """, Encoding.UTF8));
        });
    }

    private class Model
    {
        public string? Namespace { get; set; } = null;
        public string ClassName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public Location MethodLocation { get; set; } = Location.None;
        public bool IsPartialDefinition { get; set; }
        public bool EmptyMethodBody { get; set; }
    }
}
