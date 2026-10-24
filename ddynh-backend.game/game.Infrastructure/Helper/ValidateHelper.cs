using FW.Core.Helper;
using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Infrastructure.Extensions;

namespace game.Infrastructure
{
    public static class ValidateHelper
    {

        public static CheckResultModel CheckObjectIsNotNull<T>(T model , TargetColumnEnum targetColumn) where T: class
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if (model == null)
                return new CheckResultModel($"您所带入的参数{displayName}，物件为空 "); 
            
            return new CheckResultModel
            {
                IsSuccess = true
            };
        }
        public static CheckResultModel ChecIsNotEmptyOrNull<T>(IEnumerable<T> models, TargetColumnEnum targetColumn) where T : class
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if (models.IsEmptyOrNull())
                return new CheckResultModel($"您所带入的参数{displayName}，物件为空 ");

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }


        public static CheckResultModel CheckBeginEndIsValid(DateTime begin , DateTime end)
        {
            if(begin > end)
                return new CheckResultModel($"起始时间不得大于结束时间");
            
            if(begin == new DateTime())
                return new CheckResultModel($"请填写起始时间");

            if (end == new DateTime())
                return new CheckResultModel($"请填写结束时间");

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }

        public static CheckResultModel CheckTextLength_IsNotEmpty_IsValidLength(string value, TargetColumnEnum targetColumn , int? maxLength = null)
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if (string.IsNullOrWhiteSpace(value))
                return new CheckResultModel($"您所输入的 {displayName}栏位不得为空，此为必填项目 ");

            if(maxLength.HasValue)
            {
                if(value.Length > maxLength.Value)
                {
                    return new CheckResultModel($"您所输入的 {displayName}栏位字数不得大于{maxLength.Value} ");
                }
            }

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }


        public static CheckResultModel CheckDate_IsNotEmptyDate(DateTime value, TargetColumnEnum targetColumn)
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if (value == new DateTime())
                return new CheckResultModel($"您所输入的 {displayName}栏位不得为空，此为必填项目 ");

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }



        public static CheckResultModel CheckTextLength_IsValid(string value, TargetColumnEnum targetColumn , int? maxLength = null)
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if(string.IsNullOrWhiteSpace(value))
            {
                return new CheckResultModel
                {
                    IsSuccess = true
                };
            }
               
            if (maxLength.HasValue)
            {
                if (value.Length > maxLength.Value)
                {
                    return new CheckResultModel($"您所输入的 {displayName}栏位字数不得大于{maxLength.Value} ");
                }
            }

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }


        public static CheckResultModel CheckIsNotMinus(IEnumerable<int> values, TargetColumnEnum targetColumn)
        {
            List<CheckResultModel> results = values.Select(x => CheckIsNotMinus(x, targetColumn)).ToList();
            return GetFirstFailResultOrSuccess(results);
        }

        public static CheckResultModel CheckIsNotMinus(IEnumerable<long> values, TargetColumnEnum targetColumn)
        {
            List<CheckResultModel> results = values.Select(x => CheckIsNotMinus(Convert.ToInt32(x), targetColumn)).ToList();
            return GetFirstFailResultOrSuccess(results);
        }

        public static CheckResultModel CheckIsNotMinus(long value, TargetColumnEnum targetColumn)
        {
            return CheckIsNotMinus(Convert.ToInt32(value), targetColumn);
        }

        public static CheckResultModel CheckIsNotZeroOrMinus(long value, TargetColumnEnum targetColumn)
        {
            return CheckIsNotZeroOrMinus(Convert.ToInt32(value), targetColumn);
        }


        public static CheckResultModel CheckIsNotMinus(int value, TargetColumnEnum targetColumn)
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if (value < 0)
                return new CheckResultModel($"您所输入的 {displayName}栏位不得为负数 ");

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }



        public static CheckResultModel CheckIsNotZeroOrMinus(int value, TargetColumnEnum targetColumn)
        {
            string displayName = GetDisplayNameFromEnum(targetColumn);

            if(value <= 0)
                return new CheckResultModel($"您所输入的 {displayName}栏位不得为0，此为必填项目 ");

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }


        public static CRUDResultModel ConvertToCRUDResult(CheckResultModel result)
        {
            return new CRUDResultModel
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
            };
        }


        public static CRUDResultModel CheckAllSuccessToCRUDResult(IEnumerable<CheckResultModel> vailidResults)
        {
            var result = CheckAllSuccess(vailidResults);
            return new CRUDResultModel
            {
                IsSuccess = result.IsSuccess,
                Message = result.Message,
            };
        }


        public static CheckResultModel CheckAllSuccess(IEnumerable<CheckResultModel> vailidResults)
        {
            //這邊的邏輯是 判斷第一個有錯 就結束(即使可能有多個錯誤)
            return GetFirstFailResultOrSuccess(vailidResults);
        }

        
        public static CheckResultModel CheckAllSuccess(
            CheckResultModel arvailidResult1 ,
            CheckResultModel arvailidResult2 = null,
            CheckResultModel arvailidResult3 = null,
            CheckResultModel arvailidResult4 = null,
            CheckResultModel arvailidResult5 = null)
        {
            var list = new List<CheckResultModel>();
            var result = CheckAllSuccess(list);
            return result;
        }


        public static void CheckAllSuccessOrException(List<CheckResultModel> checkResults)
        {
            var result = CheckAllSuccess(checkResults);

            if (!result.IsSuccess)
                throw new Exception(result.Message);

            return;
        }


        private static string GetDisplayNameFromEnum(TargetColumnEnum targetColumn)
        {
            var attr = targetColumn.GetAttributeFromEnum<EnumDescriptionAttribute>();
            return attr.DisplayName;
        }


        public static CheckResultModel GetFirstFailResultOrSuccess(IEnumerable<CheckResultModel> vailidResults)
        {
            foreach (var item in vailidResults)
            {
                if (!item.IsSuccess)
                {
                    return item;
                }
            }

            return new CheckResultModel
            {
                IsSuccess = true
            };
        }

    }


    public enum TargetColumnEnum
    {
        [EnumDescription("Id")]
        Id,

        [EnumDescription("外部索引键值")]
        FK_Id,

        [EnumDescription("账号")]
        Account,

        [EnumDescription("名称")]
        Name,

        [EnumDescription("缩写名称")]
        ShortName,

        [EnumDescription("信箱")]
        Email,

        [EnumDescription("手机号")]
        MobilePhone,

        [EnumDescription("QQ账号")]
        QQ,

        [EnumDescription("微信账号")]
        Wechat,

        [EnumDescription("密码")]
        Password,

        [EnumDescription("标题")]
        Title,

        [EnumDescription("备注")]
        Memo,

        [EnumDescription("Code代码")]
        Code,

        [EnumDescription("内文")]
        Content,

        [EnumDescription("描述")]
        Description,
        
        [EnumDescription("状态")]
        Status,

        [EnumDescription("金额")]
        Amount,

        [EnumDescription("流水倍率")]
        FlowRate,

        [EnumDescription("起始日期")]
        BeginDate,

        [EnumDescription("结束日期")]
        EndDate,

        [EnumDescription("表单")]
        Others,



        [EnumDescription("权重")]
        Priority,

        [EnumDescription("平台编号")]
        PlatformID,

        [EnumDescription("渠道编号")]
        AreaID,

        [EnumDescription("表单(ID)")]
        OtherID,

        [EnumDescription("阵列物件")]
        ListObject,

        [EnumDescription("分类(类别)")]
        Type,

        [EnumDescription("分类(类别)")]
        Category,

        [EnumDescription("三方商编号")]
        GWID,
    }





}
