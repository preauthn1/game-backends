using FW.Core.AdminSys.Provider.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.EFProvider.Map
{
    public class QuickActionConfig : EntityTypeConfiguration<QuickAction>
    {
        public QuickActionConfig()
        {
            this.ToTable("Admin.QuickAction");
            this.Property(qa => qa.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           
        }
    }
}
