using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class AGGLiveTokenTempModel
    {
        public string Token { get; set; }

        public DateTime ExpiredTime { get; set; }

        public int OperatorID { get; set; }
    }


    public class AGGLiveBackOfficeLoginResultModel
    {
        public string Token { get; set; }

        public int OperatorID { get; set; }
    }

}
