using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryGameRTPConfigRequestParameterModel : QueryGameRTPConfigRequestViewModel
    {

        public int? GameID { get; set; }

        public int? VendorConfigID { get; set; }

    }


    public class QueryGameRTPConfigRequestViewModel : PaginationParameter
    {

        [SwaggerIgnore]
        public List<int> GameIDs { get; set; }


        [SwaggerIgnore]
        public List<int> VendorConfigIDs { get; set; }


        public decimal? RTPValue { get; set; }




    }


}
