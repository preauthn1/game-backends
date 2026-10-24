using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    
    public enum CommonErrorTypeEnum 
    {
        [EnumDescription("查无指定的资料")]
        NotFoundData = 404001
    }
}
