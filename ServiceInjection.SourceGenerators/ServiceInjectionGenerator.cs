using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ServiceInjection.SourceGenerators.Models;

namespace ServiceInjection.SourceGenerators
{
    [Generator]
    public class ServiceInjectionGenerator : IIncrementalGenerator
    {
        private List<Service> services;
        private HashSet<string> usings;
        private string projectName;
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            services = new List<Service>();
            usings = new HashSet<string>();
            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax,
                transform: (ctx, _) => ctx.Node as ClassDeclarationSyntax
            ).Where(m => m != null);

            var compilation = context.CompilationProvider.Combine(provider.Collect());
            context.RegisterSourceOutput(compilation, Execute);
        }
        private void Execute(SourceProductionContext context, (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
        {
            var (compilation, list) = tuple;
            bool first = true;
            foreach (var cl in list)
            {
                var symbol = compilation.GetSemanticModel(cl.SyntaxTree)
                    .GetDeclaredSymbol(cl) as INamedTypeSymbol;
                var attribute = symbol.GetAttributes().Any(ad => ad.AttributeClass.Name == "ServiceAttribute");
                if (!attribute)
                {
                    continue;
                }
                if (first)
                {
                    first = false;
                    projectName = symbol.ContainingNamespace.ConstituentNamespaces[0].ToString().Split('.')[0];
                }
                AppendService(symbol);
            }
            context.AddSource($"ServiceInjectionExtension.g.cs", SourceText.From(GenerateCode(), Encoding.UTF8));
        }
        private void AppendService(INamedTypeSymbol symbol)
        {
            AddUsingFromNameSpace(symbol);
            AddUsing(symbol);
            var name = symbol.Name;
            var attribute = symbol.GetAttributes().Single(ad => ad.AttributeClass.Name == "ServiceAttribute");
            var type = attribute.ConstructorArguments[0].Value as int?;
            var typeName = GetTypeName(type);
            if (typeName == null) return;
            if (attribute.ConstructorArguments.Count() == 1)
            {
                services.Add(new Service()
                {
                    ClassName = name,
                    Type = typeName
                });
            }
            else
            {
                var iName = attribute.ConstructorArguments[1].Value as string;
                var key = attribute.ConstructorArguments[2].Value as string;
                services.Add(new Service()
                {
                    ClassName = name,
                    Type = typeName,
                    InterfaceName = iName,
                    Key = key
                });
            }
        }
        private string GetTypeName(int? id)
        {
            switch (id)
            {
                case 1:
                    return "Scoped";
                case 2:
                    return "Singleton";
                case 3:
                    return "Transient";
            }
            return null;
        }
        private string GenerateCode()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            builder.AppendLine(string.Join("\n", usings));
            builder.AppendLine($"namespace {projectName} {{");
            builder.AppendLine("public static class ServiceInjectionExtension {");
            builder.AppendLine("public static void AddServiceInjection(this IServiceCollection services) {");
            foreach (var service in services)
            {
                builder.AppendLine(service.ToString());
            }
            builder.AppendLine("}");
            builder.AppendLine("}");
            builder.AppendLine("}");
            return builder.ToString();
        }
        private void AddUsingFromNameSpace(ITypeSymbol symbol)
        {
            usings.Add($"using {string.Join(".", symbol.ContainingNamespace.ConstituentNamespaces)};");
        }
        private string AddUsing(INamedTypeSymbol symbol)
        {
            var builder = new StringBuilder();
            var syntax = symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
            var ancestors = syntax.AncestorsAndSelf();
            foreach (var item in ancestors)
            {
                if (item is CompilationUnitSyntax compilationUnit)
                {
                    foreach (var usg in compilationUnit.Usings)
                    {
                        usings.Add($"using {usg.Name};");
                    }
                }
            }
            return builder.ToString();
        }
    }
}
