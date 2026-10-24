#! /bin/bash

#################
### Arguments ###
# $1: tag name
# $2: Module Server Username
# $3: Module Server Host
# $4: Module Server Store Path
#################

# Envs Define
BASE_PATH=${PWD}

VERSION=$1
TIME=`TZ=Asia/Taipei date +%FT%TZ`

BASE=.modules
FOLDER_NAME=$VERSION
BUILD_PATH=${BASE}/$FOLDER_NAME

MODULE_SERVER_USERNAME=$2
MODULE_SERVER_HOST=$3
MODULE_SERVER_STORE_PATH=$4

### Read go.mod file
exec < go.mod
read line
PKGNAME=$(echo ${line}|cut -d ' ' -f2)
DOMAIN=$(echo ${PKGNAME}|cut -d '/' -f1)

# Print Envs
echo "BASE_PATH = ${BASE_PATH}"
echo "VERSION = ${VERSION}"
echo "TIME = ${TIME}"
echo "BASE = ${BASE}"
echo "FOLDER_NAME = ${FOLDER_NAME}"
echo "BUILD_PATH = ${BUILD_PATH}"
echo "PKGNAME = ${PKGNAME}"
echo "DOMAIN = ${DOMAIN}"
echo "MODULE_SERVER_USERNAME = ${MODULE_SERVER_USERNAME}"
echo "MODULE_SERVER_HOST = ${MODULE_SERVER_HOST}"
echo "MODULE_SERVER_STORE_PATH = ${MODULE_SERVER_STORE_PATH}"

# Clean
rm -rf ${BASE}

# Make build folder
mkdir -p ${BUILD_PATH}

# Copy go.mod
cp go.mod ${BUILD_PATH}

# Create info file
echo "{\"Version\":${VERSION},\"Time\":\"${TIME}\"}" > ${BUILD_PATH}/${VERSION}.info

# Create temp module folder
mkdir -p ${BUILD_PATH}/${PKGNAME}@${VERSION}
cp -rf ./* ${BUILD_PATH}/${PKGNAME}@${VERSION}

# ZIP files
git ls-files > ${BUILD_PATH}/filelist
exec < ${BUILD_PATH}/filelist
cd ${BUILD_PATH}
while read line
do 
    zip source ${PKGNAME}@${VERSION}/$line
done

cd ${BASE_PATH}

# Remove temp module folder
rm -rf ${BUILD_PATH}/${DOMAIN}
rm -rf ${BUILD_PATH}/filelist

# Upload to Athens Proxy
ssh ${MODULE_SERVER_USERNAME}@${MODULE_SERVER_HOST} mkdir -p ${MODULE_SERVER_STORE_PATH}/${PKGNAME}/${VERSION}
scp ${BUILD_PATH}/go.mod ${MODULE_SERVER_USERNAME}@${MODULE_SERVER_HOST}:${MODULE_SERVER_STORE_PATH}/${PKGNAME}/${VERSION}
scp ${BUILD_PATH}/${VERSION}.info ${MODULE_SERVER_USERNAME}@${MODULE_SERVER_HOST}:${MODULE_SERVER_STORE_PATH}/${PKGNAME}/${VERSION}
scp ${BUILD_PATH}/source.zip ${MODULE_SERVER_USERNAME}@${MODULE_SERVER_HOST}:${MODULE_SERVER_STORE_PATH}/${PKGNAME}/${VERSION}

# Clean
rm -rf ${BASE}