using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{
    public class ConvertSuspicious
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountID { get; set; }
        public DateTime CreateTime { get; set; }

        private string _strCreateTime;
        public string StrCreateTime
        {
            get { return _strCreateTime; }
            set { _strCreateTime = CreateTime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

    }
}
