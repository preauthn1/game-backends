using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    public class GameVendorGamesModel
    {

        /// <summary>
        /// GameVendorTypeEnum
        /// </summary>
        public int GameVendorID { get; set; }


        /// <summary>
        /// slot88的遊戲會包含在 target
        /// </summary>
        public List<int> GameIDs { get; set; }

    }


}
