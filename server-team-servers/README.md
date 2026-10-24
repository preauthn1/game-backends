# server-team-servers

原始路径：`server-team-servers`　|　文件：**2222**　|　主语言：Go 1904, Vue 66, TypeScript 56

## 目录构成

```
internal/game/  — 722
internal/dbserver/  — 276
pkg/slot/  — 140
cmd/admin-web/  — 129
pkg/netproto/  — 119
cmd/game/  — 81
pkg/pachislot/  — 77
internal/apiserver/  — 52
internal/cli-api-svc/  — 51
pkg/gamelogic/  — 32
pkg/poker/  — 31
cmd/slot88/  — 31
internal/hallserver/  — 27
cmd/admin-api/  — 25
```

## 关键文件

- CI/构建：`Makefile`、`.gitlab-ci.yml`、`internal/game/jzyq/Makefile`
- 依赖清单：`cmd/admin-web/package.json`

## 主要文件

- `pkg/netproto/HallMsg.pb.go`
- `internal/game/gen/game_test.go`
- `internal/game/by/bylogic_test.go`
- `internal/game/by/bylogic.go`
- `internal/cli-api-svc/pb/cliapisvc/pachi-slot.pb.go`
- `internal/game/jlbpk/jlbpkLogic.go`
- `internal/game/ddz/ddzgamelogic.go`
- `internal/cli-api-svc/pb/cliapisvc/slot.pb.go`

## 说明

该工程为平台的一个组件。仅收录文本源码与配置，图片、音频等二进制资源未收录；凭据、密钥、内网地址与域名已按实际内容替换。

