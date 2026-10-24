package conf

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

var (
	// CLogLevel 控制台日志等级
	CLogLevel int = 3
	// FLogLevel 檔案日志等級
	FLogLevel int = 1
	// ConfigDirName 配置文件目录名
	ConfigDirName = "config"
	// EVTCHANSIZE 事件通道大小
	EVTCHANSIZE = 2000
	// MSGPACKERBUF 消息缓冲区大小
	MSGPACKERBUF int32 = 1024 * 1000
	// READBUFLEN 接收消息缓冲区大小
	READBUFLEN int = 1024 * 1000
	// DefaultEventThrottle 默认的事件并发数
	DefaultEventThrottle int = 1000
	// ClearLog 是否使用乾淨的Log
	ClearLog bool = false
	// Uptime 不知道幹嘛的
	Uptime bool = false
)

var (
	TCPSERVERNAME_HALL_CLIENT     = "hallTCP_Client"   //大厅面向客户端TCP服务器名称
	TCPSERVERNAME_HALL_GAMESERVER = "hallTCP_GSMANAGE" //大厅游戏服务器管理TCP服务器名称
	TCPSERVERNAME_ROOM_CLIENT     = "roomTcp_Client"   //房间面向客户端TCP服务器名称

	TCPSERVERNAME_DBSERVER = "dbserverTCP" //dbserver tcp服务器名称

	TCPSERVERCONN_HALL_CLIENT     = 10000 //大厅客户端连接最大连接数
	TCPSERVERCONN_HALL_GAMESERVER = 2000  //大厅游戏服务器连接最大连接数
	TCPSERVERCONN_ROOM_CLIENT     = 2000  //游戏客户端连接最大连接数

	TCPSERVERCONN_DBSERVER = 2000 //dbserverTCP连接数

	TCPSERVERCONN_NOTIFYSERVER = 2000              //notifyserver tcp连接数
	TCPSERVERNAME_NOTIFYSERVER = "notifyserverTCP" //notifyserver tcp服务器名称
)

/*
	運行測試案例時使用的一些配置
*/
var (
	TestMode      bool = false //搭配視別是否為執行測試案例
	AnalysisMode  bool = false // 是否為分析模式
	TimerFastMode int  = 1     //計時器的快轉倍數
)

func init() {
	type version struct {
		BuildTime string
		GitTag    string
		GitHash   string
		BuildNum  string
	}
	if BuildNum != "{BuildNum}" {
		v := version{BuildTime, GitTag, GitHash, BuildNum}
		b, _ := json.Marshal(v)
		util.FilePutContentsNA("version.txt", string(b))
		fmt.Fprintf(os.Stdout, "version : (%s) %s\n", GitTag, GitHash)
		fmt.Fprintf(os.Stdout, "buildtime : %s\n", BuildTime)
		fmt.Fprintf(os.Stdout, "buildNum : %s\n", BuildNum)
		FLogLevel = 1 // 正式版本的 filelog level 為 info
	} else {
		FLogLevel = 1 // 開發版本的 filelog level 為 debug
	}

	if flag.Lookup("test.v") != nil {
		TestMode = true
	}
}

// 服务器类型
const (
	ServerType_GameServer = iota //gameserver
	ServerType_HallServer        //hallserver
)

// 服务器状态
const (
	ServerStatus_Runing  int32 = 1 //运行中
	ServerStatus_Stoping int32 = 0 //停服中
	ServerStatus_Close   int32 = 2 //已关闭
	ServerStatus_Recover int32 = 3 //维护中
)

// Version Information
var (
	BuildTime = "{BuildTime}"
	GitTag    = "{GitTag}"
	GitHash   = "{GitHash}"
	BuildNum  = "{BuildNum}"
)

// Version 取得版本資訊
func Version() string {
	return fmt.Sprintf("=== 版本資訊 ===\n當前版本：%s GitHash：%s\n建置時間：%s\n建置編號：%s", GitTag, GitHash, BuildTime, BuildNum)
}
