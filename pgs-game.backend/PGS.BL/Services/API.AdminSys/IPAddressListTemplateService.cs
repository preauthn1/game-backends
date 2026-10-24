using Lck.Utility.Extensions.Helper;
using MongoDB.Driver.Linq;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace target.BL.Services
{
    public class IPAddressListTemplateService : BaseAdminSysService, IIPAddressListTemplateService
    {

        public IPAddressListTemplateRepository IPAddressListTemplateRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public IMChannelCustomizedSettingRepository IMChannelCustomizedSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        private const int BIG_ASSIGN_COUNT = 50; //超過50個商戶認定為大代理


        public PagedList<IPAddressListTemplateViewModel> GetByPagingFilter(int userID, QueryIPAddressListTemplateApiRequestParameterModel filter)
        {
            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (filter.SpecialFilterType.HasValue)
            {
                if (filter.SpecialFilterType == (int)SpecialFilterTypeOfIPAddressTemplate.AssignMerchantOnly)
                {
                    filter.IsAssignMerchantOnly = true;
                }
            }

            bool isNotInternalUsers = !currentUser.IsSuperAdminOrg && !HasTargetPermission(userID, "Special_PGS_CustomerService");
            bool isNotSuperRootAgent = !HasTargetPermission(userID, "Statistics_EazyQuery_ForPartner") && !HasTargetPermission(userID, "Special_Account_SearchAccountByMultiMerchantAssignMode");

            //一般用戶查不到*星號的
            if (isNotInternalUsers && isNotSuperRootAgent)
            {
                filter.IPAddressNotContainsThisKeyword = "*";
            }

            FilterMerchantIdByUserAccountType(currentUser, filter);

            if (HasTargetPermission(userID, "Special_PGS_CustomerService"))
            {
                if (filter.AssignMerchantID.NotNullNotZeroNotMinus())
                {
                    var queryMerchant = MerchantRepository.GetById(filter.AssignMerchantID.Value);

                    if (queryMerchant != null && queryMerchant.Type == MerchantTypeEnum.Plaform_DevelopEnvironment.ToInteger())
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"{filter.AssignMerchantID} 是測試環境。不需要加白名單。直接告知客戶已完成即可");
                    }
                }
            }

            var query = IPAddressListTemplateRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<IPAddressListTemplateEntity, IPAddressListTemplateViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<IPAddressListTemplateViewModel>();
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public CRUDResultModel Create(int userID, IPAddressListTemplateCreateRequestModel model)
        {
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();
            model.RuleFor(x => x.IpAddressWithLinebreak).Rule_NotEmptyText();

            var newEntity = model.ReturnInjecteObject<IPAddressListTemplateEntity>();

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            newEntity.IPAddressList = ConvertFormInputIPAddressToCommaFormat(model.IpAddressWithLinebreak);
            newEntity.CreateTime = DateTimeHelper.GetCurrentTime();
            newEntity.UpdateTime = DateTimeHelper.GetCurrentTime();

            var resultId = IPAddressListTemplateRepository.Create(newEntity);

            return new CRUDResultModel
            {
                ResultId = resultId,
                IsSuccess = true,
                Message = "新增成功",
            };
        }


        private void CheckViewDataPermission(int userID, int id)
        {
            var queryItems = GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
            {
                IDs = id.ToListCollection()
            });

            if (queryItems.Items.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
            }
        }


        public CRUDResultModel Update(int userID, IPAddressListTemplateUpdateRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.DisplayName).Rule_NotEmptyText();

            var queryExsit = IPAddressListTemplateRepository.GetById(model.Id);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            CheckViewDataPermission(userID, model.Id);

            queryExsit.DisplayName = model.DisplayName;
            queryExsit.Memo = model.Memo;

            int maxLength = 2000;

            string updateNewIPs = ConvertFormInputIPAddressToCommaFormat(model.IpAddressWithLinebreak);

            var checkResult = CheckNeedCombineIPsOrNot(updateNewIPs, $"TemplateID:{queryExsit.Id}" , maxLength);

            updateNewIPs = checkResult.NewIPsText;

            queryExsit.IPAddressList = updateNewIPs;
            queryExsit.UpdateTime = DateTimeHelper.GetCurrentTime();

            IPAddressListTemplateRepository.Update(queryExsit);

            WriteOperateLog(userID, queryExsit.Id, queryExsit.IPAddressList, $" `{queryExsit.DisplayName}` ");

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenUpdateIPAddressTemplateID(queryExsit.Id);

            return new CRUDResultModel(true);
        }

        public CRUDResultModel AppendNewIpToSetting(int userID, IPAddressListAppendRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.IPAddress).Rule_NotEmptyText();

            CheckViewDataPermission(userID, model.Id);

            var queryExsit = IPAddressListTemplateRepository.GetById(model.Id);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            //ps. 裡面包含了格式檢查
            var newIPsCheck = AnalyzeAnyNewIPs(queryExsit.Id, model.IPAddress);
            var newIPs = newIPsCheck.NewIPs;

            if (newIPsCheck.NewIPs.IsEmptyOrNull())
                return new CRUDResultModel(true);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var assignMerchant = GetAssignMerchantCountByTemplateID(queryExsit.Id);

            //商戶超過一定筆數 先不要讓CS客服處理
            if (queryExsit.IPAddressList == "*")
            {
                if (queryExsit.IsForbiddenAppend)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("ForbiddenAppend (技术标记禁止编辑)"));
                }

                bool isNoPermission = HasTargetPermission(userID, "Special_PGS_CustomerService") || !currentUser.IsSuperAdminOrg;

                if (isNoPermission)
                {
                    if (assignMerchant.TotalCount >= 5)
                    {
                        DateTime begin_30days = DateTimeHelper.GetCurrentTime().AddDays(-1 * 30);
                        DateTime end_30days = DateTimeHelper.GetCurrentTime();

                        var queryEachMerchant = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumOnDuration(assignMerchant.MerchantIDs, begin_30days, end_30days);

                        int toalWagerCount = queryEachMerchant.Sum(x => x.TotalRound);

                        bool isWagerTooLess = toalWagerCount < 1000;

                        if (isWagerTooLess)
                        {
                            //如果30天內的注單極少。允許配置
                        }
                        else if (model.IsSkipExceptionWhenWildCard)
                        {
                            string myMsg = $"IP配置:{queryExsit.DisplayName} ({queryExsit.Id}), 因當前IP設置為*號、直接更新有風險，原定新增以下IPs將跳過: {string.Join("、", newIPs)}";
                            SendMessageToMultipleChannel(myMsg, TelegramChannelEnum.pgs_adminSys_api);
                            return new CRUDResultModel();
                        }
                        else
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, "您设定的IP配置禁止此操作。请截图此错误告知技术人员。将由技术人员做处理");
                        }
                    }
                }
            }

            queryExsit.IPAddressList += $"," + string.Join(",", newIPs);
            queryExsit.IPAddressList = ConvertFormInputIPAddressToCommaFormat(queryExsit.IPAddressList);
            queryExsit.UpdateTime = DateTimeHelper.GetCurrentTime();

            IPAddressListTemplateRepository.Update(queryExsit);

            string memoText = string.Join("、", newIPs) + (string.IsNullOrWhiteSpace(model.Memo) ? "" : $" 【{model.Memo}】");
            WriteOperateLog(userID, queryExsit.Id, model.IPAddress, memoText);

            if (assignMerchant.TotalCount > BIG_ASSIGN_COUNT)
            {
                //2025/01 移除此通知 沒啥意義 不想管
                //string myMsg2 = $"大代理 IP配置:{queryExsit.DisplayName} ({queryExsit.Id}), 本次新增以下IPs: {string.Join("、", newIPs)}";
                //SendMessageToSlack(myMsg2, SlackChannelEnum.n_error_adminsys);
            }

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenUpdateIPAddressTemplateID(queryExsit.Id);

            return new CRUDResultModel(true);
        }


        public WhiteListNewIPsCheckResultModel AnalyzeAnyNewIPs(int ipTemplateID, string appendIPs)
        {
            var appendNewIPs = AppHelper.AnalyzeTextFromRawTextBySpaceChar(appendIPs);

            foreach (var ip in appendNewIPs)
            {
                if (!RegexRuleHelper.IsIPAddressFormat(ip))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_NotIPAddressFormat, new ErrorDetailModel($"{ip}"));
                }
            }

            var queryExsit = IPAddressListTemplateRepository.GetById(ipTemplateID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var existIPAddress = AppHelper.AnalyzeTextFromRawTextBySpaceChar(queryExsit.IPAddressList);

            //如果IP都存在
            if (appendNewIPs.All(newIP => existIPAddress.Any(ip => ip == newIP)))
            {
                return new WhiteListNewIPsCheckResultModel
                {
                    IsBigAgent = false,
                    NewIPs = new List<string>(),
                };
            }
            else
            {
                List<string> newIPs = appendNewIPs.Where(newIP => !existIPAddress.Any(ip => ip == newIP)).ToList();

                var assignMerchantCount = GetAssignMerchantCountByTemplateID(ipTemplateID).TotalCount;

                return new WhiteListNewIPsCheckResultModel
                {
                    IsBigAgent = assignMerchantCount > BIG_ASSIGN_COUNT,
                    NewIPs = newIPs
                };
            }
        }

        public void SetWhiteListIPsForSingleMerchant(int userID, int merchantID, List<string> newIPs)
        {
            CheckViewMerchantPermission(userID, merchantID);

            string myIPsText = string.Join(",", newIPs);

            var settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_CustomizedIPs;

            var queryExist = MerchantCustomizedSettingRepository.GetByUniqueKey(merchantID, settingKey);

            var combineCheck = CheckNeedCombineIPsOrNot(myIPsText, merchantID.ToString(), maxlength: 200);

            if(combineCheck.IsNeedCombine)
            {
                myIPsText = combineCheck.NewIPsText;
            }

            var now = DateTimeHelper.GetCurrentTime();

            if (queryExist == null)
            {
                MerchantCustomizedSettingRepository.Create(new MerchantCustomizedSettingEntity
                {
                    MerchantID = merchantID,
                    CreateTime = now,
                    SettingValue = myIPsText,
                    CustomizedSettingKey = settingKey.ToInteger()
                });
            }
            else
            {
                string updateIPsText = queryExist.SettingValue + "," + myIPsText;
                queryExist.SettingValue = ConvertFormInputIPAddressToCommaFormat(updateIPsText);

                MerchantCustomizedSettingRepository.Update(queryExist);
            }
        }




        private (bool IsNeedCombine, string NewIPsText) CheckNeedCombineIPsOrNot(string myIPsText, string onwerName , int maxlength = 1000)
        {
            if (myIPsText.CountOrZero() < maxlength)
            {
                return (false, myIPsText);
            }

            List<string> ipList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(myIPsText);

            List<string> combineIPs = IPHelper.AnalyzeIPsAndCombineOne(ipList);

            var combineIPsText = string.Join(",", combineIPs);

            string myMsg = $"{onwerName} 商戶IPs過多。需人工處理(無法精簡)" + Environment.NewLine + myIPsText;

            if (ipList.Count() > combineIPs.Count())
            {
                if (combineIPsText.CountOrZero() >= maxlength)
                {
                    myMsg = $"{onwerName} 商戶IPs過多。縮短成以下仍超過字數" + Environment.NewLine + combineIPsText;
                }
                else
                {
                    myMsg = $"{onwerName} 商戶IPs過多。建議濃縮成以下配置" + Environment.NewLine + combineIPsText;
                }
            }

            MyTelegramUtility.SendMessage(new TelegramMessageParameter
            {
                Message = myMsg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                TelegramExtraSetting = new TelegramExtraSetting
                {
                    IsSkipWhenMSTest = true,
                }
            });

            if(combineIPsText.CountOrZero() >= maxlength)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_TextLength, new ErrorDetailModel($"IPsText {maxlength} length"));
            }

            return (true, combineIPsText);
        }








        public CRUDResultModel AppendMultipleIPsToSetting(int userID, IPAddressListMultipleAppendRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.MultipleIPAddress).Rule_NotEmptyText();

            //重構後 合併了
            var result = AppendNewIpToSetting(userID, new IPAddressListAppendRequestModel
            {
                IPAddress = model.MultipleIPAddress,
                Id = model.Id,
                Memo = model.Memo,
                IsSkipExceptionWhenWildCard = model.IsSkipInsertWhenWildCard,
            });

            return result;
        }



        private void WriteOperateLog(int userID, int targetID, string newValue, string logExtraInfo = "")
        {
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateIPAddressTemplate, targetID);
            log.DebugInfo = newValue;
            log.Memo = logExtraInfo;

            WriteOperateLogAndNotifyCsOperate(userID, log);
        }


        public CRUDResultModel UpdateMultipleAssignMerchant(int userID, IPAddressListTemplateUpdateAssignMerchantIDRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.MerchantIDs).Rule_IsNotEmptyCollection();

            foreach (var merchantID in model.MerchantIDs)
            {
                var clone = model.ReturnInjecteObject<IPAddressListTemplateUpdateAssignMerchantIDRequestModel>();
                clone.MerchantID = merchantID;
                clone.MerchantIDs = new List<int>();

                UpdateAssignMerchant(userID, clone);
            }

            return new CRUDResultModel(true);
        }


        public CRUDResultModel UpdateAssignMerchant(int userID, IPAddressListTemplateUpdateAssignMerchantIDRequestModel model)
        {
            model.RuleFor(x => x.Id).Rule_NotZero().Rule_NotMinus();
            model.RuleFor(x => x.MerchantID).Rule_NotZero().Rule_NotMinus();

            var queryExsit = IPAddressListTemplateRepository.GetById(model.Id);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            CheckSelectMerchantsIsValid(queryExsit.Id, model.MerchantID.ToListCollection());

            //delete代表 將資料清空成不指派templateID
            if (model.IsDelete)
            {
                model.Id = 0;
            }

            string updateValue = model.Id.ToString();

            int settingKey = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger();
            MerchantCustomizedSettingRepository.CreateOrUpdate(model.MerchantID, settingKey, updateValue);

            //清除cache
            CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting(model.MerchantID, settingKey);
            CacheRemoveHelper.RemoveCache_WhenUpdateMerchantSetting_WhenAllDone(model.MerchantID);
            return new CRUDResultModel(true);
        }


        public CRUDResultModel Delete(int userID, int targetID)
        {
            var queryExsit = IPAddressListTemplateRepository.GetById(targetID);

            if (queryExsit == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryViewModel = GetByPagingFilter(userID, new QueryIPAddressListTemplateApiRequestParameterModel
            {
                IDs = targetID.ToListCollection(),
            })
                .Items
                .FirstOrDefault();

            if (queryViewModel == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            if (queryViewModel.AssignMerchantCount > 0)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel($"AssignMerchantCount"));
            }

            IPAddressListTemplateRepository.Delete(targetID);

            var log = InstanceOperateLog(userID, UserOperateTypeEnum.UpdateIPAddressTemplate, targetID);
            log.Memo = "DELETE";

            WriteOperateLog(log);

            return new CRUDResultModel(true);
        }

        private string ConvertFormInputIPAddressToCommaFormat(string inputText)
        {
            List<string> lines = inputText.Split(new[] { Environment.NewLine, "\t\n", "\n", "," }, StringSplitOptions.None)
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                  .Select(x => x.Trim().Replace(",", "").Replace("-", "").Trim())
                  .OrderBy(x =>
                  {
                      string firstNumber = x.Split(".")[0];
                      if (firstNumber.IsNumeric())
                      {
                          return Int32.Parse(firstNumber);
                      }
                      else
                      {
                          return 999;
                      }
                  })
                  .ThenBy(ip => ip)
                  .Distinct()
                  .ToList();

            foreach (string eachIP in lines)
            {
                if (eachIP == "*")
                {
                    continue;
                }

                if (eachIP.EndsWith("*"))
                {
                    continue;
                }

                if (!RegexRuleHelper.IsIPAddressFormat(eachIP))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_NotIPAddressFormat, new ErrorDetailModel($"{eachIP}"));
                }
            }

            //如果有插入至少一個IP了。則移除星號
            if (lines.Count() >= 1 && lines.Any(x => RegexRuleHelper.IsIPAddressFormat(x)))
            {
                lines = lines.Where(x => x != "*").ToList();
            }

            string finalResult = string.Join(",", lines);
            return finalResult;
        }


        private void CheckSelectMerchantsIsValid(int templateID, List<int> merchantIDs)
        {
            if (merchantIDs.IsEmptyOrNull())
                return;

            var settings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger().ToListCollection(),
                MerchantIDs = merchantIDs
            });

            foreach (var eachMerchantSettings in settings)
            {
                var parse = new SystemConfigSettingParseModel(eachMerchantSettings.CustomizedSettingKey, eachMerchantSettings.SettingValue);

                if (parse.SettingValueOfInt != templateID && parse.SettingValueOfInt != 0)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"商戶:{eachMerchantSettings.MerchantID} 已勾選其他組IP設置: {eachMerchantSettings.SettingValue}。需先取消勾選、禁止直接更改。 ");
                }
            }
        }



        private void SetExtraProperty(List<IPAddressListTemplateViewModel> query)
        {
            if (query.IsEmptyOrNull())
                return;

            List<string> ipTemplateIDs = query.Select(x => x.Id.ToString()).ToList();

            var settings = GetAssignMerchantByTemplateIDs(query.Select(x => x.Id).ToList());

            List<int> merchantIDs = settings.Select(x => x.MerchantID).Distinct().ToList();

            var queryIMChannel = IMChannelRepository.GetMerchantIMChannel(merchantIDs);

            var queryDefaultIPTemplateAssign = IMChannelCustomizedSettingRepository.GetByFilter(new QueryIMChannelCustomizedSettingRequestViewModel
            {
                SettingKeys = new List<int>
                {
                    IMChannelCustomizedSettingKeyEnum.ReferIPAddressTemplateID.ToInteger(),
                },
                SettinValueMatchList = query.Select(x => x.Id.ToString()).ToList()
            });

            bool isQuerySingle = query.Count() == 1;

            foreach (var item in query)
            {
                var matchMerchants = settings.Where(x => x.SettingValue == item.Id.ToString()).OrderBy(x => x.MerchantID).ToList();
                item.AssignMerchantCount = matchMerchants.Count();
                item.AssignMerchantIDs = matchMerchants.Select(x => x.MerchantID).ToList();

                item.IMChannels = queryIMChannel
                    .Where(z => item.AssignMerchantIDs.Contains(z.MerchantID))
                    .Where(x => !string.IsNullOrWhiteSpace(x.IMChannelName))
                    .Select(z => z.IMChannelName)
                    .Distinct()
                    .ToList();

                item.AssignDefaultIPTemplateCount = queryDefaultIPTemplateAssign.Where(x => x.SettingValue == item.Id.ToString()).Count();

                //單筆搜尋會查更詳細的資訊
                if (isQuerySingle)
                {
                    var queryMerchants = DbValueCacheHelper.GetMerchants(item.AssignMerchantIDs);
                    item.MerchantNameMapping = queryMerchants.ToDictionary(x => x.MerchantID, x => $"{x.MerchantID}｜{x.Name}");
                }

                if (!string.IsNullOrWhiteSpace(item.IPAddressList))
                {
                    var splitAllIP = item.IPAddressList.Split(",");

                    StringBuilder sb = new StringBuilder();

                    foreach (string eachIP in splitAllIP)
                    {
                        sb.AppendLine(eachIP.Trim());
                    }

                    item.IPAddressWithLinebreak = sb.ToString();
                    item.IPAddressCount = splitAllIP.Count();
                }

                item.IPAddressForDisplay = item.IPAddressList.Replace(",", "｜");
            }
        }

        private List<MerchantCustomizedSettingEntity> GetAssignMerchantByTemplateIDs(List<int> ipTemplateIDs)
        {
            List<string> ipTemplateIDTexts = ipTemplateIDs.Select(x => x.ToString()).ToList();

            string selectColumns = " MerchantID , SettingValue ";

            var settings = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger().ToListCollection(),
                MultipleSettingValueMatch = ipTemplateIDTexts,
                IsNotDeleteMerchant = true,

            }, selectColumns);

            return settings;
        }


        private (int TotalCount , List<int> MerchantIDs) GetAssignMerchantCountByTemplateID(int ipTemplateID)
        {
            var query = MerchantCustomizedSettingRepository.GetByFilter(new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting.ToInteger().ToListCollection(),
                SettingValueMatch = ipTemplateID.ToString(),
                IsNotDeleteMerchant = true,
            }, " MerchantID ");

            return (query.Count() , query.Select(x=> x.MerchantID).ToList());
        }

        private void FilterMerchantIdByUserAccountType(AdminUserCacheModelAdminUpdateViewModel currentUser, QueryIPAddressListTemplateApiRequestParameterModel filter)
        {
            if (filter.AssignMerchantID.HasValue)
            {
                CheckViewMerchantPermission(currentUser.UserID, filter.AssignMerchantID.Value);
            }

            filter.AssignMerchantIDs = filter.AssignMerchantID.ToIntegerListCollection();

            if (!currentUser.IsSuperAdminOrg)
            {
                if (currentUser.IsMultipleHierarchyMode)
                {
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.AvailableMerchantIDs;
                }
                else if (currentUser.IsHierarchyMerchantMode)
                {
                    filter.HierarchyOfTargerMerchant = currentUser.ReferenceCreditMerchant;
                }
                else
                {
                    if (filter.AssignMerchantIDs.IsEmptyOrNull())
                    {
                        filter.AssignMerchantIDs = currentUser.AvailableMerchantIDs;
                    }
                }
            }
        }

    }

}
