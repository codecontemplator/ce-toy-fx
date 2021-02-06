using System;

namespace ce_toy_fx.dynamic
{
    // Ref: https://github.com/carljohansen/miscellaneous/blob/master/SearchFilterCompiler.cs
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var drule = new DynamicRule();
            drule.CSharpScriptEvaluate();
        }
    }
}
