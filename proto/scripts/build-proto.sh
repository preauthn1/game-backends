#!/bin/bash

export DEST_NAME="proto-phnum"
echo ${DEST_NAME}
mkdir -p ${OUTPUT}/go
protoc --plugin=protoc-gen-go=/usr/bin/protoc-gen-go --go_out=${OUTPUT}/go ./*proto
rsync -a --no-o --no-g --delete ${OUTPUT}/ /mnt/release/${DEST_NAME}/src