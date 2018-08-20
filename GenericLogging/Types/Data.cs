using System;
using System.Collections.Generic;

namespace GenericLogging.Types
{
    class Data
    {
        public int Value { get; set; }
        public Data OtherData { get; set; }
        public DateTime Date { get; set; }
        public double Float { get; set; }
        public object Obj { get; set; }
        public int? NullableInt { get; set; }

        public Dictionary<string, int> Dict { get; set; }
    }
}