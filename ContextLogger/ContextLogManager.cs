using System;
using ContextLogger.Impl;
using log4net;

namespace ContextLogger
{
    public static class ContextLogManager
    {
        public static ILog GetLogger(string loggerName)
        {
            var log = LogManager.GetLogger(loggerName);
            return new StructuralLog(log);
        }

        public static ILog GetLogger(Type type)
        {
            var log = LogManager.GetLogger(type);
            return new StructuralLog(log);
        }
    }
}
