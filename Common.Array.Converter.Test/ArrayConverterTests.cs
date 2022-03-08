namespace Common.Array.Converter.Test
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using IngameScript;

    [TestFixture]
    public class ArrayConverterTest
    {
        private Dictionary<string, List<string>> TestCases = new Dictionary<string, List<string>>()
        {
            { "[1, 2, 3]", new List<string>() {"1", "2", "3"} },
            { "[]", new List<string>() },
            { "[[1,2], [3]]", new List<string>() {"[1,2]", "[3]"}},
            { @"[""My Name"", ""something else""]", new List<string>() {@"""My Name""", @"""something else"""} },
            { @"[[""My Name"", ""something else""], [""[Tag] Name""]]", new List<string>() { @"[""My Name"",""something else""]", @"[""[Tag] Name""]" } }
        };

        [Test]
        public void TestDeserialize()
        {
            Program.ArrayConverter converter = new Program.ArrayConverter();

            foreach (string input in this.TestCases.Keys)
            {
                List<string> result = converter.Deserialize(input);
                Assert.IsTrue(this.TestCases[input].TrueForAll(expected => result.Contains(expected)), $"Incorrect output for {input}.\nExpected: {string.Join(",", this.TestCases[input])}\nActual: {string.Join(",", result)}");
            }
        }
    }
}
