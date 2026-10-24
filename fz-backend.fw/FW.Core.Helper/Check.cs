using System;


namespace FW.Core.Helper
{
    public static  class Check
    {
        /// <summary>
        /// 检查参数 不能为空
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static string NotEmpty(string value, string parameterName)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }


        /// <summary>
        /// 检查参数不能为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static T NotEmpty<T>(T value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// 检查参数不能为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static T? NotNull<T>(T? value, string parameterName)
            where T : struct

        {
            if (!value.HasValue)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

    }
}
