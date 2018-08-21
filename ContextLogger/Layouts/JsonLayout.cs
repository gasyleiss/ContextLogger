using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using log4net.Core;
using log4net.Layout;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace ContextLogger.Layouts
{
    public sealed class JsonLayout : LayoutSkeleton
    {
        private const char PropertiesSeparator = ',';

        private static readonly string ProcessSessionId = Guid.NewGuid().ToString();
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        private static readonly string MachineName = Environment.MachineName;

        private JsonSerializerSettings _settings;
        private readonly object _initializeLock = new object();
        private volatile bool _initialized = false;

        public JsonLayout()
        {
            ReferenceLoopHandling = JsonLayoutSettings.ReferenceLoopHandling;
        }

        /// <summary>
        /// Refresh serialization settings
        /// </summary>
        /// <param name="settingsOverride">Specify this parameter to override completely the settings of serialization</param>
        public void UpdateSettings(JsonSerializerSettings settingsOverride = null)
        {
            _initialized = false;
            InitializeSerialization(settingsOverride);
        }

        private void InitializeSerialization(JsonSerializerSettings settingsOverride = null)
        {
            if (_initialized) return;
            lock (_initializeLock)
            {
                if (_initialized) return;

                _initialized = true;
                if (settingsOverride != null)
                {
                    _settings = settingsOverride;
                    return;
                }

                // build from parameters of layout

                _settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling
                };

                var dateTimeConverter = new IsoDateTimeConverter
                {
                    DateTimeFormat = DateTimeFormat ?? JsonLayoutSettings.DefaultDateTimeFormat
                };
                _settings.Converters.Add(dateTimeConverter);

                if (HasSkippedProperties())
                {
                    var skippedProperties = GetSkippedProperties();
                    _settings.ContractResolver = new ShouldSerializeContractResolver(skippedProperties);
                }
                else
                {
                    _settings.ContractResolver = new ShouldSerializeContractResolver(JsonLayoutSettings.AdvancedPropertyFilter);
                }
            }
        }

        private string[] GetSkippedProperties()
        {
            return string.IsNullOrWhiteSpace(SkippedProperties) 
                ? JsonLayoutSettings.SkippedProperties 
                : SkippedProperties.Split(PropertiesSeparator);
        }

        public override void ActivateOptions()
        {
        }

        public bool HasSkippedProperties()
        {
            if (string.IsNullOrWhiteSpace(SkippedProperties))
            {
                return JsonLayoutSettings.SkippedProperties != null && JsonLayoutSettings.SkippedProperties.Any();
            }

            return SkippedProperties.Split(PropertiesSeparator).Any();
        }

        #region Serialization configuration

        public string DateTimeFormat { get; set; }
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; }
        public string SkippedProperties { get; set; }

        #endregion

        public override void Format(TextWriter writer, LoggingEvent e)
        {
            InitializeSerialization();


            //PatternLayout d = new PatternLayout();

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

        internal class ShouldSerializeContractResolver : DefaultContractResolver
        {
            private readonly string[] _skippedProperties;
            private readonly Func<Type, string, bool> _advancedPropertyFilter;

            protected internal ShouldSerializeContractResolver(string[] skippedProperties)
            {
                _skippedProperties = skippedProperties;
                _advancedPropertyFilter = SimplePropertyFilter;
            }

            protected internal ShouldSerializeContractResolver(Func<Type, string, bool> advancedPropertyFilter)
            {
                _advancedPropertyFilter = advancedPropertyFilter;
            }

            private bool SimplePropertyFilter(Type declaringType, string propertyName)
            {
                return !_skippedProperties.Contains(propertyName);
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = instance => _advancedPropertyFilter(property.DeclaringType, property.PropertyName);
                return property;
            }
        }
    }
}