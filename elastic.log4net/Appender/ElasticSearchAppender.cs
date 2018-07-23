using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using elastic.log4net.Model;
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
        private BlockingCollection<KeyValuePair<LogEntry, int>> errorMessage = new BlockingCollection<KeyValuePair<LogEntry, int>>();

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
        /// Disable ping process to detect if nodes are alive or not.
        /// </summary>
        public bool DisableConnectionPing { get; set; } = false;
        /// <summary>
        /// Use an index pattern to create a part of index name dinamically
        /// </summary>
        public string IndexPattern { get; set; }
        /// <summary>
        /// Configure retry mode.
        /// </summary>
        public RetryErrorsConfiguration RetryErrorsConfiguration { get; set; }
        /// <summary>
        /// Property to be used for test purposes.
        /// </summary>
        public IElasticClient Client
        {
            set
            {
                client = value;
                if (RetryErrorsConfiguration != null)
                {
                    RetryErrorMessages();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            InitializeElasticClientConnection();
            SendLogEvent(loggingEvent);
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
                if (this.DisableConnectionPing)
                {
                    settings.DisablePing();
                }
                this.client = new ElasticClient(settings);
                if(RetryErrorsConfiguration != null)
                {
                    RetryErrorMessages();
                }
            }
        }

        private Elasticsearch.Net.StaticConnectionPool ConfigureElasticSearchConnectionPool()
        {
            var elasticNodeUris = new List<Uri>();
            this.elasticNodes.ForEach(node =>
            {
                elasticNodeUris.Add(new Uri(node));
            });
            var pool = new Elasticsearch.Net.StaticConnectionPool(elasticNodeUris);
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

        private async void SendLogEvent(LoggingEvent loggingEvent)
        {
            try
            {
                var data = CreateLogEntryForElasticsearch(loggingEvent);
                IIndexResponse result = await SendData(data);
                if (!result.IsValid && CheckIfLogEntryCanBeRetried(loggingEvent))
                {
                    this.errorMessage.Add(new KeyValuePair<LogEntry, int>(data, 1));
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Error on ElasticsearchAppender adding new index.", ex);
            }
        }

        private bool CheckIfLogEntryCanBeRetried(LoggingEvent loggingEvent)
        {
            return RetryErrorsConfiguration != null && CheckMinLogLevel(loggingEvent.Level);
        }

        private async Task<IIndexResponse> SendData(LogEntry data)
        {
            IIndexResponse result;
            if (string.IsNullOrEmpty(IndexPattern))
            {
                result = await this.client.IndexDocumentAsync(data);
            }
            else
            {
                result = await this.client.IndexAsync<LogEntry>(data,
                    idx => idx.Index(String.Format("{0}{1}", this.BaseIndex, DateTime.Now.ToString(IndexPattern))));
            }

            return result;
        }

        private bool CheckMinLogLevel(Level logEntryLevel)
        {
            if(logEntryLevel.Value >= RetryErrorsConfiguration.MinLevelToRetry.Value)
            {
                return true;
            }
            return false;
        }

        private void RetryErrorMessages()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    var data = this.errorMessage.Take();
                    var result = SendData(data.Key).Result;
                    if (!result.IsValid && data.Value < RetryErrorsConfiguration.MaxNumberOfRetries)
                    {
                        this.errorMessage.Add(new KeyValuePair<LogEntry, int>(data.Key, data.Value + 1));
                        Thread.Sleep(RetryErrorsConfiguration.WaitTimeBetweenRetry);
                    }
                } while (true);
            });
        }
    }
}