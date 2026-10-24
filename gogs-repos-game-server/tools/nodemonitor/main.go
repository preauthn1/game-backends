package main

import (
	"conf"
	"db"
	"fmt"
	"io/ioutil"
	"mlog"
	"net/http"
	"net/url"
	"nodemonitor"
	"regexp"
	"strconv"
	"strings"
	"time"
	"util"
)

func init() {
	conf.CLogLevel = 0
	conf.ClearLog = true
	conf.Uptime = true
}

//並發十個
var ExecChans = make(chan bool, 50)
var DoneChans = make(chan bool, 1)

func main() {
	defer util.PanicErrStack()
	nodemonitor.InitDatabase()
	for {
		nodeList := getNodeList()
		groupLogSize := len(nodeList)
		nodeList2 := getGroupIP()
		groupIPSize := len(nodeList2)
		nodeList = append(nodeList, nodeList2...)
		failSize := int32(0)
		tasks := int32(0)
		passSize := int32(0)
		for _, nodeip := range nodeList {
			nodeip = strings.Trim(nodeip, " ")
			pass, _ := util.InArray(nodeip, nodemonitor.Config.WhiteList)
			if pass {
				passSize++
				example.invalid("Pass nodeip : %s", nodeip)
				continue
			}
			tasks++
			go parserNode(nodeip)

			//mlog.System(nodeip)
		}
		for i := int32(0); i < tasks; i++ {
			ok := <-DoneChans
			<-ExecChans
			if !ok {
				failSize++
			}
		}

		mlog.System("grouplog node: %d, groupip node: %d, total node : %d, fail node : %d, pass node : %d, white list : %v", groupLogSize, groupIPSize, len(nodeList), failSize, passSize, nodemonitor.Config.WhiteList)
		time.Sleep(time.Duration(nodemonitor.Config.Interval) * time.Second)
	}

}

func parserNode(nodeip string) {
	ExecChans <- true
	foo := "http://%s:30000/stats;typed"
	url := fmt.Sprintf(foo, nodeip)
	body := basicAuth(url)
	if body == "" {
		m := fmt.Sprintf("Alert : url %s request timeout exceeded of %d seconds.", url, nodemonitor.Config.NodeConnTimeout)
		mlog.Error(m)
		alertMsg(m)
		DoneChans <- false
		return
	}
	re, _ := regexp.Compile("S\\..*scur\\.\\d.*u32\\:(\\d.*)")
	ret := re.FindAllStringSubmatch(body, -1)
	total := int32(0)

	for _, scur := range ret {
		i, _ := strconv.ParseInt(scur[1], 10, 32)
		total += int32(i)
	}
	if total > nodemonitor.Config.AlertConnLimit {
		m := fmt.Sprintf("Alert : Node %s Current Connections %d exceeds %d.", nodeip, total, nodemonitor.Config.AlertConnLimit)
		mlog.Error(m)
		alertMsg(m)
	}
	example.invalid("nodeip : %s, total : %v", nodeip, total)
	DoneChans <- true
}

func getNodeList() (list []string) {
	list = make([]string, 0)
	sql := "SELECT nodeip FROM grouplog GROUP BY nodeip"
	ret, err := nodemonitor.NodeDBUtil.ExecSQL(sql)
	if err != nil {
		mlog.Error("grouplog 獲取節點错误, %v, %s", err, sql)
		return
	}
	ret.ForEachRow(func(r db.RecordRow) {
		list = append(list, r[0].(string))
	})
	return
}

func getGroupIP() (list []string) {
	list = make([]string, 0)
	sql := "SELECT nodeip FROM groupip GROUP BY nodeip"
	ret, err := nodemonitor.NodeDBUtil.ExecSQL(sql)
	if err != nil {
		mlog.Error("groupip 獲取節點错误, %v, %s", err, sql)
		return
	}
	ret.ForEachRow(func(r db.RecordRow) {
		list = append(list, r[0].(string))
	})
	return
}

func basicAuth(url string) string {
	defer util.PanicErrStack()
	var username string = "adminjason"
	var passwd string = "rm7z4WinjasI7"
	client := &http.Client{Timeout: time.Duration(nodemonitor.Config.NodeConnTimeout) * time.Second}

	req, err := http.NewRequest("GET", url, nil)
	req.SetBasicAuth(username, passwd)
	resp, err := client.Do(req)
	if err != nil {
		//mlog.Error("%v", err)
		return ""
	}
	bodyText, err := ioutil.ReadAll(resp.Body)
	s := string(bodyText)
	return s
}

func alertMsg(m string) {
	s := url.QueryEscape(m)
	url := fmt.Sprintf(nodemonitor.Config.AlertBotAPI, s)
	http.Get(url)
}
