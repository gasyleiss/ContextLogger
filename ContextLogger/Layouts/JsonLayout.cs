using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Layout;
using log4net.Layout.Pattern;
using log4net.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ContextLogger.Layouts
{
    public class JsonLayout : PatternLayout
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

            var loggingEvent = PreSerialization(e, LayoutScope);

            switch (LayoutScope)
            {
                case LayoutScope.Record:
                    FormatRecord(writer, loggingEvent);
                    break;

                default:
                    FormatMessage(writer, loggingEvent);
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

                // add converters
                var jsonConverters = CreateInstances<JsonConverter>(TypeConverters);
                if (jsonConverters.Any())
                {
                    foreach (var jsonConverter in jsonConverters)
                    {
                        _settings.Converters.Add(jsonConverter);
                    }
                }
                // add format
                var dateTimeConverter = new IsoDateTimeConverter
                {
                    DateTimeFormat = DateTimeFormat ?? JsonLayoutSettings.DefaultDateTimeFormat
                };
                _settings.Converters.Add(dateTimeConverter);

                // add resolvers
                var compositeResolver = new CompositeContractResolver();
                if (HasSkippedProperties())
                {
                    var skippedProperties = GetSkippedProperties();
                    compositeResolver.Add(new ShouldSerializeContractResolver(skippedProperties));
                }
                else
                {
                    compositeResolver.Add(new ShouldSerializeContractResolver(JsonLayoutSettings.AdvancedPropertyFilter));
                }

                var jsonResolvers = CreateInstances<IContractResolver>(ContractResolvers);
                AddContractResolvers(compositeResolver, jsonResolvers);
                _settings.ContractResolver = compositeResolver;
            }
        }

        protected virtual IList<T> CreateInstances<T>(StringCollection typeConverters)
        {
            var list = new List<T>();
            foreach (var typeConverter in typeConverters)
            {
                try
                {
                    var parts = typeConverter.Split(',');
                    var typeName = parts[0];
                    var assemblyName = parts[1];
                    var instance = Activator.CreateInstance(assemblyName, typeName);
                    if (instance.Unwrap() is T converter)
                    {
                        list.Add(converter);
                    }
                }
                catch (Exception ex)
                {
                    // if there is any exception while crating a converter, then just don't use this converter
                    _log.Error($"Converter of type '{typeConverter}' cannot be loaded. See error details.", ex);
                }
            }

            return list;
        }

        protected virtual void AddContractResolvers(CompositeContractResolver compositeResolver, IList<IContractResolver> contractResolvers)
        {
            if (compositeResolver == null)
            {
                throw new ArgumentNullException(nameof(compositeResolver));
            }

            if (contractResolvers == null || contractResolvers.Count == 0) return;

            foreach (var contractResolver in contractResolvers)
            {
                compositeResolver.Add(contractResolver);
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

        private void FormatRecord(TextWriter writer, LoggingEvent e)
        {
            var data = GetData(e);
            var serializedObject = SerializeObject(data);
            writer.Write(serializedObject);
            writer.WriteLine();
        }

        protected virtual Dictionary<string, object> GetData(LoggingEvent e)
        {
            Func<string, int, string, LoggingEvent, Dictionary<string, object>> createCompleteJsonData = JsonLayoutSettings.CreateCompleteJsonDataFull;

            if (JsonLayoutStyle == JsonLayoutStyle.Simple)
            {
                createCompleteJsonData = JsonLayoutSettings.CreateCompleteJsonDataCompact;
            }

            var data = createCompleteJsonData(ProcessSessionId, ProcessId, MachineName, e);
            return data;
        }

        protected virtual LoggingEvent PreSerialization(LoggingEvent e, LayoutScope layoutScope)
        {
            var loggingEvent = e;
            if (e.MessageObject is string)
            {
                return loggingEvent;
            }

            // prevent serialization to an empty representation
            var message = SerializeObject(e.MessageObject);
            if (message == "{}" || layoutScope == LayoutScope.Message)
            {
                // if serialization is not OK, then replace ObjectMessage with its rendered version
                if (message == "{}")
                {
                    message = e.RenderedMessage;
                }
                loggingEvent = new LoggingEvent(null, e.Repository, e.LoggerName, e.Level, message, e.ExceptionObject);
            }
            else
            {
                // when serialization supposed to be OK, don't touch the event's MessageObject
            }

            return loggingEvent;
        }

        protected virtual string SerializeObject(object objectGraph)
        {
            return JsonConvert.SerializeObject(objectGraph, _settings);
        }

        private void FormatMessage(TextWriter writer, LoggingEvent e)
        {
            for (var current = _patternConverter; current != null; current = current.Next)
            {
                current.Format(writer, e);
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
        public StringCollection ContractResolvers { get; set; } = new StringCollection();
        public StringCollection TypeConverters { get; set; } = new StringCollection();

        #endregion
    }
}