using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ContextLogger;
using GenericLogging.Types;
using log4net;

//using log4net;

namespace GenericLogging
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var data = new Data
            {
                Value = 10,
                Date = DateTime.UtcNow,
                OtherData = new Data
                {
                    Value = 12,
                    Float = 12.345,
                    Text = "\" \\ / \b \f \n \r \t ."
                },
                Obj = new int[] {1, 2, 3},
                NullableInt = 10,
                Dict = new Dictionary<string, int> {{"aaaa", 1}, {"bbb", 2}},
                Text = "simple text"
            };
            data.OtherData.OtherData = data;

            // simple text
            _log.Debug(new {info = "log message"});
            _log.Debug(new {info = "log message"}, new Exception("exception message"));
            _log.DebugFormat("log message {0}", DateTime.Now);
            _log.DebugFormat("log message {0} {1}", new object[] { DateTime.Today, DateTime.Now });
            _log.DebugFormat(CultureInfo.CurrentCulture, "log message {0} {1}", new object[] { DateTime.Today, DateTime.Now });
            _log.DebugFormat("log message {0} {1}", DateTime.Today, DateTime.Now);
            _log.DebugFormat("log message {0} {1} {2}", DateTime.Now.Date.Month, DateTime.Today, DateTime.Now);

            // object
            _log.Debug(data);
            _log.Debug(new {obj = data, exc=new Exception("exception message")});
            _log.DebugFormat("log message {0} {1}", DateTime.Now, data);
            _log.DebugFormat("log message {0} {1} {2}", new object[] {DateTime.Today, DateTime.Now, data});
            _log.DebugFormat(CultureInfo.CurrentCulture, "log message {0} {1} {2}", new object[] {DateTime.Today, DateTime.Now, data});
            _log.DebugFormat("log message {0} {1} {2}", DateTime.Today, DateTime.Now, data);
            _log.DebugFormat("log message {0} {1} {2} {3}", DateTime.Now.Date.Month, DateTime.Today, DateTime.Now, data);

            
        }
    }
}
