using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{

    public class UnionpayBankPlatformMappingViewModel : UnionpayBankPlatformMappingEntity
    {

    }

    public class UnionpayBankPlatformUpdateViewModel
    {
        public int PlatformID { get; set; }

        public List<UnionpayBankPlatformMappingEntity> Items { get; set; }
    }


}
