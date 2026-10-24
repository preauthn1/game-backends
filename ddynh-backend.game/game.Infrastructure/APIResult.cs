using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    public class APIResult
    {
        public APIResult(bool result, string message = null, object extend = null)
        {
            this.Result = result;
            this.Message = message;
            this.Extend = extend;
        }
        
        public APIResult()
        {
        }


        public APIResult(bool result)
        {
            this.Result = result;
            this.Message = "成功";
        }


        /// <summary>
        /// 执行结果
        /// </summary>
        public bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 扩展信息
        /// </summary>
        public object Extend { get; set; }
    }

    public class APIResult2
    {
        public APIResult2(bool result, int resultCode, string message = null, object extend = null)
        {
            this.Result = result;
            this.ResultCode = resultCode;
            this.Message = message;
            this.Extend = extend;
        }
        /// <summary>
        /// 执行结果
        /// </summary>
        public bool Result
        {
            get;
            set;
        }
        /// <summary>
        /// 結果代碼
        /// </summary>
        public int ResultCode
        {
            get;
            set;
        }
        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 扩展信息
        /// </summary>
        public object Extend { get; set; }
    }
}
