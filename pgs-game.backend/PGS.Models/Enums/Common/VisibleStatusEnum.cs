using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CultureText;


namespace target.Models.Enums
{
    public enum VisibleStatusEnum
    {
        Disable = 0,

        Visible = 1,

        OnlyForPreview = 2,

        PermanentDelete = 999,
    }
}
