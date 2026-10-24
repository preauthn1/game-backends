#!/bin/bash

release_folder=$(ls -d ./release/*/|head -n 1)
build_folder="$release_folder$GOOS"
echo $build_folder

mkdir -p $build_folder

GIT_HASH=`git rev-parse --short HEAD`
BUILD_TIME=`TZ=Asia/Taipei date +%FT%T%z`
BUILD_NUM=`basename $release_folder`
LDFLAGS="-X example.invalid/ServerTeam/Servers/pkg/conf.GitTag=${DRONE_TAG} -X example.invalid/ServerTeam/Servers/pkg/conf.BuildTime=$BUILD_TIME -X example.invalid/ServerTeam/Servers/pkg/conf.GitHash=$GIT_HASH -X example.invalid/ServerTeam/Servers/pkg/conf.BuildNum=$BUILD_NUM"
echo $LDFLAGS
extname=""
if [ "$GOOS" = "windows" ]; then
    extname=".exe"
fi

go build -ldflags "$LDFLAGS" -i -o $build_folder/dbserver/dbserver$extname ./cmd/dbserver || { echo 'go build dbserver fail' ; exit 1; }
# go build -ldflags "$LDFLAGS" -i -o $build_folder/notifyserver/notifyserver$extname ./cmd/notifyserver || { echo 'go build notifyserver fail' ; exit 1; }
# go build -ldflags "$LDFLAGS" -i -o $build_folder/hallserver/hallserver$extname ./cmd/hallserver || { echo 'go build hallserver fail' ; exit 1; }
# go build -ldflags "$LDFLAGS" -i -o $build_folder/tfserver/tfserver$extname tfserver/main.go || { echo 'go build tfserver fail' ; exit 1; }
# go build -ldflags "$LDFLAGS" -i -o $build_folder/newbydbserve/newbydbserver$extname tools/newbydbserver/newbydbserver.go || { echo 'go build newbydbserver fail' ; exit 1; }

