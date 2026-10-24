package main

import (
	"conf"
	"config"
	"fmt"
	"io"
	"mlog"
	"net"
	"net/http"
	"os"
	"strconv"
	"strings"
	"tfserver"
	"util"
)

const (
	ConfigSectionName = "tfconfig"
	ConfigDirName     = "config"

	PortRouterFileName = "portrouter.xml"
)

func init() {
	conf.CLogLevel = 0
}

func main() {
	//获取配置
	initConfig()

	http.HandleFunc("/config", getConfigHandler)
	http.HandleFunc("/submit", submitStatusHandler)
	http.HandleFunc("/tfstatus", tfstatusHandler)

	//侦听端口
	port := cfg.GetConfigInt(ConfigSectionName, "port")

	addr := fmt.Sprintf(":%d", port)
	go func() {
		defer util.PanicErrStack()

		err := http.ListenAndServe(addr, nil)
		if err != nil {
			fmt.Println(err)
			os.Exit(1)
		}
	}()

	example.invalid("服务器启动成功,侦听在地址%s...", addr)
	select {}
}

//加载配置
func getConfigHandler(w http.ResponseWriter, req *http.Request) {
	defer util.PanicErrStack()

	data := util.ReadFile(PortRouterFileName)

	ip, _, _ := net.SplitHostPort(req.RemoteAddr)
	example.invalid("转发%s加载配置信息", ip)
	io.WriteString(w, data)
}

//提交转发服务器状态
func submitStatusHandler(w http.ResponseWriter, req *http.Request) {
	defer util.PanicErrStack()

	ip, _, _ := net.SplitHostPort(req.RemoteAddr)

	query := req.URL.Query()

	if len(query["cn"]) == 0 || len(query["code"]) == 0 {
		mlog.Error("客户端%s提交服务器状态信息,但参数不正确%s", ip, req.URL.String())
		io.WriteString(w, "error")
		return
	}

	cn, _ := strconv.Atoi(query["cn"][0])
	code := query["code"][0]

	if code != cfg.GetConfigValue(ConfigSectionName, "submitcheckcode") {
		mlog.Error("客户端%s提交服务器状态信息,但校验码不正确%s", ip, req.URL.String())
		io.WriteString(w, "error")
		return
	}

	tfserver.TFServerManager.UpdateTFServer(ip, cn)

	example.invalid("转发%s提交当前连接数%d", ip, cn)

	io.WriteString(w, "ok")
}

//读取转发服务器运行状况
func tfstatusHandler(w http.ResponseWriter, req *http.Request) {
	defer util.PanicErrStack()

	ip, _, _ := net.SplitHostPort(req.RemoteAddr)

	allowip := cfg.GetConfigValue(ConfigSectionName, "statusqueryip")
	if strings.Index(allowip, ip) == -1 && strings.Index(allowip, "*") == -1 {
		example.invalid("客户端%s请求查询转发列表,但不在允许的IP范围.", ip)
		io.WriteString(w, "forbidden")
		return
	}

	tfs := tfserver.TFServerManager.GetServerList()

	data := "<table>"
	data += "<tr><td>IPAddress</td><td>当前连接数</td><td>最后更新时间</td></tr>"
	for _, t := range tfs {
		data += fmt.Sprintf("<tr><td>%s</td><td>%d</td><td>%s</td></tr>", t.IPAddress, t.CurrentConn, t.LastRegTime.Format("2006-01-02 15:04:05"))
	}
	data += "</table>"

	io.WriteString(w, data)

}

var cfg *config.Config

//初始化配置
func initConfig() {
	cfgs := config.GetConfigGroup(ConfigDirName)
	cfg = cfgs.GetConfig("tfconfig")

}
