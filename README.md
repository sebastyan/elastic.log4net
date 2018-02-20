# elastic.log4net
ElasticSearch appender for log4net

[![Build Status](https://travis-ci.org/sebastyan/elastic.log4net.svg?branch=master)](https://travis-ci.org/sebastyan/elastic.log4net)

Library with a log4net appender for ElasticSearch.

##How to configure it
The appender for ElasticSearch could be configured adding configuration in App.config or Web.config file.

A new section with log4net name needs to be declared inside configSections tag on config file. 
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