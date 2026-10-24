using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    /// <summary>
    /// 回應參數
    /// </summary>
    public class RoleViewModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public int Status { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public string UpdateBy { get; set; }
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 對應的帳號數
        /// </summary>
        public int UserMappingCount { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [SwaggerIgnoreOutputWhenPagedList]
        public List<int> Permissions { get; set; }

        public string Memo { get; set; }

        public int Sort { get; set; }

    }
}
