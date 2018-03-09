using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elastic.log4net.Configuration
{
    /// <summary>
    /// Enumeration types for ElasticSearch connection pool types.
    /// </summary>
    public enum ElasticSearchPoolType
    {
        /// <summary>
        /// Single node connection pool type.
        /// </summary>
        SingleNodeConnectionPool,
        /// <summary>
        ///Static connection pool type.
        /// </summary>
        StaticConnectionPool,
        /// <summary>
        /// Sniffing connection pool type.
        /// </summary>
        SniffingConnectionPool,
        /// <summary>
        /// Sticky connection pool type.
        /// </summary>
        StickyConnectionPool
    }
}
