using System.Collections.Generic;
using System.Threading.Tasks;
using JeppeRoi.Roslyn.Operations;
using Xunit;

namespace Tests
{
    public class FillPropertiesConstructurTests
    {
        [Fact]
        public async Task FillsConstructorWithProperties()
        {
            var code = @"
using using System.Collections.Generic;

namespace UnitTest 
{
    public class Source
    {
        public string FirstProperty { get;set; }
        public int IntProperty { get;set; }
        public List<string> StringList { get;set; }
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
}
";
            var testData = new TestDataWithSemanticModel(code);
            var operation = new FillPropertiesInConstructor();
            var output = await operation.GenerateAsync(testData.Tree, testData.Model);
           
        }
    }
}