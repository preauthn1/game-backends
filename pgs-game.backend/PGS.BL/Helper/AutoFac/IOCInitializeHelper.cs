using Autofac;
using target.BL.Interface;
using target.BL.Services;
using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.IOC
{
    public class IOCInitializeHelper
    {       
        public static void InitializeIOC(List<string> csProjectNames)
        {
            // 容器建立者
            var builder = new ContainerBuilder();
            
            
            foreach(string name in csProjectNames)
            {
                //傳入的專案名稱 e.g. game.Service
                var service = Assembly.Load(name);
                builder.RegisterAssemblyTypes(service).AsImplementedInterfaces();
            }

            builder.RegisterType<PGSIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.target);
            builder.RegisterType<PGSIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Slot88);
            builder.RegisterType<FunkyIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Funky);
            builder.RegisterType<CrazyFishingIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.CrazyFishing);
            builder.RegisterType<AGGIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.AGG);
            builder.RegisterType<AGGLiveIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.AGGLive);
            builder.RegisterType<AGGFishIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.AGGFish);
            builder.RegisterType<JiliIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Jili);
            builder.RegisterType<GPIIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.GPI);
            builder.RegisterType<MarioIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Mario);
            builder.RegisterType<YesGameIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.YesGame);
            builder.RegisterType<ATIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.AT);
            builder.RegisterType<ATGamesIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.ATGames);
            builder.RegisterType<GalaxyIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Galaxy);
            builder.RegisterType<AGinIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.AGin);
            builder.RegisterType<ClotPlayIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.ClotPlay);
            builder.RegisterType<RGIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.RG);
            builder.RegisterType<VPlusIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.SlotMania);
            builder.RegisterType<FumaIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.Fuma);
            builder.RegisterType<EFGIntegrationService>().Keyed<IGameVendorIntegrationService>((int)GameVendorTypeEnum.EFG);

            //建立容器
            var container = builder.Build();
            IOCHelper.SetContainer(container);

        }        
    }
}
