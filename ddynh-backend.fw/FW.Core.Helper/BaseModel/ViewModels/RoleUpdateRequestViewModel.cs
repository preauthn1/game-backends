using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.ViewModels
{
    public class RoleUpdateRequestViewModel
    {
        public string RoleName { get; set; }

        public string Memo { get; set; }

        public int OrgID { get; set; }

        public bool IsTemplateForCopy
        {
            get
            {
                return IsTemplateForCopyCheckBox == "on";
            }
        }


        public string IsTemplateForCopyCheckBox { get; set; }


    }
}
