using FW.Core.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryAreaRequestViewModel 
    {
        public string AreaIDsText { get; set; }

        public string PlatformIDsText { get; set; }

        public List<int> AreaIDs { get; set; }

        public List<int> PlatformIDs { get; set; }

    }




}
