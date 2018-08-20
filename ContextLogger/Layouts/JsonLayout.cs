using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ContextLogger.Serialization;
using log4net.Core;
using log4net.Layout;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ContextLogger.Layouts
{
    public class JsonLayout : LayoutSkeleton
    {
        private static readonly string ProcessSessionId = Guid.NewGuid().ToString();
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        private static readonly string MachineName = Environment.MachineName;
        private readonly JsonSerializerSettings _settings;

        public JsonLayout()
        {
            _settings = new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            _settings.Converters.Add(new IsoDateTimeConverter {DateTimeFormat = "yyyy-MM-dd HH:mm:ss"});
            _settings.ContractResolver = new ShouldSerializeContractResolver();
        }

        public override void ActivateOptions()
        {
        }

        public override void Format(TextWriter writer, LoggingEvent e)
        {
            var dic = new Dictionary<string, object>
            {
                ["processSessionId"] = ProcessSessionId,
                ["level"] = e.Level.DisplayName,
                ["messageObject"] = e.MessageObject,
                ["renderedMessage"] = e.RenderedMessage,
                ["timestampUtc"] = e.TimeStamp.ToUniversalTime().ToString("O"),
                ["logger"] = e.LoggerName,
                ["thread"] = e.ThreadName,
                ["exceptionObject"] = e.ExceptionObject,
                ["exceptionObjectString"] = e.ExceptionObject == null ? null : e.GetExceptionString(),
                ["userName"] = e.UserName,
                ["domain"] = e.Domain,
                ["identity"] = e.Identity,
                ["location"] = e.LocationInformation.FullInfo,
                ["pid"] = ProcessId,
                ["machineName"] = MachineName,
                ["workingSet"] = Environment.WorkingSet,
                ["osVersion"] = Environment.OSVersion.ToString(),
                ["is64bitOS"] = Environment.Is64BitOperatingSystem,
                ["is64bitProcess"] = Environment.Is64BitProcess,
                ["properties"] = e.GetProperties()
            };
            writer.Write(JsonConvert.SerializeObject(dic, _settings));
        }
    }
}