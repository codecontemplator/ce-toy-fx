using ce_toy_fx.sample.Dynamic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ce_toy_fx.tests.Dynamic
{
    public class JsonParserTests
    {
        private readonly ITestOutputHelper output;

        public JsonParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            var json = GetJsonData();
            output.WriteLine(json);

            var result = JsonParser.ParseMRule(json);
        }

        private string GetJsonData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "ce_toy_fx.tests.Dynamic.processdef.json";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
