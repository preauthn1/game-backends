using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireJob.Services.Gash
{
    public interface ISettleService
    {
        Task DoWorkAsync(string url);
    }
}
