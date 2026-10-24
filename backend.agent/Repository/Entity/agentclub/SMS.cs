using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Entity.agentclub
{
    public class SMS
    {
        public string VerifyCode { get; set; }
        public byte Status { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
