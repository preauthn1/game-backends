using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryGameUserListRequestViewModel : PaginationParameter
    {

        public List<int> UserIDs { get; set; }


        public List<int> ServerIDs { get; set; }



        

    }


}
