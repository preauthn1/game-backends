
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;

namespace game.Repository.Customer
{
    public class QuestionListConfig:EntityTypeConfiguration<game.Entity.Customer.QuestionList>
    {
        public QuestionListConfig()
        {
            this.ToTable("Customer.QuestionList");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => example.invalid)
                .WithMany()
                .HasForeignKey(m=>m.SiteID);

            this.HasRequired(m => m.QuestionType)
                .WithMany()
                .HasForeignKey(m => m.QsTypeId);

            //this.HasOptional(m => m.QuestionType)
            // .WithMany()
            // .HasForeignKey(m => m.QsTypeId);

            //this.Property(m => m.UserID)
            //    .HasColumnName("UserID");
             
            this.HasOptional(m => m.UserInfo)
                .WithMany()
                .HasForeignKey(m => m.UserID);
        }
    }
}
