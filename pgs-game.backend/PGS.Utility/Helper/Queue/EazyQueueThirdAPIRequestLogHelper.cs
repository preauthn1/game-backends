using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
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
    /// </summary>
    public static class EazyQueueThirdAPIRequestLogHelper
    {

        private const string IdentityKey = "ThirdAPIRequestLogHelper";

        public static BaseQueueHelper<FullInfoThirdPartyApiRequestLogESModel> MyQueueManager = new BaseQueueHelper<FullInfoThirdPartyApiRequestLogESModel>(IdentityKey);


        public static void Append(FullInfoThirdPartyApiRequestLogESModel data)
        {
            MyQueueManager.Append(data);
        }


        public static void AppendMultiple(List<FullInfoThirdPartyApiRequestLogESModel> items)
        {
            MyQueueManager.AppendMultiple(items);
        }

        public static List<FullInfoThirdPartyApiRequestLogESModel> GetMultipleItems(int takeSize = 200)
        {
            return MyQueueManager.GetMultipleItems(takeSize);
        }

    }


}
