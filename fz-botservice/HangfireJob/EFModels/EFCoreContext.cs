using HangfireJob.EFModels.Gash;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HangfireJob.EFModels
{
    public class EFCoreContext : DbContext
    {
        public EFCoreContext(DbContextOptions<EFCoreContext> options) : base(options)
        {
        }

        public DbSet<GashSettleLog> GashSettles { get; set; }
    }
}
