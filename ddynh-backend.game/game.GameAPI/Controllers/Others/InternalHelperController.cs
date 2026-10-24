using FW.Core.Helper;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Infrastructure.Extensions;
using example.invalid.Http;
using example.invalid.Http.Headers;

namespace game.GameAPI.Controllers
{
    public class InternalHelperController : BaseAPIController
    {

        /// <summary>
        /// 透過postman貼上 SSMS複製的schema格式 可以轉成C# class程式碼
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [Route("DB_to_Class/{tableName}")]
        [HttpPost]
        public object ConvertDataSchema_To_CSharpClass([FromUri]string tableName)
        {
            var requestBody = System.Web.HttpContext.Current.Request.GetRequestBodyString();

            if (string.IsNullOrWhiteSpace(requestBody))
                return "no input text";

            List<string> lines = requestBody.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Where(x=> !string.IsNullOrWhiteSpace(x)).ToList();

            int totalColumns = 0;
            string yourClassText = $"public class {tableName}Entity " + Environment.NewLine;
            yourClassText += "{" + Environment.NewLine;


            foreach (var eachDBColumn in lines)
            {
                List<string> split = eachDBColumn.Split('\t').Where(x=> !string.IsNullOrWhiteSpace(x)).ToList();

                if (split.Count() != 3)
                    continue;

                var columName = split[0];
                var columType = split[1].Trim();
                var isNullChecked = split[2].Trim();

                var csharpType = "string";

                if (columType == "int")
                    csharpType = "int";
                if (columType == "bit")
                    csharpType = "bool";
                else if (columType == "bigint")
                    csharpType = "long";
                else if (columType == "tinyint")
                    csharpType = "byte";
                else if (columType == "smallint")
                    csharpType = "short";
                else if (columType == "float")
                    csharpType = "float";        
                else if (columType == "datetime")
                    csharpType = "DateTime";
                else if (columType == "date")
                    csharpType = "DateTime";
                else if (columType == "uniqueidentifier")
                    csharpType = "Guid";                
                else if (columType.Contains("decimal"))
                    csharpType = "decimal";
                else if (columType.Contains("char"))
                    csharpType = "string";

                string get_set_text = "{get;set;}";
                string isNullableText = "";

                if(csharpType != "string")
                {
                    if(isNullChecked == "Checked")
                    {
                        isNullableText = "?";
                    }
                }
                

                yourClassText += $"　public {csharpType}{isNullableText} {columName} {get_set_text}" + Environment.NewLine;
                totalColumns++;
            }

            yourClassText += "}" + Environment.NewLine;

            var response = new HttpResponseMessage();
            response.Content = new StringContent(yourClassText);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }


    }

}
