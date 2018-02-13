using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elastic.log4net.Model
{
    public class LogEntryLocationInformation
    {
        public String ClassName { get; set; }
        public String LineNumber { get; set; }
        public String FullPath { get; set; }
        public String MethodName { get; set; }
        public String FullInfo { get; set; }
    }
}
