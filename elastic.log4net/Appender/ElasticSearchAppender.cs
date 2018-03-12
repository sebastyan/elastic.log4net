using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using elastic.log4net.Model;
using Elasticsearch.Net;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Util;
using Nest;

namespace elastic.log4net.Appender
{

    public class ElasticSearchAppender : AppenderSkeleton
    {/// <summary>
     /// Log4net appender for ElasticSearch.
     /// </summary>
        private IElasticClient client;
        private List<string> elasticNodes = new List<string>();
        private ReadOnlyPropertiesDictionary globalPropertiesProcessed;

        private const string RELOAD_GLOBAL_CACHE = "RELOAD_GLOBAL_CACHE";


        /// <summary>
        /// Base index to insert data into a Elasticsearch database.
        /// </summary>
        public string BaseIndex { get; set; } = "log4net";
        /// <summary>
        /// Username for Elasticsearch Basic Authentication.
        /// </summary>
        public string UserName { get; set; } = null;
        /// <summary>
        /// Password for Elasticsearch Basic Authentication.
        /// </summary>
        public string Password { get; set; } = null;
        /// <summary>
        /// Enables log variables in GlobalContext
        /// </summary>
        public bool EnableGlobalContextLog { get; set; } = false;
        /// <summary>
        /// Disables location information on log messages.
        /// </summary>
        public bool DisableLocationInfo { get; set; } = false;
        /// <summary>
        /// Property to be used for test purposes.
        /// </summary>
        public IElasticClient Client { set => client = value; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            InitializeElasticClientConnection();
            var logEntry = CreateLogEntryForElasticsearch(loggingEvent);
            SendLogEvent(logEntry);
        }

        /// <summary>
        /// Public method to add ElasticSearch nodes.
        /// </summary>
        /// <param name="elasticNode"></param>
        public void AddElasticNode(string elasticNode)
        {
            var node = elasticNode.Trim().ToLower();
            if (!this.elasticNodes.Contains(node))
            {
                this.elasticNodes.Add(node);
            }
        }

        private void InitializeElasticClientConnection()
        {
            if (this.client == null)
            {
                var pool = ConfigureElasticSearchConnectionPool();
                var settings = new ConnectionSettings(pool).DefaultIndex(this.BaseIndex);
                if (UserName != null && Password != null)
                {
                    settings.BasicAuthentication(UserName, Password);
                }
                this.client = new ElasticClient(settings);
            }
        }

        private StaticConnectionPool ConfigureElasticSearchConnectionPool()
        {
            var elasticNodeUris = new List<Uri>();
            this.elasticNodes.ForEach(node =>
            {
                elasticNodeUris.Add(new Uri(node));
            });
            var pool = new StaticConnectionPool(elasticNodeUris);
            return pool;
        }

        private LogEntry CreateLogEntryForElasticsearch(LoggingEvent loggingEvent)
        {
            return new LogEntry
            {
                TimeStamp = loggingEvent.TimeStamp,
                Message = loggingEvent.RenderedMessage,
                Level = loggingEvent.Level.ToString(),
                Domain = loggingEvent.Domain,
                LoggerName = loggingEvent.LoggerName,
                UserName = loggingEvent.UserName,
                ThreadName = loggingEvent.ThreadName,
                Exception = CreateExceptionForLogEntry(loggingEvent.ExceptionObject),
                LocationInfo = CreateLocationInfoForLogEntry(loggingEvent.LocationInformation),
                GlobalContext = GetLog4NetGlobalContext()
            };
        }

        private LogEntryException CreateExceptionForLogEntry(Exception exception)
        {
            if (exception == null)
                return null;
            return new LogEntryException
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace,
                Type = exception.GetType().Name,
                InnerException = exception.InnerException != null ? exception.InnerException.Message : string.Empty
            };
        }

        private LogEntryLocationInformation CreateLocationInfoForLogEntry(LocationInfo location)
        {
            if (location == null || DisableLocationInfo)
                return null;
            return new LogEntryLocationInformation
            {
                ClassName = location.ClassName,
                FullInfo = location.FullInfo,
                FullPath = location.FileName,
                LineNumber = location.LineNumber,
                MethodName = location.MethodName
            };
        }

        private ReadOnlyPropertiesDictionary GetLog4NetGlobalContext()
        {
            if (!EnableGlobalContextLog)
            {
                return null;
            }

            //Avoid load GlobalContext information in for each log call.
            if (this.globalPropertiesProcessed == null
                || (GlobalContext.Properties[RELOAD_GLOBAL_CACHE] != null
                && (bool)GlobalContext.Properties[RELOAD_GLOBAL_CACHE]))
            {
                GlobalContext.Properties.Remove(RELOAD_GLOBAL_CACHE);
                var globalContexPropertiesMethods = GlobalContext.Properties
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);

                MethodInfo methodInfo = globalContexPropertiesMethods.FirstOrDefault(x => x.Name == "GetReadOnlyProperties");
                this.globalPropertiesProcessed = new ReadOnlyPropertiesDictionary(((ReadOnlyPropertiesDictionary)methodInfo.Invoke(GlobalContext.Properties, null)));
            }
            return this.globalPropertiesProcessed;
        }

        private async void SendLogEvent(LogEntry data)
        {
            try
            {
                var result = await this.client.IndexDocumentAsync(data);
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Error on ElasticsearchAppender adding new index.", ex);
            }
        }
    }
}
