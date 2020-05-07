using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using elastic.log4net.Appender;
using elastic.log4net.Model;
using log4net;
using log4net.Config;
using log4net.Repository;
using Moq;
using Nest;
using Xunit;

namespace elastic.log4net.Test
{
    public class ElasticSearchAppenderTest
    {
        private Fixture fixture = new Fixture();

        [Fact]
        public void LogMessageAreCorrectlySendToElasticSearch()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            var mockedElasticClient = new Mock<IElasticClient>();
            var elasticAppender = new ElasticSearchAppender();
            elasticAppender.Client = mockedElasticClient.Object;

            BasicConfigurator.Configure(repository, elasticAppender);

            ILog log = LogManager.GetLogger(repository.Name, "LogMessageAreCorrectlySendToElasticSearch");
            log.Error("Testing error message", fixture.Create<InvalidOperationException>());
            log.Debug("Debug message", fixture.Create<InvalidOperationException>());

            mockedElasticClient.Verify(m => m.IndexDocumentAsync(It.IsAny<LogEntry>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}
