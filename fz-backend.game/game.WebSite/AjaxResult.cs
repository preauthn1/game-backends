using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.WebSite
{
    /// <summary>
    /// ajax请求结果    
    /// </summary>
    public class AjaxResult
    {
        public bool success { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}