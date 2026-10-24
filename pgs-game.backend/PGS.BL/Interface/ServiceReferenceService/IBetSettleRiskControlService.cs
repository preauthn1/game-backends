using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IBetSettleRiskControlService
    {

        RiskControlResultModel CheckRiskControl(RiskControlCheckParameterModel model);


        /// <summary>
        /// 這個主要是給MSTest測試用的
        /// </summary>
        RiskControlResultModel CheckRiskControlByEntity(RiskControlConfigEntity eachRiskControl, RiskControlCheckParameterModel model);

    }
}
