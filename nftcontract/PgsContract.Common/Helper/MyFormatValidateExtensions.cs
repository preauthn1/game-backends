using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using target.Utility.Extensions;
using System.Linq.Expressions;
using System.Reflection;
using target.Models.Models;

namespace target.Utility.Extensions
{
    public static class MyFormatValidateExtensions
    {
        public static RuleModel<T, T2> RuleFor<T, T2>(this T source, Expression<Func<T, T2>> selector) where T : class
        {
            //這個擴充方法 禁止傳入null物件
            if (source == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ModelIsNull);

            var name = ((MemberExpression)selector.Body).Member?.Name;
            return new RuleModel<T, T2>
            {
                PropertyValue = (T2)source.GetType().GetProperty(name).GetValue(source),
                PropertyName = name
            };
        }
     
        public static RuleModel<T, DateTime?> Rule_NotNullDateTime<T>(this RuleModel<T, DateTime?> model)
        {
            if (!model.PropertyValue.HasValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRequired, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, DateTime> Rule_NotEmptyDateTime<T>(this RuleModel<T, DateTime> model)
        {
            if(model.PropertyValue == new DateTime())
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRequired, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, DateTime> Rule_StartTimeMustSmallThanEndTime<T>(this RuleModel<T, DateTime> model, DateTime endTime)
        {
            if (model.PropertyValue > endTime)
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);

            return model;
        }

        public static RuleModel<T, DateTime?> Rule_StartTimeMustSmallThanEndTime<T>(this RuleModel<T, DateTime?> model, DateTime endTime)
        {
            if (model.PropertyValue > endTime)
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);

            return model;
        }

        public static RuleModel<T, DateTime> Rule_StartTimeEndTimeMinutesRange<T>(this RuleModel<T, DateTime> model, DateTime endTime, int minutesRange)
        {
            if (model.PropertyValue > endTime)
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);

            if ((endTime - model.PropertyValue).TotalMinutes > minutesRange)
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);

            return model;
        }

        public static RuleModel<T, DateTime> Rule_StartTimeEndTimeRange<T>(this RuleModel<T, DateTime> model , DateTime endTime , int daysRange)
        {
            if (model.PropertyValue > endTime)
                throw new TerproException(ApiErrorCode.InvalidParameter_StartTimeMustSmallThanEndTime);

            if ((endTime - model.PropertyValue).TotalDays > daysRange)
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);

            return model;
        }

        public static RuleModel<T, string> Rule_NotEmptyText<T>(this RuleModel<T, string> model)
        {
            if (string.IsNullOrWhiteSpace(model.PropertyValue))
                throw new TerproException(ApiErrorCode.InvalidParameter_TextEmpty , new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, T2> ThrowExceptionWithColumnName<T, T2>(this RuleModel<T, T2> model , ApiErrorCode code)
        {
            throw new TerproException(code, new ErrorDetailModel(model.PropertyName));
        }


        public static RuleModel<T, string> Rule_IsValidGuid<T>(this RuleModel<T, string> model)
        {
            if (string.IsNullOrWhiteSpace(model.PropertyValue))
                throw new TerproException(ApiErrorCode.InvalidParameter_TextEmpty, new ErrorDetailModel(model.PropertyName));

            if (!model.PropertyValue.IsGuid())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, string> Rule_IsValidHashAddress<T>(this RuleModel<T, string> model)
        {
            if (string.IsNullOrWhiteSpace(model.PropertyValue))
                throw new TerproException(ApiErrorCode.InvalidParameter_TextEmpty, new ErrorDetailModel(model.PropertyName));

            if (!model.PropertyValue.StartsWith("0x") || model.PropertyValue.Length < 20)
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, List<int>> Rule_IsNotEmptyCollection<T>(this RuleModel<T, List<int>> model)
        {
            if (model.PropertyValue.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_CollectionIsEmptyNull, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, List<int>> Rule_MustSingleSelected<T>(this RuleModel<T, List<int>> model)
        {
            if (model.PropertyValue.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.InvalidParameter_IsSupportSingleSelectOnly, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, int?> Rule_Rule_IsNotNull<T>(this RuleModel<T, int?> model)
        {
            if (model.PropertyValue == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowNull, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, string> Rule_MinLengthEnough<T>(this RuleModel<T, string> model, int minLength)
        {
            Rule_NotEmptyText(model);

            if(model.PropertyValue.Length < minLength)
                throw new TerproException(ApiErrorCode.InvalidParameter_TextLength, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, string> Rule_MaxLengthOK<T>(this RuleModel<T, string> model, int maxLength)
        {
            model.PropertyValue = model.PropertyValue ?? string.Empty;

            if (model.PropertyValue.Length > maxLength)
                throw new TerproException(ApiErrorCode.InvalidParameter_TextLength, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, int> Rule_NotZero<T>(this RuleModel<T, int> model) where T : class
        {
            if(model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, int> Rule_NotZeroNotMinus<T>(this RuleModel<T, int> model) where T : class
        {
            if (model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, int> Rule_NotEqual<T>(this RuleModel<T, int> model , int targetValue) where T : class
        {
            if (model.PropertyValue == targetValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_SourceValueMustNotEqualTargetValue);

            return model;
        }

        public static RuleModel<T, long> Rule_NotZeroNotMinus<T>(this RuleModel<T, long> model) where T : class
        {
            if (model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, long> Rule_IsValidTradeID<T>(this RuleModel<T, long> model) where T : class
        {
            if (model.PropertyValue < 2000000000000000)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooSmall);

            return model;
        }

        public static RuleModel<T, decimal> Rule_NotZero<T>(this RuleModel<T, decimal> model) where T : class
        {
            if (model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal> Rule_IsIntegerValue<T>(this RuleModel<T, decimal> model) where T : class
        {
            if (!model.PropertyValue.IsIntegerValue())
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowFloat, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal> Rule_NotTooSmall<T>(this RuleModel<T, decimal> model , decimal minValue) where T : class
        {
            if (minValue > model.PropertyValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooSmall, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal> Rule_NotTooLarge<T>(this RuleModel<T, decimal> model, decimal maxValue) where T : class
        {
            if (maxValue < model.PropertyValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooLarge, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, int> Rule_NotTooLarge<T>(this RuleModel<T, int> model, int maxValue) where T : class
        {
            if (maxValue < model.PropertyValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooLarge, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal> Rule_NotZeroNotMinus<T>(this RuleModel<T, decimal> model) where T : class
        {
            if (model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal?> Rule_NotZeroNotMinus<T>(this RuleModel<T, decimal?> model) where T : class
        {
            if (!model.PropertyValue.HasValue)
                return model;

            if (model.PropertyValue == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(model.PropertyName));

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal?> Rule_NotMinus<T>(this RuleModel<T, decimal?> model) where T : class
        {
            if (!model.PropertyValue.HasValue)
                return model;

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal?> Rule_NotNull<T>(this RuleModel<T, decimal?> model) where T : class
        {
            if (!model.PropertyValue.HasValue)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowNull, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, decimal> Rule_NotMinus<T>(this RuleModel<T, decimal> model) where T : class
        {
            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, int?> Rule_NotMinus<T>(this RuleModel<T, int?> model) where T : class
        {
            if (!model.PropertyValue.HasValue)
                return model;

            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }

        public static RuleModel<T, int> Rule_NotMinus<T>(this RuleModel<T, int> model) where T : class
        {
            if (model.PropertyValue < 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, new ErrorDetailModel(model.PropertyName));

            return model;
        }


        public static RuleModel<T, List<T2>> Rule_NotEmptyNull<T, T2>(this RuleModel<T, List<T2>> model)
            where T : class
        {
            if (model.PropertyValue == null || model.PropertyValue.Count() == 0)
                throw new TerproException(ApiErrorCode.InvalidParameter_CollectionIsEmptyNull, new ErrorDetailModel(model.PropertyName));

            return model;
        }



        public static RuleModel<T, T2> Rule_ModelNotNull<T , T2>(this RuleModel<T, T2> model) 
            where T : class 
            where T2 : class
        {
            if (model.PropertyValue == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ModelIsNull, new ErrorDetailModel(model.PropertyName));

            return model;
        }

    }





}
