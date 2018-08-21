using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        //private static ILog _log = ContextLogManager.GetLogger(typeof(Program));
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

            _log.Debug(data);
            _log.Debug("log message");
            
        }
    }
}
