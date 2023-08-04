using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManageSite.Models.Library
{
    public class JqDataTableInfo
    {
        public string sEcho { get; set; }
        public long iTotalRecords { get; set; }
        public long iTotalDisplayRecords { get; set; }
        public IEnumerable<string[]> aaData { get; set; }
    }
}