using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
  
    public class EnumSettingValue : System.Attribute
    {
        public string Value { get; set; }


        public EnumSettingValue(string myValue)
        {
            this.Value = myValue;
        }

    }


}
