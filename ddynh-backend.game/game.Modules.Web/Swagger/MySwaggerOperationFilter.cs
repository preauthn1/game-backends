using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Entity.ViewModel.GamePlayAPI.Core;

namespace target.API.AdminSys
{

    public class MySwaggerOperationFilter : IOperationFilter
    {

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            string path = apiDescription.RelativePath;

            bool isDebugMatch = path.Contains("merchants");

            if (operation.parameters.IsEmptyOrNull())
                operation.parameters = new List<Parameter>();

            var responseContent = operation.responses.FirstOrDefault().Value;

            bool isPaged = false;

            var classFullName = operation.responses.FirstOrDefault().Value.schema.@ref ?? string.Empty;
            classFullName = FormatteRefName(classFullName);

            if (classFullName.Contains("PagedList"))
            {
                isPaged = true;
            }

            if (isDebugMatch)
            {
                string testDebug = "斷點請設在這";
            }

            if (operation.responses.IsNotEmptyNull())
            {
                int loopTimes = 0; //避免物件無窮迴圈

                var responseSchema = operation.responses.FirstOrDefault().Value;

                var responseObject = GenerateColumnDocument(schemaRegistry.Definitions, "data", responseSchema.schema, isPaged, loopTimes);


                //因應我方API 會多包裝一層data的結構 修改swagger格式
                responseSchema.schema.example = new ApiResponseModel
                {
                    Data = responseObject
                };

                responseSchema.schema.@ref = null;
            }
        }


        private object GenerateColumnDocument(IDictionary<string, Schema> schemas, string columnName, Schema target, bool isPaged, int loopTimes)
        {
            target.format = target.format ?? string.Empty;
            if (target.@ref == null && target.items?.@ref == null)
            {
                return GenerateBaseTypeColumnOpenApiObject(schemas, columnName, target, isPaged);
            }
            else
            {
                if (target.type == "array")
                {
                    if (target.items.@ref != null)
                    {
                        loopTimes++;
                        return new List<object>
                        {
                            GenerateFromReferenceClass(schemas, target.items.@ref, isPaged, loopTimes)
                        };     
                    }
                }
                else
                {
                    if (target.@ref != null)
                    {
                        loopTimes++;
                        return GenerateFromReferenceClass(schemas, target.@ref, isPaged, loopTimes);
                    }
                }
            }

            return null;
        }


        private object GenerateBaseTypeColumnOpenApiObject(IDictionary<string, Schema> schemas, string columnName, Schema target, bool isPaged)
        {
            string type = target.type.ToLower();
            switch (type)
            {
                case "string":

                    if (target.format.Contains("date-time"))
                    {
                        return "2020-01-01T12:00:00Z";
                    }

                    return "string";

                case "long":
                case "number":
                case "integer":

                    if (target.format.Contains("double"))
                    {
                        return 0.0;
                    }

                    return 0;


                case "boolean":
                    return false;

                case "array":
                    return "array";

                default:
                    return columnName;
            }
        }


        private object GenerateFromReferenceClass(IDictionary<string, Schema> schemas, string refID, bool isPaged, int loopTimes)
        {
            //避免無窮迴圈
            if (loopTimes >= 6)
                return "object";

            refID = FormatteRefName(refID);

            var match = schemas.Where(x => x.Key == refID).FirstOrDefault();

            Dictionary<string,object> dic = new Dictionary<string, object>();

            foreach (var item in match.Value?.properties)
            {
                if (isPaged)
                    continue;

                dic.Add(item.Key, GenerateColumnDocument(schemas, item.Key, item.Value, isPaged, loopTimes));            
            }

            return dic;
        }


        private string FormatteRefName(string refName)
        {
            return refName.Replace("#/definitions/", "");
        }


    }



}
