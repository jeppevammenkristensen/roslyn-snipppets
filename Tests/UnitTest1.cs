using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Tests
{

    public class TestDataWithSemanticModel
    {
        public SyntaxTree Tree { get; }

        public SemanticModel Model { get; set; }

        public TestDataWithSemanticModel(string text)
        {
            Code = text;
            var tree = CSharpSyntaxTree.ParseText(text);

            Tree = tree;

            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            
            Model = compilation.GetSemanticModel(tree);
        }

        public string Code { get; set; }
    }
}