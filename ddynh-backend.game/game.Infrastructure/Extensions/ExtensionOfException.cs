using game.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace game.Infrastructure.Extensions
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


        /// <summary>
        /// 取得偵錯用debug訊息
        /// </summary>
        public static string GetDebugMessage(this Exception ex)
        {
            var errorLine = GetErrorOccurLineFromStackTrace(ex.StackTrace);
            return ex.Message + " , " + errorLine;
        }


        /// <summary>
        /// 取得程式碼錯在第幾行的重點資訊
        /// </summary>
        public static string GetErrorOccurLine(this Exception ex)
        {
            var result = GetErrorOccurLineFromStackTrace(ex.StackTrace);
            return result;
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
                            string lineText = text.Truncate(150);
                            lineText = lineText.Replace("System.", "");
                            lineText = lineText.Replace(".service", "");
                            listResult.Add(lineText);
                        }
                    }
                }

            }
            catch
            {

            }
            return " " + string.Join("", listResult);
        }

    }
}
