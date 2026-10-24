# joecool-servers

> 本目录原始 README 见文末，本节为快照补充信息。

原始路径：`joecool-servers`　|　文件 **1387**　|　主语言：Go 1355, Shell 12, YAML 4

## 目录构成

```
internal/game/  — 468
internal/dbserver/  — 270
pkg/slot/  — 122
pkg/netproto/  — 105
cmd/game/  — 55
pkg/gamelogic/  — 31
internal/hallserver/  — 27
pkg/gameserver/  — 23
internal/monitorserver/  — 21
internal/notifyserver/  — 20
pkg/robot/  — 17
pkg/util/  — 16
```

## 关键文件

- CI/构建：`Makefile`、`.gitlab-ci.yml`

---

## 原始说明

# Servers

[![pipeline status](http://example.invalid:1688/server-team/servers/badges/master/pipeline.svg)](http://example.invalid:1688/server-team/servers/commits/master)
[![coverage report](http://example.invalid:1688/server-team/servers/badges/master/coverage.svg)](http://example.invalid:1688/server-team/servers/commits/master)

### Games

#### Slot 
* [多福多財(DFDC), 招財進寶(ZCJB)](internal/game/dfdc/README.md)
* [糖果屋(TGW)](internal/game/candyhouse/README.md)
* [星光派對(XGPD), 阿拉丁(ALD)](internal/game/aladdin/README.md)
* [五龍爭霸(WLZB)](internal/game/wlzb/README.md)
* [五龍爭霸_彩金版(WLZBJP), 財神到_採金版(CSDJP)](internal/game/wlzbjp/README.md)
* [五龍皇朝(WLHC)](internal/game/wlhc/README.md)
* [百萬金輪五龍爭霸(WLJR)](internal/game/wljr/README.md)
* [熊貓報喜(XMBX)](internal/game/xmbx/README.md)
* [擊鼓迎福(JGYF)](internal/game/jgyf/README.md)
* [吉祥如意(JXRY)](internal/game/jxry/README.md)
* [金吉報喜(JJBX)](internal/game/jjbx/README.md)
* [東海之王(DHZW)](internal/game/dhzw/README.md)

