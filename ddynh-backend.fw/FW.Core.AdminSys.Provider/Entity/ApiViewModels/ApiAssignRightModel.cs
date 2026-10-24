using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{

    public class ApiAssignRightModel
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public int RoleID { get; set; }

        public int RightID { get; set; }

    }

}
