using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using target.Models.Enums;

namespace target.Models.Models
{
    public class GameImageDomainAndStyleModel
    {
        public string ImageDomainUrl { get; set; }

        public GameImageStyleTypeEnum StyleType { get; set; }
    }
}
