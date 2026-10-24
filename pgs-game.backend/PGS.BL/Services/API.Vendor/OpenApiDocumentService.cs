using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;

namespace target.BL.Services
{

    public class OpenApiDocumentService : BaseVendorService, IOpenApiDocumentService
    {       
        public GameListRepository GameListRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }


        private const int EXPIRED_DAYS = 90;




        public string GenerateResponseHtmlForOpenApiDocument(string merchantGuid)
        {
            StringBuilder sb = new StringBuilder();

            string imageDoaminUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl)?.SettingValue;

            var queryGamesAndMerchant = GetMerchantInfoByGuid(merchantGuid);

            string innerHtml = "";

            if (queryGamesAndMerchant.QueryMerchant == null)
            {
                innerHtml = @"<div class='f16'>⚠️ This page has expired. Please use #4.6.1 API /games to fetch game list. </div>
                              <div class='f16'>⚠️ 此页面已过期失效。请使用 #4.6.1 API /games 获取游戏清单与图片 </div>";
            }
            else
            {
                foreach (var item in queryGamesAndMerchant.Games)
                {
                    string rows = GenerateGamesRowsHtml(
                            item.GameID,
                            item.GameNameEN,
                            item.GameNameCN,
                            item.GameTypeText,
                            item.ImageUrl
                       );

                    sb.Append(rows);
                }

                var iconStyle = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryGamesAndMerchant.QueryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameImage_IconStyle);

                var iconStyleSetting = (GameImageStyleTypeEnum)iconStyle.SettingValueOfInt;

                //ps. 放在空間的檔名請全部小寫
                string pgsLogo1 = AppHelper.GenerateGameImageUrl(imageDoaminUrl, "pgs_logo_512x512");
                string pgsLogo2 = AppHelper.GenerateGameImageUrl(imageDoaminUrl, "pgs_logo_shadow_512x512");
                string pgsLogoPSD = $"{imageDoaminUrl.Replace("/game-icon/", "")}/Artist/pgs_logo_psd.zip".ReplaceDuplicateUrlChar();


                string imageDomainPrefix = imageDoaminUrl.Replace("game-icon/" , "/");

                string batchDownloadGameImagesLink = $"{imageDomainPrefix}/Artist/slot-png.zip";

                if(iconStyleSetting == GameImageStyleTypeEnum.WithPGSIcon)
                {
                    batchDownloadGameImagesLink = $"{imageDomainPrefix}/Artist/slot-png-logo-ver.zip"; ;                
                }

                string batchDownloadGamesHtml = $@"<div>
                                                 Game images : <a target='_blank' class='link-logo' href='{batchDownloadGameImagesLink.ReplaceDuplicateUrlChar()}'>Batch download (PNG)</a>
                                               </div>    ";

                if(queryGamesAndMerchant.Games.Any(x=> x.GameID >= 390000))
                {
                    batchDownloadGamesHtml = string.Empty;
                }

                string logoImageLink = $@"
                            <div class='logoPanel mt20'> 
                                <div>
                                target Logo : <a target='_blank' class='link-logo' href='{pgsLogo1}'>PNG image file</a>
                                </div>   
                                <div>
                                target Logo : <a target='_blank' class='link-logo' href='{pgsLogoPSD}'>PSD file</a>
                                {batchDownloadGamesHtml}
                                </div>                                      
                            </div>";

                //ION印尼不提供logo連結
                if (queryGamesAndMerchant.QueryMerchant.Currency == "IDR" ||
                    HasIONSlotKeyword(queryGamesAndMerchant.IMChannelName) ||
                    HasIONSlotKeyword(queryGamesAndMerchant.QueryMerchant?.Name))
                {
                    logoImageLink = "";
                }

                string expiredHint = string.Empty;
                //string expiredHint = $"<span class='f12 grey-text font400'>*this page will expired at {expiredDate}</span> ";
                innerHtml = $@"
                            <div class='logoPageTitle'>
                               Enable Games (商户开启游戏)  
                               {expiredHint}
                            </div>
                            <div> MerchantName：{queryGamesAndMerchant.QueryMerchant.Name} ({queryGamesAndMerchant.QueryMerchant.MerchantID}) </div>
                            {logoImageLink}
                            <div class=''>                               
                            </div>
                            <div class='mt30 exportButton'>                               
                                <a target='_blank' href='/document/{merchantGuid}/export-enableGames'>
                                    <button class='exportButton'>Export Games (汇出)</button>
                                </a>  
                                <span class='ml-1'>⚠️You can use <span style='font-weight: 700;'>API 4.6.1 /games</span> to fetch all games. (refer the API Document)</span> 
                            </div>

                            <table class='table enumsTable gamesTable'>
                                <tr>
                                    <th class='text-l'>
                                        GameID
                                    </th>
                                    <th class='text-l'>
                                        GameName (EN)
                                    </th>      
                                    <th class='text-l'>
                                        GameName (CN)
                                    </th>    
                                    <th class='text-l'>
                                        GameType
                                    </th>    
                                    <th class='text-l'>
                                        ImageUrl
                                    </th>   
                                </tr>
                                {sb.ToString()}
                            </table>";


            }

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta charset='utf-8' />
                                    <title>{queryGamesAndMerchant.QueryMerchant?.Name} ({queryGamesAndMerchant.QueryMerchant?.MerchantID}) Enable games</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body>
                                    <div class='documentPanel'>
                                     {innerHtml}
                                    </div>                                   
                                </body>
                                </html>";

            return finalHtml;

        }



        public QueryMerchantInfoForVendorApiDocumentPageViewModel GetMerchantInfoByGuid(string merchantGuid)
        {
            if (string.IsNullOrWhiteSpace(merchantGuid))
                throw new TerproException(ApiErrorCode.InvalidParameter);

            if (!merchantGuid.IsGuid())
                throw new TerproException(ApiErrorCode.InvalidParameter);

            var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                {
                    MerchantGuid = merchantGuid
                })
                .FirstOrDefault();

            if (queryMerchant == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundMerchant);
            }

            var result = new QueryMerchantInfoForVendorApiDocumentPageViewModel();
          
            if (SettingHelper.IsProduction)
            {
                //過期後就不再提供使用此功能了
                if (DateTimeHelper.GetCurrentTime() > queryMerchant.CreateTime.AddDays(EXPIRED_DAYS))
                {
                    return result;
                }
            }

            var queryIMChannel = IMChannelRepository.GetMerchantIMChannel(queryMerchant.MerchantID);

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = queryMerchant.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
            });

            List<int> gameIDs = queryGames.Select(x => x.GameID).ToList();

            StringBuilder sb = new StringBuilder();

            var gameNameDic_EN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs, LanguageEnum.en_us);
            var gameNameDic_CN = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs, LanguageEnum.zh_cn);

            string imageDoaminUrl = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.LobbyPageGameImageUrl)?.SettingValue;

            List<DocumentGameListItemViewModel> items = new List<DocumentGameListItemViewModel>();

            var iconStyle = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.GameImage_IconStyle);

            var iconStyleSetting = (GameImageStyleTypeEnum)iconStyle.SettingValueOfInt;

            foreach (var item in queryGames)
            {
                string imageUrl = AppHelper.GenerateGameImageUrl(imageDoaminUrl, item.GameCode , iconStyleSetting);

                items.Add(new DocumentGameListItemViewModel
                {
                    GameID = item.GameID,
                    GameNameEN = gameNameDic_EN.GetDictionaryOrDefaultValue(item.GameID),
                    GameNameCN = gameNameDic_CN.GetDictionaryOrDefaultValue(item.GameID),
                    GameTypeText = MultiLanguageTextHelper.GetEnumsCultureText((GameTypeEnum)item.GameType),
                    ImageUrl = imageUrl
                });
            }

            result.QueryMerchant = queryMerchant;
            result.Games = items;
            result.IMChannelName = queryIMChannel?.IMChannelName;

            return result;
        }


        public bool HasIONSlotKeyword(string text)
        {
            return CommonLogicHelper.HasIONSlotKeyword(text);
        }


        private string GenerateGamesRowsHtml(int gameID, string name_EN, string name_CN, string typeName, string imageUrl)
        {
            return $@"<tr>
                        <td class='text-l'>
                            {(gameID == -1 ? "-" : gameID.ToString())}
                        </td>
                        <td class='text-l'>
                            {name_EN}
                        </td>      
                        <td class='text-l'>
                            {name_CN}
                        </td>    
                        <td class='text-l'>
                            {typeName}
                        </td>    
                        <td class='text-l'>
                            <a target='_blank' href='{imageUrl}'>{imageUrl}</a>                                        
                        </td>   
                    </tr>";
        }



     






    }
}
