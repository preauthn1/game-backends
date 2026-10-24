using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models
{
    public class RazorPagingRequestModel
    {        

        public int PageSize { get; set; }


        /// <summary>
        /// 第几页
        /// </summary>
        public int P { get; set; }        
        
    }
}