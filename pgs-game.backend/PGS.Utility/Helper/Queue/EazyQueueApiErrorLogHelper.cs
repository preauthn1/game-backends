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
using System.Reflection;
using System.Runtime.Versioning;
using target.Models.Models;
using target.Models.CultureText;
using target.Models.Enums;
using Lck.Utility.Extensions.Helper;
using System.Collections.Concurrent;
using target.Models.Table;

namespace target.Utility.Helper
{

    /// <summary>
    /// 雖然號稱queue 但是懶得建置服務 (MQ)
    /// 基本上就是存在本機記憶體而已
    /// </summary>
    public static class EazyQueueApiErrorLogHelper
    {

        private const string IdentityKey = "ApiErrorLogHelper";


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
