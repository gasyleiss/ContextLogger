using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net.Core;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ContextLogger.Layouts
{
    public sealed class JsonLayout : PatternLayout
    {
        private const char PropertiesSeparator = ',';

        private static readonly string ProcessSessionId = Guid.NewGuid().ToString();
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        private static readonly string MachineName = Environment.MachineName;
        private readonly object _initializeLock = new object();
        private volatile bool _initialized;
        private PatternConverter _patternConverter;

        private JsonSerializerSettings _settings;

        public JsonLayout()
        {
            ReferenceLoopHandling = JsonLayoutSettings.ReferenceLoopHandling;
            LayoutScope = JsonLayoutSettings.LayoutScope;
            JsonLayoutStyle = JsonLayoutSettings.JsonLayoutStyle;
        }

        /// <summary>
        ///     Refresh serialization settings
        /// </summary>
        /// <param name="settingsOverride">Specify this parameter to override completely the settings of serialization</param>
        public void UpdateSettings(JsonSerializerSettings settingsOverride = null)
        {
            _initialized = false;
            InitializeSerialization(settingsOverride);
        }

        public override void ActivateOptions()
        {
            // NB: this piece of code is taken from log4net source for PatternLayout

            _patternConverter = CreatePatternParser(ConversionPattern).Parse();
            for (var current = _patternConverter; current != null; current = current.Next)
            {
                if (!(current is PatternLayoutConverter patternLayoutConverter) || patternLayoutConverter.IgnoresException) continue;
                IgnoresException = false;
                break;
            }
        }

        public override void Format(TextWriter writer, LoggingEvent e)
        {
            InitializeSerialization();

            if (writer == null) throw new ArgumentNullException(nameof(writer));

            if (e == null) throw new ArgumentNullException(nameof(e));

            switch (LayoutScope)
            {
                case LayoutScope.Record:
                    FormatComplete(writer, e);
                    break;

                default:
                    FormatMessage(writer, e);
                    break;
            }
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
                : SkippedProperties.Split(PropertiesSeparator).Select(s => s.Trim()).ToArray();
        }

        private bool HasSkippedProperties()
        {
            if (string.IsNullOrWhiteSpace(SkippedProperties)) return JsonLayoutSettings.SkippedProperties != null && JsonLayoutSettings.SkippedProperties.Any();

            return SkippedProperties.Split(PropertiesSeparator).Any();
        }

        private void FormatComplete(TextWriter writer, LoggingEvent e)
        {
            Func<string, int, string, LoggingEvent, Dictionary<string, object>> createCompleteJsonData = JsonLayoutSettings.CreateCompleteJsonDataFull;
            if (JsonLayoutStyle == JsonLayoutStyle.Simple)
            {
                createCompleteJsonData = JsonLayoutSettings.CreateCompleteJsonDataCompact;
            }
            var data = createCompleteJsonData(ProcessSessionId, ProcessId, MachineName, e);
            writer.Write(JsonConvert.SerializeObject(data, _settings));
            writer.WriteLine();
        }

        private void FormatMessage(TextWriter writer, LoggingEvent e)
        {
            var eventData = e.GetLoggingEventData();
            eventData.Message = JsonConvert.SerializeObject(e.MessageObject, _settings);
            var loggingEvent = new LoggingEvent(eventData);
            for (var current = _patternConverter; current != null; current = current.Next)
            {
                current.Format(writer, loggingEvent);
            }
        }

        internal class ShouldSerializeContractResolver : DefaultContractResolver
        {
            private readonly Func<Type, string, bool> _advancedPropertyFilter;
            private readonly string[] _skippedProperties;

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

        #region Serialization configuration

        public string DateTimeFormat { get; set; }
        public ReferenceLoopHandling ReferenceLoopHandling { get; set; }
        public string SkippedProperties { get; set; }
        public LayoutScope LayoutScope { get; set; }
        public JsonLayoutStyle JsonLayoutStyle { get; set; }
        #endregion
    }
}