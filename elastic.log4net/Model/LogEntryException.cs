using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elastic.log4net.Model
{
    public class LogEntryException
    {
        public String Type { get; set; }
        public String Message { get; set; }
        public String StackTrace { get; set; }
        public String InnerException { get; set; }
    }
}
