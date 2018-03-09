# elastic.log4net
ElasticSearch appender for log4net

[![Build history](https://buildstats.info/travisci/chart/sebastyan/elastic.log4net)](https://travis-ci.org/sebastyan/elastic.log4net/builds)

[![NuGet Badge](https://buildstats.info/nuget/log4net.els)](https://www.nuget.org/packages/log4net.els/)

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

### Current version (0.2.0)
```xml
<appender name="elasticappender" type="elastic.log4net.Appender.ElasticSearchAppender, elastic.log4net">
	<elasticNode value="http://localhost:9200"></elasticNode>
	<elasticNode value="http://192.168.0.120:9200"></elasticNode>
	<elasticNode value="http://my.dns.host:9200"></elasticNode>
	<enableGlobalContextLog value="true"></enableGlobalContextLog>
	<disableLocationInfo value="true"></disableLocationInfo>
     <baseIndex value="logentry"></baseIndex>
</appender>
```

`baseIndex` is not a mandatory parameter. If yoy don't specify it, the default index value to use will be `log4net`.

`elasticNode` each node represents an ElasticSearch node in a cluster. A new connection pool is created with the number of nodes defined on configuration. *You can define only one node if you prefer, only one is mandatory*

`enableGlobalContextLog` this configuration option sends with each log message the information in GlobalContext properties. **If this key is not specified the default value will be false**

`disableLocationInfo` this configuration option delete location information from all log messages in order to reduce the information logged. **If this key is not specified the default value will be false**

**Important note:** `enableGlobalContextLog` uses reflection and it could penalize the performace. By default, only one time the GlobalContext properties will be loaded. In case that you add more GlobalContext properties runtime plese add this piece of code just after ser the new GlobalContext property.
```c#
GlobalContext.Properties["RELOAD_GLOBLAL_CACHE"] = true;
```


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
		<elasticNode value="http://192.168.0.120:9200"></elasticNode>
		<elasticNode value="http://my.dns.host:9200"></elasticNode>
		<enableGlobalContextLog value="true"></enableGlobalContextLog>
		<disableLocationInfo value="true"></disableLocationInfo>
		<baseIndex value="logentry"></baseIndex>
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
         "fullInfo":"Application.Program.Main(C:\\git\\elastic.log4net\\Application\\Program.cs:24)"
      },
      "globalContext":{
         "RELOAD_GLOBLAL_CACHE":true,
         "OPERATION":"Operation1",
         "log4net:HostName":"HostName"
      }
   }
}
```

### Version 0.1.0
```xml
<appender name="elasticappender" type="elastic.log4net.Appender.ElasticSearchAppender, elastic.log4net">
     <elasticNode value="http://localhost:9200"></elasticNode>
     <baseIndex value="logentry"></baseIndex>
</appender>
```

`baseIndex` is not a mandatory parameter. If yoy don't specify it, the default index value to use will be `log4net`.
`elasticNode` url address where ElasticSearch is listening request.

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
	  <baseIndex value="logentry"></baseIndex>
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
         "fullInfo":"Application.Program.Main(C:\\git\\elastic.log4net\\Application\\Program.cs:24)"
      }
   }
}
```


