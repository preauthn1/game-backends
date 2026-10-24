using game.Entity.Game;
using System.Collections.Generic;

namespace game.Entity.Payment
{
    public class PayGatewayConvertTypeMappingUpdateRequestModel 
    {

        public int GWID { get; set; }

        public List<PayGatewayConvertTypeMappingEntity> Items { get; set; }

    }


}
