using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Attributes;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class AnnouncementGeneralService : BaseEventService, IAnnouncementGeneralService
    {

        public ContentBlockRepository ContentBlockRepository { get; set; }

        public AnnouncementRepository AnnouncementRepository { get; set; }


        public AnnouncementViewModel ConvertToAnnoucementContentModel(int merchantID , List<AnnouncementEntity> queryAnnouncement)
        {
            var viewModel = new AnnouncementViewModel
            {
                Items = new List<AnnouncementItemViewModel>()
            };
                
            if (queryAnnouncement.IsEmptyOrNull())
            {
                return viewModel;
            }

            var queryContent = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuidList = queryAnnouncement.Select(z => z.AnnouncementGuid).ToList()
            });

            var userLang = HeaderValueHelper.GetCurrentLanguage();
            bool isChineseLangRequest = AppHelper.IsChineseLang(userLang);

            foreach (var eachAnn in queryAnnouncement)
            {
                var firstMatch = queryContent
                    .Where(z => z.OwnerGuid == eachAnn.AnnouncementGuid)
                    .Where(z => !IsAllContentEmpty(z))
                    .OrderBy(z => AppHelper.GetLanguageOrder(z.LanguageCode, userLang))
                    .ThenBy(z => z.IsDefault ? 0 : 999)
                    .FirstOrDefault();

                if (firstMatch != null)
                {
                    var itemModel = firstMatch.ReturnInjecteObject<AnnouncementItemViewModel>();

                    itemModel.AnnouncementGuid = eachAnn.AnnouncementGuid;
                    itemModel.AnnouncementID = eachAnn.AnnouncementID;

                    viewModel.Items.Add(itemModel);
                }
            }


            SetEventExtraLinkUrl(merchantID, viewModel.Items);
            return viewModel;
        }


        /// <summary>
        /// 是否完全沒填寫內容
        /// </summary>
        private bool IsAllContentEmpty(ContentBlockEntity entity)
        {
            if (entity.Title.IsNotEmptyTextNotNull())
                return false;

            if (entity.Description.IsNotEmptyTextNotNull())
                return false;

            if (entity.ImageUrlBottom.IsNotEmptyTextNotNull())
                return false;

            if (entity.ImageUrlMiddle.IsNotEmptyTextNotNull())
                return false;

            if (entity.ImageUrlTop.IsNotEmptyTextNotNull())
                return false;

            return true;
        }

        private void SetEventExtraLinkUrl(int merchantID, List<AnnouncementItemViewModel> anns)
        {
            if (anns.IsEmptyOrNull())
                return;

            var redLinkType = (int)LinkTypeEnum.LobbyPage_RedEnvelopeDescription;

            bool isAnyRedEventLinkType = anns.Any(z => z.LinkType == redLinkType);

            List<EventEntity> queryRedEvents = new List<EventEntity>();

            if (isAnyRedEventLinkType)
            {
                var eventTypes = new List<int>
                {
                    EventTypeEnum.BetAmountDrawRedEnvelope.ToInteger()
                };

                queryRedEvents = GetEventsByMerchantAndEventType(merchantID, eventTypes, 0);
            }

            foreach (var item in anns)
            {
                if (item.LinkType == (int)LinkTypeEnum.LobbyPage_RedEnvelopeDescription)
                {
                    var redEvent = queryRedEvents.FirstOrDefault(z => z.EventType == (int)EventTypeEnum.BetAmountDrawRedEnvelope);

                    if (redEvent != null)
                    {
                        item.LinkType = LinkTypeEnum.ToInternalLink.ToInteger();
                        item.LinkReference = AppHelper.GenerateEventDescriptionPageUrl(redEvent.EventID, false, "");
                    }
                }
            }
        }



    }

}
