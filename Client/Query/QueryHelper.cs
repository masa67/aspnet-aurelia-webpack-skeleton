using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Client.Query
{
    public class QueryParserResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Query Query { get; set; }
    }

    public static class QueryHelper
    {
        private static string ParseError { get; set; }

        /*
        Examples of queryJson:
        
        Simple:

        [  
           {  
              "property":"name",
              "operator":"Contains",
              "value":"As. Oy"
           },
           {  
              "property":"phone",
              "operator":"Contains",
              "value":"123"
           }
        ]

        Complex #1:

        [ 
             {
                 "operator": "OR",
                 "operands": [
                     {
                         "property": "phone",
                         "operator": "Contains",
                         "value": "123"
                     },
                     {
                         "property": "phoneMobile",
                         "operator": "Contains",
                         "value": "123"
                     }
                 ]
             }
         ]

         Complex #2:

         [ 
             {
                 "operator": "AND",
                 "operands": [
                     {
                         "property": "name",
                         "operator": "Contains",
                         "value": "Pekka"
                     },
                     {
                         "operator": "OR",
                         "operands": [
                             {
                                 "property": "phone",
                                 "operator": "Contains",
                                 "value": "123"
                             },
                             {
                                 "property": "phoneMobile",
                                 "operator": "Contains",
                                 "value": "123"
                             }
                         ]
                     }
                 ]
             }
         ]

        */

        public static QueryParserResult GetQuery(string queryJson)
        {
            var ret = new QueryParserResult();
            ret.Success = false;

            dynamic jsonObject = JsonConvert.DeserializeObject(queryJson);
            if (jsonObject == null)
            {
                ret.ErrorMessage = "invalid json";
                return ret;
            }

            if (jsonObject.Type != Newtonsoft.Json.Linq.JTokenType.Array)
            {
                ret.ErrorMessage = "array expected";
                return ret;
            }

            ret.Query = new Query();
            foreach (dynamic queryItem in jsonObject)
            {
                var parsedItem = ParseQueryItem(queryItem);
                if (parsedItem == null)
                {
                    ret.ErrorMessage = ParseError;
                    return ret;
                }

                ret.Query.Parameters.Add(parsedItem);
            }

            ret.Success = true;
            return ret;
        }

        private static QueryParameterBase ParseQueryItem(dynamic queryItem)
        {
            var op = queryItem["operator"];
            if (op == null)
            {
                ParseError = "operator expected";
                return null;
            }

            FieldOperator fieldOp;
            try
            {
                fieldOp = Enum.Parse(typeof(FieldOperator), op.ToString(), true);
            }
            catch
            {
                LogicalOperator logicalOp;
                try
                {
                    logicalOp = Enum.Parse(typeof(LogicalOperator), op.ToString(), true);
                }
                catch
                {
                    ParseError = "invalid operator " + op;
                    return null;
                }

                var operands = queryItem["operands"];
                if (operands == null)
                {
                    ParseError = "operands expected";
                    return null;
                }

                if (operands.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                {
                    ParseError = "operands should be an array";
                    return null;
                }


                var operandItems = new List<QueryParameterBase>();
                foreach (dynamic operand in operands)
                {
                    var operandItem = ParseQueryItem(operand);
                    if (operandItem == null)
                    {
                        return null;
                    }

                    operandItems.Add(operandItem);
                }

                return new LogicalParameter(logicalOp, operandItems);
            }

            var prop = queryItem["property"].ToString();
            if (prop == null)
            {
                ParseError = "property expected";
                return null;
            }

            var val = queryItem["value"].ToString();
            if (val == null)
            {
                ParseError = "value expected";
                return null;
            }

            return new FieldParameter(prop, val, fieldOp);
        }

        public static IQueryable<TEntity> AddQueryParameters<TEntity>(IQueryable<TEntity> q, Query query)
        {
            if (query.Parameters != null)
            {
                return q.Where(GenerateWhere<TEntity>(query));
            }

            return q;
        }

        public static Expression<Func<TEntity, bool>> GenerateWhere<TEntity>(Query query)
        {
            var predicate = PredicateBuilder.True<TEntity>();

            if (query.Parameters != null)
            {
                foreach (QueryParameterBase queryParam in query.Parameters)
                {
                    predicate = predicate.And(GenerateFilter<TEntity>(queryParam));
                }
            }

            return predicate;
        }

        private static Expression<Func<TEntity, bool>> GenerateFilter<TEntity>(QueryParameterBase queryParam)
        {
            Expression<Func<TEntity, bool>> ret;

            if (queryParam is FieldParameter)
            {
                ret = GenerateFieldFilter<TEntity>(queryParam as FieldParameter);
            }
            else if (queryParam is LogicalParameter)
            {
                ret = GenerateLogicalFilter<TEntity>(queryParam as LogicalParameter);
            }
            else
                throw new Exception(message: $"Internal error, invalid type of query parameter.");

            return ret;
        }

        private static Expression<Func<TEntity, bool>> GenerateFieldFilter<TEntity>(FieldParameter fieldParameter)
        {
            var property = fieldParameter.Property;
            var propertyValue = fieldParameter.Value;

            var entityType = typeof(TEntity);

            Expression expression;
            var entityProperty = entityType.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (entityProperty == null)
            {
                throw new Exception(message: $"Property '{property}' not found.");
            }

            var propertyType = entityProperty.PropertyType;
            var parameter = Expression.Parameter(entityType, "entity");

            if (entityProperty == null)
            {
                throw new Exception(message: $"Property '{property}' not found for entity '{entityType.Name}'");
            }

            // List handling - convert JArray and each JValue to generic list of the correct type
            var jsonArray = propertyValue as IList;
            if (jsonArray != null)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(propertyType));
                foreach (var item in jsonArray)
                {
                    var converted = Convert.ChangeType(item, propertyType);
                    if (converted != null)
                        list.Add(converted);
                    else
                        throw new Exception($"Cannot convert query filter value '{propertyValue}' to List<{propertyType}>");
                }
                propertyValue = list;
            }

            // Guid handling
            if (propertyType.Equals(typeof(Guid)) && !(propertyValue is Guid))
            {
                if (!(propertyValue is IList))
                {
                    if (!Guid.TryParse(propertyValue.ToString(), out Guid compareGuid))
                        throw new Exception($"Cannot convert query filter value '{propertyValue}' to Guid");
                    propertyValue = compareGuid;
                }
            }
            else if (propertyType.Equals(typeof(DateTime)) && !(propertyValue is DateTime) ||
                    propertyType.Equals(typeof(DateTime?)) && !(propertyValue is DateTime?))
            {
                if (!DateTime.TryParse(propertyValue.ToString(), out DateTime compareDate))
                    throw new Exception($"Cannot convert query filter value '{propertyValue}' to DateTime");
                propertyValue = compareDate;
            }

            // check if the property type is a value type
            // only value types work
            if (propertyType.IsValueType || propertyType.Equals(typeof(string)))
            {
                switch (fieldParameter.Operator)
                {
                    case FieldOperator.Eq:
                        if (propertyValue is IList)
                        {
                            List<Expression> equalExpressions = new List<Expression>();

                            // Create equal expressions from all values in list
                            foreach (var value in (IList)propertyValue)
                            {
                                equalExpressions.Add(Expression.Equal(
                                    Expression.Property(parameter, entityProperty),
                                    Expression.Convert(Expression.Constant(value), propertyType)));
                            }

                            if (!equalExpressions.Any())
                            {
                                // Called with empty list. Create expression that equals false.
                                expression = Expression.Equal(
                                    Expression.Convert(Expression.Constant(0), typeof(int)),
                                    Expression.Convert(Expression.Constant(1), typeof(int)));
                            }
                            else if (equalExpressions.Count == 1)
                            {
                                // Single value converts to equal expression.
                                expression = equalExpressions.Single();
                            }
                            else
                            {
                                // Create or-expressions from previously generated equal-expressions
                                Expression orExpression = equalExpressions.First();

                                for (int i = 1; i < equalExpressions.Count; i++)
                                {
                                    orExpression = Expression.OrElse(orExpression, equalExpressions[i]);
                                }

                                expression = orExpression;
                            }
                        }
                        else
                        {
                            expression = Expression.Equal(
                                Expression.Property(parameter, entityProperty),
                                Expression.Convert(Expression.Constant(propertyValue), propertyType));
                        }
                        break;
                    case FieldOperator.Gt:
                        // Expression: entity.Property > value
                        expression = Expression.GreaterThan(
                            Expression.Property(parameter, entityProperty),
                            Expression.Convert(Expression.Constant(propertyValue), propertyType)
                        );
                        break;
                    case FieldOperator.Gte:
                        // Expression: entity.Property >= value
                        expression = Expression.GreaterThanOrEqual(
                            Expression.Property(parameter, entityProperty),
                            Expression.Convert(Expression.Constant(propertyValue), propertyType)
                        );
                        break;
                    case FieldOperator.Lt:
                        // Expression: entity.Property < value
                        expression = Expression.LessThan(
                            Expression.Property(parameter, entityProperty),
                            Expression.Convert(Expression.Constant(propertyValue), propertyType)
                        );
                        break;
                    case FieldOperator.Lte:
                        // Expression: entity.Property <= value
                        expression = Expression.LessThanOrEqual(
                            Expression.Property(parameter, entityProperty),
                            Expression.Convert(Expression.Constant(propertyValue), propertyType)
                        );
                        break;
                    case FieldOperator.Contains:
                        expression = Expression.GreaterThanOrEqual(
                        Expression.Call(Expression.Property(parameter, entityProperty),
                        typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                            new Expression[] {
                              Expression.Constant(propertyValue.ToString()),
                              Expression.Constant(StringComparison.OrdinalIgnoreCase)
                            }
                          ), Expression.Constant(0));
                        break;
                    case FieldOperator.StartsWith:
                        // Expression: entity.Property starts with value. Ignore case
                        expression = Expression.Equal(
                         Expression.Call(Expression.Property(parameter, entityProperty),
                         typeof(String).GetMethod("StartsWith", new Type[] { typeof(String), typeof(StringComparison) }),
                             new Expression[] {
                          Expression.Constant(propertyValue.ToString()),
                          Expression.Constant(StringComparison.OrdinalIgnoreCase)
                             }
                           ), Expression.Constant(true));
                        break;
                    case FieldOperator.EndsWith:
                        // Expression: entity.Property ends with value. Ignore case
                        expression = Expression.Equal(
                            Expression.Call(Expression.Property(parameter, entityProperty),
                            typeof(String).GetMethod("EndsWith", new Type[] { typeof(String), typeof(StringComparison) }),
                                new Expression[] {
                                Expression.Constant(propertyValue.ToString()),
                                Expression.Constant(StringComparison.OrdinalIgnoreCase)
                                }
                                ), Expression.Constant(true));
                        break;
                    default:
                        throw new NotImplementedException($"Field operator '{fieldParameter.Operator.ToString()}' is not implemented.");
                }
            }
            // if not, then use the key (entity)
            else
            {
                // get the Id property of entity
                var idProperty = propertyType.GetProperty("Id");

                if (idProperty == null)
                {
                    throw new ArgumentException(string.Format("Could not find Id property from type '{0}'. Only entity properties implementing IEntity interface can be used.", propertyType.FullName));
                }
                expression = Expression.Equal(
                    Expression.Property(
                        Expression.Property(parameter, entityProperty),
                        idProperty
                    ),
                    Expression.Constant(
                        idProperty.GetValue(propertyValue),
                        idProperty.PropertyType
                    )
                );
            }

            return Expression.Lambda<Func<TEntity, bool>>(expression, parameter);
        }

        private static Expression<Func<TEntity, bool>> GenerateLogicalFilter<TEntity>(LogicalParameter logicalParameter)
        {
            Expression<Func<TEntity, bool>> predicate = logicalParameter.Operator == LogicalOperator.AND
                ? PredicateBuilder.True<TEntity>()
                : PredicateBuilder.False<TEntity>();

            foreach (var operand in logicalParameter.Operands)
            {
                if (logicalParameter.Operator == LogicalOperator.OR)
                {
                    predicate = predicate.Or(GenerateFilter<TEntity>(operand));
                }
                else
                {
                    predicate = predicate.And(GenerateFilter<TEntity>(operand));
                }
            }

            return predicate;
        }

        private class PropertyData
        {
            public bool IsEnumerable { get; }
            public ParameterExpression ParamExp { get; }
            public MemberExpression MemberExp { get; }

            public PropertyData(bool isEnumerable, ParameterExpression paramExp, MemberExpression memberExp)
            {
                IsEnumerable = isEnumerable;
                ParamExp = paramExp;
                MemberExp = memberExp;
            }
        }

        public static Expression<Func<TEntity, bool>> GeneratePropertyFilter<TEntity>(string property, string value)
        {
            Expression<Func<TEntity, bool>> predicate = null;
            var propertyList = new List<PropertyData>();

            var fields = property.Split(".");

            PropertyData propData = null;
            PropertyInfo propertyInfo = null;
            for (int i = 0; i < fields.Length; i++)
            {
                ParameterExpression paramExp;
                MemberExpression memberExp;
                bool isEnumberable;

                if (propData == null || !propData.IsEnumerable)
                {
                    if (i == fields.Length - 1)
                        break;

                    paramExp = Expression.Parameter(i == 0 ? typeof(TEntity) : propData.MemberExp.Type, "x" + i.ToString());
                    if (propData != null)
                    {
                        memberExp = Expression.Property(propData.MemberExp, fields[i]);
                    }
                    else
                    {
                        memberExp = Expression.Property(paramExp, fields[i]);
                    }
                    isEnumberable = memberExp.Type.GetInterface(nameof(IEnumerable)) != null;
                }
                else
                {
                    var enumerableItemType = propData.MemberExp.Type.GetProperty("Item").PropertyType;
                    propertyInfo = enumerableItemType.GetProperty(fields[i]);
                    paramExp = Expression.Parameter(enumerableItemType, "x" + i.ToString());
                    memberExp = Expression.Property(paramExp, propertyInfo);
                    isEnumberable = memberExp.Type.GetInterface(nameof(IEnumerable)) != null;
                }

                propData = new PropertyData(isEnumberable, paramExp, memberExp);
                propertyList.Add(propData);
            }

            var fieldProperty = propData.IsEnumerable
                ? Expression.Property(propData.ParamExp, propertyInfo)
                : Expression.Property(propData.MemberExp, fields[fields.Length - 1]);

            var call1 = Expression.Call(
                fieldProperty,
                typeof(String).GetMethod("IndexOf", new Type[] { typeof(String), typeof(StringComparison) }),
                    new Expression[] {
                        Expression.Constant(value),
                        Expression.Constant(StringComparison.OrdinalIgnoreCase)
                    }
            );

            var condition = Expression.GreaterThanOrEqual(call1, Expression.Constant(0));

            var rootPropertyData = propertyList.ElementAt(0);
            MethodCallExpression anyCall = null;
            for (int i = propertyList.Count - 1; i > 0; i--)
            {
                var propertyData = propertyList.ElementAt(i);
                var parentPropertyData = propertyList.ElementAt(i - 1);

                if (parentPropertyData.IsEnumerable)
                {
                    if (anyCall == null)
                    {
                        anyCall = Expression.Call(
                            typeof(Enumerable),
                            nameof(Enumerable.Any),
                            new Type[] { propertyData.ParamExp.Type },
                            propertyList.ElementAt(i - 1).MemberExp,
                            Expression.Lambda(condition, propertyData.ParamExp)
                        );
                    }
                    else
                    {
                        anyCall = Expression.Call(
                            typeof(Enumerable),
                            nameof(Enumerable.Any),
                            new Type[] { propertyData.ParamExp.Type },
                            propertyList.ElementAt(i - 1).MemberExp,
                            Expression.Lambda(anyCall, propertyData.ParamExp)
                        );
                    }
                }
            }

            predicate = anyCall != null
                ? Expression.Lambda<Func<TEntity, bool>>(anyCall, rootPropertyData.ParamExp)
                : Expression.Lambda<Func<TEntity, bool>>(condition, rootPropertyData.ParamExp);

            return predicate;
        }
    }
}
