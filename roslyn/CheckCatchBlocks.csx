#r "nuget: Microsoft.CodeAnalysis.CSharp, 4.8.0"

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

int violations = 0;
string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.cs", SearchOption.AllDirectories);

foreach (var file in files)
{
    var code = File.ReadAllText(file);
    var tree = CSharpSyntaxTree.ParseText(code);
    var root = tree.GetRoot();

    var catchClauses = root.DescendantNodes().OfType<CatchClauseSyntax>();

    foreach (var catchClause in catchClauses)
    {
        var block = catchClause.Block;

        if (block == null || !block.Statements.Any())
        {
            Console.WriteLine($"❌ Empty catch block: {file}");
            violations++;
            continue;
        }

        bool hasLogError = block.Statements
            .OfType<ExpressionStatementSyntax>()
            .Any(stmt =>
                stmt.ToString().Contains("LogError"));

        if (!hasLogError)
        {
            Console.WriteLine($"❌ Catch block missing LogError: {file}");
            violations++;
        }
    }
}

if (violations > 0)
{
    Console.WriteLine($"\nFound {violations} catch block violations.");
    Environment.Exit(1);
}
else
{
    Console.WriteLine("✅ All catch blocks are valid.");
}
