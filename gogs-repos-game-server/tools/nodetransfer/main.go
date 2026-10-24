package main

import (
	"conf"
	"db"
	"flag"
	"fmt"
	"mlog"
	"nodetransfer"
	"os"
	"strconv"
	"strings"
	"util"
)

var UserData map[int32]int32

func init() {
	conf.CLogLevel = 0
	conf.ClearLog = true
	conf.Uptime = false
}
func main() {
	fromIpPtr := flag.String("fromip", "0.0.0.0", "transfer user from [fromip]")
	toIpPtr := flag.String("toip", "0.0.0.0", "transfer user to [toip]")
	flag.Parse()
	if *fromIpPtr == "" {
		fmt.Println("error: must required -fromip")
		os.Exit(1)
	}
	if *toIpPtr == "" {
		fmt.Println("error: must required -toip")
		os.Exit(1)
	}
	loadUIDData()
	userList := getUserListByFromIp(*fromIpPtr)
	transferSize := int32(0)
	nodetransfer.NodeDBUtil.Open()
	defer nodetransfer.NodeDBUtil.Close()
	for _, uid := range userList {
		if v, ok := UserData[uid]; ok && v > 0 {
			example.invalid("transfer uid : %d, gametime : %d, toip : %s", uid, UserData[uid], *toIpPtr)
			sql := "UPDATE grouplog SET nodeip = ? WHERE userid = ?"
			_, err := nodetransfer.NodeDBUtil.Exec(sql, *toIpPtr, uid)
			if err != nil {
				mlog.Error("transfer uid : %d failure, because %v", uid, err)
			}
			transferSize++
		}
	}
	fmt.Printf("find user %d, transfer user %d\n", len(userList), transferSize)
}

func getUserListByFromIp(fromip string) (list []int32) {
	list = make([]int32, 0)
	nodetransfer.InitDatabase()
	ret, err := nodetransfer.NodeDBUtil.ExecSQL("SELECT userid FROM grouplog WHERE nodeip = ?", fromip)
	if err != nil {
		mlog.Error("獲取用戶信息錯誤, %v", err)
		return
	}
	ret.ForEachRow(func(r db.RecordRow) {
		list = append(list, int32(r[0].(int64)))
	})
	return
}

func loadUIDData() {
	UserData = make(map[int32]int32)
	txt := util.ReadFile("data/UID.txt")
	lines := strings.Split(txt, "\n")
	for _, line := range lines {
		t := strings.Trim(line, "\r")
		if t == "" {
			continue
		}
		a := strings.Split(t, "||")
		uid := parseInt32(a[0])
		gameTime := parseInt32(a[1])
		if uid > 0 {
			if _, ok := UserData[uid]; ok {
				UserData[uid] += gameTime
			} else {
				UserData[uid] = gameTime
			}
		}
	}
	//mlog.System("%v", UserData)
}

func parseInt32(s string) int32 {
	i, _ := strconv.ParseInt(s, 10, 32)
	return int32(i)
}
