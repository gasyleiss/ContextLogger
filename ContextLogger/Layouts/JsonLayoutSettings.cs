using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using Newtonsoft.Json;

namespace ContextLogger.Layouts
{
    public class JsonLayoutSettings
    {
        public static string DefaultDateTimeFormat { get; set;  } = "yyyy-MM-dd HH:mm:ss";
        public static ReferenceLoopHandling ReferenceLoopHandling { get; set; } = ReferenceLoopHandling.Ignore;
        public static string[] SkippedProperties { get; set; } = null;

        public static Func<Type, string, bool> AdvancedPropertyFilter = (type, prop) => true;

        public static LayoutScope LayoutScope { get; set; } = LayoutScope.Message;
        public static JsonLayoutStyle JsonLayoutStyle { get; set; } = JsonLayoutStyle.Complete;

        public static Dictionary<string, object> CreateCompleteJsonDataFull(string processSessionId, int processId, string machineName, LoggingEvent loggingEvent)
        {
            var dic = new Dictionary<string, object>
            {
                ["processSessionId"] = processSessionId,
                ["level"] = loggingEvent.Level.DisplayName,
                ["messageObject"] = loggingEvent.MessageObject,
                ["renderedMessage"] = loggingEvent.RenderedMessage,
                ["timestampUtc"] = loggingEvent.TimeStamp.ToUniversalTime().ToString(DefaultDateTimeFormat),
                ["logger"] = loggingEvent.LoggerName,
                ["thread"] = loggingEvent.ThreadName,
                ["exceptionObject"] = loggingEvent.ExceptionObject,
                ["exceptionObjectString"] = loggingEvent.ExceptionObject == null ? null : loggingEvent.GetExceptionString(),
                ["userName"] = loggingEvent.UserName,
                ["domain"] = loggingEvent.Domain,
                ["identity"] = loggingEvent.Identity,
                ["location"] = loggingEvent.LocationInformation.FullInfo,
                ["pid"] = processId,
                ["machineName"] = machineName,
                ["workingSet"] = Environment.WorkingSet,
                ["osVersion"] = Environment.OSVersion.ToString(),
                ["is64bitOS"] = Environment.Is64BitOperatingSystem,
                ["is64bitProcess"] = Environment.Is64BitProcess,
                ["properties"] = loggingEvent.GetProperties()
            };
            return dic;
        }

        public static Dictionary<string, object> CreateCompleteJsonDataCompact(string processSessionId, int processId, string machineName, LoggingEvent loggingEvent)
        {
            var dic = new Dictionary<string, object>
            {
                ["level"] = loggingEvent.Level.DisplayName,
                ["messageObject"] = loggingEvent.MessageObject,
                ["timestampUtc"] = loggingEvent.TimeStamp.ToUniversalTime().ToString(DefaultDateTimeFormat),
                ["logger"] = loggingEvent.LoggerName,
                ["thread"] = loggingEvent.ThreadName
            };
            return dic;
        }
    }
}
