using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Service.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using FW.Core.Engine;

namespace game.Service.Service
{
    public class BaseService 
    {        
        public BaseService()
        {
            InstanceAllRepositoryImplement();
        }
        
        protected void InstanceAllRepositoryImplement()
        {
            //如果service class內的全域變數
            //1. 定義為 Property (非Field)
            //2. 且命名包含 repository
            //3. 有繼承 IDapperRepository
            

            var allRep = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.ToLower().Contains("repository") || x.Name.ToLower().Contains("rep"))
                .ToList();

            foreach (PropertyInfo item in allRep)
            {
                if(item.PropertyType.BaseType != null)
                {
                    bool isMigrateRep = item.PropertyType.BaseType == typeof(BaseDataMigratePendingQueueRepository);
                    bool isDapperRep = item.PropertyType.BaseType.IsGenericType && 
                        item.PropertyType.BaseType.GetGenericTypeDefinition() == typeof(IDapperRepository<>);

                    if (isMigrateRep || isDapperRep)
                    {
                        var instanceValue = Activator.CreateInstance(item.PropertyType);
                        item.SetValue(this, instanceValue);
                    }

                }                
            }

            var allService = this.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name.ToLower().Contains("service"))
                .ToList();


            foreach (PropertyInfo item in allService)
            {
                if (item.PropertyType.IsInterface)
                {
                    var instanceValue = IOCHelper.Resolve(item.PropertyType);
                    item.SetValue(this, instanceValue);
                }
            }

        }


        protected CRUDResultModel InstanceFailModel(CommonErrorTypeEnum type)
        {
            var msg = type.GetEnumDescription();
            return new CRUDResultModel(msg);
        }

        
    }
}
