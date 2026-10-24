using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_UserRate
    {
        public Nullable<int> UserID { get; set; }
        public byte UserLevel { get; set; }
        public int Stage1 { get; set; }
        public int Stage2 { get; set; }
        public int Stage3 { get; set; }
        public int SystemStage1 { get; set; }
        public int SystemStage2 { get; set; }
        public int SystemStage3 { get; set; }

        public decimal UseStage1
        {
            get { return Convert.ToDecimal(this.Stage1) * 0.1m; }
            set { this.Stage1 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
        public decimal UseStage2
        {
            get { return Convert.ToDecimal(this.Stage2) * 0.1m; }
            set { this.Stage2 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
        public decimal UseStage3
        {
            get { return Convert.ToDecimal(this.Stage3) * 0.1m; }
            set { this.Stage3 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
        public decimal UseSystemStage1
        {
            get { return Convert.ToDecimal(this.SystemStage1) * 0.1m; }
            set { this.SystemStage1 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
        public decimal UseSystemStage2
        {
            get { return Convert.ToDecimal(this.SystemStage2) * 0.1m; }
            set { this.SystemStage2 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
        public decimal UseSystemStage3
        {
            get { return Convert.ToDecimal(this.SystemStage3) * 0.1m; }
            set { this.SystemStage3 = Convert.ToInt32(Math.Floor(value * 10m)); }
        }
    }
}
