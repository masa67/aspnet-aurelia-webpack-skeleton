using System.Collections.Generic;

namespace Client.Query
{
    public class LogicalParameter : QueryParameterBase
    {
        public LogicalOperator Operator { get; set; }

        public List<QueryParameterBase> Operands { get; set; }

        public LogicalParameter(LogicalOperator lOperator, List<QueryParameterBase> operands)
        {
            this.Operator = lOperator;
            this.Operands = operands;
        }
    }
}
