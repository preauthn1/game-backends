using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class DomainSettingModel
    {
        public int ID { get; set; }
        public int PlatformID { get; set; }
        public Nullable<int> AreaID { get; set; }
        public string Domain { get; set; }
        public byte Status { get; set; }
        public System.DateTime CreateTime { get; set; }
        public string PlatformName { get; set; }
    }
}
