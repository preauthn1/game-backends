using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Cors.Infrastructure;
using NPOI.HPSF;
using target.Models.Enums;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Lck.GoogleApi
{
    /// <summary>
    /// service account 的 credential是在這個帳號底下
    /// user@example.invalid
    /// </summary>
    public static class GoogleDocUtility
    {
        private static Dictionary<PGSDocumentTypeEnum, GoogleApiDocumentInfoModel> DocDic = new Dictionary<PGSDocumentTypeEnum, GoogleApiDocumentInfoModel>();

        static GoogleDocUtility()
        {
            DocDic.Add(PGSDocumentTypeEnum.TransferAPI_EN, new GoogleApiDocumentInfoModel
            {
                FileName = "_TransferAPI_EN",
                GoogleDocUrl = "https://example.invalid/document/d/1MGr8qB4Q5-RDPcUwC-Ojxk_9ysdtjPezRyXfDKu8eG4/edit?tab=t.0"
            });

            DocDic.Add(PGSDocumentTypeEnum.TransferAPI_CN, new GoogleApiDocumentInfoModel
            {
                FileName = "_TransferAPI_中文",
                GoogleDocUrl = "https://example.invalid/document/d/1BQhauNjmVmWDQHWbCE_M1d8h9AKofH1HH4IvCcSfvx4/edit?tab=t.0"
            });

            DocDic.Add(PGSDocumentTypeEnum.SeamlessAPI_EN, new GoogleApiDocumentInfoModel
            {
                FileName = "_SeamlessAPI_EN",
                GoogleDocUrl = "https://example.invalid/document/d/1Xiq6R4Pll2QPEKIMsmw1kRgqz-DSiEvs8POoT012b9Q/edit?tab=t.0"
            });

            DocDic.Add(PGSDocumentTypeEnum.SeamlessAPI_CN, new GoogleApiDocumentInfoModel
            {
                FileName = "_SeamlessAPI_中文",
                GoogleDocUrl = "https://example.invalid/document/d/17l2IO3k_vKE_olbJxPZ61_-xSD0bUde0nKttIjTcAyg/edit?tab=t.0"
            });
        }


        public static void ReadAndReplaceDocFile(PGSDocumentTypeEnum docType , PromoteBrandTypeEnum brandType)
        {
            string brandName = "target";
            string brandNameTitle = "target／ION-Slot";
            string brandNameLower = brandName.ToLower();

            if (brandType == PromoteBrandTypeEnum.BWG)
            {
                brandName = "BWG";
                brandNameTitle = "BWG";
                brandNameLower = brandName.ToLower();
            }

            var matchDic = DocDic[docType];

            // 1. 設定您的 Google Docs 網址與要替換的文字
            string docUrl = matchDic.GoogleDocUrl;
            string targetFileId = ExtractFileId(docUrl);

            string fileName = brandName + matchDic.FileName;
            string outputPdfPath = $@"C:\Gitlab_DDYNH\gameicons\cs-download\{fileName}.pdf";

            // 2. 載入憑證 (金鑰憑證 credentials.json 請至 Google Cloud Console 申請下載)
            string tokenPath = Directory.GetCurrentDirectory() + @"\myGoogleToken_gmail_account.json";

            UserCredential credential;

            string[] authScopes =
            {
                DriveService.Scope.Drive
            };

            using (var stream =
             new FileStream(tokenPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.FromStream(stream).Secrets,
                            authScopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore("token.json", true))
                            .Result;
                    };

            // 初始化服務
            var driveService = new DriveService(new BaseClientService.Initializer() { HttpClientInitializer = credential });
            var docsService = new DocsService(new BaseClientService.Initializer() { HttpClientInitializer = credential });

            try
            {
                // 3. 在雲端建立一份臨時副本，避免弄髒母檔
                var copiedFile = new Google.Apis.Drive.v3.Data.File 
                { 
                    Name = $"Temp_{fileName}_{DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss")}" 
                };

                //driveService.Files.EmptyTrash().Execute();

                var copyRequest = driveService.Files.Copy(copiedFile, targetFileId);
                var tempFileId = copyRequest.Execute().Id;

                // 4. 執行關鍵字取代 (BatchUpdate)
                var requests = new List<Request>
                {
                    new Request
                    {
                        ReplaceAllText = new ReplaceAllTextRequest
                        {
                            ContainsText = new SubstringMatchCriteria { Text = "{{$_BRAND_TITLE}}", MatchCase = true },
                            ReplaceText = brandNameTitle
                        }
                    },new Request
                    {
                        ReplaceAllText = new ReplaceAllTextRequest
                        {
                            ContainsText = new SubstringMatchCriteria { Text = "{{$_BRAND_L}}", MatchCase = true },
                            ReplaceText = brandNameLower
                        }
                    },
                    new Request
                    {
                        ReplaceAllText = new ReplaceAllTextRequest
                        {
                            ContainsText = new SubstringMatchCriteria { Text = "{{$_BRAND}}", MatchCase = true },
                            ReplaceText = brandName
                        }
                    }
                };

                var batchUpdateRequest = new BatchUpdateDocumentRequest { Requests = requests };
                docsService.Documents.BatchUpdate(batchUpdateRequest, tempFileId).Execute();

                // 5. 將取代後的副本直接匯出並下載為 PDF
                var exportRequest = driveService.Files.Export(tempFileId, "application/pdf");

                // 確保本機目錄存在
                Directory.CreateDirectory(Path.GetDirectoryName(outputPdfPath));
                using (var fileStream = new FileStream(outputPdfPath, FileMode.Create, FileAccess.Write))
                {
                    exportRequest.Download(fileStream);
                }

                // 6. 刪除雲端上的臨時副本檔案
                Console.WriteLine("正在清理雲端臨時檔案...");
                driveService.Files.Delete(tempFileId).Execute();

                Console.WriteLine($"【成功】PDF 已成功產出至: {outputPdfPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"發生錯誤: {ex.Message}");
            }
        }

        // 從網址中解析出 File ID 的方法
        static string ExtractFileId(string url)
        {
            if (url.Contains("/d/"))
            {
                int start = url.IndexOf("/d/") + 3;
                int end = url.IndexOf("/", start);
                return url.Substring(start, end - start);
            }
            throw new ArgumentException("不合法的 Google Docs 網址格式！");
        }


    }
}
