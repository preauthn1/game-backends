using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{

    /// <summary>
    /// 會根據當前時間產生兩組key (上一個時段 跟 這個時段)
    /// </summary>
    public class HashKeyWithServerTimeModel
    {
        public DateTime Time0 { get; set; }

        public DateTime Time1 { get; set; }


        /// <summary>
        /// 當前時間的Hash
        /// </summary>
        public string Hash0 { get; set; }

        /// <summary>
        /// 前一個時段的
        /// </summary>
        public string Hash1 { get; set; }


        public bool IsMatchAnyKey(string otherHash0 , string otherHash1 = "")
        {
            return Hash0 == otherHash0 || Hash0 == otherHash1 || Hash1 == otherHash0 || Hash1 == otherHash1;
        }

    }
}
