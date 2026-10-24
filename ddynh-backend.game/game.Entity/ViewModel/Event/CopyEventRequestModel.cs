using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class CopyEventRequestModel
    {

        public List<int> TemplateEventIDs { get; set; }

        public List<int> TargetPlatformIDs { get; set; }

        public string UpdateBy { get; set; }

    }

}
