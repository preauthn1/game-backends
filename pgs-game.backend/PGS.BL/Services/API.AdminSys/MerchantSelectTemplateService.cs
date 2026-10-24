using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Utility;
using System.Transactions;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public class MerchantSelectTemplateService : BaseAdminSysService, IMerchantSelectTemplateService
    {

        public MerchantSelectTemplateRepository MerchantSelectTemplateRepository { get; set; }

        public MerchantSelectMappingItemRepository MerchantSelectMappingItemRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }


        public MerchantSelectTemplateViewModel GetByTemplateID(int userID, int templateID)
        {
            var query = GetByTemplateID(templateID);
            return query;
        }


        private MerchantSelectTemplateViewModel GetByTemplateID(int templateID)
        {
            var query = MerchantSelectTemplateRepository.GetById(templateID);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryItems = MerchantSelectMappingItemRepository.GetByFilter(new QueryMerchantSelectMappingItemRequestViewModel
            {
                TemplateIDs = templateID.ToListCollection()
            });

            var viewModel = query.ReturnInjecteObject<MerchantSelectTemplateViewModel>();

            viewModel.ExcludeMerchantIDs = GetMerchantIDsByType(queryItems, MerchantSelectMappingItemTypeEnum.ExcludeMerchantID);
            viewModel.IncludeMerchantIDs = GetMerchantIDsByType(queryItems, MerchantSelectMappingItemTypeEnum.IncludeMerchantID);
            viewModel.ParentMerchantID = GetMerchantIDsByType(queryItems, MerchantSelectMappingItemTypeEnum.ParentMerchantID).FirstOrDefault();
            viewModel.AlwaysAddMerchantIDs = GetMerchantIDsByType(queryItems, MerchantSelectMappingItemTypeEnum.AlwaysIncludeMerchantID);

            return viewModel;
        }


        public CRUDResultModel CreateTemplate(int userID, MerchantSelectTemplateRequestModel model)
        {
            model.RuleFor(x => x.CategoryID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ValueType).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();

            //防呆檢查
            ValidateParameterByValueType(model.ValueType, model);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var insert = new MerchantSelectTemplateEntity
            {
                CategoryID = model.CategoryID,
                DisplayName = model.DisplayName,
                Memo = "",
                Status = NormalActiveStatus.Enable.ToInteger(),
                UpdateBy = currentUser.UserName,
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                ValueType = model.ValueType
            };

            List<MerchantSelectMappingItemEntity> insertItems = InstanceInsertMerchantSelectMappingItems(model, 0);

            int resultTemplateID = 0;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                resultTemplateID = MerchantSelectTemplateRepository.Create(insert);

                insertItems.ForEach(x => x.TemplateID = resultTemplateID);

                MerchantSelectMappingItemRepository.CreateMany(insertItems);

                scope.Complete();
            }

            return new CRUDResultModel
            {
                ResultId = resultTemplateID
            };
        }


        public CRUDResultModel UpdateTemplate(int userID, MerchantSelectTemplateRequestModel model)
        {
            model.RuleFor(x => x.TemplateID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.CategoryID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ValueType).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();

            var queryExist = MerchantSelectTemplateRepository.GetById(model.TemplateID);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryOriginalMerchantIDs = MerchantSelectMappingItemRepository.GetAvailableMerchantIDsByTemplateID(model.TemplateID);

            //防呆檢查
            ValidateParameterByValueType(queryExist.ValueType, model);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            List<MerchantSelectMappingItemEntity> insertItems = InstanceInsertMerchantSelectMappingItems(model, queryExist.TemplateID);

            queryExist.DisplayName = model.DisplayName;
            queryExist.UpdateBy = currentUser.UserName;
            queryExist.UpdateTime = DateTimeHelper.GetCurrentTime();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                MerchantSelectTemplateRepository.Update(queryExist);

                MerchantSelectMappingItemRepository.DeleteAndInsertNew(queryExist.TemplateID, insertItems);

                scope.Complete();
            }

            ResetCacheWhenTemplateMerchantChanged(queryExist.CategoryID, queryOriginalMerchantIDs, insertItems);

            return new CRUDResultModel
            {
                ResultId = queryExist.TemplateID
            };
        }
      
        public void RegenerateAssignMerchantIDsForAgentAssignMode(List<int> affectMerchantIDs)
        {
            //先找出代理型的配置
            var queryTemplates = MerchantSelectTemplateRepository.GetByFilter(new QueryMerchantSelectTemplateRequestViewModel
            {
                ValueType = MerchantSelectValueTypeEnum.SelectAgentAndExcludeMerchants.ToInteger(),
                IsIncludeDeleteData = false
            });

            if (queryTemplates.IsEmptyOrNull())
            {
                return;
            }

            foreach (var template in queryTemplates)
            {
                try
                {
                    var queryDetail = GetByTemplateID(template.TemplateID);

                    bool isAnyEffect = affectMerchantIDs.Any(merchantID => queryDetail.IncludeMerchantIDs.Contains(merchantID));

                    if (isAnyEffect)
                    {
                        var param = new MerchantSelectTemplateRequestModel
                        {
                            ExcludeMerchantIDs = queryDetail.ExcludeMerchantIDs,
                            ParentMerchantID = queryDetail.ParentMerchantID,
                        };

                        var newMappingItems = InstanceInsertMerchantSelectMappingItems(param, template.TemplateID);

                        //刪除 & 重建資料
                        MerchantSelectMappingItemRepository.DeleteByModel(new
                        {
                            TemplateID = template.TemplateID
                        });

                        MerchantSelectMappingItemRepository.BulkInsert(newMappingItems);

                        //清除緩存
                        ResetCacheWhenTemplateMerchantChanged(queryDetail.CategoryID, queryDetail.IncludeMerchantIDs, newMappingItems);
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.GetCommonNotifyMessage("RegenerateAssignMerchantIDsForAgentAssignMode");
                    SendMessageToMultipleChannel(msg);
                }
            }
        }



        public void Delete(int userID, int templateID)
        {
            new { TemplateID = templateID }.RuleFor(x => x.TemplateID).Rule_NotZeroNotMinus();

            var queryExist = MerchantSelectTemplateRepository.GetById(templateID);

            if (queryExist == null)
                return;

            queryExist.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            MerchantSelectTemplateRepository.Update(queryExist);
        }

        private void ValidateParameterByValueType(int myValueType, MerchantSelectTemplateRequestModel model)
        {
            var valueType = (MerchantSelectValueTypeEnum)myValueType;

            if (!valueType.IsDefinedValue())
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);
            }

            if (valueType == MerchantSelectValueTypeEnum.SelectIncludeMerchants)
            {
                model.RuleFor(x => x.IncludeMerchantIDs).Rule_IsNotEmptyCollection();
            }
            else if (valueType == MerchantSelectValueTypeEnum.SelectAgentAndExcludeMerchants)
            {
                model.RuleFor(x => x.ParentMerchantID).Rule_NotMinus();
            }
        }


        private List<int> GetMerchantIDsByType(List<MerchantSelectMappingItemEntity> items, MerchantSelectMappingItemTypeEnum type)
        {
            int myType = type.ToInteger();
            return items.Where(x => x.Type == myType)
                .Select(x => x.MerchantID)
                .ToList();
        }

        private MerchantSelectByParentAvailableResultModel GetAvailableMerchantIDsByParentAgent(int parentMerchantID, List<int> excludeMerchantIDs)
        {
            var queryHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
            {
                HierarchyOfTargerMerchant = parentMerchantID
            });

            List<int> allMerchantIDs = queryHierarchy.Select(x => x.MerchantID).ToList();

            //排除掉不需要的代理 (如果上級代理設置關閉、下級代理一律視為關閉)
            if (excludeMerchantIDs.IsNotEmptyNull())
            {
                queryHierarchy = queryHierarchy
                    .Where(x => !AppHelper.IsHierarchyChildOfTargetMerchant(x.Hierarchy, excludeMerchantIDs))
                    .ToList();
            }

            return new MerchantSelectByParentAvailableResultModel
            {
                ParentMerchantID = parentMerchantID,
                AllHierarchyMerchantIDs = allMerchantIDs,
                ExcludeMerchantIDs = excludeMerchantIDs,
                AvailableMerchantIDs = queryHierarchy.Select(x => x.MerchantID).ToList()
            };
        }


        private List<MerchantSelectMappingItemEntity> InstanceInsertMerchantSelectMappingItems(MerchantSelectTemplateRequestModel model, int tempalteID)
        {
            List<MerchantSelectMappingItemEntity> insertItems = new List<MerchantSelectMappingItemEntity>();

            if (model.ExcludeMerchantIDs.IsNotEmptyNull())
            {
                foreach (var merchantID in model.ExcludeMerchantIDs)
                {
                    insertItems.Add(new MerchantSelectMappingItemEntity
                    {
                        MerchantID = merchantID,
                        TemplateID = tempalteID,
                        Type = MerchantSelectMappingItemTypeEnum.ExcludeMerchantID.ToInteger()
                    });
                }
            }

            //如果是指定代理的方式來選擇
            if (model.ParentMerchantID.NotNullNotZeroNotMinus())
            {
                insertItems.Add(new MerchantSelectMappingItemEntity
                {
                    MerchantID = model.ParentMerchantID.Value,
                    TemplateID = tempalteID,
                    Type = MerchantSelectMappingItemTypeEnum.ParentMerchantID.ToInteger()
                });

                var avialbleResult = GetAvailableMerchantIDsByParentAgent(model.ParentMerchantID.Value, model.ExcludeMerchantIDs);
                model.IncludeMerchantIDs = avialbleResult.AvailableMerchantIDs;
            }

            //如果DB有手動塞值。會有這種資料
            var existAlwaysMerchantItems = MerchantSelectMappingItemRepository.GetByFilter(new QueryMerchantSelectMappingItemRequestViewModel
            {
                TemplateIDs = tempalteID.ToListCollection(),
                Types = MerchantSelectMappingItemTypeEnum.AlwaysIncludeMerchantID.ToInteger().ToListCollection()
            });

            if (existAlwaysMerchantItems.IsNotEmptyNull())
            {
                model.IncludeMerchantIDs = model.IncludeMerchantIDs ?? new List<int>();

                foreach (var item in existAlwaysMerchantItems)
                {
                    insertItems.Add(new MerchantSelectMappingItemEntity
                    {
                        MerchantID = item.MerchantID,
                        TemplateID = tempalteID,
                        Type = item.Type
                    });

                    model.IncludeMerchantIDs.Add(item.MerchantID);
                }
            }

            //IncludeMerchantIDs的操作需放最後 (因為前面的邏輯會assign值)
            if (model.IncludeMerchantIDs.IsNotEmptyNull())
            {
                model.IncludeMerchantIDs = model.IncludeMerchantIDs.Distinct().ToList();

                foreach (var merchantID in model.IncludeMerchantIDs)
                {
                    insertItems.Add(new MerchantSelectMappingItemEntity
                    {
                        MerchantID = merchantID,
                        TemplateID = tempalteID,
                        Type = MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger()
                    });
                }
            }

            //額外的防呆檢查
            if(model.CategoryID == (int)DataCategoryEnum.MerchantSelect_Event && !model.ParentMerchantID.HasValue)
            {
                var merchantIDs = insertItems.Select(x => x.MerchantID).ToList();

                string selectColumns = " MerchantID , Currency ";

                var queryMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    MerchantIDs = merchantIDs

                } , selectColumns);

                List<string> distinctCurrency = queryMerchants.Select(x => x.Currency).Distinct().ToList();

                if(distinctCurrency.Count() >= 2)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "活動類選擇的商戶幣別需一致");
                }
            }

          
            return insertItems;
        }

        public void ResetCacheByTemplateID(int templateID)
        {
            var query = GetByTemplateID(templateID);
            ResetCacheWhenTemplateMerchantChanged(query.CategoryID, query.IncludeMerchantIDs, new List<MerchantSelectMappingItemEntity>());
        }

        private void ResetCacheWhenTemplateMerchantChanged(int templateCategory , List<int> originalMerchantIDs, List<MerchantSelectMappingItemEntity> insertItems)
        {
            var afterMerchantIDs = insertItems
                .Where(x => x.Type == MerchantSelectMappingItemTypeEnum.IncludeMerchantID.ToInteger())
                .Select(x => x.MerchantID)
                .ToList();

            ResetCacheWhenTemplateMerchantChanged(templateCategory , originalMerchantIDs, afterMerchantIDs);
        }


        private void ResetCacheWhenTemplateMerchantChanged(int templateCategory, List<int> originalMerchantIDs, List<int> afterMerchantIDs)
        {
            List<int> affectMerchantIDs = new List<int>();

            if (originalMerchantIDs.IsNotEmptyNull())
            {
                affectMerchantIDs.AddRange(originalMerchantIDs);
            }

            if (afterMerchantIDs.IsNotEmptyNull())
            {
                affectMerchantIDs.AddRange(afterMerchantIDs);
            }

            affectMerchantIDs = affectMerchantIDs.Distinct().ToList();

            //[活動類] 需要清除以下cache
            if(templateCategory == (int)DataCategoryEnum.MerchantSelect_Event)
            {
                //這邊迴圈筆數破千 會變很慢
                Task.Factory.StartNew(() =>
                {
                    foreach (var merchantID in affectMerchantIDs)
                    {
                        CacheRemoveHelper.RemoveCache_WhenMerchantSelectTemplateMappingMerchantChanged(merchantID);
                    }
                });                
            }
        }



    }
}
