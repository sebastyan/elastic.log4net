﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using log4net;
using elastic;
using log4net.Config;
using log4net.Repository;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using log4net.Core;

namespace elastic.log4net.Test
{
    [TestFixture]
    public class ElasticSearchAppenderTest
    {
        private Fixture fixture = new Fixture();

        [Test]
        public void ShouldLogMessageWithoutAutogeneratedIndexPatternSendToElasticSearch()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            var mockedElasticClient = new Mock<Nest.IElasticClient>();
            var elasticAppender = new Appender.ElasticSearchAppender();
            elasticAppender.Client = mockedElasticClient.Object;

            BasicConfigurator.Configure(repository, elasticAppender);

            ILog log = LogManager.GetLogger(repository.Name, "LogMessageAreCorrectlySendToElasticSearch");
            log.Error("Testing error message", fixture.Create<InvalidOperationException>());
            log.Debug("Debug message", fixture.Create<InvalidOperationException>());

            mockedElasticClient.Verify(m => m.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            mockedElasticClient.Verify(m => m.IndexAsync(It.IsAny<Model.LogEntry>(),
                It.IsAny<Func<Nest.IndexDescriptor<Model.LogEntry>, Nest.IIndexRequest<Model.LogEntry>>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void ShouldLogMessageWithAutogeneratedIndexPatternSendToElasticSearch()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            var mockedElasticClient = new Mock<Nest.IElasticClient>();
            var elasticAppender = new Appender.ElasticSearchAppender();
            elasticAppender.IndexPattern = "dd-MM-yyyy";
            elasticAppender.Client = mockedElasticClient.Object;

            BasicConfigurator.Configure(repository, elasticAppender);

            ILog log = LogManager.GetLogger(repository.Name, "LogMessageAreCorrectlySendToElasticSearch");
            log.Error("Testing error message", fixture.Create<InvalidOperationException>());
            log.Debug("Debug message", fixture.Create<InvalidOperationException>());

            mockedElasticClient.Verify(m => m.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>()), Times.Never);
            mockedElasticClient.Verify(m => m.IndexAsync<Model.LogEntry>(It.IsAny<Model.LogEntry>(),
                It.IsAny<Func<Nest.IndexDescriptor<Model.LogEntry>, Nest.IIndexRequest<Model.LogEntry>>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public void ShouldRetryErrorMessagesThreeTimesBeforeDiscardIt()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            var mockedElasticClient = new Mock<Nest.IElasticClient>();
            var elasticAppender = new Appender.ElasticSearchAppender();
            elasticAppender.RetryErrorsConfiguration = new RetryErrorsConfiguration { MaxNumberOfRetries = 2, WaitTimeBetweenRetry = 50};
            elasticAppender.Client = mockedElasticClient.Object;

            BasicConfigurator.Configure(repository, elasticAppender);

            var indexResponse = "{\"_index\":\"logEntry\",\"_type\":\"log\",\"_id\":\"20\",\"_version\":\"1.0.0\",\"created\":false}";
            var response = JsonConvert.DeserializeObject<Nest.IndexResponse>(indexResponse);
            mockedElasticClient.Setup(x => x.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<Nest.IIndexResponse>(response));

            ILog log = LogManager.GetLogger(repository.Name, "LogMessageAreCorrectlySendToElasticSearch");
            log.Error("Testing error message", fixture.Create<InvalidOperationException>());

            Thread.Sleep(2000);

            mockedElasticClient.Verify(m => m.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Test]
        public void ShouldRetryOnlyErrorMessagesAndNoMessagesWithLessPriority()
        {
            ILoggerRepository repository = LogManager.CreateRepository(Guid.NewGuid().ToString());
            var mockedElasticClient = new Mock<Nest.IElasticClient>();
            var elasticAppender = new Appender.ElasticSearchAppender();
            elasticAppender.RetryErrorsConfiguration = new RetryErrorsConfiguration { MaxNumberOfRetries = 1, WaitTimeBetweenRetry = 50, MinLevelToRetry = Level.Error };
            elasticAppender.Client = mockedElasticClient.Object;

            BasicConfigurator.Configure(repository, elasticAppender);

            var indexResponse = "{\"_index\":\"logEntry\",\"_type\":\"log\",\"_id\":\"20\",\"_version\":\"1.0.0\",\"created\":false}";
            var response = JsonConvert.DeserializeObject<Nest.IndexResponse>(indexResponse);
            mockedElasticClient.Setup(x => x.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<Nest.IIndexResponse>(response));

            ILog log = LogManager.GetLogger(repository.Name, "LogMessageAreCorrectlySendToElasticSearch");
            log.Error("Testing error message", fixture.Create<InvalidOperationException>());
            log.Debug("Testing debug message", fixture.Create<InvalidOperationException>());
            log.Info("Testing info message", fixture.Create<InvalidOperationException>());

            Thread.Sleep(2000);

            mockedElasticClient.Verify(m => m.IndexDocumentAsync(It.IsAny<Model.LogEntry>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
        }
    }
}
