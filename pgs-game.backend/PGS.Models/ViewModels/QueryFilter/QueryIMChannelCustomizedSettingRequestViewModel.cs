using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryIMChannelCustomizedSettingRequestViewModel : PaginationParameter
    {

        /// <summary>
        /// 參考 IMChannelCustomizedSettingKeyEnum
        /// </summary>
        public List<int> SettingKeys { get; set; }

        public List<int> IMChannelIDs { get; set; }

        public string SettinValueMatch { get; set; }

        public List<string> SettinValueMatchList { get; set; }

        public int? TagID { get; set; }

        public int? IMChannelType { get; set; }

        public string ThirdPartyChatID { get; set; }

    }


}
