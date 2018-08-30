using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using FluentAssert;
using log4net;
using Xunit;

namespace ContextLogger.Tests.IntegrationTests
{
    public class LogWritingTests
    {
        ~LogWritingTests()
        {
            File.Delete(@"c:\logs\LabourProductivity.log4net.Tests.log");
        }

        // keep these commented lines for later usage. I'm very lazy and don't want to search them again and again.
        // need to write tests for dynamic configuration
        // and for setting custom method (instead of complete and simple ones) to create a record representation
    

        // <conversionPattern value="%date [%thread] %-5level %logger [%ndc] - %message%newline" />

        //JsonLayoutSettings.SkippedProperties = new [] {"Value", "Obj"};
        //JsonLayoutSettings.DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        //JsonLayoutSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //JsonLayoutSettings.LayoutScope = LayoutScope.Record;
        //JsonLayoutSettings.JsonLayoutStyle = JsonLayoutStyle.Simple;

        ////object l = LogManager.GetRepository().GetAppenders()[0];
        ////var d = l as JsonLayout;
        ////d.UpdateSettings();

        [Fact]
        public void ShouldCorrectlyWriteToLogsSimpleText_WithoutSpecialConfiguration()
        {
            // Arrange
            var today = DateTime.Today;
            var now = DateTime.Now;
            var todayText = today.ToString("yyyy-MM-dd HH:mm:ss");
            var nowText = now.ToString("yyyy-MM-dd HH:mm:ss");
            var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            var data = new Data
            {
                Value = 10,
                Date = today,
                OtherData = new Data
                {
                    Value = 12,
                    Text = "text value nested"
                },
                Dict = new Dictionary<string, int> {{"text_key_1", 1}, {"text_key_2", 2}},
                Text = "text value"
            };

            // Act
            log.Debug(new {info = "log message 1"});
            log.Debug(new {info = "log message 2"}, new Exception("exception message"));
            log.DebugFormat("log message 3 {0}", nowText);
            log.DebugFormat("log message 4 {0} {1}", new object[] { todayText, nowText });
            log.DebugFormat(CultureInfo.CurrentCulture, "log message 5 {0} {1}", todayText, nowText);
            log.DebugFormat("log message 6 {0} {1}", todayText, nowText);
            log.DebugFormat("log message 7 {0} {1} {2}", now.Date.Month, todayText, nowText);

            // with object
            log.Debug(data);
            log.Debug(data, new Exception("exception message"));
            log.DebugFormat("log message 3 {0}", data);
            log.DebugFormat("log message 4 {0} {1}", new object[] { todayText, data });
            log.DebugFormat(CultureInfo.CurrentCulture, "log message 5 {0} {1}", todayText, data);
            log.DebugFormat("log message 6 {0} {1}", todayText, data);
            log.DebugFormat("log message 7 {0} {1} {2}", now.Date.Month, todayText, data);

            LogManager.Shutdown();
            
            // Assert
            var lines = File.ReadAllLines(@"c:\logs\LabourProductivity.log4net.Tests.log");
            lines[0].ShouldBeEqualTo(@"{""info"":""log message 1""}");
            lines[1].ShouldBeEqualTo(@"{""info"":""log message 2""}");
            lines[2].ShouldBeEqualTo(@"System.Exception: exception message");
            lines[3].ShouldBeEqualTo($"log message 3 {nowText}");
            lines[4].ShouldBeEqualTo($"log message 4 {todayText} {nowText}");
            lines[5].ShouldBeEqualTo($"log message 5 {todayText} {nowText}");
            lines[6].ShouldBeEqualTo($"log message 6 {todayText} {nowText}");
            lines[7].ShouldBeEqualTo($"log message 7 {now.Date.Month} {todayText} {nowText}");
            // with object
            lines[8].ShouldBeEqualTo($"{{\"Value\":10,\"OtherData\":{{\"Value\":12,\"OtherData\":null,\"Date\":\"0001-01-01 00:00:00\",\"Dict\":null,\"Text\":\"text value nested\"}},\"Date\":\"{todayText}\",\"Dict\":{{\"text_key_1\":1,\"text_key_2\":2}},\"Text\":\"text value\"}}");
            lines[9].ShouldBeEqualTo($"{{\"Value\":10,\"OtherData\":{{\"Value\":12,\"OtherData\":null,\"Date\":\"0001-01-01 00:00:00\",\"Dict\":null,\"Text\":\"text value nested\"}},\"Date\":\"{todayText}\",\"Dict\":{{\"text_key_1\":1,\"text_key_2\":2}},\"Text\":\"text value\"}}");
            lines[10].ShouldBeEqualTo(@"System.Exception: exception message");
            lines[11].ShouldBeEqualTo($"log message 3 {data}");
            lines[12].ShouldBeEqualTo($"log message 4 {todayText} {data}");
            lines[13].ShouldBeEqualTo($"log message 5 {todayText} {data}");
            lines[14].ShouldBeEqualTo($"log message 6 {todayText} {data}");
            lines[15].ShouldBeEqualTo($"log message 7 {now.Date.Month} {todayText} {data}");
        }

        class Data
        {
            public int Value { get; set; }
            public Data OtherData { get; set; }
            public DateTime Date { get; set; }
            public Dictionary<string, int> Dict { get; set; }
            public string Text { get; set; }
        }
    }
}
