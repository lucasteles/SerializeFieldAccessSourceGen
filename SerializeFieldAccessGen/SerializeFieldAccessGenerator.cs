using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
namespace SerializeFieldAccessGen;

[Generator]
public class SerializeFieldAccessGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        //if (!Debugger.IsAttached) Debugger.Launch();
        //Debug.WriteLine("Waiting debugger...");
        //SpinWait.SpinUntil(() => Debugger.IsAttached);
        //Debug.WriteLine("Debugger attached");
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var syntaxTrees = context.Compilation.SyntaxTrees;
        foreach (var syntaxTree in syntaxTrees)
        {
            var declarations =
                from typeDeclaration in syntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                let serializeFields =
                    typeDeclaration
                        .ChildNodes()
                        .OfType<FieldDeclarationSyntax>()
                        .Where(field => field.AttributeLists.Any(att => att.ToString().StartsWith("[SerializeField")))
                where serializeFields.Any()
                select (typeDeclaration, serializeFields);

            foreach (var (declaration, fieldDeclaration) in declarations)
            {
                var usingDirectives =
                    syntaxTree.GetRoot()
                        .DescendantNodes()
                        .OfType<UsingDirectiveSyntax>()
                        .Aggregate(string.Empty, (acc, x) =>
                            $"{acc}{Environment.NewLine}{x}");

                var sourceBuilder = new StringBuilder(usingDirectives);
                var className = declaration.Identifier.ToString();

                sourceBuilder.Append($"namespace {GetNamespace(declaration)} {{");
                sourceBuilder.Append($"public partial class {className} {{");

                foreach (var fields in fieldDeclaration)
                {
                    var type = fields.Declaration.Type;
                    foreach (var field in fields.Declaration.Variables)
                    {
                        var name = field.Identifier.ToString();
                        sourceBuilder.Append($@"
public void SetSerializeField_{name}({type} __{name}__) =>
    this.{name} = __{name}__;
");
                    }
                }

                sourceBuilder.Append("}}");

                context.AddSource($"{className}_SerializeField.Generated.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }

        }

        static string GetNamespace(SyntaxNode syntax)
        {
            var nameSpace = string.Empty;
            var potentialNamespaceParent = syntax.Parent;
            while (potentialNamespaceParent != null &&
                    potentialNamespaceParent is not NamespaceDeclarationSyntax
                    && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
                potentialNamespaceParent = potentialNamespaceParent.Parent;

            if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceParent)
                return nameSpace;

            nameSpace = namespaceParent.Name.ToString();

            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                    break;
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }

            return nameSpace;
        }
    }
}