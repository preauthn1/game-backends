using DocumentFormat.OpenXml.Drawing.Charts;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Http;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Utility.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Helper
{

    /// <summary>
    /// 雖然號稱queue 但是懶得建置服務 (MQ)
    /// 基本上就是存在本機記憶體而已
    /// </summary>
    public static class EazyQueueApiRequestLogHelper
    {

        private const string IdentityKey = "ApiRequestLogQueueHelper";


        public static BaseQueueHelper<ApiRequestLogQueueTempModel> MyQueueManager = new BaseQueueHelper<ApiRequestLogQueueTempModel>(IdentityKey);


        public static void Append(ApiRequestLogQueueTempModel data)
        {
            MyQueueManager.Append(data);
        }


        public static void AppendMultiple(List<ApiRequestLogQueueTempModel> items)
        {
            MyQueueManager.AppendMultiple(items);
        }

        public static List<ApiRequestLogQueueTempModel> GetMultipleItems(int takeSize = 500)
        {
            return MyQueueManager.GetMultipleItems(takeSize);
        }

    }


}
