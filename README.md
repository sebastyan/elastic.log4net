# elastic.log4net
ElasticSearch appender for log4net

[![Build history](https://buildstats.info/travisci/chart/sebastyan/elastic.log4net)](https://travis-ci.org/sebastyan/elastic.log4net/builds)

[![NuGet Badge](https://buildstats.info/nuget/log4net.els)](https://www.nuget.org/packages/log4net.els/)

Library with a log4net appender for ElasticSearch.

## **Introduction**

This module is a custom appender for log4net. It sends your log messages to an ElasticSearch cluster to be reviewed using Kibana or obtains log statics using another tools like Grafana. 

The current version is develop to be compatible with the following frameworks:

- .NET Framework 4.6.1
- .NET Core 2.0+

It was developed using Visual Studio Code (https://code.visualstudio.com/)

## How to build it

In order to compile the project we need to instala NETCoreSDK 2.0+( You can find the last version at https://dotnet.microsoft.com/download).

If you already have installed the NetCoreSDK you can execute this command on a terminal or command line. 

`dotnet build ./elastic.log4net/elastic.log4net.csproj`

If you want to execute test units you need to execute the following commands:

```shell
dotnet build ./elastic.log4net/elastic.log4net.csproj
dotnet restore ./elastic.log4net.Test/elastic.log4net.Test.csproj
dotnet test ./elastic.log4net.Test/elastic.log4net.Test.csproj
```



## How to configure it

#### NET Framework 4.6.1+

The log4net appender for ElasticSearch could be configured adding configuration in App.config, Web.config file.

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
	<elasticNode value="http://192.168.0.120:9200"></elasticNode>
	<elasticNode value="http://my.dns.host:9200"></elasticNode>
	<enableGlobalContextLog value="true"></enableGlobalContextLog>
	<disableLocationInfo value="true"></disableLocationInfo>
	<disableConnectionPing value="true"></disableConnectionPing>
	<indexPattern value="ddMMyyy"></disableConnectionPing>
     	<baseIndex value="logentry"></baseIndex>
</appender>
```

#### NET Core 2.0+

The log4net appender for ElasticSearch in NetCore could be configure adding *log4net.config* file in you project.

The file contains the log4net appenders configurations. You can see an example in the snippet below.

```xml
<log4net>
    <root>
        <level value="ALL" />
        <appender-ref ref="elasticappender" />
    </root>
    <appender name="elasticappender" type="elastic.log4net.Appender.ElasticSearchAppender, elastic.log4net">
      <elasticNode value="http://localhost:9200"></elasticNode>
      <enableGlobalContextLog value="true"></enableGlobalContextLog>
      <disableLocationInfo value="true"></disableLocationInfo>
      <disableConnectionPing value="true"></disableConnectionPing>
      <indexPattern value="ddMMyyy"></indexPattern>
      <baseIndex value="netcoreappender"></baseIndex>
    </appender>
</log4net>
```

#### Configuration parameters

| Parameter                | Description                                                  | Versions  compatible |
| ------------------------ | ------------------------------------------------------------ | -------------------- |
| `elasticNode`            | It represents an ElasticSearch node in a cluster. A new connection pool is created with the number of nodes defined on configuration. *You can define only one node if you prefer, **only one is mandatory*** | 0.1.0+               |
| `enableGlobalContextLog` | This configuration option sends with each log message the information in GlobalContext properties. **If this key is not specified the default value will be false** | 0.2.0+               |
| `disableLocationInfo`    | This configuration option delete location information from all log messages in order to reduce the information logged. **If this key is not specified the default value will be false** | 0.2.0+               |
| `disableConnectionPing`  | This configuration option disables the ping validation before call for first time to Elasticsearch connection or after a long period time in order to verify if Elasticsearch node is alive. **If this key is not specified the default value will be false** | 0.3.0+               |
| `baseIndex`              | Indicates the index name to use in Elasticsearch where the documents will be storaged. It is not a mandatory parameter. If you don't specify it, the default index value to use will be | 0.1.0+               |
| `indexPattern`           | It appends datetime pattern after baseIndex string in order to create dynamic document indexes. **It is an optional parameter. If any indexPattern is specify, the index to be used will be only baseIndex** | 0.3.0+               |



**Important note:** `enableGlobalContextLog` uses reflection and it could penalize the performance. By default, only one time the GlobalContext properties will be loaded. In case that you add more GlobalContext properties at runtime please add this piece of code just after the set the new GlobalContext property, it reloads  the GlobalContext properties.

```c#
GlobalContext.Properties["RELOAD_GLOBAL_CACHE"] = true;
```

## How to use in code

### NetFramework 4.1.6+

Add the following line of code in file *AssemblyInfo.cs* file:

`[assembly: log4net.Config.XmlConfigurator(Watch = true)`

To declare it on C# code you can obtain the instance of the logger using this line of code:

```c#
ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
```

### NetCore 2.0+

To declare it on C# for NetCore, you can obtain the instance of the logger using this line of code:
```c#
ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
```



### Log message in Elasticsearch example


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


