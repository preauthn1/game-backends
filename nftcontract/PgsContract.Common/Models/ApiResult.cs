using System;

namespace PgsTokenContract.Models
{
    public class ApiResult<T>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

    }


}
