using target.Models.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class CommonSPExecuteModel
    {
        public string SPName { get; set; }

        /// <summary>
        /// 請參考 DBConnType
        /// </summary>
        public string DBName { get; set; }

    }



    public class CommonSPResultModel
    {
        public bool IsSuccess
        {
            get
            {
                var code = (int)this.Code;
                return code == 1 || code == 0;
            }
        }


        public ApiErrorCode Code { get; set; }

    }
}
