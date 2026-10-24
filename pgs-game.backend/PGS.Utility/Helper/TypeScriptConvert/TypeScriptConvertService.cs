using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using target.Utility.Helper.HttpContext;

namespace target.Utility
{
    public static class TypeScriptConvert
    {      
        public static (string LootClassName, string FinalClassText) ConvertClassToTypeScriptModel(Type type , string categoryName = "")
        {
            var service = new TypeScriptConvertService(categoryName);
            var result = service.GenerateNestedClassToTypeScriptClassText(type);
            return result;
        }

    }


    public class TypeScriptConvertService
    {
        public TypeScriptConvertService(string CategoryName)
        {
            this.DebugInfo = CategoryName;
        }

        public string DebugInfo { get; set; }

        public Dictionary<Type, string> ClassMappingText = new Dictionary<Type, string>();

        public Type LootClass { get; set; }


        public (string LootClassName , string FinalClassText) GenerateNestedClassToTypeScriptClassText(Type type)
        {
            ConvertNestedClassToTypeScriptModel(type , true , 0);

            string lootClassName = GetClassName(this.LootClass);

            return (lootClassName , GetConvertFinalText());
        }


        private string GetConvertFinalText()
        {
            string text = "";

            foreach (var item in ClassMappingText.OrderBy(z => z.Key == LootClass ? 1 : 999))
            {
                text += item.Value + Environment.NewLine;
            }

            return text;
        }


        private void ConvertNestedClassToTypeScriptModel(Type type, bool isLootClass, int loopLayer = 0)
        {
            if (ClassMappingText.ContainsKey(type))
                return;

            if (!type.IsClass)
                return;

            if (typeof(string) == type || typeof(Type) == type)
                return;

            if (loopLayer >= 4)
                return;

            bool isList = IsList(type);

            if(isList && loopLayer == 0)
            {
                Type itemClassType = type.GetGenericArguments().Single(); 
                ConvertNestedClassToTypeScriptModel(itemClassType, true, ++loopLayer);
                return;
            }

            string result = "export interface " + GetClassName(type) + " {" + Environment.NewLine;
          
            var allProp = type.GetProperties();


            foreach (var prop in allProp)
            {
                string columnName = prop.Name.ToCamelCase();
                string dataTypeText = "any";

                if (prop.PropertyType == typeof(string))
                {
                    dataTypeText = "string";
                }

                else if (prop.PropertyType == typeof(decimal) ||
                    prop.PropertyType == typeof(decimal?) ||
                    prop.PropertyType == typeof(int) ||
                    prop.PropertyType == typeof(int?) || 
                    prop.PropertyType == typeof(long?) ||
                    prop.PropertyType == typeof(long))
                {
                    dataTypeText = "number";
                }
                else if (prop.PropertyType == typeof(object))
                {
                    dataTypeText = "any";
                }
                else if (prop.PropertyType == typeof(DateTime) ||
                         prop.PropertyType == typeof(DateTime?))
                {
                    dataTypeText = "string | null";
                }

                else if (prop.PropertyType == typeof(bool))
                {
                    dataTypeText = "boolean";
                }
                else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericArguments().Count() == 1) //list
                {
                    Type itemClassType = prop.PropertyType.GetGenericArguments().Single(); ;

                    string childModelName = GetClassName(itemClassType);

                    bool isUseNextClassToLootClass = isList && loopLayer == 0;

                    ConvertNestedClassToTypeScriptModel(itemClassType, isUseNextClassToLootClass , ++loopLayer);
                    dataTypeText = $"{childModelName}[]";

                }
                else if (prop.PropertyType.IsClass)
                {
                    if (loopLayer >= 4)
                    {
                        dataTypeText = "any";
                    }
                    else
                    {
                        string childModelName = GetClassName(prop.PropertyType);
                        ConvertNestedClassToTypeScriptModel(prop.PropertyType, false , ++loopLayer);
                        dataTypeText = $"{childModelName}";
                    }
                }

                result += "\t " + $"{columnName} : {dataTypeText}; " + Environment.NewLine;
            }

            result += "}" + Environment.NewLine;

            if(!ClassMappingText.ContainsKey(type) && !isList)
            {
                ClassMappingText.Add(type, result);

                if(isLootClass)
                {
                    this.LootClass = type;
                }
            }


            return;
        }

        private bool IsList(Type type)
        {
            return type.IsGenericType && type.Name.StartsWith("List");
        }


        private string GetClassName(Type type)
        {
            if (type == null)
                return string.Empty;

            string className = type.Name;
            return "I" + className
                .Replace("ViewModel" , "")
                .Replace("Model", "");
        }


    }

}
