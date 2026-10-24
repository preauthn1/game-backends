using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FW.Core.Helper
{
    public static class SQLHelper
    {
        
        public static string GetInsertCommand<T>()
        {
            return GetInsertCommand<T>(null);
        }


        public static string GetInsertCommand<T>(List<string> ignoreFields)
        {
            string tablename = GetTableName<T>();
            if (string.IsNullOrEmpty(tablename))
            {
                throw new Exception("No Table attribute was found.");
            }

            var query = GetInsertCommand<T>(tablename, ignoreFields);
            return query;
        }


        public static string GetInsertCommand<T>(string tableName , List<string> ignoreFields = null)
        {
            var query = string.Format("INSERT INTO {0} ({1}) VALUES({2})",
                tableName,
                GetFieldList<T>(ignoreFields),
                GetFieldPatternList<T>(ignoreFields));

            return query;
        }

        public static string GetTableName<T>()
        {
            var tableAttr = typeof(T).Name;
            return tableAttr;
        }

        private static string GetFieldList<T>(List<string> ignoreFields, string prefix = "")
        {
            var sb = new StringBuilder();
            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields);

            foreach (var item in filterProperties.OrderBy(x=> x.Name))
            {
                if (IsICollection(item.PropertyType))
                {
                    continue;
                }

                if (IsVirtual(item))
                {
                    continue;
                }

                if(IsKeyColumn(item))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    sb.Append($" [{item.Name}],");
                }
                else
                {
                    sb.Append($" {prefix}{item.Name},");
                }
            }
            var query = sb.ToString();
            return query.Remove(query.Length - 1);
        }


        public static string ReplaceToTotalCountCommand(string sql )
        {
            if (sql.Contains("select") && sql.Contains("from"))
                throw new Exception("Please use upper case.");
            
            return System.Text.RegularExpressions.Regex.Replace(sql, "SELECT(.*)FROM", string.Format("SELECT Count(*) FROM"));
        }

        public static string ConcatPagingCommand(string sql , string sortColumn , string descOrder = "desc")
        {
            sql+= @" ORDER BY {0} {1} 
                     OFFSET @skipRows Rows 
                     FETCH Next @count Rows only";

            string descOrNot = descOrder == "desc" ? "desc" : "";
            sql = string.Format(sql, sortColumn, descOrNot);
            return sql;
        }
        

        private static string GetFieldPatternList<T>(List<string> ignoreFields)
        {
            var sb = new StringBuilder();
            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields);

            foreach (var item in filterProperties.OrderBy(x=> x.Name))
            {
                if (IsICollection(item.PropertyType))
                {
                    continue;
                }

                if (IsVirtual(item))
                {
                    continue;
                }

                if (IsKeyColumn(item))
                {
                    continue;
                }

                sb.Append("@" + item.Name + ",");
            }
            var query = sb.ToString();
            return query.Remove(query.Length - 1);
        }
        

        private static List<PropertyInfo> GetPropertiesAndFilterIgnore<T>(List<string> ignoreFields)
        {
            var properties = typeof(T).GetProperties();
            List<PropertyInfo> filterProperties = properties.ToList();
            if (ignoreFields != null && ignoreFields.Count > 0)
            {
                filterProperties = properties
                    .Where(a => !ignoreFields.Contains(a.Name)).ToList();
            }

            return filterProperties;
        }

        private static bool IsKeyColumn(PropertyInfo info)
        {
            var attr = info.GetCustomAttribute<KeyAttribute>();

            if (attr == null)
                return false;

            return true;
        }


        private static bool IsSkipUpdateWhenMerge(PropertyInfo info)
        {
            var attr = info.GetCustomAttribute<NotUpdateWhenMerge>();

            if (attr == null)
                return false;

            return true;
        }


        private static bool IsVirtual(PropertyInfo info)
        {
            if (info.PropertyType == typeof(decimal) ||
                info.PropertyType == typeof(byte))
            {
                //某些情況下 (不明原因) 這種基本型別會判斷是true...
                return false;
            }
            
            bool IsVirtual = info.GetGetMethod().IsVirtual;
            return IsVirtual;
        }


        private static bool IsICollection(Type type)
        {
            if (type == null || !type.IsGenericType)
            {
                return false;
            }

            return typeof(ICollection<>).IsAssignableFrom(type
                .GetGenericTypeDefinition());

        }

     
        public static string GetUpdateCommand<T>()
        {
            var pkFields = GetPrimaryKeyFields<T>();
            var tableName = GetTableName<T>();
            return GetUpdateCommand<T>(tableName, null, pkFields);
        }

        public static string GetUpdateCommand<T>(string tableName)
        {
            var pkFields = GetPrimaryKeyFields<T>();
            return GetUpdateCommand<T>(tableName, null, pkFields);
        }

        public static string GetUpdateCommand<T>(string tableName , List<string> ignoreColumns)
        {
            var pkFields = GetPrimaryKeyFields<T>();
            return GetUpdateCommand<T>(tableName, ignoreColumns, pkFields);
        }
     

        private static string GetUpdateCommand<T>(string tableName, List<string> ignoreFields, List<string> pkFields)
        {
            var updateFieldString = GetUpdateFieldString<T>(ignoreFields, pkFields);

            //無可奈何 先這樣寫 (如果有model有定義多組Key、會移除名稱為Id的那一個)
            if (pkFields != null && pkFields.Count() > 1)
                pkFields.RemoveAll(x => x == "Id");

            var primaryKeyString = GetPrimaryKeyString(pkFields);
            
            var command = $"UPDATE {tableName} SET {updateFieldString} WHERE {primaryKeyString}";
            return command;
        }

        private static string GetUpdateFieldString<T>(List<string> ignoreFields, List<string> pkFields)
        {
            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields).OrderBy(a => a.Name);
            var array = (from item in filterProperties
                where !IsICollection(item.PropertyType) &&
                      !IsVirtual(item) &&
                      !pkFields.Contains(item.Name)
                select item.Name + "=@" + item.Name).ToArray();
            return string.Join(",", array);
        }

        private static string GetPrimaryKeyString(IEnumerable<string> pkFields)
        {
            var array = pkFields.Select(a => a + "=@" + a).ToArray();
            return string.Join(" AND ", array);
        }

        public static List<string> GetPrimaryKeyFields<T>()
        {
            var result = new List<string>();

            foreach (var prop in typeof(T).GetProperties())
            {
                var attr = (KeyAttribute)prop.GetCustomAttribute
                    (typeof(KeyAttribute), false);

                var attr_noneAutoKey = (NoneAutoKey)prop.GetCustomAttribute
                   (typeof(NoneAutoKey), false);

                if (attr != null)
                {
                    result.Add(prop.Name);
                }

                if(attr_noneAutoKey != null)
                {
                    result.Add(prop.Name);
                }
            }

            return result;
        }

        public static string GetExecuteTableValueTypeCommend<T>(string tableName, List<string> insertIgnoreFields = null, List<string> updateIgnoreFields = null)
        {
            var pkFields = GetPrimaryKeyFields<T>();
            
            if (updateIgnoreFields != null && updateIgnoreFields.Count > 0)
            {
                updateIgnoreFields.AddRange(pkFields);
            }
            
            var primaryKeyString = GetTableValueTypePrimaryKeyString(pkFields);
            var updateCommand = GetTableValueTypeUpdateCommand<T>(updateIgnoreFields);
            var insertCommand = GetTableValueTypeInsertCommand<T>(insertIgnoreFields);
            var result = $@"MERGE INTO {tableName} [t]
                            USING @UpdatedTable [ut]
                            ON {primaryKeyString}
                            WHEN MATCHED THEN UPDATE SET {updateCommand}
                            WHEN NOT MATCHED THEN {insertCommand};";

            return result;
        }

        private static string GetTableValueTypePrimaryKeyString(IEnumerable<string> pkFields)
        {
            var array = pkFields.Select(a => $"[t].[{a}] = [ut].[{a}]").ToArray();
            return string.Join(" AND ", array);
        }

        public static string GetTableValueTypeUpdateCommand<T>(List<string> ignoreFields = null)
        {
            var names = GetFieldNames<T>(ignoreFields);
            var result = new List<string>();

            foreach (var name in names)
            {
                result.Add($"[{name}] = [ut].[{name}]");
            }

            return string.Join(",", result);
        }

        public static string GetTableValueTypeInsertCommand<T>(List<string> ignoreFields = null)
        {
            var query = $"INSERT ({GetFieldList<T>(ignoreFields)}) VALUES({GetTableValueTypeFieldPatternList<T>(ignoreFields)})";
            return query;
        }

        private static string GetTableValueTypeFieldPatternList<T>(List<string> ignoreFields)
        {
            var names = GetFieldNames<T>(ignoreFields);
            var result = new List<string>();

            foreach (var name in names)
            {
                result.Add($"[ut].[{name}]");
            }
            
            return string.Join(",", result);
        }

        public static List<string> GetFieldNames<T>(List<string> ignoreFields = null)
        {
            var result = new List<string>();
            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields);

            foreach (var item in filterProperties.OrderBy(x=> x.Name))
            {
                if (IsICollection(item.PropertyType))
                {
                    continue;
                }

                if (IsVirtual(item))
                {
                    continue;
                }

                result.Add(item.Name);
            }

            return result;
        }


        public static string GetInsertOrUpdateCommandByIgnoreId<T>(string tableName , List<string> matchColumnName) where T : class
        {
            string ignoreColumn = "ID";
            List<string> fields = GetFieldNames<T>(new List<string> { ignoreColumn });
            List<string> fieldPatterns = GetFieldNames<T>(new List<string> { ignoreColumn });
            List<string> ignoreUpdateColumns = GetSkipUpdateColumns<T>();
            List<string> fieldUpdate = GetFieldNames<T>(ignoreUpdateColumns);

            var fieldsStr = string.Join(",", fields.Select(fieldName => $"@{fieldName} AS {fieldName}"));

            List<string> matchKeys = matchColumnName;

            if(matchKeys == null || matchColumnName.Count == 0)
            {
                matchKeys = GetPrimaryKeyFields<T>();
            }

            string joinStr = string.Join(" AND ", matchKeys.Select(fieldName => $"T.{fieldName} = S.{fieldName}"));
            var updateStr = string.Join(",", fieldUpdate.Select(fieldName => $"T.{fieldName} = S.{fieldName}"));

            string sql = $@"
                MERGE INTO {tableName} as T
                USING (SELECT {fieldsStr}) AS S ON {joinStr}
                WHEN MATCHED THEN
                    UPDATE 
        	        SET 
        		        {updateStr}
                WHEN NOT MATCHED THEN
                    INSERT({string.Join(",", fieldPatterns)}) 
                    VALUES({string.Join(",", fieldPatterns.Select(e => $"S.{e}"))});";

            return sql;
        }

        public static List<string> GetSkipUpdateColumns<T>()
        {
            var result = new List<string>();
            var filterProperties = typeof(T).GetProperties();

            foreach (var prop in filterProperties.OrderBy(x => x.Name))
            {
                if(IsKeyColumn(prop))
                {
                    result.Add(prop.Name);
                    continue;
                }

                if (IsSkipUpdateWhenMerge(prop))
                {
                    result.Add(prop.Name);
                    continue;
                }
            }

            return result;
        }


    }



    /// <summary>
    /// PK鍵值、但非自動增長而是要自己填寫的。產生 update語法會不太相同
    /// </summary>
    public class NoneAutoKey : System.Attribute
    {
        
    }

    /// <summary>
    /// 當被標記此attribute，merge語法的時候會略過此欄位
    /// </summary>
    public class NotUpdateWhenMerge : System.Attribute
    {

    }

}
