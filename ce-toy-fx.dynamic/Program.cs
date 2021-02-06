using System;
using System.IO;

namespace ce_toy_fx.dynamic
{
    // Ref: https://github.com/carljohansen/miscellaneous/blob/master/SearchFilterCompiler.cs
    class Program
    {
        static void Main(string[] args)
        {
            var program = File.ReadAllText("SampleProcess.txt");
            var drule = new DynamicRule();
            var x = drule.CSharpScriptEvaluate(program);
            Console.WriteLine(x);
        }
    }
}
