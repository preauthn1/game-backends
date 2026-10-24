using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryDetailLogRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public List<int> UserIDs { get; set; }

        public List<int> GameIDs { get; set; }


        public List<int> LogIDs { get; set; }



        public DateTime? GameTimeBegin { get; set; }

        public DateTime? GameTimeEnd { get; set; }

    }


}
