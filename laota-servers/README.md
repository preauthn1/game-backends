# laota-servers

原始路径：`laota-servers`　|　文件：**1277**　|　主语言：Go 1248, Shell 12, YAML 4

## 目录构成

```
internal/game/  — 407
internal/dbserver/  — 266
pkg/slot/  — 105
pkg/netproto/  — 98
cmd/game/  — 47
pkg/gamelogic/  — 30
internal/hallserver/  — 27
pkg/gameserver/  — 23
internal/monitorserver/  — 21
internal/notifyserver/  — 20
pkg/robot/  — 17
pkg/util/  — 16
internal/tfserver/  — 15
pkg/module/  — 15
```

## 关键文件

- CI/构建：`Makefile`、`.gitlab-ci.yml`

## 主要文件

- `pkg/netproto/HallMsg.pb.go`
- `internal/game/by/bylogic_test.go`
- `internal/game/by/bylogic.go`
- `internal/game/ddz/ddzgamelogic.go`
- `internal/game/ddz/ddzrobotlogic.go`
- `internal/game/jlbpk/jlbpkLogic.go`
- `internal/game/bjl/baijialelogic.go`
- `internal/game/dchjk/dchjkLogic.go`

## 说明

该工程为平台的一个组件。仅收录文本源码与配置，图片、音频等二进制资源未收录；凭据、密钥、内网地址与域名已按实际内容替换。

