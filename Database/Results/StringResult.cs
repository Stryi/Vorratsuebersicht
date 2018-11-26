using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    [DebuggerDisplay("{Value}")]
    public class StringResult
    {
        public string Value {get; set; }
    }

    [DebuggerDisplay("{Value1}, {Value2}")]
    public class StringPairResult
    {
        public string Value1 {get; set; }
        public string Value2 {get; set; }
    }
}