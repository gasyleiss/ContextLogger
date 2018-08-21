using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ContextLogger.Layouts
{
    public class JsonLayoutSettings
    {
        public static string DefaultDateTimeFormat { get; set;  } = "yyyy-MM-dd HH:mm:ss";
        public static ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Ignore;
        public static string[] SkippedProperties { get; set; } = null;

        public static Func<Type, string, bool> AdvancedPropertyFilter = (type, prop) => false;

        public static JsonLayoutStyle JsonLayoutStyle { get; set; } = JsonLayoutStyle.Message;
    }
}
