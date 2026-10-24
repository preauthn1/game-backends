using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using example.invalid;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.ViewModels.Base;
using target.Utility;
using target.Utility.Helper;
using target.Utility.IOC;

namespace target.API.ExternalVendor.Controllers
{
    [ApiController]
    public abstract class BaseGameVendorController : BaseController
    {

        public IGameVendorSingleWalletService GameVendorSingleWalletService { get; set; }

        protected abstract GameVendorTypeEnum _gameVendor { get; }




    }
}
