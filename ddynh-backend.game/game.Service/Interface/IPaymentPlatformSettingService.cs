using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Payment;
using game.Entity.ViewModels;
using game.Infrastructure;

namespace game.Service
{
    public interface IPaymentPlatformSettingService
    {
        PlatformPayTypeSetting GetById(int platformSettingId);

        List<PayTypeCategoryMapping> GetAllPayTypeCategoryMapping();

        APIResult UpdatePlatformSettingPriority(UpdatePlatformSettingPriorityRequestModel model);

    }
}
