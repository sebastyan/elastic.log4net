using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using elastic.log4net.Model;
using Elasticsearch.Net;
using log4net.Appender;
using log4net.Core;
using Nest;

namespace elastic.log4net.Appender
{
    /// <summary>
    /// Log4net appender for ElasticSearch.
    /// </summary>
    public class ElasticSearchAppender : AppenderSkeleton
    {
        private IElasticClient client;

        /// <summary>
        /// URL string of Elasticsearch node.
        /// </summary>
        private List<string> elasticNodes = new List<string>();
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
            if(this.client == null)
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
            if (location == null)
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

        private async void SendLogEvent(LogEntry data)
        {
            try
            {
                var result = await this.client.IndexDocumentAsync(data);
            }
            catch(Exception ex)
            {
                ErrorHandler.Error("Error on ElasticsearchAppender adding new index.", ex);
            }
        }
    }
}
