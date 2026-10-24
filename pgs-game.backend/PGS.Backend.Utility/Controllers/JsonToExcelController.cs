using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lck.GoogleApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using target.Backend.Utility.Models;
using target.Models.CultureText;
using System.Reflection;
using target.Utility.Extensions;
using Newtonsoft.Json.Linq;

namespace target.Backend.Utility.Controllers
{

    public class JsonToExcelController : Controller
    {

        private static string ChangeLine =Environment.NewLine;

        Dictionary<string, string> eachClassName = new Dictionary<string, string>();


        [Route("Json_to_Excel")]
        [HttpPost]
        public IActionResult ConvertDataSchema_To_CSharpClass(bool isOrderByColumnName = true)
        {
            var requestBody = Request.GetRequestBodyString();

            if (string.IsNullOrWhiteSpace(requestBody))
                throw new Exception("requestBody empty");            

            var eachKeyValues = requestBody.JsonConvertToModel<Dictionary<string , dynamic>>();

            string outputText = string.Empty;

            Dictionary<string, dynamic> keyValuesOrder = eachKeyValues;

            if (isOrderByColumnName)
            {
                keyValuesOrder = keyValuesOrder.OrderBy(x => x.Key).ToDictionary(x=> x.Key , x=> x.Value);
            }

            foreach(var item in keyValuesOrder)
            {
                outputText += item.Key + "\t" + item.Value + Environment.NewLine;      
            }

            return Content(outputText);
        }



        [Route("Json_to_Excel/Read_My_PC_Files")]
        [HttpGet]
        public IActionResult ConvertDataSchema_To_CSharpClass(string fileFolder = @"C:\Gitlab_DDYNH\web-lobby\src\static\lang\")
        {
            List<Dictionary<string, dynamic>> allDic = new List<Dictionary<string, dynamic>>();

            allDic.Add(ReadFileAndConvert(fileFolder, "en_us.json"));
            allDic.Add(ReadFileAndConvert(fileFolder, "zh_cn.json"));
            allDic.Add(ReadFileAndConvert(fileFolder, "id_id.json"));
            allDic.Add(ReadFileAndConvert(fileFolder, "ms_my.json"));

            string outputText = string.Empty;

            List<string> allKeys = allDic.SelectMany(x => x.Keys).ToList().Distinct().OrderBy(x => x).ToList();

            foreach (string eachKey in allKeys)
            {
                outputText += eachKey;

                foreach(var eachLang in allDic)
                {
                    outputText += "\t" + eachLang.GetDictionaryOrDefaultValue(eachKey, "");
                }

                outputText += Environment.NewLine;
            }

            return Content(outputText);
        }


        private Dictionary<string, dynamic> ReadFileAndConvert(string prefixLocation , string fileName)
        {
            string text = example.invalid.File.ReadAllText(prefixLocation + fileName);
            Dictionary<string, dynamic> parseToDic = text.JsonConvertToModel<Dictionary<string, dynamic>>();
            return parseToDic;
        }


    }
}
