using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class PlayerLoginDeviceModel
    {

        public int PlayerID { get; set; }

        public int HDType { get; set; }

        public int LoginCount { get; set; }


    }
}
