using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryContentBlockRequestViewModel : PaginationParameter
    {
        public Guid? OwnerGuid { get; set; }
        public List<Guid> OwnerGuidList { get; set; }
        public List<int> ContentTemplateIDs { get; set; }
    }


}
