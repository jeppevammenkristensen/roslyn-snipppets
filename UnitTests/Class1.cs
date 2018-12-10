using System.Threading.Tasks;
using JeppeRoi.Roslyn.Operations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

public class FillPropertiesConstructurTests
{
    [Fact]
    public async Task FillsConstructorWithProperties()
    {
        var code = @"public class Source
{
    public string FirstProperty { get;set; }
    public int IntProperty { get;set; }
}

public class Program 
{
    public static void Main() 
    {
        var source = new Source()
        {
        
        };
    }
}
";
        var testData = new TestDataWithSemanticModel(code);
        var operation = new FillPropertiesInConstructor();
        await operation.GenerateAsync(testData.Tree, testData.Model);
    }
}

public class TestDataWithSemanticModel
{
    public SyntaxTree Tree { get; }

    public SemanticModel Model { get; set; }

    public TestDataWithSemanticModel(string text)
    {
        Code = text;
        var tree = CSharpSyntaxTree.ParseText(@"
	text");

        Tree = tree;

        var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create("MyCompilation",
            syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
        //Note that we must specify the tree for which we want the model.
        //Each tree has its own semantic model
        Model = compilation.GetSemanticModel(tree);
    }

    public string Code { get; set; }
}