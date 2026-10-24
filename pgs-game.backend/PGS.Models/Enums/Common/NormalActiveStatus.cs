using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
    /// <summary>
    /// 如果狀態只有 開啟/關閉/永久隱藏 這幾種
    /// 請統一使用這個enum
    /// </summary>
    public enum NormalActiveStatus
    {
        Disable = 0,

        Enable = 1,

        /// <summary>
        /// 禁止被編輯
        /// (只有管理員層級可以顯示這種選項)
        /// </summary>
        DisableAndForbiddenEdit = 88,

        PermanentDelete = 999,
    }

    /// <summary>
    /// 如果狀態只有 開啟/關閉/永久隱藏 這幾種
    /// 請統一使用這個enum
    /// </summary>
    public enum NormalYesOrNoEnum
    {
        Yes = 1,

        No = 0,
    }


    public enum NormalYesOrNoOrNorSelectEnum
    {
        /// <summary>
        /// 請選擇
        /// </summary>
        NotSelect = -1,

        Yes = 1,

        No = 0,
    }
}
