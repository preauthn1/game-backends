using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace game.Entity.Game
{

    public class LazyCommonParameterRequestModel
    {

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public int SplitMinutes { get; set; }


    }


}
