using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;

namespace elastic.log4net
{
    public class RetryErrorsConfiguration
    {
        public int WaitTimeBetweenRetry { get; set; } = 200;
        public int MaxNumberOfRetries { get; set; } = 0;
        public Level MinLevelToRetry { get; set; } = Level.All;
    }
}
