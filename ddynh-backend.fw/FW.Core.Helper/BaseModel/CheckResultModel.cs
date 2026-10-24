using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Helper
{
    public class CheckResultModel
    {
        public CheckResultModel()
        {

        }
        
        public CheckResultModel(string msg)
        {
            Message = msg;
        }


        public bool IsSuccess { get; set; }

        public string Message { get; set; }


    }
}
