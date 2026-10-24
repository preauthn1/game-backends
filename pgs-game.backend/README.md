# pgs-game.backend

原始路径：`pgs-game.backend`　|　文件：**2427**　|　主语言：C# 2287, JSON 59, YAML 12

## 目录构成

```
PGS.Models/ViewModels/  — 605
PGS.DA/Repository/  — 213
PGS.Models/TableEntity/  — 202
PGS.BL/Services/  — 192
PGS.Models/Models/  — 145
PGS.Testing/Services/  — 145
PGS.Models/Enums/  — 141
PGS.BL/Interface/  — 136
PGS.Utility/Helper/  — 72
PGS.API.AdminSys/Controllers/  — 56
PGS.API.ExternalVendor/Controllers/  — 48
PGS.Testing/Repository/  — 33
PGS.AzureFunctionV2/Functions/  — 28
PGS.Testing/Helper/  — 26
```

## 关键文件

- CI/构建：`.gitlab-ci.yml`、`DockerFiles/Dockerfile`
- 服务端主程序：`PGS.AzureFunctionV2/Program.cs`、`PGS.API.Vendor/Program.cs`、`PGS.API.Vendor/Startup.cs`

## 主要文件

- `PGS.Models/CultureText/i18nKeys.json`
- `PGS.API.AdminSys/Controllers/HomePageReportController.cs`
- `PGS.BL/Services/ServiceReferenceService/MonthlyAccountingExportService.cs`
- `PGS.BL/Services/API.ExternalVendor/GameVendorSingleWalletService.cs`
- `PGS.BL/Services/API.AdminSys/MerchantService.cs`
- `PGS.BL/Services/API.AdminSys/CustomizedColumnDisplayService.cs`
- `PGS.BL/Services/ServiceReferenceService/MonthlyAccountingExportServiceV2Refactor.cs`
- `PGS.BL/Services/API.AdminSys/BetRecordService.cs`

## 说明

该工程为平台的一个组件。仅收录文本源码与配置，图片、音频等二进制资源未收录；凭据、密钥、内网地址与域名已按实际内容替换。

