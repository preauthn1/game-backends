using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryGameListCustomizedSettingRequestViewModel : PaginationParameter
    {
        public int? GameID { get; set; }

        public List<int> GameIDs { get; set; }


        /// <summary>
        /// 參考 GameListCustomizedSettingKeyEnum
        /// </summary>
        public int? SettingKey { get; set; }

        /// <summary>
        /// 參考 GameListCustomizedSettingKeyEnum
        /// </summary>
        public List<int> SettingKeys { get; set; }


        public string SettingValueMatch { get; set; }

    }


}
