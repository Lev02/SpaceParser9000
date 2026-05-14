using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SpaceParser9000.Generator;

[Generator]
public class BinarySerializerGenerator : IIncrementalGenerator
{
    private const string AttributeShortName = "GenerateBinarySerializerAttribute";
    private const string AttributeFullName = $"SpaceParser9000.Generator.{AttributeShortName}";
    private const string AttributeSource = $@"
using System;

namespace SpaceParser9000.Generator
{{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class {AttributeShortName} : Attribute {{ }}
}}
";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
            ctx.AddSource($"{AttributeShortName}.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

        var recordDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is RecordDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, ct) => GetSemanticTarget(ctx, ct))
            .Where(static symbol => symbol is not null)!;

        context.RegisterSourceOutput(recordDeclarations,
            static (spc, classSymbol) => Execute(classSymbol!, spc));
    }

    private static INamedTypeSymbol? GetSemanticTarget(GeneratorSyntaxContext context, CancellationToken ct)
    {
        var recordDeclaration = (RecordDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(recordDeclaration, ct) is not INamedTypeSymbol symbol)
            return null;

        foreach (var attr in symbol.GetAttributes())
        {
            if (attr.AttributeClass?.ToDisplayString() == AttributeFullName)
                return symbol;
        }

        return null;
    }

    private static void Execute(INamedTypeSymbol classSymbol, SourceProductionContext context)
    {
        var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : classSymbol.ContainingNamespace.ToDisplayString();

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static p => p.DeclaredAccessibility == Accessibility.Public
                               && !p.IsStatic
                               && p.GetMethod is not null)
            .ToImmutableArray();

        var indent = namespaceName is not null ? "        " : "    ";
        var sb = new StringBuilder();

        sb.AppendLine("using System.IO;");
        sb.AppendLine();

        if (namespaceName is not null)
        {
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");
        }

        sb.AppendLine($"{indent}partial record {classSymbol.Name}");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}    public void SerializeToBinary(Stream stream)");
        sb.AppendLine($"{indent}    {{");
        sb.AppendLine($"{indent}        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);");

        foreach (var prop in properties)
        {
            var writeStatement = GetWriteStatement(prop);
            if (writeStatement is not null)
                sb.AppendLine($"{indent}        {writeStatement}");
        }

        sb.AppendLine($"{indent}    }}");
        sb.AppendLine($"{indent}}}");

        if (namespaceName is not null)
            sb.AppendLine("}");

        context.AddSource($"{classSymbol.Name}.BinarySerializer.g.cs",
            SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static string? GetWriteStatement(IPropertySymbol prop)
    {
        return prop.Type.SpecialType switch
        {
            SpecialType.System_Boolean  => $"writer.Write({prop.Name});",
            SpecialType.System_Byte     => $"writer.Write({prop.Name});",
            SpecialType.System_SByte    => $"writer.Write({prop.Name});",
            SpecialType.System_Char     => $"writer.Write({prop.Name});",
            SpecialType.System_Int16    => $"writer.Write({prop.Name});",
            SpecialType.System_UInt16   => $"writer.Write({prop.Name});",
            SpecialType.System_Int32    => $"writer.Write({prop.Name});",
            SpecialType.System_UInt32   => $"writer.Write({prop.Name});",
            SpecialType.System_Int64    => $"writer.Write({prop.Name});",
            SpecialType.System_UInt64   => $"writer.Write({prop.Name});",
            SpecialType.System_Single   => $"writer.Write({prop.Name});",
            SpecialType.System_Double   => $"writer.Write({prop.Name});",
            SpecialType.System_Decimal  => $"writer.Write({prop.Name});",
            SpecialType.System_String   => $"writer.Write({prop.Name} ?? string.Empty);",
            SpecialType.System_DateTime => $"writer.Write({prop.Name}.Ticks);",
            _                           => null
        };
    }
}