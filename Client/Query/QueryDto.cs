using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Client.Query
{
    public class QueryDto
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public dynamic Parameters { get; set; }

        public QueryDto()
        {
            this.PageSize = 20;
        }
    }
}
