using target.Models.Table;
using target.Models.ViewModels.API.AdminSys;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class BackendDefineButtonViewModel
    {

        public List<BackendDefineButtonItemViewModel> Items { get; set; }

    }

    public class BackendDefineButtonItemViewModel
    {

        /// <summary>
        /// 請參考 BackendButtonEnum
        /// </summary>
        public int BackendButtonActionID { get; set; }


        public string DisplayName { get; set; }


        public List<string> RequiredInput { get; set; }


        public string MemoHint { get; set; }


        /// <summary>
        /// 按鈕旁邊會多一個info說明
        /// </summary>
        public string InfoDescription { get; set; }



        public List<DropdownValueViewModel> MyExtraDropdown { get; set; }

    }


    public class BackendDefineButtonActionRequestModel
    {

        /// <summary>
        /// 請參考 BackendButtonEnum
        /// </summary>
        public int BackendButtonActionID { get; set; }


        /// <summary>
        /// 判斷是否為 客服或AM操作
        /// </summary>
        [SwaggerIgnore]
        public bool IsOperateByCSOrAM { get; set; }

        /// <summary>
        /// 每小時最多執行幾次的檢查 (錯誤也算算一次)
        /// </summary>
        public int? MaxTimesPerHour { get; set; }


        public BackendDefineButtonActionExtraParameterModel ExtraParameter { get; set; }

    }



    public class BackendDefineButtonActionExtraParameterModel
    {
        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public int VendorConfigID { get; set; }

        public int GameVendorType { get; set; }

        public int TagID { get; set; }

        public string TargetTable { get; set; }

        public string TextArea { get; set; }

        public string TextArea2 { get; set; }

        public string Text { get; set; }

        public string Text2 { get; set; }

        public string NumberText { get; set; }

        public string PlayerAccount { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

        /// <summary>
        /// 關閉特殊通知
        /// </summary>
        public bool IsDisableNotify { get; set; }
    }



    public class BackendButtonResultModel
    {
        public BackendButtonResultModel()
        {

        }

        public BackendButtonResultModel(string msg)
        {
            Message = msg;
        }

        public BackendButtonResultModel(bool isOK, string msg = "储存成功")
        {
            IsSuccess = isOK;
            Message = msg;
        }

        public MyFileResultModel FileResult { get; set; }

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string DebugMessage { get; set; }

        public long ResultLongId { get; set; }

        public decimal ResultNumber { get; set; }

        public DateTime? ResultTime { get; set; }

        public List<int> ResultListIDs { get; set; }

        public List<string> ResultListText { get; set; }

        public object ResultObject { get; set; }

        public List<object> ResultListObject { get; set; }
    }


    public class MyFileResultModel
    {
        public Stream FileStream { get; set; }

        public string FileName { get; set; }
    }



}
