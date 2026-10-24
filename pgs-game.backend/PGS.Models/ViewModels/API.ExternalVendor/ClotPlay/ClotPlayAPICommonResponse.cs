using System;

namespace target.Models.ViewModels
{
    public class ClotPlayAPICommonResponse<T>
    {
        /// <summary>
        /// 0代表成功
        /// </summary>
        public int Code { get; set; }

        public T Data { get; set; }
    }


    public class ClotPlayAPIErrorResponse
    {
        public ClotPlayAPIErrorResponse()
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



    public class ClotPlayBalanceResponse
    {
        public decimal Balance { get; set; }
    }


}
