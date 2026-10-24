using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModels.Query
{

    public class QueryGameAliasNameRequestViewModel : PaginationParameter
    {

        public List<int> GameIDs { get; set; }

    }


}
