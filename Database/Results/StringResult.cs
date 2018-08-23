using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    [DebuggerDisplay("{Value}")]
    public class StringResult
    {
        public string Value {get; set; }
    }
}