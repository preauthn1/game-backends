using Lck.GoogleApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using target.Backend.Utility.Models;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace target.Backend.Utility.Controllers
{
    public class CultureTextConvertController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IWebHostEnvironment _env;

        private readonly static string _sheetUrl = "https://example.invalid/spreadsheets/d/12NfEknrEBXXxA3x8eoaXdc111GGlVCN3r96_oyousbw/edit#gid=0";
        
        private static readonly int _startIndex = 2;


        public List<string> availableLanguage = new List<string>
        {
            "zh-tw",
            "zh-cn",
            "en-us"
        };


        public CultureTextConvertController(IWebHostEnvironment env)
        {
            _env = env;
            GoogleSheetUtility.WebRootDirection = _env.ContentRootPath;
        }



        /// <summary>
        /// 讀取C#檔案 並轉成可以直接貼上的Excel文字格式
        /// </summary>
        [HttpGet]
        public ActionResult Culture_To_ExcelForamt()
        {
            List<SheetReadModel> convertExcelValues = GetOnlineExcelValueAndMergeCurrentClass();

            //依序覆蓋 (必須以excel的為準)
            //若發現新增的 塞入
            return PartialView(convertExcelValues);
        }



        /// <summary>
        /// 讀取Excel 並轉成 C# 對應的18nKey語系檔案
        /// </summary>
        [HttpGet]
        public ActionResult Culture_To_Class(string filterClassName = "")
        {
            var excelSheetResult = GetOnlineExcelValueAndMergeCurrentClass(isMergeCurrentClass : false);

            if (excelSheetResult == null || excelSheetResult.Count == 0)
            {
                return Content("無解析任何內容或解析失敗");
            }

            List<ExcelConvertToClass> convertResults = new List<ExcelConvertToClass>();

            string resultContent = string.Empty;

            var allClass = new List<string>
            {
                "i18n"
            };

            Dictionary<string , Dictionary<string , string>> i18nKeysDic = new Dictionary<string, Dictionary<string, string>>();
            i18nKeysDic.Add("zh-tw", new Dictionary<string, string>());
            i18nKeysDic.Add("zh-cn", new Dictionary<string, string>());
            i18nKeysDic.Add("en-us", new Dictionary<string, string>());

            //接著依序建立每個語系
            foreach (var eachClass in allClass)
            {
                int startIndex = _startIndex; //中文是在第幾個                

                foreach (var eachLang in availableLanguage)
                {
                    string className = eachClass + "_" + eachLang.ToUpper();

                    var myItem = new ExcelConvertToClass
                    {
                        ClassName = className,
                        Languages = eachLang,
                        PropertyValues = new List<ConvertClassPropertyValues>()
                    };

                    foreach (var eachRow in excelSheetResult)
                    {
                        string propertyName = eachRow.ColumnValues[1];

                        i18nKeysDic[eachLang].Add(propertyName, eachRow.ColumnValues[startIndex]);
                    }

                    //切換到下一個語言後 從第2欄 改成第3欄 持續往後+1
                    startIndex++;
                }
            }


            string finalJson = i18nKeysDic.ConvertToJsonString();
            return Content(finalJson);
        }



        private List<SheetReadModel> GetOnlineExcelValueAndMergeCurrentClass(bool isMergeCurrentClass = true)
        {
            var originalExcelValues = GoogleSheetUtility.ReadBySheetUrl(_sheetUrl);

            RemoveHeaderLine(originalExcelValues);

            //讀取特定幾格class的檔案 並且轉成
            List<SheetReadModel> convertExcelValues = new List<SheetReadModel>();

            var file1 = ConvertClassToExcelForamt(availableLanguage);

            convertExcelValues.AddRange(file1);

            //補上新加的errorCode
            AddNewDefineErrorCode(convertExcelValues);

            //特定的enum自動補上新編號 (定義在這個檔案中)
            var allEnums = BackendUtilityHelper.GetAutoGeneratei18nKeysEnums();

            foreach(Type enumType in allEnums)
            {
                AddNewDefineForSpecificEnum(convertExcelValues , enumType);
            }

            //補上i18n新加的keys
            if (isMergeCurrentClass)
            {
                AddNewDefineIn_i18nKeys(convertExcelValues);
            }

            //讀取線上的excel檔案 以線上的excel版本為主 覆蓋之
            foreach (var item in convertExcelValues)
            {
                var match = originalExcelValues.FirstOrDefault(x => x.ColumnValues[0] == item.ColumnValues[0] && x.ColumnValues[1] == item.ColumnValues[1]);

                if (match != null)
                {
                    int startIndex = _startIndex;

                    int maxLength = item.ColumnValues.Count() > match.ColumnValues.Count() ? match.ColumnValues.Count() : item.ColumnValues.Count();

                    for (int i = _startIndex; i < maxLength; i++)
                    {
                        item.ColumnValues[i] = match.ColumnValues[i];
                    }
                }
                else
                {
                    string date = DateTime.Now.ToShortDateString();
                    item.ColumnValues.Add(date + " **新增**");
                }
            }

            //讀取線上的excel檔案 找出是否有線上excel有 但class檔案無得
            foreach (var item in originalExcelValues)
            {
                var match = convertExcelValues.FirstOrDefault(x => x.ColumnValues[0] == item.ColumnValues[0] && x.ColumnValues[1] == item.ColumnValues[1]);

                if (match == null)
                {
                    convertExcelValues.Add(item);
                }
            }

            //不要再出現了 (黑名單排除特定的key)
            List<string> ignoreKeyword = BackendUtilityHelper.IgnoreExcelColumnsKeyword();

            //排除關鍵字 (可能是已經被改名的enums)
            convertExcelValues = convertExcelValues
                .Where(x => !ignoreKeyword.Any(text => text == x.ColumnValues[1]))
                .ToList();

            //重新整理排序
            convertExcelValues = convertExcelValues
                .OrderByDescending(x => x.ColumnValues[0] == "Common")
                .ThenByDescending(x => x.ColumnValues[0] == "Enums")
                .ThenByDescending(x => x.ColumnValues[0] == "ErrorCode")
                .ThenBy(x => x.ColumnValues[1])
                .ToList();

            return convertExcelValues;
        }


        private Dictionary<string, Dictionary<string, string>> ReadExistJsonKeyValues()
        {
            return RuntimeHelper.Readi18nTextJsonKeyValues();
        }



        private List<SheetReadModel> ConvertClassToExcelForamt(List<string> each_langs)
        {
            Dictionary<string, Dictionary<string, string>> convertToEachLangDic = ReadExistJsonKeyValues();

            var first = convertToEachLangDic["zh-cn"];

            List<string> columns = first.Select(x => x.Key).ToList();

            List<SheetReadModel> listResult = new List<SheetReadModel>();

            foreach (var eachColumnKey in columns)
            {
                var newItem = new SheetReadModel
                {
                    ColumnValues = new List<string>(),
                };

                string categoryName = eachColumnKey.Split('_').FirstOrDefault() ?? "Common";

                newItem.ColumnValues.Add(categoryName);
                newItem.ColumnValues.Add(eachColumnKey);

                foreach (var lang in each_langs)
                {
                    var cultureText = convertToEachLangDic[lang][eachColumnKey];
                    newItem.ColumnValues.Add(cultureText);
                }

                listResult.Add(newItem);
            }

            return listResult;
        }

        private void AddNewDefineIn_i18nKeys(List<SheetReadModel> excelModels)
        {
            var allCodes = ExtensionOfEnum.GetEnumAllValue<ApiErrorCode>();

            Dictionary<string, Dictionary<string, string>> convertToEachLangDic = ReadExistJsonKeyValues();

            var matchFirst = convertToEachLangDic["zh-tw"];

            foreach (var eachKey in matchFirst.Keys)
            {
                var propName = eachKey;
                bool isExist = excelModels.Any(x => x.ColumnValues[1] == propName);

                if (!isExist)
                {
                    string prefix = propName.Split('_').FirstOrDefault() ?? "Common";
                    excelModels.Add(new SheetReadModel
                    {
                        ColumnValues = new List<string>
                        {
                            prefix,
                            propName,
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",                        }
                    });
                }
            }
        }


        private void AddNewDefineErrorCode(List<SheetReadModel> excelModels)
        {
            var allCodes = ExtensionOfEnum.GetEnumAllValue<ApiErrorCode>();
            string prefixName = "ErrorCode";

            foreach (ApiErrorCode item in allCodes)
            {
                string enumPropName = prefixName + "_" + item.ToString();
                var isExist = excelModels.Any(x => x.ColumnValues[1] == enumPropName);

                if (!isExist)
                {
                    excelModels.Add(new SheetReadModel
                    {
                        ColumnValues = new List<string>
                        {
                            prefixName,
                            enumPropName,
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",                        }
                    });
                }
            }
        }


        private void AddNewDefineForSpecificEnum<T>(List<SheetReadModel> excelModels) where T : IComparable, IFormattable, IConvertible
        {
            var allCodes = ExtensionOfEnum.GetEnumAllValue<T>();
            string prefixName = "Enums";

            foreach (var item in allCodes)
            {
                string enumPropName = prefixName + "_" + typeof(T).Name +"_" + item.ToString();
                var isExist = excelModels.Any(x => x.ColumnValues[1] == enumPropName);

                if (!isExist)
                {
                    excelModels.Add(new SheetReadModel
                    {
                        ColumnValues = new List<string>
                        {
                            prefixName,
                            enumPropName,
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",                        
                        }
                    });
                }
            }
        }


        private void AddNewDefineForSpecificEnum(List<SheetReadModel> excelModels , System.Type enumType)
        {
            List<object> enumsAll = new List<object>();

            foreach (var enumValue in System.Enum.GetValues(enumType))
            {
                enumsAll.Add(enumValue);
            }

            string prefixName = "Enums";

            foreach (var item in enumsAll)
            {
                string enumPropName = prefixName + "_" + enumType.Name + "_" + item.ToString();
                var isExist = excelModels.Any(x => x.ColumnValues[1] == enumPropName);

                if (!isExist)
                {
                    excelModels.Add(new SheetReadModel
                    {
                        ColumnValues = new List<string>
                        {
                            prefixName,
                            enumPropName,
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            "XXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                        }
                    });
                }
            }
        }



        private void RemoveHeaderLine(List<SheetReadModel> query)
        {
            if (query == null || query.Count == 0)
            {
                return;
            }

            //表頭列 不處理、移除之
            query.RemoveAt(0);
        }



    }
}
