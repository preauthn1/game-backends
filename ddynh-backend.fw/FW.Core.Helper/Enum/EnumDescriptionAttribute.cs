using System;

namespace FW.Core.Helper.Enum
{
    public class EnumDescriptionAttribute : Attribute
    {
        private string _displayName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="displayName"></param>
        public EnumDescriptionAttribute(string displayName)
        {
            this._displayName = displayName;
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName
        {
            get
            {
                return this._displayName;
            }
        }


    }

    public class Disabled : Attribute
    {
        private bool _disabled ;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="displayName"></param>
        public Disabled(bool disabled)
        {
            this._disabled = disabled;
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public bool IsDisplay
        {
            get
            {
                return this._disabled;
            }
        }
    }
}
