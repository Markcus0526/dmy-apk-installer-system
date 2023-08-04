using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace ManageSite.Models.Library
{
    public class JqGridFilters
    {
        public enum GroupOp
        {
            AND,
            OR
        }
        public enum Operations
        {
            eq, // "equal"
            ne, // "not equal"
            lt, // "less"
            le, // "less or equal"
            gt, // "greater"
            ge, // "greater or equal"
            bw, // "begins with"
            bn, // "does not begin with"
            //in, // "in"
            //ni, // "not in"
            ew, // "ends with"
            en, // "does not end with"
            cn, // "contains"
            nc  // "does not contain"
        }
        public class Rule
        {
            public string field { get; set; }
            public Operations op { get; set; }
            public string data { get; set; }
        }

        public GroupOp groupOp { get; set; }
        public List<Rule> rules { get; set; }
        public static readonly string[] FormatMapping = {
            "({0} == '{1}')",                 // "eq" - equal
            "({0} != '{1}')",                // "ne" - not equal
            "({0} < {1})",                 // "lt" - less than
            "({0} <= {1})",                // "le" - less than or equal to
            "({0} > {1})",                 // "gt" - greater than
            "({0} >= {1})",                // "ge" - greater than or equal to
            "({0} LIKE '{1}%')",        // "bw" - begins with
            "({0} NOT LIKE '{1}%')",    // "bn" - does not begin with
            "({0} LIKE '%{1}')",        // "ew" - ends with
            "({0} NOT LIKE '%{1}')",    // "en" - does not end with
            "({0} LIKE '%{1}%')",    // "cn" - contains
            "({0} NOT LIKE '%{1}%')" //" nc" - does not contain
        };

//         public List<T> FilterObjectSet<T>(ObjectQuery<T> inputQuery) where T : class
//         {
//             if (rules.Count <= 0)
//                 return inputQuery;
// 
//             var sb = new StringBuilder();
//             var objParams = new List<ObjectParameter>(rules.Count);
// 
//             foreach (Rule rule in rules)
//             {
//                 PropertyInfo propertyInfo = typeof(T).GetProperty(rule.field);
//                 if (propertyInfo == null)
//                     continue; // skip wrong entries
// 
//                 if (sb.Length != 0)
//                     sb.Append(groupOp);
// 
//                 var iParam = objParams.Count;
//                 sb.AppendFormat(FormatMapping[(int)rule.op], rule.field, iParam);
// 
//                 // TODO: Extend to other data types
//                 objParams.Add(String.Compare(propertyInfo.PropertyType.FullName,
//                                              "System.Int32", StringComparison.Ordinal) == 0
//                                   ? new ObjectParameter("p" + iParam, Int32.Parse(rule.data))
//                                   : new ObjectParameter("p" + iParam, rule.data));
//             }
// 
//             ObjectQuery<T> filteredQuery = inputQuery.Where(sb.ToString());
//             foreach (var objParam in objParams)
//                 filteredQuery.Parameters.Add(objParam);
// 
//             return filteredQuery;
//         }
    }

}