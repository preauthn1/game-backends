# game-backends

60 个工程，35997 个文本文件。每目录一份 README（结构信息 + 原始说明）。

## 索引

| 工程 | 内容 | 文件 | 主语言 |
|---|---|---|---|
| [`alex-deploy-server`](./alex-deploy-server) | internal/deploy · configs · pkg/copy | 7 | Go |
| [`alex-origin-protocol`](./alex-origin-protocol) | C# | 102 | C# |
| [`alex-servers`](./alex-servers) | internal/dbserver · pkg/netproto · internal/game | 804 | Go |
| [`app-frontend`](./app-frontend) | lib/screens · lib/models · lib/widgets | 177 | XML |
| [`auer-backend.fw`](./auer-backend.fw) | FW.Core.AdminSys.WebUI/Content · FW.Core.AdminSys.WebUI/Themes · FW.Core.IO/FastDFS | 1055 | CSS |
| [`auer-backend.game`](./auer-backend.game) | game.AdminSystem/Content · game.AdminSystem/Views · game.WebSite/PaymentHelper | 1724 | C# |
| [`backend-side-project`](./backend-side-project) | Common.Modules/Helper · gitlab_hook · gitlab_hook/Controllers | 58 | C# |
| [`backend.agent`](./backend.agent) | Agent/scripts · Repository/Models · Repository/Entity | 331 | C# |
| [`backend.clubagent`](./backend.clubagent) | ClubLiveQRCode/com · ClubReportAgent · ClubLiveQRCode | 15 | C# |
| [`backend.portal`](./backend.portal) | src/components · src/views · src/models | 335 | Vue |
| [`cm-api-frontend`](./cm-api-frontend) | database/migrations · app/Models · app/Repositories | 303 | PHP |
| [`cm-backend`](./cm-backend) | app/Models · app/Http · app/Repositories | 407 | PHP |
| [`database`](./database) | CenterDB/dbo · Club02/dbo · Fish/dbo | 1322 | SQL |
| [`dd-dev`](./dd-dev) | values | 34 | YAML |
| [`dd-qa`](./dd-qa) | values | 25 | YAML |
| [`ddynh-777agent`](./ddynh-777agent) | anframework/vendor · public/assets · public/bjui | 2930 | PHP |
| [`ddynh-backend.agent77`](./ddynh-backend.agent77) | Agent77/Areas · Repository/Models · Repository/ViewModel | 1125 | JavaScript |
| [`ddynh-backend.fw`](./ddynh-backend.fw) | FW.Core.AdminSys.WebUI/Content · FW.Core.AdminSys.WebUI/Themes · FW.Core.IO/FastDFS | 1077 | CSS |
| [`ddynh-backend.game`](./ddynh-backend.game) | game.AdminSystem/Content · game.AdminSystem/Views · game.Entity/EF | 2152 | C# |
| [`ddynh-botservice`](./ddynh-botservice) | HangfireJob/wwwroot · game.Service.Job/ServiceImplement · packages/Newtonsoft.Json.12.0.2 | 282 | C# |
| [`docker90`](./docker90) | game/ddz · game/by · game/zjh | 26 | Shell |
| [`docker91`](./docker91) | game/ddz · game/by · game/zjh | 27 | YAML |
| [`electron-jp-game`](./electron-jp-game) | — | 8 | JavaScript |
| [`fz-777agent`](./fz-777agent) | anframework/vendor · public/assets · public/bjui | 2930 | PHP |
| [`fz-backend.agent77`](./fz-backend.agent77) | Agent/Agent77 · Agent/Repository · Agent/Service | 1120 | JavaScript |
| [`fz-backend.fw`](./fz-backend.fw) | FW.Core.AdminSys.WebUI/Content · FW.Core.AdminSys.WebUI/Themes · FW.Core.IO/FastDFS | 1055 | CSS |
| [`fz-backend.game`](./fz-backend.game) | game.AdminSystem/Content · game.AdminSystem/Views · game.WebSite/PaymentHelper | 1718 | C# |
| [`fz-botservice`](./fz-botservice) | HangfireJob/wwwroot · packages/Newtonsoft.Json.12.0.2 · HangfireJob/Views | 162 | C# |
| [`go-mod-proxy-test`](./go-mod-proxy-test) | b · a | 6 | Go |
| [`gogs-repos-deploy-server`](./gogs-repos-deploy-server) | src/deployer · config | 5 | Go |
| [`gogs-repos-game-server`](./gogs-repos-game-server) | dbserver/src · framework/src · roomserver/src | 1163 | Go |
| [`joecool-chessgameserver`](./joecool-chessgameserver) | framework/src · roomserver/src · dbserver/src | 786 | Go |
| [`joecool-origin-protocol`](./joecool-origin-protocol) | C# · phnum · scripts | 123 | C# |
| [`joecool-servers`](./joecool-servers) | internal/game · internal/dbserver · pkg/slot | 1387 | Go |
| [`laota-origin-protocol`](./laota-origin-protocol) | C# · phnum · scripts | 123 | C# |
| [`laota-servers`](./laota-servers) | internal/game · internal/dbserver · pkg/slot | 1276 | Go |
| [`nftcontract`](./nftcontract) | PgsContract.Common/Helper · PgsTokenContract/wwwroot · PgsContract.Common/Models | 118 | C# |
| [`origin-game-server`](./origin-game-server) | dbserver/src · framework/src · roomserver/src | 1163 | Go |
| [`pgs-dev`](./pgs-dev) | values | 34 | YAML |
| [`pgs-game.backend`](./pgs-game.backend) | PGS.Models/ViewModels · PGS.DA/Repository · PGS.Models/TableEntity | 2427 | C# |
| [`pgs-qa`](./pgs-qa) | values | 41 | YAML |
| [`phnum-protocal`](./phnum-protocal) | C# | 104 | C# |
| [`proto`](./proto) | scripts | 4 | YAML |
| [`protocol`](./protocol) | C# | 100 | C# |
| [`prototest`](./prototest) | — | 2 | YAML |
| [`release`](./release) | Release-v1.1.3/SQL-Update · Release-v1.0.4/SQL-Update · Release-v1.1.2/SQL-Update | 47 | SQL |
| [`server-team-deploy-server`](./server-team-deploy-server) | internal/deploy · configs · pkg/copy | 7 | Go |
| [`server-team-origin-protocol`](./server-team-origin-protocol) | C# · phnum · scripts | 133 | C# |
| [`server-team-phnum-servers`](./server-team-phnum-servers) | internal/dbserver · internal/game · pkg/netproto | 851 | Go |
| [`server-team-servers`](./server-team-servers) | internal/game · internal/dbserver · pkg/slot | 2221 | Go |
| [`server-team-tf-server`](./server-team-tf-server) | internal/tfserver · internal/tfconfig · cmd/tfconfig | 19 | Go |
| [`sql-update`](./sql-update) | 20190701-coupon · 20190708-aepayment | 61 | SQL |
| [`tutorial`](./tutorial) | — | 0 | — |
| [`upload_to_s3`](./upload_to_s3) | — | 1 | YAML |
| [`white-chessgameserver`](./white-chessgameserver) | framework/src · roomserver/src · dbserver/src | 786 | Go |
| [`white-kube`](./white-kube) | pgs-dev/deployment · core/docker-registry · core | 38 | YAML |
| [`white-origin-protocol`](./white-origin-protocol) | C# | 104 | C# |
| [`white-phnum-servers`](./white-phnum-servers) | internal/dbserver · pkg/netproto · internal/game | 784 | Go |
| [`white-servers`](./white-servers) | internal/dbserver · pkg/netproto · internal/game | 753 | Go |
| [`white-tf-server`](./white-tf-server) | internal/tfserver · internal/tfconfig · cmd/tfconfig | 19 | Go |
