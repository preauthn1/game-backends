using HangfireJob.EFModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireJob.Services.Gash
{
    public class SettleService : ISettleService
    {
        private readonly IServiceProvider _provider;
        public SettleService(IServiceProvider serviceProvider)
        {
            _provider = serviceProvider;
        }

        async Task ISettleService.DoWorkAsync(string url)
        {
            using (IServiceScope scope = _provider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EFCoreContext>();
                var list = await context?.GashSettles?.Where(s => !s.IsSuccess).ToListAsync();
                foreach (var item in list)
                {
                    try
                    {
                        //item.COId = item.COId.Split("_")[0] + "_" + DateTime.Now.Millisecond.ToString();
                        if (InvokeHelper.CallDoAPI(item, url))
                        {
                            item.IsSuccess = true;
                        }
                        item.LastUpdateTime = DateTime.UtcNow.AddHours(8);
                        context.Entry(item).CurrentValues.SetValues(item);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
