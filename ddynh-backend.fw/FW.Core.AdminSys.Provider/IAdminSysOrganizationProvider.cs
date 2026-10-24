using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Helper;
using System;
using System.Collections.Generic;

namespace FW.Core.AdminSys.Provider
{
    public interface IAdminSysOrganizationProvider
    {

        List<Organization> GetOrganizations(string orgId);

    }
}
