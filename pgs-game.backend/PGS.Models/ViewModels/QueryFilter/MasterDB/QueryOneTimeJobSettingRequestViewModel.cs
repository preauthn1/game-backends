using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryOneTimeJobApiRequestParameterModel : QueryOneTimeJobSettingRequestViewModel
    {
        public int? Type { get; set; }

    }


    public class QueryOneTimeJobSettingRequestViewModel : PaginationParameter
    {

        public bool IsReadyToExecuted { get; set; }

        public string MemoMatch { get; set; }

        public string Keyword { get; set; }


        /// <summary>
        /// 參考 OneTimesJobTypeEnum 
        /// </summary>
        public List<int> Types { get; set; }

        /// <summary>
        /// 可能是 eventID 或 其他ID
        /// </summary>
        public List<int> SourceIDs { get; set; }

    }


}
