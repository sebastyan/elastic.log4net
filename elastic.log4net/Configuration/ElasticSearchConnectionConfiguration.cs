using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elastic.log4net.Configuration
{
    /// <summary>
    /// Elasticsearch connection configuration.
    /// </summary>
    public class ElasticSearchConnectionConfiguration
    {
        /// <summary>
        /// Type of ElasticSearch pool to configure.
        /// </summary>
        public ElasticSearchPoolType PoolType { get; set; } = ElasticSearchPoolType.SingleNodeConnectionPool;
        /// <summary>
        /// Elastic search nodes.
        /// </summary>
        public List<Uri> ElasticNodes { get; set; }
        /// <summary>
        /// Username for Elasticsearch Basic Authentication.
        /// </summary>
        public String User { get; set; }
        /// <summary>
        /// Password for Elasticsearch Basic Authentication.
        /// </summary>
        public string Password { get; set; }
    }
}
