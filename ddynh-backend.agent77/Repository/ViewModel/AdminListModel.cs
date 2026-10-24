using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class AdminListModel
    {
        public int ID { get; set; }

        public string Account { get; set; }

        public int Status { get; set; }

        public RoleType AdminType { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
