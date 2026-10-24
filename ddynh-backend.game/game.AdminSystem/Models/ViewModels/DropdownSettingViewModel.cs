using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;

namespace game.AdminSystem
{
    public class DropdownPartialSettingViewModel
    {

        public DropdownPartialSettingViewModel()
        {

        }


        public DropdownPartialSettingViewModel(FirstOptionValueEnum setting)
        {
            SetFirstOptionValueByType(setting);
        }


        public DropdownPartialSettingViewModel(FirstOptionValueEnum setting , object defaultValue)
        {
            SetFirstOptionValueByType(setting);
            DefaultSelectValue = defaultValue.ToString();
        }


        public DropdownPartialSettingViewModel(string defaultValue)
        {
            DefaultSelectValue = defaultValue;
        }

        

        private void SetFirstOptionValueByType(FirstOptionValueEnum setting)
        {
            if (setting == FirstOptionValueEnum.DefaultSelectAll)
            {
                FirstOption = new FirstOptionSettingModel
                {
                    Text = "--不限--",
                    Value = ""
                };
            }
            else if (setting == FirstOptionValueEnum.MustSelect)
            {
                FirstOption = new FirstOptionSettingModel
                {
                    Text = "--请选择--",
                    Value = ""
                };
            }
        }

        public bool IsLockSelect { get; set; }


        public string DOM_ElementName { get; set; }


        /// <summary>
        /// 若有指定 会选择指定的选项
        /// </summary>
        public string DefaultSelectValue { get; set; }



        public FirstOptionSettingModel FirstOption { get; set; }

    }



    public enum FirstOptionValueEnum
    {
        /// <summary>
        /// 无预设选项 或 自行指定
        /// </summary>
        None,

        /// <summary>
        /// 预设选单为: --不限--
        /// </summary>
        DefaultSelectAll,
        
        /// <summary>
        /// 预设选单为: --请选择--
        /// </summary>
        MustSelect,
    }


    public class FirstOptionSettingModel
    {
        public string Text { get; set; }

        public string Value { get; set; }
    }



}