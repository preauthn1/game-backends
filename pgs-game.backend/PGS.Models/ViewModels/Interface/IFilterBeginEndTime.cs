using System;

namespace target.Models.ViewModels
{
    public interface IFilterBeginEndTime
    {
         DateTime? Begin { get; set; }

         DateTime? End { get; set; }
    }

}
