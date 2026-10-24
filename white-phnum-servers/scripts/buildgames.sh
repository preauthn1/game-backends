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

go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/common-slot/file/common-slot$extname ./cmd/game/common-slot || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dfdc/file/dfdc$extname ./cmd/game/dfdc || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/tgw/file/tgw$extname ./cmd/game/tgw || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ald/file/ald$extname ./cmd/game/ald || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/zcjb/file/zcjb$extname ./cmd/game/dfdc || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wlzb/file/wlzb$extname ./cmd/game/wlzb || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/xgpd/file/xgpd$extname ./cmd/game/ald || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/xmbx/file/xmbx$extname ./cmd/game/xmbx || { echo 'go build fail' ; exit 1; }