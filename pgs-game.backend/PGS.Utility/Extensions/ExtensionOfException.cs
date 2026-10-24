using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace target.Utility.Extensions
{
    public static class ExtensionOfException
    {
       
        /// <summary>
        ///取得 最底層的 inner exception
        /// </summary>
        public static Exception GetMostInnerException(this Exception ex)
        {
            int times = 0;

            Exception resultEx = ex;
            while (true)
            {
                times++;

                if (times >= 25)
                    break;

                if(resultEx.InnerException == null)
                {
                    break;
                }      
                else
                {
                    resultEx = resultEx.InnerException;
                }
            }

            return resultEx;
        }

        public static string GetCommonNotifyMessage(this Exception ex , string methodNameOrPrefix , string extraMemo = "")
        {
            return $"{methodNameOrPrefix} 出錯啦 , {extraMemo} {ex.GetMessageAndErrorLine()}";
        }


        public static string GetMessageAndErrorLine(this Exception ex)
        {
            return $"{ex.Message}，{GetErrorOccurLine(ex)}";
        }


        public static int GetPgsErrorCode(this Exception ex)
        {
            var myEx = ex as TerproException;

            if (myEx == null)
                return ApiErrorCode.InternalServerError.ToInteger();

            return (int)myEx.ErrroCode;
        }


        /// <summary>
        /// 取得程式碼錯在第幾行的重點資訊
        /// </summary>
        private static string GetErrorOccurLine(this Exception ex)
        {
            //特定error可以不秀這些資訊 
            if(GetPgsErrorCode(ex) == (int)ApiErrorCode.Forbidden_IPAddressNotAllow)
            {
                return "";
            }

            var result = GetErrorOccurLineFromStackTrace(ex.StackTrace);
            return result;
        }


        public static bool IsSqlUniqueException(this Exception ex)
        {
            var sqlEx = ex as SqlException;

            if (sqlEx == null)
                return false;

            //鍵值重複的錯誤碼
            if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                return true;

            return false;
        }

        public static bool IsSqlTimeoutException(this Exception ex)
        {
            var sqlEx = ex as SqlException;

            if (sqlEx == null)
                return false;

            //鍵值重複的錯誤碼
            if (sqlEx.Number == -2)
                return true;

            return false;
        }


        /// <summary>
        /// 取得程式碼錯在第幾行的重點資訊
        /// </summary>
        public static string GetErrorOccurLineFromStackTrace(string stackTrace)
        {
            List<string> listResult = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(stackTrace))
                    return string.Empty;

                var split = stackTrace.Split(new string[] { "at " }, StringSplitOptions.None);

                foreach (var eachText in split)
                {
                    var splitAgain = eachText.Split(new string[] { @"\" }, StringSplitOptions.None);

                    foreach (var text in splitAgain)
                    {
                        if (text.Contains(".cs"))
                        {
                            string inputText = text
                                .Replace("/builds", "")
                                .Replace("/backend/target", "")
                                .Replace("/game.backend/", "")
                                .Replace("DA.", "")
                                .Replace("BL.", "")
                                .Replace("BL/", "")
                                .Replace("Int32", "")
                                .Replace("Int64", "")
                                .Replace("Services.", "")
                                .Replace("target.", "");

                            var codeLine = inputText.GetLastText(10).ReplaceNonNumeric();

                            listResult.Add(inputText.Truncate(120) + $" ({codeLine})");
                        }
                    }
                }

            }
            catch
            {

            }
            return " " + string.Join(Environment.NewLine, listResult);
        }

    }
}
