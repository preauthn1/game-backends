using System;

namespace target.Models.ViewModels
{



    public class EFGAPICommonResponse<T>
    {
        /// <summary>
        /// 200代表成功
        /// </summary>
        public int Status { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }
    }


    public class  EFGAPIErrorResponse
    {
        public EFGAPIErrorResponse()
        {
            this.Code = 0;
            this.Msg = "Success";
        }

        /// <summary>
        /// 0代表成功
        /// </summary>
        public int Code { get; set; }

        public string Msg { get; set; }

        public ClotPlayBalanceResponse Data { get; set; }
    }



}
