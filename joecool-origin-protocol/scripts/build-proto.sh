#!/bin/bash
if [ "${CI_COMMIT_REF_NAME}" = "phnum/master" ]; then
    export DEST_NAME="proto-phnum"
fi
echo ${DEST_NAME}
mkdir -p ${OUTPUT}/go
protoc --plugin=protoc-gen-go=/usr/bin/protoc-gen-go --go_out=${OUTPUT}/go ./*proto
protoc --plugin=protoc-gen-go=/usr/bin/protoc-gen-go --go_out=${OUTPUT}/go ./phnum/*proto

# rsync -a --no-o --no-g --delete ${OUTPUT}/ /mnt/release/${DEST_NAME}/src