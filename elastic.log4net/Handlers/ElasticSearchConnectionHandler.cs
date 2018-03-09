using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using elastic.log4net.Configuration;
using Elasticsearch.Net;
using Nest;

namespace elastic.log4net.Handlers
{
    public static class ElasticSearchConnectionHandler
    {
        /// <summary>
        /// Handler to create connections with connection pool configuration.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static ConnectionSettings CreateElasticSearchConnection(ElasticSearchConnectionConfiguration configuration)
        {
            IConnectionPool pool;
            switch (configuration.PoolType)
            {
                case ElasticSearchPoolType.SingleNodeConnectionPool:
                    pool = new SingleNodeConnectionPool(configuration.ElasticNodes.First());
                    break;
                case ElasticSearchPoolType.StaticConnectionPool:
                    pool = new StaticConnectionPool(configuration.ElasticNodes);
                    break;
                case ElasticSearchPoolType.SniffingConnectionPool:
                    pool = new SniffingConnectionPool(configuration.ElasticNodes);
                    break;
                case ElasticSearchPoolType.StickyConnectionPool:
                    pool = new StickyConnectionPool(configuration.ElasticNodes);
                    break;
                default:
                    throw new InvalidOperationException("Pool configuration type not defined");
            }

            var settings = new ConnectionSettings(pool);
            if (configuration.User != null && configuration.Password != null)
            {
                settings.BasicAuthentication(configuration.User, configuration.Password);
            }

            return settings;
        }
    }
}
