#!/bin/bash
release_folder=$(ls -d ./release/*/|head -n 1)
build_folder="$release_folder$GOOS"
echo $build_folder
GIT_HASH=`git rev-parse --short HEAD`
BUILD_TIME=`TZ=Asia/Taipei date +%FT%T%z`
BUILD_NUM=`basename $release_folder`
LDFLAGS="-X conf.GitTag=${DRONE_TAG} -X conf.BuildTime=$BUILD_TIME -X conf.GitHash=$GIT_HASH -X conf.BuildNum=$BUILD_NUM"
echo $LDFLAGS
extname=""
if [ "$GOOS" = "windows" ]; then
    extname=".exe"
fi
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/bjl/file/bjl$extname game/bjl/bjl.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ddz/file/ddz$extname game/ddz/ddz.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/dzpk/file/dzpk$extname game/dzpk/dzpk.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/lhd/file/lhd$extname game/lhd/lhd.go || { echo 'go build fail' ; exit 1; }
#go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/qz21/file/qz21$extname game/qz21/qz21.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/qzniuniu/file/qzniuniu$extname game/qzniuniu/qzniuniu.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/scmj/file/scmj$extname game/scmj/scmj.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/sg/file/sg$extname game/sg/sangong.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/sgj/file/sgj$extname game/sgj/shuiguoji.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/srniuniu/file/srniuniu$extname game/srniuniu/srniuniu.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/sss/file/sss$extname game/sss/sss.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/tbniuniu/file/tbniuniu$extname game/tbniuniu/tbniuniu.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ttz/file/ttz$extname game/ttz/ttz.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/wuzhang/file/wuzhang$extname game/wuzhang/wuzhang.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/zjh/file/zjh$extname game/zjh/zhajinhua.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/fkeb/file/fkeb$extname game/fkeb/fkeb.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/brnn/file/brnn$extname game/brnn/brnn.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hhdz/file/hhdz$extname game/hhdz/hhdz.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/hhnn/file/hhnn$extname game/hhnn/hhnn.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/by/file/by$extname game/by/by.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/cd21/file/cd21$extname game/cd21/cd21.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/lhj/file/lhj$extname game/lhj/lhj.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/fkeb/file/fkeb$extname game/fkeb/fkeb.go || { echo 'go build fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/gameserver/ermj/file/ermj$extname game/ermj/ermj.go || { echo 'go build fail' ; exit 1; }