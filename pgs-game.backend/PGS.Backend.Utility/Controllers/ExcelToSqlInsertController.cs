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
using target.Models.Models;
using System.Text;
using Newtonsoft.Json;

namespace target.Backend.Utility.Controllers
{
    public class ExcelToSqlInsertController : Controller
    {


        /// <summary>
        /// 轉成demo站的 game_desc 
        /// </summary>
        [Route("post/excel_to_demo_site_lang_desc")]
        [HttpPost]
        public IActionResult Post_ExcelFormat_ConvertTo_DemoSiteGameDescription()
        {
            try
            {
                var requestBody = Request.GetRequestBodyString();

                List<string> lines = requestBody.Split(new[] { Environment.NewLine, "\t\n", "\n" }, StringSplitOptions.None)
                 .Where(x => !string.IsNullOrWhiteSpace(x))
                 .ToList();

                if (lines.IsEmptyOrNull() || lines.Count() <= 1)
                    return Content("解析失敗");

                var headerLine = lines.FirstOrDefault();

                List<string> columns = headerLine.Split('\t').ToList();

                string sql_columns = string.Join(",", columns);

                StringBuilder sb = new StringBuilder();

                Dictionary<string, int> langMappingPosition = new Dictionary<string, int>();

                var headerRows = lines[0];
                List<string> headerCells = headerRows.Split('\t').ToList();

                int myPosition = 0;
                foreach (var cell in headerCells)
                {
                    if (cell == "zh-cn")
                    {
                        langMappingPosition.Add("zh-cn", myPosition);
                    }
                    if (cell == "en-us")
                    {
                        langMappingPosition.Add("en-us", myPosition);
                    }
                    if (cell == "th-th")
                    {
                        langMappingPosition.Add("th-th", myPosition);
                    }
                    if (cell == "ms-my")
                    {
                        langMappingPosition.Add("ms-my", myPosition);
                    }
                    if (cell == "vi-vn")
                    {
                        langMappingPosition.Add("vi-vn", myPosition);
                    }
                    myPosition++;
                }

                if(langMappingPosition.IsEmptyOrNull())
                    return Content("你沒有傳送Header列 (表頭列也需要)");

                var eachGameJsonDic = new Dictionary<int, object>();

                string lastGameID = "";

                foreach (var eachLine in lines.Skip(1))
                {
                    List<string> value_eachColumns = eachLine.Split('\t').ToList();

                    var eachLang = new Dictionary<string, object>();

                    eachLang.Add("zh-cn", new { name = "", desc = GetTextFromArrayPosition(value_eachColumns, GetTargetLanguagePosition(langMappingPosition, "zh-cn")) });
                    eachLang.Add("en-us", new { name = "", desc = GetTextFromArrayPosition(value_eachColumns, GetTargetLanguagePosition(langMappingPosition, "en-us")) });
                    eachLang.Add("vi-vn", new { name = "", desc = GetTextFromArrayPosition(value_eachColumns, GetTargetLanguagePosition(langMappingPosition, "vi-vn")) });
                    eachLang.Add("ms-my", new { name = "", desc = GetTextFromArrayPosition(value_eachColumns, GetTargetLanguagePosition(langMappingPosition, "ms-my")) });
                    eachLang.Add("th-th", new { name = "", desc = GetTextFromArrayPosition(value_eachColumns, GetTargetLanguagePosition(langMappingPosition, "th-th")) });

                    string gameIDText = value_eachColumns[0];

                    if(!gameIDText.IsNumbericOfPositiveInt32())
                    {
                        throw new Exception($"{gameIDText.Truncate(50)} 不是合法gameID , 可能發生在 {lastGameID} 的區塊");
                    }

                    eachGameJsonDic.Add(Int32.Parse(gameIDText), eachLang);

                    lastGameID = gameIDText;
                }


                string fianlSQL = JsonConvert.SerializeObject(eachGameJsonDic, Formatting.Indented);
                fianlSQL = fianlSQL.TruncateFirstWordIfMatch('{');
                fianlSQL = fianlSQL.TruncateLastWordIfMatch('}');
                fianlSQL = fianlSQL + ",";


                return Content(fianlSQL, "text/plain");
            }
            catch (Exception ex)
            {
                string msg = ex.GetMessageAndErrorLine();
                return Content(msg, "text/plain");
            }
        }


        private int GetTargetLanguagePosition(Dictionary<string, int> dic, string lang)
        {
            if (dic.ContainsKey(lang))
                return dic[lang];

            return -1;
        }




        private string GetTextFromArrayPosition(List<string> text, int targetIndex)
        {
            try
            {
                if (targetIndex < 0)
                    return "";

                if (text.Count() <= targetIndex)
                    return "";

                return text[targetIndex];
            }
            catch
            {

            }

            return "";
        }






        /// <summary>
        /// 透過postman送出excel格式 轉成insert into 語法
        /// </summary>
        [Route("post/excel_to_sql_insert")]
        [HttpPost]
        public IActionResult Post_ExcelFormat_ConvertTo_InsertSQL()
        {
            var requestBody = Request.GetRequestBodyString();

            List<string> lines = requestBody.Split(new[] { Environment.NewLine, "\t\n", "\n" }, StringSplitOptions.None)
             .Where(x => !string.IsNullOrWhiteSpace(x))
             .ToList();

            if (lines.IsEmptyOrNull() || lines.Count() <= 1)
                return Content("解析失敗");

            var headerLine = lines.FirstOrDefault();

            List<string> columns = headerLine.Split('\t').ToList();

            string sql_columns = string.Join(",", columns);

            StringBuilder sb = new StringBuilder();

            sb.Append($"INSERT INTO XXXXXXXXXXX ({sql_columns}) VALUES ");

            foreach (var eachLine in lines.Skip(1))
            {
                List<string> value_eachColumns = eachLine.Split('\t').ToList();

                string sql_value_columns = string.Join(",", value_eachColumns.Select(x => ParseToSpecialValue(x)));
                sb.Append($" {Environment.NewLine} ( {sql_value_columns} ) ,");
            }

            string fianlSQL = sb.ToString();
            fianlSQL = fianlSQL.TruncateLastWordIfMatch(',');

            return Content(fianlSQL);
        }


        /// <summary>
        /// 透過postman送出excel格式 轉成insert into 語法 (另一種語法)
        /// </summary>
        [Route("post/excel_to_sql_insert_mode2")]
        [HttpPost]
        public IActionResult Post_ExcelFormat_ConvertTo_InsertSQL_Mode2(string tableName = "XXXXXXXXXXX")
        {
            var requestBody = Request.GetRequestBodyString();

            List<string> lines = requestBody.Split(new[] { Environment.NewLine, "\t\n", "\n" }, StringSplitOptions.None)
             .Where(x => !string.IsNullOrWhiteSpace(x))
             .ToList();

            if (lines.IsEmptyOrNull() || lines.Count() <= 1)
                return Content("解析失敗");

            var headerLine = lines.FirstOrDefault();

            List<string> columns = headerLine.Split('\t').ToList();

            string sql_columns = string.Join(",", columns);

            StringBuilder sb = new StringBuilder();

            string sql_prefix_columns = $"INSERT INTO {tableName} ({sql_columns}) VALUES ";
            sb.Append($"  ");

            foreach (var eachLine in lines.Skip(1))
            {
                string sql_value_columns = string.Join(",", eachLine.Split('\t').Select(x => ParseToSpecialValue(x)));
                sb.Append(sql_prefix_columns);
                sb.Append($" ( {sql_value_columns} ) {Environment.NewLine}");
            }

            string finalSQL = sb.ToString();
            finalSQL = finalSQL.TruncateLastWordIfMatch(',');
            return Content(finalSQL);
        }


        private string ParseToSpecialValue(string value)
        {
            if (value.Contains("getDate"))
                return "GETDATE()";

            return $"'{value.Replace("'", " ")}'";
        }



    }
}
