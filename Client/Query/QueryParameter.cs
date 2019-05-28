using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Client.Query
{
    public class QueryParserResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<QueryParameterBase> Query { get; set; }
    }

    public static class Query
    {
        public static List<QueryParameterBase> Parameters { get; set; }
        private static string ParseError {get; set; }

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

            ret.Query = new List<QueryParameterBase>();
            foreach (dynamic queryItem in jsonObject)
            {
                var parsedItem = ParseQueryItem(queryItem);
                if (parsedItem == null)
                {
                    ret.ErrorMessage = ParseError;
                    return ret;
                }

                ret.Query.Add(parsedItem);
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
    }
}
