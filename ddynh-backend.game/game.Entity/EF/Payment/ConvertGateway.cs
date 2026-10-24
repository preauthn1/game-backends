using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Payment
{
    public class ConvertGateway : BaseEntity
    {  
        public string GatewayName
        {
            get;
            set;
        }
        /// <summary>
        /// 代付方式
        /// </summary>
        public int ConvertType
        {
            get;
            set;
        }        

        /// <summary>
        /// 是否启动
        /// </summary>
        public bool Status
        {
            get;
            set;
        }

        /// <summary>
        /// 最小金額
        /// </summary>
        public int Company
        {
            get;
            set;
        }

        /// <summary>
        /// 最大金額
        /// </summary>
        public long MaxValue
        {
            get;
            set;
        }

        /// <summary>
        /// 热门程度
        /// </summary>
        public int Popular
        {
            get;
            set;
        }
    }
}
