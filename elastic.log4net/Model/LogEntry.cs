using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;

namespace elastic.log4net.Model
{
    /// <summary>
    /// Model class with log entry.
    /// </summary>
    public class LogEntry
    {
        public DateTimeOffset TimeStamp { get; set; }
        public String Message { get; set; }
        public String Level { get; set; }
        public String LoggerName { get; set; }
        public String Domain { get; set; }
        public String UserName { get; set; }
        public String ThreadName { get; set; }
        public LogEntryException Exception { get; set; }
        public LogEntryLocationInformation LocationInfo { get; set; }
        public ReadOnlyPropertiesDictionary GlobalContext { get; set; }
    }
}
