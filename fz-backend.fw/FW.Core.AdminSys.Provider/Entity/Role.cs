using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class Role
    {
        public int RoleID
        {
            get;
            set;
        }

        [Required]
        [StringLength(32)]
        public string RoleName
        {
            get;
            set;
        }
    }
}
