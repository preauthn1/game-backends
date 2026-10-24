using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class StoreProcedureResultModel
    {
        public bool IsSuccess { get; set; }


        public string ResultMsg { get; set; }
    }


    public class StoreProcedureResultModel<T>
    {
        public StoreProcedureResultModel()
        {

        }

        public StoreProcedureResultModel(bool isSuccess ,T data)
        {
            this.IsSuccess = isSuccess;
            this.ResultData = data;
        }
        
        public StoreProcedureResultModel(T data)
        {
            this.IsSuccess = true;
            this.ResultData = data;
        }



        public bool IsSuccess { get; set; }


        public string ResultMsg { get; set; }


        public T ResultData { get; set; }

    }
}
