using target.Models.Models;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class CustomizedColumnDisplayRequestModel
    {

        /// <summary>
        /// 對應 DynamicQueryCommandRepository 這張表的ID
        /// </summary>
        public int QuerySqlID { get; set; }

        public int? PlayerID { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public string PlayerName { get; set; }

        public string TransactionNo { get; set; }

        public string TableName { get; set; }

        public string Text { get; set; }

        public string FreespinID { get; set; }

        public double TimeZone { get; set; }

        public bool IsQueryStraightCustomerOnly { get; set; }

        public int? SortDirection { get; set; }

        public string SortByColumn { get; set; }


        public int? MerchantID { get; set; }

        public int? GameVendor { get; set; }

        public int? GameID { get; set; }

        public int? EventID { get; set; }

        public int? RoleID { get; set; }

        public int? OrganizationID { get; set; }

        public int? PermissionID { get; set; }

        public int CurrentUserID { get; set; }

        public int IMChannelID { get; set; }

        public int? VendorConfigID { get; set; }
        public int? Type { get; set; }

        public string Currency { get; set; }

        public int? TargetID { get; set; }

        public string TargetKey { get; set; }


        public string UniqueID { get; set; }




        public string TargetDayText { get; set; }

        public string OrderBy { get; set; }
                

        public DateTime? BeginDateTW { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public DateTime? TargetTime { get; set; }

        /// <summary>
        /// 指定API 不要擅自轉千分位符號
        /// </summary>
        public bool IsSkipFormatAmountColumn { get; set; }

    }


    public class CustomizedColumnDynamicQueryResultViewModel
    {


        public List<Dictionary<string , object>> Items { get; set; }

        public string ExtraDisplayName { get; set; }

        public int ExtraDisplayType { get; set; }

        public CustomizeQueryMethodResultModel ExtraInfo { get; set; }

        public CustomizedColumnExtraInfoViewModel ExtraObject { get; set; }
    }


    public class CustomizedColumnExtraInfoViewModel
    {

        public CustomizedColumnExtraInfoViewModel()
        {
            this.CopyExcel = "";
        }

        public Dictionary<string, string> ColumnTranslateMapping { get; set; }

        public List<CustomizedColumnDisplayColumnViewModel> Columns { get; set; }

        public string CopyExcel { get; set; }
    }

    public class CustomizedColumnDisplayColumnViewModel
    {

        public CustomizedColumnDisplayColumnViewModel()
        {

        }

        public CustomizedColumnDisplayColumnViewModel(string columnName)
        {
            this.ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }


    public class CustomizedColumnDisplayViewModel
    {
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public string Column6 { get; set; }
        public string Column7 { get; set; }
        public string Column8 { get; set; }
        public string Column9 { get; set; }
    }



    public class CustomizedColumnDynamicQueryExtraSettingJson
    {
        public int? Filter_MaxDay { get; set; }

        /// <summary>
        /// 若true。會自動生成一列。加總列
        /// </summary>
        public bool IsGenerateTotalSum { get; set; }

        public bool IsSkipCsvFormat { get; set; }

        public bool IsSkipFormatColumns { get; set; }

        /// <summary>
        /// 是否為直接下載csv的類型
        /// </summary>
        public bool IsCsvFormat { get; set; }


        /// <summary>
        /// (僅部分有效) 自動插入分隔線
        /// </summary>
        public bool IsAppendBreakLine { get; set; }



    }


}
