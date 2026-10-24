using FW.Core.Helper;
using game.Entity.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class CreateOrganizationrRequestViewModel : Organization
    {        

        public bool IsGenerateTemplateCheck
        {
            get
            {
                return IsGenerateTemplate == "on";
            }
        }

        public string IsGenerateTemplate { get; set; }

    }
}
