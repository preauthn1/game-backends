using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper
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

        public int ResultId { get; set; }

        public object ResultObject { get; set; }
    }
}
