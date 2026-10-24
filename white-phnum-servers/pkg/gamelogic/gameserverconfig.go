package gamelogic

import (
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/config"
	"example.invalid/ServerTeam/Servers/pkg/module/logicmodule"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	ScoreUpdateMode_Now    = 0 //写分方式,即时写分
	ScoreUpdateMode_Logout = 1 //写分方式,退房间写分
	ScoreUpdateMode_PiSai  = 2 //寫分方式, 比賽積分
)

const (
	PokerType_Random  = 1 //随机发牌
	PokerType_Config  = 2 //发牌器发牌
	PokerType_Shuffle = 3 //不洗牌
)

//牌局记录方式
const (
	DealLogType_None int32 = 0 //不记录
	DealLogType_File int32 = 1 //文件记录
	DealLogType_DB   int32 = 2 //数据库记录
	DealLogType_Fish int32 = 3 //捕魚紀錄
)

//服务器配置信息
type GameServerConfig struct {
	ProxyIP          string //代理服務器 ip
	ServerAddr       string //gameserver 侦听地址
	RealAddr         string
	BServerAddr      string //B服 gameserver ip
	HallServerAddr   string //大厅地址,以逗号分隔
	DBServerAddr     string //dbserver地址
	NotifyServerAddr string //通知程序地址

	ServerID              int32 //服务器ID
	ScoreMode             int32 //写分方式
	MaxUser               int32 //人数上限
	RegHallServerInterval int32 //注册广场时间间隔

	GameID            int32  //游戏ID
	GameName          string //游戏名称
	ServerName        string //服务器名称
	Flag              string //服务器标记
	MinPlayer         int32  //最小人数
	MaxPlayer         int32  //最大人数
	TableCount        int32  //桌子数
	SeatCount         int32  //椅子数
	LoginMoney        int32  //入场金币数
	TaxScale          int32  //抽水百分比
	TaxAddPool        int32  //抽取稅收的百分比放入獎金池
	ReadyWaitTime     int32  //准备超时时间
	BaseScore         int32  //底分
	PokerType         int32  //发牌方式
	MatchMakingTime   int32  //未滿遊戲最小人數的等待時間
	MatchMakingNumber int32  //開始倒數時間的最低人數
	Kickoutmoney      int32  //達到此积分，強制退出房間
	MaxOdd            int32
	HttpServerAddr    string //http服务地址
	HttpVCode         string //http校验码

	DealLogType           int32 //牌局记录方式
	MaxGameCountSameTable int32 //同一桌最大游戏次数
	GameTimeOut           int32 //當遊戲開始後超過N分鐘未結束該局就踢出此張桌子所有人

	Single        int32 //直接進入取第一個服務器進入, 0=>顯示房間列表, 1=>第一個服務器進入
	HostAndBackup int32 //AB服. 0主機 1備援轉正主機接手中 2備援
	GameType      int32 //0真金 1比賽 2休閒
	SuperToolCgf  SuperToolConfig
}

//超端設定檔
type SuperToolConfig struct {
	SuperToolModeType int32 //超端: 1配桌型
	SyncInterval      int32 //放分更新時間,單位秒
}

//获取服务器ip和端口
func (cfg *GameServerConfig) GetServerIPPort() (string, int32) {
	ip, port := util.ParseIPAndPort(cfg.ServerAddr)
	if cfg.ProxyIP != "" {
		ip = cfg.ProxyIP
	}
	return ip, int32(port)
}

func (cfg *GameServerConfig) GetReadServerIPPort() (string, int32) {
	ip, port := util.ParseIPAndPort(cfg.RealAddr)
	if cfg.ProxyIP != "" {
		ip = cfg.ProxyIP
	}
	return ip, int32(port)
}

//获取游戏信息
func (cfg *GameServerConfig) GetGameInfo() *Game {
	game := NewGame()
	game.GameID = cfg.GameID
	game.GameName = cfg.GameName
	game.MinPlayer = cfg.MinPlayer
	game.MaxPlayer = cfg.MaxPlayer

	return game
}

//获取Http服务器配置
func (cfg *GameServerConfig) GetHttpServerCfg() *logicmodule.HttpServerConfig {
	httpCfg := new(logicmodule.HttpServerConfig)
	httpCfg.ServerAddr = cfg.HttpServerAddr
	httpCfg.VCode = cfg.HttpVCode

	return httpCfg
}

const (
	CONFIGSECTIONNAME_SERVER     = "server"
	CONFIGSECTIONNAME_GAMESERVER = "gameserver"
	CONFIGSECTIONNAME_GAMEQUEUE  = "gamequeue"
	CONFIGSECTIONNAME_HTTP       = "http"
	CONFIGSECTIONNAME_GAMEINFO   = "gameinfo"

	CONFIGFILENAME_GAMESERVER   = "gameserver"
	CONFIGFILENAME_SERVERINFO   = "serverinfo"
	CONFIGFILENAME_LOGICCONFIG  = "logicconfig"
	CONFIGSECTIONNAME_SUPERTOOL = "supertool"
)

//获取服务器配置
func GetGameServerConfig(serverInfo, cfg *config.Config) *GameServerConfig {
	gscfg := new(GameServerConfig)

	proxyip := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "proxyip")
	addr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "serveraddr")
	realaddr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "realaddr")
	dbaddr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "dbserveraddr")
	nsaddr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "nsserveraddr")
	hsaddr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_SERVER, "hallsvraddr")

	servername := serverInfo.GetConfigValue(CONFIGSECTIONNAME_GAMESERVER, "servername")
	gameid := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "gameid"))
	gamename := cfg.GetConfigValue(CONFIGSECTIONNAME_GAMESERVER, "gamename")
	minplayer := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "minplayer"))
	maxplayer := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "maxplayer"))
	tablecount := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "tablecount"))
	seatcount := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "seatcount"))
	flag := cfg.GetConfigValue(CONFIGSECTIONNAME_GAMESERVER, "flag")
	loginmoney := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "loginmoney"))
	maxuser := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "maxuser"))
	serverid := int32(serverInfo.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "serverid"))
	scoremode := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "scoremode"))
	taxscale := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "taxscale"))
	taxaddpool := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "taxaddpool"))
	readywaittime := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "readywaittime"))
	reghallserverinterval := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "reghallserverinterval"))
	basescore := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "basescore"))
	pokertype := int32(cfg.GetConfigInt(CONFIGFILENAME_GAMESERVER, "pokertype"))
	matchmakingtime := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "matchmakingtime"))
	matchmakingnumber := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "matchmakingnumber"))
	kickoutmoney := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "kickoutmoney"))
	maxodd := int32(cfg.GetConfigInt(CONFIGSECTIONNAME_GAMESERVER, "maxodd"))

	deallog := int32(cfg.GetConfigInt(CONFIGFILENAME_GAMESERVER, "deallog"))
	maxgamecountsametable := int32(cfg.GetConfigInt(CONFIGFILENAME_GAMESERVER, "maxgamecountsametable"))
	gametimeout := int32(cfg.GetConfigInt(CONFIGFILENAME_GAMESERVER, "gametimeout"))
	if gametimeout == int32(0) {
		gametimeout = 30
	}

	gscfg.ProxyIP = proxyip
	httpServerAddr := serverInfo.GetConfigValue(CONFIGSECTIONNAME_HTTP, "addr")
	httpVCode := serverInfo.GetConfigValue(CONFIGSECTIONNAME_HTTP, "vcode")

	single := int32(cfg.GetConfigInt(CONFIGFILENAME_GAMESERVER, "single"))
	hostandbackup := serverInfo.GetConfigInt32(CONFIGSECTIONNAME_GAMESERVER, "hostandbackup")
	gametype := serverInfo.GetConfigInt32(CONFIGFILENAME_GAMESERVER, "gametype")

	superToolModeType := serverInfo.GetConfigInt32(CONFIGSECTIONNAME_SUPERTOOL, "modetype")
	superToolSyncInterval := serverInfo.GetConfigInt32(CONFIGSECTIONNAME_SUPERTOOL, "syncinterval")

	gscfg.ServerAddr = addr
	gscfg.RealAddr = realaddr
	gscfg.HallServerAddr = hsaddr
	gscfg.DBServerAddr = dbaddr
	gscfg.NotifyServerAddr = nsaddr

	gscfg.GameID = gameid
	gscfg.GameName = gamename
	gscfg.ServerID = serverid
	gscfg.ServerName = servername
	gscfg.ScoreMode = scoremode
	gscfg.Flag = flag
	gscfg.MinPlayer = minplayer
	gscfg.MaxPlayer = maxplayer
	gscfg.TableCount = tablecount
	gscfg.SeatCount = seatcount
	gscfg.LoginMoney = loginmoney
	gscfg.MaxUser = maxuser
	gscfg.TaxScale = taxscale
	gscfg.TaxAddPool = taxaddpool
	gscfg.ReadyWaitTime = readywaittime
	gscfg.RegHallServerInterval = reghallserverinterval
	gscfg.BaseScore = basescore
	gscfg.PokerType = pokertype
	gscfg.DealLogType = deallog
	gscfg.MaxGameCountSameTable = maxgamecountsametable
	gscfg.GameTimeOut = gametimeout
	gscfg.MatchMakingTime = matchmakingtime
	gscfg.MatchMakingNumber = matchmakingnumber
	gscfg.Kickoutmoney = kickoutmoney
	gscfg.MaxOdd = maxodd
	gscfg.HttpServerAddr = httpServerAddr
	gscfg.HttpVCode = httpVCode

	gscfg.Single = single
	gscfg.HostAndBackup = hostandbackup
	gscfg.GameType = gametype
	gscfg.SuperToolCgf.SuperToolModeType = superToolModeType
	gscfg.SuperToolCgf.SyncInterval = superToolSyncInterval

	return gscfg
}

func (cfg *GameServerConfig) String() string {
	return fmt.Sprintf("gameid:%d gamename:%s serverid:%d,servername:%s,listen:%s,flag:%s,scoremode:%d,loginmoney:%d,base score:%d", cfg.GameID, cfg.GameName, cfg.ServerID, cfg.ServerName, cfg.ServerAddr, cfg.Flag, cfg.ScoreMode, cfg.LoginMoney, cfg.BaseScore)
}
