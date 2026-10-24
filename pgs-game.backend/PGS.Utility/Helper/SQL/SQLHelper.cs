using target.Models.CoreLibs;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace target.Core.Helper
{

    public static class SQLHelper
    {

        private static Dictionary<string, string> _insertSQLDic = new Dictionary<string, string>();

        private static Dictionary<string, string> _updateSQLDic = new Dictionary<string, string>();

        private static Dictionary<string, string> _updateSQLWithoutWhereDic = new Dictionary<string, string>();

        private static object _thisLock = new object();


        public static string GetInsertCommand<T>(string tableName) where T : class
        {
            string uniqueName = GetDictionaryUniqueName<T>(tableName);

            //舊版寫法: 每一次insert都要重做一次反射產生SQL 可能很浪費資源
            //新版寫法: 第一次產生SQL後，會用Dic記住語法
            if (_insertSQLDic != null && _insertSQLDic.ContainsKey(uniqueName))
            {
                return _insertSQLDic[uniqueName];
            }

            var insertSQL = string.Format("INSERT INTO {0} ({1}) VALUES({2})",
                            tableName,
                            GetFieldList<T>(ignoreFields : null),
                            GetFieldList<T>(ignoreFields : null, prefix: "@"));

            lock (_thisLock)
            {
                if(_insertSQLDic.ContainsKey(uniqueName))
                {
                    return insertSQL;
                }

                _insertSQLDic.Add(uniqueName, insertSQL);
            }

            return insertSQL;
        }


        public static string ReplaceToTotalCountCommand(string sql)
        {
            if (sql.Contains("select") && sql.Contains("from"))
                throw new Exception("Please use upper case.");

            return System.Text.RegularExpressions.Regex.Replace(sql, "SELECT(.*)FROM", string.Format("SELECT Count(*) FROM"));
        }


        public static string ConcatPagingCommand(string sql, string sortColumn, string descOrder = "desc")
        {
            sql += @" ORDER BY {0} {1} 
                     OFFSET @skipRows Rows 
                     FETCH Next @count Rows only";

            string descOrNot = descOrder == "desc" ? "desc" : "";
            sql = string.Format(sql, sortColumn, descOrNot);
            return sql;
        }


        public static (string FullSQL, string UpdateColumnSQL, string WhereSQL) GetUpdateCommand<T>(string tableName)
        {
            var pkFields = GetPrimaryKeyFields<T>();
            return GetUpdateCommand<T>(tableName, null, pkFields);
        }


        public static string GetUpdateCommandWithoutWhere<T>(string tableName)
        {
            string uniqueName = GetDictionaryUniqueName<T>(tableName);

            //舊版寫法: 每一次insert都要重做一次反射 可能很浪費資源
            //新版寫法: 第一次產生SQL後，會用Dic記住語法
            if (_updateSQLWithoutWhereDic.ContainsKey(uniqueName))
            {
                return _updateSQLWithoutWhereDic[uniqueName];
            }

            var pkFields = GetPrimaryKeyFields<T>();
            string updateSQL = GetUpdateCommandWithoutWhere<T>(tableName, null, pkFields);

            lock (_thisLock)
            {
                if (_updateSQLWithoutWhereDic.ContainsKey(uniqueName))
                {
                    return updateSQL;
                }

                _updateSQLWithoutWhereDic.Add(uniqueName, updateSQL);
            }

            return updateSQL;
        }


        private static (string FullSQL, string UpdateColumnSQL, string WhereSQL) GetUpdateCommand<T>(string tableName, List<string> ignoreFields, List<string> pkFields)
        {
            var updateFieldString = GetUpdateFieldString<T>(ignoreFields, pkFields);

            //無可奈何 先這樣寫 (如果有model有定義多組Key、會移除名稱為Id的那一個)
            if (pkFields != null && pkFields.Count() > 1)
                pkFields.RemoveAll(x => x == "Id");

            var primaryKeyString = GetPrimaryKeyString(pkFields);

            string updateSQL = $" UPDATE {tableName} SET {updateFieldString} ";
            string whereSQL = $" WHERE {primaryKeyString} ";

            var command = updateSQL + whereSQL;
            return (command, updateSQL, whereSQL);
        }


        private static string GetUpdateCommandWithoutWhere<T>(string tableName, List<string> ignoreFields, List<string> pkFields)
        {
            var updateFieldString = GetUpdateFieldString<T>(ignoreFields, pkFields);
            var command = $"UPDATE {tableName} SET {updateFieldString} WHERE "; //太危險了 預設加上where 請自行補上後面
            return command;
        }


        private static string GetDictionaryUniqueName<T>(string table)
        {
            return $"{typeof(T).Name}_{table}";
        }


        public static string GetInsertOrUpdateCommandByIgnoreId<T>(string tableName) where T : class
        {
            var fields = GetFieldList<T>(null);
            var fieldPatterns = GetFieldList<T>(null, "@");

            var fieldsStr = string.Join(",", fields.Select(fieldName => $"@{fieldName} AS {fieldName}"));

            var joinStr = string.Join(" AND ", GetPrimaryKeyFields<T>().Select(fieldName => $"T.{fieldName} = S.{fieldName}"));

            var updateStr = string.Join(",", fieldPatterns.Select(fieldName => $"T.{fieldName} = S.{fieldName}"));

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



        private static string GetFieldList<T>(List<string> ignoreFields, string prefix = "") where T : class
        {
            var sb = new StringBuilder();
            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields);

            bool isAutoKeyTable = IsNotAutoKey<T>() == false;

            foreach (var item in filterProperties.OrderBy(x => x.Name))
            {
                if (IsICollection(item.PropertyType))
                {
                    continue;
                }

                if (isAutoKeyTable && IsKeyColumn(item))
                {
                    continue;
                }

                if (IsSkipColumnForInsertUpdate(item))
                {
                    continue;
                }

                if (prefix == "@")
                {
                    sb.Append($" @{item.Name},");
                }
                else
                {
                    sb.Append($" [{item.Name}],");
                }
            }

            var query = sb.ToString();
            return query.Remove(query.Length - 1);
        }


        public static List<string> GetFieldListCollection<T>(List<string> ignoreFields, string prefix = "") where T : class
        {
            List<string> columns = new List<string>();

            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields);

            bool isAutoKeyTable = IsNotAutoKey<T>() == false;

            foreach (var item in filterProperties.OrderBy(x => x.Name))
            {
                if (IsICollection(item.PropertyType))
                {
                    continue;
                }

                if (isAutoKeyTable && IsKeyColumn(item))
                {
                    continue;
                }

                if (IsSkipColumnForInsertUpdate(item))
                {
                    continue;
                }

                columns.Add(item.Name);
            }

            return columns;
        }

        public static string GenerateInsertDataCommandByRows<T>(string tableName , List<T> rows) where T : class
        {
            if (rows == null || rows.Count() <= 0)
                return string.Empty;

            bool isAutoKeyTable = IsNotAutoKey<T>() == false;

            List<PropertyInfo> props = GetPropertiesAndFilterIgnore<T>(null)
                .Where(z=> !isAutoKeyTable || !IsKeyColumn(z))
                .OrderBy(z => z.Name)
                .ToList();

            string sql = $" INSERT INTO  {tableName} ( " + string.Join("," , props.Select(z=> $"[{z.Name}]")) +  ") "+ Environment.NewLine + Environment.NewLine;
            sql += " VALUES " + Environment.NewLine;

            int loopTimes = 0;

            foreach (var row in rows)
            {
                loopTimes++;

                List<string> values = new List<string>();

                foreach (var prop in props)
                {
                    var value = prop.GetValue(row);

                    if(prop.PropertyType == typeof(DateTime))
                    {
                        values.Add("GETUTCDATE()");
                    }
                    else
                    {
                        if(prop.PropertyType == typeof(string) && value != null)
                        {
                            string text = value.ToString() ?? "";
                            values.Add($"'{text.Replace("'" , "''")}'"); //跳脫字元
                        }
                        else
                        {
                            values.Add($"'{value}'");
                        }
                    }
                }

                sql += " ( " + string.Join(", " , values) + " )";

                if (loopTimes < rows.Count())
                {
                    sql += ", " + Environment.NewLine;
                }
            }

            return sql;
        }

        public static string GenerateInsertDataCommandByDictionary(string tableName, List<Dictionary<string,object>> rows , List<string> excludeColumns)
        {
            if (rows == null || rows.Count() <= 0)
                return string.Empty;

            excludeColumns = excludeColumns ?? new List<string>();

            if (excludeColumns != null && excludeColumns.Count() > 0)
            {
                excludeColumns = excludeColumns.Select(x => x.ToLower()).ToList();
            }

            string parseTableName = tableName.Split('.').LastOrDefault().Replace("[", "").Replace("]", "");

            List<string> idColumns = new List<string>
            {
                "ID",
                "Id",
                (parseTableName + "ID")
            };

            List<string> columns = rows.FirstOrDefault().Keys
                .Where(columnName => !excludeColumns.Contains(columnName.ToLower()))
                .OrderBy(columnName=> idColumns.Any(z=> z == columnName) ? 0 : 999)
                .ThenBy(text => text)
                .ToList();

            string sql = $" INSERT INTO  {tableName} ( " + string.Join(",", columns.Select(text => $"[{text}]")) + ") " + Environment.NewLine + Environment.NewLine;
            sql += " VALUES " + Environment.NewLine;

            int loopTimes = 0;

            foreach (var row in rows)
            {
                loopTimes++;

                List<string> values = new List<string>();

                foreach (var columnName in columns)
                {
                    var value = row[columnName];

                    if(value == null)
                    {
                        values.Add($"NULL");
                    }
                    else if (value.GetType() == typeof(DateTime))
                    {
                        values.Add("'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss") + "'");
                    }
                    else
                    {
                        if (value.GetType() == typeof(string) && value != null)
                        {
                            string text = value.ToString() ?? "";
                            values.Add($"'{text.Replace("'", "''")}'"); //跳脫字元
                        }
                        else
                        {
                            values.Add($"'{value}'");
                        }
                    }
                }

                sql += " ( " + string.Join(", ", values) + " )";

                if (loopTimes < rows.Count())
                {
                    sql += ", " + Environment.NewLine;
                }
            }

            return sql;
        }



        public static string ReplaceSqlParameterToValue<T>(string sql , T param) where T : new()
        {
            try
            {
                foreach (var item in typeof(T).GetProperties())
                {
                    string paramName = $"@{item.Name}";
                    string paramNameLower = paramName.ToLower();

                    var myValue = item.GetValue(param);
                    string textValue = "";

                    if (myValue != null)
                    {
                        if (item.PropertyType == typeof(DateTime))
                        {
                            textValue = "'" + ((DateTime)myValue).ToString("yyyy-MM-dd HH:mm") + "'" ;
                        }
                        else if (item.PropertyType == typeof(DateTime?))
                        {
                            textValue = "'" + ((DateTime?)myValue).Value.ToString("yyyy-MM-dd HH:mm") + "'";
                        }
                        else if (item.PropertyType == typeof(List<int>))
                        {
                            textValue = "(" + string.Join(",", (List<int>)myValue) + ")";
                        }
                        else if (item.PropertyType == typeof(List<long>))
                        {
                            textValue = "(" + string.Join(",", (List<long>)myValue) + ")";
                        }
                        else if (item.PropertyType == typeof(List<string>))
                        {
                            textValue = "(" + string.Join(",", (List<string>)myValue) + ")";
                        }
                        else
                        {
                            textValue = "'" + myValue.ToString() + "'";
                        }
                    }

                    sql = sql.Replace(paramName, textValue);
                    sql = sql.Replace(paramNameLower, textValue);
                }
            }
            catch
            {

            }

            return sql;
        }






        private static List<PropertyInfo> GetPropertiesAndFilterIgnore<T>(List<string> ignoreFields)
        {
            var properties = typeof(T).GetProperties();
            List<PropertyInfo> filterProperties = properties.ToList();
            if (ignoreFields != null && ignoreFields.Count > 0)
            {
                filterProperties = properties
                    .Where(a => !ignoreFields.Contains(a.Name))
                    .ToList();
            }

            return filterProperties;
        }


        /// <summary>
        /// 主要是針對 insert / update 要略過寫入的欄位
        /// </summary>
        private static bool IsKeyColumn(PropertyInfo info)
        {
            var attr1 = info.GetCustomAttribute<KeyAttribute>();

            if (attr1 == null)
                return false;

            return true;
        }



        /// <summary>
        /// 主要是針對 insert / update 要略過寫入的欄位
        /// </summary>
        private static bool IsSkipColumnForInsertUpdate(PropertyInfo info)
        {
            var attr = info.GetCustomAttribute<IgnoreWhenInsertUpdate>();

            if (attr == null)
                return false;

            return true;
        }



        private static bool IsNotAutoKey<T>() where T : class
        {
            var keys = GetPrimaryKeyFields<T>();

            int numbersOfKeys = keys?.Count() ?? 0;

            if (numbersOfKeys >= 2)
                return true;

            if (numbersOfKeys == 0)
                return true;

            if (IsNotAutoKeyAttributeExist<T>())
                return true;

            return false;
        }

        private static bool IsNotAutoKeyAttributeExist<T>() where T : class
        {
            var attr = (IsNotAutoKeyTable)typeof(T).GetCustomAttribute(typeof(IsNotAutoKeyTable), false);

            if (attr == null)
                return false;

            return true;
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


        private static string GetUpdateFieldString<T>(List<string> ignoreFields, List<string> pkFields)
        {
            ignoreFields = ignoreFields ?? new List<string>();

            //加入不需要更新的欄位
            ignoreFields.AddRange(GetDoNotUpdateColumns<T>());

            var filterProperties = GetPropertiesAndFilterIgnore<T>(ignoreFields).OrderBy(a => a.Name);

            var array = (from item in filterProperties
                         where !IsICollection(item.PropertyType) &&
                               !pkFields.Contains(item.Name)
                         select item.Name + "=@" + item.Name).ToArray();

            return string.Join(",", array);
        }


        private static List<string> GetDoNotUpdateColumns<T>()
        {
            List<string> list = new List<string>();

            foreach (var prop in typeof(T).GetProperties())
            {
                var attr1 = (DoNotUpdate)prop.GetCustomAttribute(typeof(DoNotUpdate), false);
                var attr2 = (IgnoreWhenInsertUpdate)prop.GetCustomAttribute(typeof(IgnoreWhenInsertUpdate), false);

                if (attr1 != null || attr2 != null)
                {
                    list.Add(prop.Name);
                }
            }
            return list;
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

                if (attr != null)
                {
                    result.Add(prop.Name);
                }
            }

            return result;
        }



    }








}
