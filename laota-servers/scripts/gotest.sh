#!/bin/bash
#
PKG_LIST=$(go list ./... | grep -v /vendor/ | grep -v /game/ddz | grep -v /game/hhnn | grep -v /game/hhdz | grep -v /game/ermj | grep -v /game/sg | grep -v /game/dczz| grep -v /game/jdesyd | grep -v /game/jlbpk | grep -v /game/dhzw | grep -v /pkg/mylog)
# go test -race -short ${PKG_LIST}
go test -count=1 -short -v ${PKG_LIST} 2>&1 | grep -iE '^(ok|FAIL|\?)' > output
cat output
if grep -q FAIL output ; then
  echo "Fail"
  exit 1
fi
echo "Success"
exit 0