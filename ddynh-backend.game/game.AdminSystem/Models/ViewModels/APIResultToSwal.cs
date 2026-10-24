using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.AdminSystem.Models.ViewModels
{
    /// <summary>
    /// GameAPI回傳值轉JS的Swal使用
    /// </summary>
    public class APIResultToSwal
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        private bool check;
        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="result">执行结果</param>
        /// <param name="message">返回消息</param>
        /// <param name="extend">扩展信息</param>
        /// <returns></returns>
        public APIResultToSwal(bool result, string message = null, object extend = null)
        {
            check = result;
            Message = message;
            Extend = extend;
        }

        /// <summary>
        /// 执行结果傳給Swal字串
        /// </summary>
        public string Result
        {
            get
            {
                if (check)
                    return "success";
                else
                    return "error";
            }
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