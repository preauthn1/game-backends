using System;
using System.Collections.Generic;

namespace target.Models.CoreLibs
{
    public class CRUDResultModel
    {
        public CRUDResultModel()
        {

        }
        
        public CRUDResultModel (string msg)
        {
            Message = msg;
        }

        public CRUDResultModel(bool isOK , string msg = "储存成功")
        {
            IsSuccess = isOK;
            Message = msg;
        }



        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public string DebugMessage { get; set; }

        public int ResultId { get; set; }

        public long ResultLongId { get; set; }

        public decimal ResultNumber { get; set; }

        public DateTime? ResultTime { get; set; }

        public List<int>  ResultListIDs { get; set; }

        public List<string> ResultListText { get; set; }

        public string ResultExtraString { get; set; }

        public object ResultObject { get; set; }

        public List<object> ResultListObject { get; set; }
    }
}
