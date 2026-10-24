using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using target.Models;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper.HttpContext;
using target.Web.Modules.Filter;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace target.API.AdminSys
{

    /// <summary>
    /// 產生swagger文件的時候，移除掉有被標記特定attr的欄位 (沒有研究的很好，有些bug...)
    /// </summary>
    public class PGSSwaggerQueryParameterIgnoreFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //如果是特殊分類。只秀出groupName 完全符合的API (for 單錢包/轉帳API文件拆開)
            bool isOnlyGroupNameMatchVisible = example.invalid.Version == SystemConstant.SwaggerGroupName_Richmond ||
                                               example.invalid.Version == SystemConstant.SwaggerGroupName_Other ||
                                               example.invalid.Version == SystemConstant.SwaggerGroupName_Freespin;

            bool isFreespinAPI = example.invalid.Version == SystemConstant.SwaggerGroupName_Freespin;

            string mustMatchGroupName = example.invalid.Version;

            var model = context.ApiDescriptions;

            List<string> removePropertyNames = new List<string>();

            List<string> filterVisiblePath = new List<string>();

            foreach (var description in context.ApiDescriptions)
            {
                var debug = description.ActionDescriptor;
                var actiob = debug.DisplayName;

                //特殊分類 只會顯示有標記的API
                if (isOnlyGroupNameMatchVisible)
                {
                    if (description.GroupName == mustMatchGroupName)
                    {
                        filterVisiblePath.Add("/" + description.RelativePath);
                    }
                }

                foreach (var each in description.ParameterDescriptions)
                {
                    var myModelType = each.ModelMetadata?.ContainerType;

                    if (myModelType != null)
                    {
                        //檢查是否有特殊標記的attribute
                        var prop = myModelType.GetProperty(each.Name);

                        if (prop == null)
                            continue;

                        var ignoreAttr = prop.GetCustomAttribute(typeof(SwaggerIgnoreAttribute), false);

                        if (ignoreAttr != null)
                        {
                            removePropertyNames.Add(each.Name);
                        }
                    }
                }

                //如果有被標記要ignore的欄位，移除之
                if (removePropertyNames.IsNotEmptyNull())
                {
                    foreach (string removeName in removePropertyNames)
                    {
                        var match = description.ParameterDescriptions.Where(x => x.Name == removeName).FirstOrDefault();
                        description.ParameterDescriptions.Remove(match);
                    }
                }
            }

            bool isVendorAPI = PGSSwaggerSpecialCaseSetting.IsVendorAPI();

            //如果是vendorAPI 才有的特殊處理
            if (isVendorAPI)
            {
                if (PGSSwaggerSpecialCaseSetting.IsLanguageEN())
                {
                    if (isFreespinAPI)
                    {
                        example.invalid.Description = "The API is currently undergoing adjustments and is temporarily unavailable.";

                        //example.invalid.Description = $@"expand to following items to see more detail{Environment.NewLine}
                        //⭐Suggested implement sequence (Simplified Version){Environment.NewLine}
                        //- Get freespin-support games and available betting amounts. /v1/event/FreespinSpin/betting-info
                        //- Send freespin to a specific player. /v1/event/FreespinSpin/Create
                        //- Other APIs can be used optional according to your requirement.";

                    }
                    else
                    {
                        example.invalid.Description = "expand to following items to see more detail.";
                    }
                }
                else
                {
                    if(isFreespinAPI)
                    {
                        example.invalid.Description = "API目前調整中 暫不開放";

//                        example.invalid.Description = $@"点击下方各个项目，可展开查看更多API说明{Environment.NewLine}
//⭐建议的调用顺序 (简易版){Environment.NewLine}
//- 取得可设置freespin的游戏&可设置投注额 /v1/event/FreespinSpin/betting-info
//- 指定特定玩家发送freespin /v1/event/FreespinSpin/Create
//- 其他API则根据您自身需求选择性使用";



                    }
                    else
                    {
                        example.invalid.Description = "点击下方各个项目，可展开查看更多API说明";
                    }

                }

                var sortItems = new OpenApiPaths();

                var sort = swaggerDoc.Paths.OrderBy(z => z.Value?.Operations?.FirstOrDefault().Value?.Summary);

                string apiVersion = SystemConstant.VendorAPIVersionPath.ToString();

                foreach (var item in sort)
                {
                    string apiUrl = item.Key;

                    string apiUrlDisplayName = (item.Key ?? "").Replace("{version}", apiVersion);

                    if (isOnlyGroupNameMatchVisible)
                    {
                        if (filterVisiblePath.Any(x => x == item.Key))
                        {
                            sortItems.Add(apiUrlDisplayName, item.Value);
                        }
                    }
                    else
                    {
                        sortItems.Add(apiUrlDisplayName, item.Value);
                    }
                }

                swaggerDoc.Paths = sortItems;
            }

            return;

        }
    }




    public class PGSSwaggerGameAPICertificateSampleValueFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            string path = context.ApiDescription.RelativePath;

            if (operation.Parameters.IsEmptyOrNull())
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-Certificate",
                In = ParameterLocation.Header,
                Description = "每隻API都要帶此header",
                Schema = new OpenApiSchema
                {
                    Type = "String",
                    Default = new OpenApiString(SettingHelper.GameApiSetting.ApiCertificateValue)
                }
            });

        }
    }




    public class PGSOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            string path = context.ApiDescription.RelativePath;

            if (operation.Parameters.IsEmptyOrNull())
                operation.Parameters = new List<OpenApiParameter>();

            operation.Description = operation.Description + GetApiDescription(context);

            var firstResponseContent = operation.Responses.FirstOrDefault().Value;
            var responseContent = operation.Responses.FirstOrDefault().Value.Content;

            bool isPaged = false;

            var classFullName = responseContent?.Values?.FirstOrDefault()?.Schema?.Reference?.Id ?? string.Empty;

            dynamic actionDesc = context.ApiDescription.ActionDescriptor;

            string methodName = (actionDesc.ActionName).ToString();

            string modelName = string.Empty;

            var parameters = context.MethodInfo.GetParameters();

            if (parameters.IsNotEmptyNull())
            {
                modelName = parameters.FirstOrDefault().ParameterType.Name;
            }

            bool isPost = context.ApiDescription.HttpMethod == "POST";

            if (classFullName.Contains("PagedList"))
            {
                isPaged = true;
            }

            bool isDebugMatch = path.Contains("merchants");

            if (isDebugMatch)
            {
                string testDebug = "斷點請設在這";
            }

            if (methodName.Contains("GetAllGames"))
            {
                string testDebug = "斷點請設在這";
            }

            if (modelName.Contains("SettleInfoChangedGameRecordViewModel"))
            {
                string testDebug = "斷點請設在這";
            }

            //HttpPost 才會有下方這段
            var requestBody = operation.RequestBody;

            //這邊是處理Http Post 的請求參數 (request model)
            if (requestBody != null)
            {
                int loopTimes = 0; //避免物件無窮迴圈
                var match = requestBody.Content?.FirstOrDefault().Value;

                if (match != null)
                {
                    var newModels = GenerateColumnDocument(context.SchemaRepository.Schemas, "data", match.Schema, isPaged, loopTimes);

                    dynamic listOfModels = newModels;

                    //因應我方API 會多包裝一層data的結構 修改swagger格式
                    match.Schema.Example = newModels;
                    match.Schema.Reference = null;

                    if (isPost)
                    {
                        if (operation.Parameters.IsEmptyOrNull())
                        {
                            operation.Parameters = new List<OpenApiParameter>();
                        }

                        if (listOfModels is IEnumerable)
                        {
                            foreach (var item in listOfModels)
                            {
                                try
                                {
                                    //有些結構是物件包物件 先不處理了
                                    string key = item.Key.ToString().ToLower();

                                    if(key.ContainsWihtoutCase("gameList"))
                                    {
                                        string debug_112277 = "";
                                    }

                                    string descriptionText = ""; //modelName
                                    string sepcialColumnDefine = ($"{modelName}.{key}").ToLower();

                                    bool isRequiredField = PGSSwaggerSpecialCaseSetting.IsExistInRequiredField(modelName, key);

                                    var matchDescription = PGSSwaggerSpecialCaseSetting.GetParameterDescriptionByColumnName(key, sepcialColumnDefine);

                                    descriptionText = matchDescription.FullDescription + PGSSwaggerSpecialCaseSetting.GenerateRequiredFieldHtmlStyle(isRequiredField);

                                    var dataType = TryGetDataType(item);

                                    operation.Parameters.Add(new OpenApiParameter
                                    {
                                        Name = item.Key,
                                        In = ParameterLocation.Query,
                                        Description = descriptionText,
                                        Schema = new OpenApiSchema
                                        {
                                            Type = dataType,
                                        }
                                    });
                                }
                                catch(Exception ex)
                                {
                                    //如果是物件類型 會拋exception
                                    string debug_554433 = "";
                                }
                            }
                        }

                    }

                }
            }

            //這段應該是處理 response model的範例值
            if (responseContent.IsNotEmptyNull())
            {
                int loopTimes = 0; //避免物件無窮迴圈
                var responseSchema = responseContent.FirstOrDefault().Value;

                //嘗試抓出這個api的response model型別
                string className = responseSchema.Schema.Reference?.Id?.Replace("PagedList", "") ?? "";

                if (string.IsNullOrWhiteSpace(className))
                {
                    className = responseSchema.Schema.Items?.Reference?.Id?.Replace("PagedList", "") ?? "";
                }

                if(className.Contains("AggGameInfoResponseViewModel"))
                {
                    string debug665533 = "";
                }

                var newModels = GenerateColumnDocument(context.SchemaRepository.Schemas, "data", responseSchema.Schema, isPaged, loopTimes);

                //因應我方API 會多包裝一層data的結構 修改swagger格式
                responseSchema.Schema.Example = new OpenApiObject
                {
                    ["code"] = new OpenApiLong(0),
                    ["message"] = new OpenApiString(null),
                    ["data"] = newModels
                };

                dynamic listOfModels = newModels;

                HashSet<string> columns = new HashSet<string>();
                Dictionary<string, (string DataType, string Value)> dataTypeMapping = new Dictionary<string, (string DataType, string Value)>();

                if (listOfModels is IEnumerable)
                {
                    foreach (var item in listOfModels)
                    {
                        ParseColumnAndAddToDic(item, columns, dataTypeMapping);
                    }
                }

                firstResponseContent.Description = "<table class='responseColmns'>";
                firstResponseContent.Description += "<tr>"
                    + " <th>Column</th> "
                    + " <th>Description</th> "
                    + " <th>DataType</th> "
                    + " </tr> ";

                foreach (var columnName in columns)
                {
                    if (columnName == "gameCode" || columnName == "serverTimeTW")
                    {
                        string debug888 = "";
                    }

                    string sepcialColumnDefine = $"{className}.{columnName}";

                    string descriptionText = string.Empty;

                    string dataType = dataTypeMapping.ContainsKey(columnName) ? ConvertToDispalyDataType(dataTypeMapping[columnName].DataType) : string.Empty;

                    var matchDesc = PGSSwaggerSpecialCaseSetting.GetParameterDescriptionByColumnName(columnName, sepcialColumnDefine);

                    //有些model雖然欄位名稱相同，但說明需要客製化 (e.g playerName)
                    if (PGSSwaggerSpecialCaseSetting.IsColumnExist(sepcialColumnDefine))
                    {
                        descriptionText = matchDesc.Description + " <span style='color:#cb3434'> " + matchDesc.ExtraMemo + "</span>";
                    }
                    else
                    {
                        descriptionText = matchDesc.Description + " <span style='color:#cb3434'> " + matchDesc.ExtraMemo + "</span>";
                    }

                    firstResponseContent.Description += $"<tr> <td class='columnName'>{columnName}</td> <td>{descriptionText}</td>  <td>{dataType}</td> </tr>";
                }

                firstResponseContent.Description += "</table>";
                responseSchema.Schema.Reference = null;
            }

            operation.Parameters = operation.Parameters.OrderByDescending(z => z.Name == "version ").ToList();
            return;
        }


        private string TryGetDataType(dynamic item)
        {
            try
            {
                return item.Value.PrimitiveType.ToString();
            }
            catch
            {
                return "object / array object";
            }
        }


        private void ParseColumnAndAddToDic(dynamic item, HashSet<string> columns, Dictionary<string, (string DataType, string Value)> dataTypeMapping, int recursiveLayer = 1)
        {
            int newRecursiveLayer = ++recursiveLayer;

            try
            {
                if (IsList(item))
                {
                    foreach (var child in item)
                    {
                        string myKey = child.Key.ToString();
                        columns.Add(myKey);

                        if (!dataTypeMapping.ContainsKey(myKey))
                        {
                            dataTypeMapping.Add(myKey, (GetPropertyDataType(myKey, child), ""));
                        }
                    }
                }
                else if (item.Key.ToString() == "items")
                {
                    var childColumns = item.Value[0];

                    foreach (var columnsOfItems in childColumns)
                    {
                        if (newRecursiveLayer > 3)
                        {
                            //避免無窮迴圈
                        }
                        else
                        {
                            ParseColumnAndAddToDic(columnsOfItems, columns, dataTypeMapping, newRecursiveLayer);
                        }
                    }
                }
                else if (IsList(item.Value))
                {
                    //如果class裡面 又有class
                    string parentKey = item.Key.ToString();
                    columns.Add(parentKey);

                    if (!dataTypeMapping.ContainsKey(parentKey))
                    {
                        dataTypeMapping.Add(parentKey, ("Object", ""));
                    }

                    foreach (var eachClassProperty in item.Value)
                    {
                        //debug後 發現還有多一層的樣子
                        foreach(var eachP in eachClassProperty)
                        {
                            string childKey = eachP.Key.ToString();
                            columns.Add(childKey);

                            if (!dataTypeMapping.ContainsKey(childKey))
                            {
                                dataTypeMapping.Add(childKey, (GetPropertyDataType(childKey, eachP), ""));
                            }
                        }
                    }
                }
                else
                {
                    string key = item.Key.ToString();
                    columns.Add(key);

                    if (!dataTypeMapping.ContainsKey(key))
                    {
                        dataTypeMapping.Add(key, (GetPropertyDataType(key , item), ""));
                    }
                }

            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }


        private string GetPropertyDataType(string columnName , dynamic eachProperty)
        {
            try
            {
                return eachProperty.Value.PrimitiveType.ToString();
            }
            catch
            {

            }

            if (columnName.ContainsWihtoutCase("betRanges"))
                return "array decimal";

            if (columnName.ContainsWihtoutCase("gameSummary"))
                return "array object";

            return "object";
        }



        private string ConvertToDispalyDataType(string dataType)
        {
            dataType = dataType.ToLower();

            if (dataType == "double")
                return "float/decimal";

            if (dataType == "integer")
                return "int";

            if (dataType == "long")
                return "bigint";

            return dataType;
        }


        public bool IsList(object o)
        {
            return (o is System.Collections.IEnumerable);
        }

        private IOpenApiAny GenerateColumnDocument(Dictionary<string, OpenApiSchema> schemas, string columnName, OpenApiSchema target, bool isPaged, int loopTimes)
        {
            target.Format = target.Format ?? string.Empty;

            if (target.Reference == null && target.Items?.Reference == null)
            {
                return GenerateBaseTypeColumnOpenApiObject(schemas, columnName, target, isPaged);
            }
            else
            {
                if (target.Type == "array")
                {
                    if (target.Items.Reference != null)
                    {
                        loopTimes++;
                        return new OpenApiArray
                        {
                            GenerateFromReferenceClass(schemas , target.Items.Reference.Id , isPaged , loopTimes)
                        };
                    }
                }
                else
                {
                    if (target.Reference != null)
                    {
                        loopTimes++;
                        return GenerateFromReferenceClass(schemas, target.Reference.Id, isPaged, loopTimes);
                    }
                }
            }

            return null;
        }


        private IOpenApiAny GenerateBaseTypeColumnOpenApiObject(Dictionary<string, OpenApiSchema> schemas, string columnName, OpenApiSchema target, bool isPaged)
        {
            if (PGSSwaggerSpecialCaseSetting.IsOutputKeyExist(columnName))
                return new OpenApiString(PGSSwaggerSpecialCaseSetting.OutputParameterSampleValueDefined[columnName]);

            if (PGSSwaggerSpecialCaseSetting.IsOutputNumberColumnsExist(columnName))
            {
                var match = PGSSwaggerSpecialCaseSetting.OutputParameterSampleNumberValueDefined[columnName];

                if(match.DataType == typeof(int))
                {
                    return new OpenApiInteger((int)match.SampleValue);
                }
                else
                {
                    return new OpenApiLong(match.SampleValue);
                }
            }

            if (PGSSwaggerSpecialCaseSetting.IsOutputDecimalColumnsExist(columnName))
                return new OpenApiDouble((double)PGSSwaggerSpecialCaseSetting.OutputParameterSampleDecimalValueDefined[columnName]);

            string type = target.Type.ToLower();
            switch (type)
            {
                case "string":

                    if (target.Format.Contains("date-time"))
                    {
                        return new OpenApiString("2020-01-01T12:00:00Z");
                    }

                    return new OpenApiString("string");

                case "long":
                case "number":
                case "integer":

                    if (target.Format.Contains("double"))
                    {
                        return new OpenApiDouble(0.0);
                    }

                    return new OpenApiInteger(0);


                case "boolean":
                    return new OpenApiBoolean(false);

                case "array":
                    return new OpenApiArray
                    {

                    };

                default:
                    return new OpenApiString(columnName);
            }
        }

        private string GetApiDescription(OperationFilterContext context)
        {
            string resultText = string.Empty;
            string changeLine = Environment.NewLine + "<BR>";

            if (context.MethodInfo.CustomAttributes != null)
            {
                var attr_DocumentMemo = (DocumentMemo)context.MethodInfo.GetCustomAttribute(typeof(DocumentMemo), false);
                var desc_EN = (SwaggerApiDescriptionEN)context.MethodInfo.GetCustomAttribute(typeof(SwaggerApiDescriptionEN), false);
                var desc_CN = (SwaggerApiDescriptionCN)context.MethodInfo.GetCustomAttribute(typeof(SwaggerApiDescriptionCN), false);


                if (attr_DocumentMemo != null)
                {
                    resultText += changeLine + attr_DocumentMemo.Memo;
                }

                if (desc_EN != null && PGSSwaggerSpecialCaseSetting.IsLanguageEN())
                {
                    resultText += changeLine + desc_EN.Description;
                }
                else if (desc_CN != null && !PGSSwaggerSpecialCaseSetting.IsLanguageEN())
                {
                    resultText += changeLine + desc_CN.Description;
                }

                if (IsAttributeExist<SwaggerDocumentAccessTokenRequired>(context) ||
                    IsAttributeExist<PlayerTokenAuthorizeFilter>(context) ||
                    IsAttributeExist<AdminTokenrAuthorizeFilter>(context))
                {
                    resultText += changeLine + " 此API需帶header `access_token` ";
                }

                if (IsAttributeExist<SignKeyVerifyFilter>(context))
                {
                    resultText += changeLine + " 此API需帶header `X-SignKey` (加密驗證簽) ";
                }

            }

            return resultText;
        }


        private bool IsAttributeExist<T>(OperationFilterContext context) where T : class
        {
            dynamic attr = context.MethodInfo.GetCustomAttribute(typeof(T), false);
            var convert = (T)attr;
            return convert != null;
        }




        private IOpenApiAny GenerateFromReferenceClass(Dictionary<string, OpenApiSchema> schemas, string refID, bool isPaged, int loopTimes)
        {
            //避免無窮迴圈
            if (loopTimes >= 6)
                return new OpenApiString("object");

            var match = schemas.Where(x => x.Key == refID).FirstOrDefault();

            var newModel = new OpenApiObject();

            if(refID.ContainsWihtoutCase("AggGameInfoResponseViewModel"))
            {
                string debug_446688 = "";
            }

            foreach (var item in match.Value.Properties)
            {
                var isIgnoreWhenPaged = PGSSwaggerSpecialCaseSetting.IsExistInSkipOutputResponseColumnWhenPagedList(refID, item.Key , loopTimes);

                //若有標記attribute 特定的欄位 不會輸出到swagger文件 (1)
                if (isPaged && isIgnoreWhenPaged)
                    continue;

                var isIgnore = PGSSwaggerSpecialCaseSetting.IsExistInSkipOutputRequestBodyList(refID, item.Key);

                //若有標記attribute 特定的欄位 不會輸出到swagger文件 (2)
                if (isIgnore)
                    continue;

                //if (PGSSwaggerSpecialCaseSetting.OutputParameterIgnoreOutputDocumentColumns.Contains(item.Key))
                //{
                //    newModel.Add(item.Key, new OpenApiString("無窮延伸的巢狀物件"));
                //    continue;
                //}

                newModel.Add(item.Key, GenerateColumnDocument(schemas, item.Key, item.Value, isPaged, loopTimes));
            }

            return newModel;
        }

    }



    /// <summary>
    /// 第一次讀取的時候 會先把不要的欄位記住、稍後輸出的時候會進行隱蔽
    /// </summary>
    public class PGSSwaggerTemporarySkipOutputColumnsFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var type = context.ApiModel.Type;

            foreach (var item in context.ApiModel.Type.GetProperties())
            {
                var name = item.Name;
                var ignoreAttr = item.GetCustomAttribute(typeof(SwaggerIgnoreOutputWhenPagedList), false);
                var ignoreAttr2 = item.GetCustomAttribute(typeof(SwaggerIgnoreAttribute), false);
                var ignoreAttr3 = item.GetCustomAttribute(typeof(SwaggerApiRequiredFieldAttribute), false);

                string concateKeyName = $"{context.ApiModel.Type.Name}_{name}";

                if (ignoreAttr != null)
                {
                    //滿鳥的寫法，總之就是先在這邊記住 model + 欄位名稱，後面輸出的時候會拿來判斷
                    PGSSwaggerSpecialCaseSetting.AddToPagedIgnoreDictionary(concateKeyName);
                }

                if (ignoreAttr2 != null)
                {
                    PGSSwaggerSpecialCaseSetting.AddToRequestBodyIgnoreDictionary(concateKeyName);
                }

                if (ignoreAttr3 != null)
                {
                    PGSSwaggerSpecialCaseSetting.AddToRequiredFieldDictionary(concateKeyName);
                }

            }
        }
    }


    /// <summary>
    /// 第一次讀取的時候 會先把不要的欄位改名、隱蔽
    /// </summary>
    public class PGSSwaggerQueryParameterChangeParameterNameFilter : IParameterFilter
    {

        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            var prop = context.PropertyInfo;

            string parameterNameOriginal = parameter.Name;
            parameter.Name = parameter.Name.ToCamelCase();

            var modelName = context.ParameterInfo?.ParameterType?.Name;

            bool isEN = false;

            if (MyHttpContext.Current != null && MyHttpContext.Current.Request.Cookies.ContainsKey(SystemConstant.HeaderKey_Lang))
            {
                var cookieValue = MyHttpContext.Current.Request.Cookies[SystemConstant.HeaderKey_Lang];
                isEN = cookieValue == LanguageEnum.en_us.ToString();
            }

            //這個檔案裏面管理某些命名的欄位會顯示自訂的sampleValue                           
            if (parameter.Name.Contains("merchantID"))
            {
                string qqqqqq = "";
            }

            if (parameter.Name.Contains("version"))
            {
                parameter.Name = "XXXXXXXXXX";
                parameter.Description = "--------------此欄位已隱蔽，請無視或F5重新刷新頁面--------------";

                //parameter.Description = "Api Version (v1)";
                //parameter.Required = false;
            }

            string modelSpecialColumnDescription = $"{modelName}.{parameter.Name}";

            var matchDesc = PGSSwaggerSpecialCaseSetting.GetParameterDescriptionByColumnName(parameter.Name, modelSpecialColumnDescription);

            string finalColumnDescription = matchDesc.FullDescription;

            if (context.ParameterInfo?.ParameterType != null)
            {
                var qqqq = context.ParameterInfo?.ParameterType.GetProperty(parameterNameOriginal);

                var hasAttribute = qqqq.HasAttribute<SwaggerApiRequiredFieldAttribute>();

                if (hasAttribute)
                {
                    finalColumnDescription += PGSSwaggerSpecialCaseSetting.GenerateRequiredFieldHtmlStyle(true);
                }
            }

            //如果是HttpGet的參數說明 這邊組出 (POST 是另外一個middleware處理)
            parameter.Description = finalColumnDescription;

            //搞不太懂swagger的生命週期 要第二次進入頁面 才會把標記 SwaggerIgnoreAttribute 的欄位隱藏
            //無可奈何之下只好在第一次的時候把特定定義的欄位隱藏起來 (很瞎的作法)
            if (prop != null)
            {
                var ignoreAttr = context.PropertyInfo.GetCustomAttribute(typeof(SwaggerIgnoreAttribute), false);

                if (ignoreAttr != null)
                {
                    parameter.Name = "XXXXXXXXXX";
                    parameter.Description = "--------------此欄位已隱蔽，請無視或F5重新刷新頁面--------------";
                }
            }
        }
    }



}
