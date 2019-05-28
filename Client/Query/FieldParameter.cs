namespace Client.Query
{
    public class FieldParameter : QueryParameterBase
    {
        public string Property { get; set; }

        public FieldOperator Operator { get; set; }

        public object Value { get; set; }

        public FieldParameter(string property, object value, FieldOperator fOperator = FieldOperator.Eq)
        {
            this.Property = property;
            this.Operator = fOperator;
            this.Value = value;
        }
    }
}
