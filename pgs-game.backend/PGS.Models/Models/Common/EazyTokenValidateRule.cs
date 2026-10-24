using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class EazyTokenValidateRule
    {
        /// <summary>
        /// 若有指定 則會解析PGS單號、檢查是否已經是N小時以前的token了 
        /// 若檢查逾時過久 直接拋錯誤、不需查DB
        /// </summary>
        public int? CheckExpiredHourBeforeQueryDB { get; set; }


        /// <summary>
        /// 檢查是否為允許查詢的資料ID或單號
        /// e.g. 拿別人的token 來查詢別的單
        /// </summary>
        public string ValidateValueMatch { get; set; }

    }
}
