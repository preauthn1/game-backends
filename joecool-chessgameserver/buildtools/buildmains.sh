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

go build -ldflags "$LDFLAGS" -i -o $build_folder/dbserver/dbserver$extname dbserver/dbserver.go || { echo 'go build dbserver fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/notifyserver/notifyserver$extname notifyserver/notifyserver.go || { echo 'go build notifyserver fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/hallserver/hallserver$extname hallserver/hallserver.go || { echo 'go build hallserver fail' ; exit 1; }
go build -ldflags "$LDFLAGS" -i -o $build_folder/tfserver/tfserver$extname tfserver/main.go || { echo 'go build tfserver fail' ; exit 1; }