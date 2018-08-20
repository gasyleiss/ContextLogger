using System;
using ContextLogger.Serialization;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ContextLogger.Impl
{
    public class StructuralLog : LogImpl
    {
        private readonly JsonSerializerSettings _settings;

        public StructuralLog(ILogger logger) : base(logger)
        {
            _settings = new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore};
            _settings.Converters.Add(new IsoDateTimeConverter {DateTimeFormat = "yyyy-MM-dd HH:mm:ss"});
            _settings.ContractResolver = new ShouldSerializeContractResolver();
        }

        public StructuralLog(ILog log) : this(log.Logger)
        {
        }

        public override void Debug(object message)
        {
            base.Debug(ConvertToJson(message));
        }

        public override void Debug(object message, Exception exception)
        {
            base.Debug(ConvertToJson(message), exception);
        }

        public override void Error(object message)
        {
            base.Error(ConvertToJson(message));
        }

        public override void Error(object message, Exception exception)
        {
            base.Error(ConvertToJson(message), exception);
        }

        public override void Info(object message)
        {
            base.Info(ConvertToJson(message));
        }

        public override void Info(object message, Exception exception)
        {
            base.Info(ConvertToJson(message), exception);
        }

        public override void Fatal(object message)
        {
            base.Fatal(ConvertToJson(message));
        }

        public override void Fatal(object message, Exception exception)
        {
            base.Fatal(ConvertToJson(message), exception);
        }

        public override void Warn(object message)
        {
            base.Warn(ConvertToJson(message));
        }

        public override void Warn(object message, Exception exception)
        {
            base.Warn(ConvertToJson(message), exception);
        }

        protected virtual string ConvertToJson(params object[] data)
        {
            return JsonConvert.SerializeObject(data, _settings);
        }

    }
}