# ContextLogger

## Enable JSON logging in log4net

Extend log4net with simple configuration options to create JSON log entries. 

## First steps

- Install the latest nuget package of ContextLogger
- [Configure log4net](https://logging.apache.org/log4net/release/manual/configuration.html)
- Use the `ContextLogger.Layouts.JsonLayout, ContextLogger` as your appender's layout
- Log as you normally would

### Installation

You only need to reference the ContextLogger package. Few packages will be installed as a dependency:
- log4net
- Newtonsoft.Json

### Configuration basics

Below is the example of the log4net appender configuration with all settings supported by JsonLayout.

```xml
<appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
  <file value="app.log"/>
  <param name="AppendToFile" value="true" />
  <param name="RollingStyle" value="Once" />
  <layout type="ContextLogger.Layouts.JsonLayout, ContextLogger">
    <dateTimeFormat value="yyyy-MM-dd HH:mm:ss"/> 
    <referenceLoopHandling value="Ignore"/> 
    <skippedProperties value="Value, Obj"/>
    <conversionPattern value="%date [%thread] %-5level %logger [%ndc] - %message%newline" />
    <layoutScope value="Record" />
    <jsonLayoutStyle value="Simple" />
  </layout>
</appender>
```
Parameter | Description
----------|-------------
dateTimeFormat | Allows to specify date/time format for all values of this type in log entries. It's "yyyy-MM-dd HH:mm:ss" if not specified.
referenceLoopHandling | Specifies reference loop handling. It's Ignore if not specified. Possible values: Error, Ignore, Serialize.
skippedProperties | Comma separated list of property names to be skipped from serialization. You can also use advanced version of the filter programmatically.
conversionPattern | Same thing as for PatternLayout. It is used when LayoutScope = Message
layoutScope | Denotes whether all records or only message should be presented as JSON. Possible values: Message, Record.
jsonLayoutStyle | Denotes volume of information to be logged when LayoutScope = Record. Possible values: Complete, Simple.
