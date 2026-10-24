using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper.Dependency;

namespace target.BL.Services
{

    public partial class AnnouncementService : BaseEventService, IAnnouncementService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public AnnouncementRepository AnnouncementRepository { get; set; }

        public ContentBlockRepository ContentBlockRepository { get; set; }

        public AnnouncementReadRepository AnnouncementReadRepository { get; set; }

        public IAnnouncementGeneralService AnnouncementGeneralService { get; set; }


        public AnnouncementViewModel GetNotReadAnnoucementList(int playerID)
        {
            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);
          
            var queryAnnouncement = AnnouncementRepository.GetByFilter(new QueryAnnouncementRequestViewModel
            {
                AssignMerchantIDs = queryPlayer.MerchantID.ToListCollection(),
                IsNotExpired = true,
                NotReadMessageByPlayerID = queryPlayer.PlayerID,
                Types = new List<int>
                {
                    AnnouncementTypeEnum.PopupAdvertisement.ToInteger()
                },
                Status = NormalActiveStatus.Enable.ToInteger(),
            });

            var viewModel = AnnouncementGeneralService.ConvertToAnnoucementContentModel(queryPlayer.MerchantID, queryAnnouncement);
            return viewModel;
        }


        public void SetMessageRead(int playerID, AnnouncementReadRequestModel model)
        {
            model.RuleFor(z => z.AnnouncementID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var entity = new AnnouncementReadEntity
            {
                AnnouncementID = model.AnnouncementID,
                PlayerID = playerID,
                CreateTime = DateTimeHelper.GetCurrentTime()
            };

            AnnouncementReadRepository.CreateTryCatch(entity, isNotifyWhenException: false);
        }
         

    }
}
