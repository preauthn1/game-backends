using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAssignTemplateParticipaterRequestViewModel : PaginationParameter
    {

        public Guid? AssignTemplateID { get; set; }

        public List<Guid> AssignTemplateIDs { get; set; }

        public int? AssignType { get; set; }

    }


}
