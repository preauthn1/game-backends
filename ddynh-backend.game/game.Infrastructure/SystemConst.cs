using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 系统常量定义
    /// </summary>
    public static class SystemConst
    {
        /// <summary>
        /// 系统消息用户ID
        /// </summary>
        public const int Message_SystemUserID = 0;

        public const int ExcelMaxExportSize = 20000;

        public const int DefaultAdminID = 1;

        /// <summary>
        /// 很多地方都要 *100
        /// </summary>
        public const int CashAmountRate = 100;


        public const double TIMEZONE_TAIWAN = 8.0;



        /// <summary>
        /// 分頁預設取幾筆
        /// </summary>
        public const int DefaultPageSize = 20;

        public const string ApiPassword = [REDACTED]";
        public const string AdminSys_AlwaysHeaderAdminID = "X-UserAdminID";
        public const string AdminSys_AlwaysHeaderUserName = "X-UserName";
        public const string AdminSys_AlwaysHeaderMemo = "X-Memo";
        public const string AdminSys_AlwaysHeaderUniqueRequestID = "X-UniqueRequestID";
        public const string Excel_Format_ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Default_Close_WebView_Url = "http://example.invalid";
        public const string SUCCESS_MSG = "成功";


        /// <summary>
        /// 某些程式判斷的萬用通行碼
        /// </summary>
        public const string MAGIC_CODE = "5cf8b095e448";

        //可能包含密碼的api 
        public static List<string> PasswordApiUrls = new List<string>
        {
            "passowrd",
            "passow",
            "login",
            "psw",
        };

    }
}
