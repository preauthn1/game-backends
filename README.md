# game-backends

60 个工程，共 36065 个文本文件。

## 成员

- `server-team-deploy-server` — `server-team/deploy-server`（8 文件）
- `server-team-tf-server` — `server-team/tf-server`（20 文件）
- `white-tf-server` — `white/tf-server`（20 文件）
- `docker90` — `server-team/gogs-repos/docker90`（26 文件）
- `docker91` — `server-team/gogs-repos/docker91`（27 文件）
- `tutorial` — `server-team/gogs-repos/tutorial`（1 文件）
- `white-chessgameserver` — `white/chessgameserver`（787 文件）
- `server-team-phnum-servers` — `server-team/phnum-servers`（855 文件）
- `white-phnum-servers` — `white/phnum-servers`（788 文件）
- `phnum-protocal` — `server-team/phnum-protocal`（105 文件）
- `protocol` — `white/protocol`（101 文件）
- `joecool-servers` — `joecool/servers`（1391 文件）
- `prototest` — `white/prototest`（2 文件）
- `joecool-origin-protocol` — `joecool/origin-protocol`（124 文件）
- `alex-servers` — `alex/servers`（808 文件）
- `ddynh-backend.game` — `backend/ddynh/backend.game`（2155 文件）
- `ddynh-backend.agent77` — `backend/ddynh/backend.agent77`（1127 文件）
- `ddynh-777agent` — `backend/ddynh/777agent`（2930 文件）
- `ddynh-backend.fw` — `backend/ddynh/backend.fw`（1078 文件）
- `alex-origin-protocol` — `alex/origin-protocol`（103 文件）
- `backend-side-project` — `backend/backend-side-project`（58 文件）
- `laota-servers` — `laota/servers`（1280 文件）
- `laota-origin-protocol` — `laota/origin-protocol`（124 文件）
- `alex-deploy-server` — `alex/deploy-server`（8 文件）
- `pgs-game.backend` — `backend/pgs/game.backend`（2430 文件）
- `origin-game-server` — `white/origin-game-server`（1164 文件）
- `backend.portal` — `backend/pgs/backend.portal`（336 文件）
- `release` — `server-team/phnum/release`（47 文件）
- `ddynh-botservice` — `backend/ddynh/botservice`（284 文件）
- `proto` — `server-team/phnum/proto`（5 文件）
- `white-kube` — `white/kube`（38 文件）
- `fz-botservice` — `backend/fz/botservice`（162 文件）
- `dd-dev` — `server-team/kube/dd-dev`（35 文件）
- `dd-qa` — `server-team/kube/dd-qa`（26 文件）
- `fz-backend.game` — `backend/fz/backend.game`（1718 文件）
- `pgs-dev` — `server-team/kube/pgs-dev`（35 文件）
- `database` — `backend/fz/database`（1322 文件）
- `fz-backend.agent77` — `backend/fz/backend.agent77`（1120 文件）
- `pgs-qa` — `server-team/kube/pgs-qa`（42 文件）
- `sql-update` — `backend/fz/sql-update`（61 文件）
- `fz-backend.fw` — `backend/fz/backend.fw`（1055 文件）
- `fz-777agent` — `backend/fz/777Agent`（2930 文件）
- `backend.agent` — `backend/fz/backend.agent`（332 文件）
- `electron-jp-game` — `backend/electron-jp-game`（9 文件）
- `nftcontract` — `backend/pgs/NFTContract`（118 文件）
- `backend.clubagent` — `backend/fz/backend.clubagent`（15 文件）
- `cm-backend` — `cm/backend`（408 文件）
- `app-frontend` — `cm/app-frontend`（178 文件）
- `cm-api-frontend` — `cm/api-frontend`（304 文件）
- `upload_to_s3` — `cm/upload_to_s3`（2 文件）
- `gogs-repos-game-server` — `server-team/gogs-repos/game-server`（1164 文件）
- `server-team-servers` — `server-team/servers`（2225 文件）
- `white-servers` — `white/servers`（757 文件）
- `auer-backend.fw` — `backend/auer/Backend.FW`（1055 文件）
- `auer-backend.game` — `backend/auer/Backend.Game`（1724 文件）
- `gogs-repos-deploy-server` — `server-team/gogs-repos/deploy-server`（6 文件）
- `joecool-chessgameserver` — `joecool/chessgameserver`（787 文件）
- `server-team-origin-protocol` — `server-team/origin-protocol`（134 文件）
- `go-mod-proxy-test` — `white/go-mod-proxy-test`（6 文件）
- `white-origin-protocol` — `white/origin-protocol`（105 文件）

## 脱敏（按各仓实际命中）

| 类型 | 命中 |
|---|---|
| 数据库口令 | 906 |
| 连接串 | 255 |
| Telegram bot token | 21 |
| Slack webhook/密钥 | 8 |
| AWS 访问密钥 | 4 |
| 私钥 | 4 |
| GitLab 访问令牌 | 3 |

处理最集中的工程：

- `backend/pgs/game.backend` — 数据库口令 199、连接串 102、Slack webhook/密钥 2
- `backend/ddynh/botservice` — 数据库口令 64、连接串 39、Telegram bot token 4
- `backend/ddynh/backend.game` — 数据库口令 53、连接串 16、Telegram bot token 3
- `backend/fz/database` — 数据库口令 49
- `laota/servers` — 数据库口令 30、连接串 13
- `white/kube` — 数据库口令 23、连接串 19
- `server-team/phnum-servers` — 数据库口令 27、连接串 13
- `white/phnum-servers` — 数据库口令 27、连接串 13
- `alex/servers` — 数据库口令 27、连接串 13
- `server-team/servers` — 数据库口令 30、连接串 6、Telegram bot token 2

## 说明

按仓库实际内容处理：命中的令牌、口令、私钥、云凭据、第三方 API key、内网地址、域名与邮箱均已替换或删除；二进制资源未收录。
