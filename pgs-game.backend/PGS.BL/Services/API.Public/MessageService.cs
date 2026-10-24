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

    public partial class MessageService : BaseEventService, IMessageService
    {
        public MessageRepository MessageRepository { get; set; }

        public MessageReadRepository MessageReadRepository { get; set; }

        public MessageHintRepository MessageHintRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public ContentBlockRepository ContentBlockRepository { get; set; }

        public MessageCustomizedSettingRepository MessageCustomizedSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public MessageViewModel GetPlayerMessages(int playerID)
        {
            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var timeZoneSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.Common_DefaultTimezone);

            var unreadMessages = MessageRepository.GetPlayerMessages(queryPlayer , size : 20);

            var viewModel = new MessageViewModel();
            viewModel.TimeZone = timeZoneSetting.SettingValueOfDecimal;
            viewModel.Items = new List<MessageViewModelItemViewModel>();
            
            if (unreadMessages.IsEmptyOrNull())
                return viewModel;

            viewModel.Items = unreadMessages.Select(z =>
            {
                var convert = z.ReturnInjecteObject<MessageViewModelItemViewModel>();
                convert.CreateTimeText = convert.CreateTime.AddHours((double)viewModel.TimeZone).ToString("yyyy-MM-dd HH:mm");
                return convert;

            }).ToList();

            SetViewModelExtraProperty(queryPlayer, viewModel.Items);

            return viewModel;
        }


        public void SetMessageRead(int playerID, MessageReadRequestModel model)
        {
            model.RuleFor(z => z.MessageID).Rule_NotZeroNotMinus();

            var queryMessage = MessageRepository.GetById(model.MessageID);

            if (queryMessage == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            MessageReadRepository.CreateTryCatch(new MessageReadEntity
            {
                MessageID = model.MessageID,
                PlayerID = playerID,
                CreateTime = DateTimeHelper.GetCurrentTime()
            });
        }


        public void SetMessageHintRead(int playerID,  int type)
        {
            var debug = MessageHintTypeEnum.ToQuestPage;
            MessageHintRepository.SetHintToRead(playerID, type);
        }


        public void SetViewModelExtraProperty(PlayerEntity currentPlayer, List<MessageViewModelItemViewModel> messages)
        {
            //客製化內文
            SetMessageCustomizedContent(messages);

            var contentTemplateIDs = messages
                .Where(z=> z.ContentTemplateID > 0)
                .Select(z => z.ContentTemplateID)
                .ToList();
                 
            var userLang = HeaderValueHelper.GetCurrentLanguage();

            List<ContentTemplateAndContentBlockModel> queryTemplateContent = ContentBlockRepository.GetContentByTemplateIDs(contentTemplateIDs);

            var basePlayerInfo = new ContentTemplateParameterBindModel
            {
                PlayerID = currentPlayer.PlayerID,
                MerchantID = currentPlayer.MerchantID,
                PlayerName = currentPlayer.PlayerName
            }; 

            foreach (var item in messages)
            {
                var matchLang = queryTemplateContent
                    .Where(z => z.ContentTemplateID == item.ContentTemplateID)
                    .SelectMany(z => z.ContentBlocks)
                    .OrderBy(z => AppHelper.GetLanguageOrder(z.LanguageCode, userLang))
                    .FirstOrDefault();

                if(matchLang != null)
                {
                    var bindContent = AppHelper.BindTemplateContentParameterValue(item.ParameterJson , basePlayerInfo, matchLang);
                    item.Title = bindContent.Title;
                    item.Content = bindContent.Content;
                    item.LanguageCode = matchLang.LanguageCode;
                }
            }

            return;
        }



        private void SetMessageCustomizedContent(List<MessageViewModelItemViewModel> messages)
        {
            //部分訊息可能不使用內容樣版 而是自己輸入內容
            var customizedContentMessageIDs = messages
                .Where(z => z.ContentTemplateID == 0)
                .Select(z => z.MessageID)
                .ToList();

            if (customizedContentMessageIDs.IsEmptyOrNull())
                return;

            var querySettings = MessageCustomizedSettingRepository.GetByFilter(new QueryMessageCustomizedSettingRequestViewModel
            {
                MessageIDs = customizedContentMessageIDs,
                SettingKeys = new List<int>
                    {
                        MessageCustomizedSettingKeyEnum.CustomizedTitle.ToInteger(),
                        MessageCustomizedSettingKeyEnum.CustomizedContent.ToInteger()
                    }
            });              

            foreach (int messageID in customizedContentMessageIDs)
            {
                var matchMessage = messages.FirstOrDefault(x => x.MessageID == messageID);

                var settings = querySettings
                    .Where(z => z.MessageID == messageID)
                    .Select(x => new SystemConfigSettingParseModel(x.SettingKey, x.Value))
                    .ToList();

                matchMessage.Title = settings.GetBySettingKey(MessageCustomizedSettingKeyEnum.CustomizedTitle)?.SettingValue;
                matchMessage.Content = settings.GetBySettingKey(MessageCustomizedSettingKeyEnum.CustomizedContent)?.SettingValue;
            }
        }


    }
}
