#!/bin/bash
release_folder=$(ls -d ./release/*/|head -n 1)
build_folder="$release_folder$GOOS"
echo $build_folder
GIT_HASH=`git rev-parse --short HEAD`
BUILD_TIME=`TZ=Asia/Taipei date +%FT%T%z`
BUILD_NUM=`basename $release_folder`
LDFLAGS="-X example.invalid/ServerTeam/Servers/pkg/conf.GitTag=${DRONE_TAG} -X example.invalid/ServerTeam/Servers/pkg/conf.BuildTime=$BUILD_TIME -X example.invalid/ServerTeam/Servers/pkg/conf.GitHash=$GIT_HASH -X example.invalid/ServerTeam/Servers/pkg/conf.BuildNum=$BUILD_NUM"
echo $LDFLAGS
extname=""
if [ "$GOOS" = "windows" ]; then
    extname=".exe"
fi

### 老遊戲
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/brnn/file/brnn$extname ./cmd/game/brnn || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/bjl/file/bjl$extname ./cmd/game/bjl || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/by/file/by$extname ./cmd/game/by || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ddz/file/ddz$extname ./cmd/game/ddz || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dzpk/file/dzpk$extname ./cmd/game/dzpk || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ermj/file/ermj$extname ./cmd/game/ermj || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/fkeb/file/fkeb$extname ./cmd/game/fkeb || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hhdz/file/hhdz$extname ./cmd/game/hhdz || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hhnn/file/hhnn$extname ./cmd/game/hhnn || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/lhd/file/lhd$extname ./cmd/game/lhd || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/qzniuniu/file/qzniuniu$extname ./cmd/game/qzniuniu || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/scmj/file/scmj$extname ./cmd/game/scmj || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/sg/file/sg$extname ./cmd/game/sg || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/srniuniu/file/srniuniu$extname ./cmd/game/srniuniu || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/sss/file/sss$extname ./cmd/game/sss || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ttz/file/ttz$extname ./cmd/game/ttz || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wuzhang/file/wuzhang$extname ./cmd/game/wuzhang || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/zjh/file/zjh$extname ./cmd/game/zjh || { echo 'go build fail' ; exit 1; }

### Slot
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/common-slot/file/common-slot$extname ./cmd/game/common-slot || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dfdc/file/dfdc$extname ./cmd/game/dfdc || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/tgw/file/tgw$extname ./cmd/game/tgw || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ald/file/ald$extname ./cmd/game/ald || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/zcjb/file/zcjb$extname ./cmd/game/zcjb || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wlzb/file/wlzb$extname ./cmd/game/wlzb || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/xgpd/file/xgpd$extname ./cmd/game/xgpd || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/xmbx/file/xmbx$extname ./cmd/game/xmbx || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/jjbx/file/jjbx$extname ./cmd/game/jjbx || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dhzw/file/dhzw$extname ./cmd/game/dhzw || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wlzbjp/file/wlzbjp$extname ./cmd/game/wlzbjp || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/csdjp/file/csdjp$extname ./cmd/game/csdjp || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wlhc/file/wlhc$extname ./cmd/game/wlhc || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wljr/file/wljr$extname ./cmd/game/wljr || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/jgyf/file/jgyf$extname ./cmd/game/jgyf || { echo 'go build fail' ; exit 1; }

### 棋牌類
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/jlbpk/file/jlbpk$extname ./cmd/game/jlbpk || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/jdesyd/file/jdesyd$extname ./cmd/game/jdesyd || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dczz/file/dczz$extname ./cmd/game/dczz || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hdc/file/hdc$extname ./cmd/game/hdc || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dchjk/file/dchjk$extname ./cmd/game/dchjk || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hbsl/file/hbsl$extname ./cmd/game/hbsl || { echo 'go build fail' ; exit 1; }