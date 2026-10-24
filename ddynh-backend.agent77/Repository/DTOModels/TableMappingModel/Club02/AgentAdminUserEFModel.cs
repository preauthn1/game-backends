using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTOModels
{
    public class AgentAdminUserEFModel
    {
        [Key]
        public int ID { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public int Status { get; set; }
        public int AdminType { get; set; }
        public DateTime CreateTime { get; set; }
        public string Encrypt { get; set; }
        public string Memo { get; set; }
        public int Mode1 { get; set; }
        public int Mode2 { get; set; }
        public int Mode3 { get; set; }
        public int Mode4 { get; set; }
        public int Mode5 { get; set; }
        public int Mode6 { get; set; }
        public DateTime UpdateTime { get; set; }


    }
}
