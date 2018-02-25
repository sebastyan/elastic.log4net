# elastic.log4net
ElasticSearch appender for log4net

[![Build Status](https://travis-ci.org/sebastyan/elastic.log4net.svg?branch=master)](https://travis-ci.org/sebastyan/elastic.log4net)

Library with a log4net appender for ElasticSearch.

## How to configure it
The appender for ElasticSearch could be configured adding configuration in App.config or Web.config file.

A new section with log4net name needs to be declared inside configSections section on config file. 
```xml
<configSections>
	<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
</configSections>
```

In order to configure configure the ElasticSearch's appender for the log4net, new appender tag needs to be define inside <log4net>. 
```xml
<appender name="elasticappender" type="elastic.log4net.Appender.ElasticSearchAppender, elastic.log4net">
     <elasticNode value="http://localhost:9200"></elasticNode>
     <baseIndex value="logEntry"></baseIndex>
</appender>
```

`baseIndex` is not a mandatory parameter. If yoy don't specify it, the default index value to use will be `log4net`.

Here you can see a complete config file example:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
	<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
  </configSections>
	<log4net>
	<root>
	  <level value="ALL" />
	  <appender-ref ref="elasticappender" />
	</root>
	<appender name="elasticappender" type="elastic.log4net.Appender.ElasticSearchAppender, elastic.log4net">
	  <elasticNode value="http://localhost:9200"></elasticNode>
	  <baseIndex value="logEntry"></baseIndex>
	</appender>
  </log4net>
</configuration>
```

To declare it on C# code you can obtain the instance of the logger using this line of code:
```c#
ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
```

The log message will be sent to ElasticSearch in json format, using the index configured in the appender or default index. This is an example of message log stored.
```json
{
   "_index":"log4net",
   "_type":"object",
   "_id":"ESJbkWEBbaUJNS1lOmMj",
   "_score":1,
   "_source":{
      "timeStamp":"2018-02-13T23:48:49.9385645+01:00",
      "message":"Excepcion",
      "level":"ERROR",
      "loggerName":"Application.Program",
      "domain":"Application.exe",
      "userName":"devgroup\\developer",
      "threadName":"1",
      "exception":{
         "type":"DivideByZeroException",
         "message":"Number cannot be divided by 0.",
         "stackTrace":"   en Application.Program.Main(String[] args) en C:\\git\\elastic.log4net\\Application\\Program.cs:line 18"
      },
      "locationInfo":{
         "className":"Application.Program",
         "lineNumber":"24",
         "fullPath":"C:\\git\\elastic.log4net\\Application\\Program.cs",
         "methodName":"Main",
         "fullInfo":"ConsoleApp1.Program.Main(C:\\git\\elastic.log4net\\Application\\Program.cs:24)"
      }
   }
}
```
