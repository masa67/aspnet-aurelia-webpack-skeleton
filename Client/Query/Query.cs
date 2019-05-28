using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections;

namespace Client.Query
{
    public class Query
    {
        public List<QueryParameterBase> Parameters { get; set; }

        public Query()
        {
            Parameters = new List<QueryParameterBase>();
        }
    }
}
