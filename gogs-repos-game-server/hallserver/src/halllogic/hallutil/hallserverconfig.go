package hallutil

import (
	"config"
	"fmt"
	"example.invalid/golang/protobuf/proto"
	"strconv"
	"strings"
)

//大厅配置
type HallServerConfig struct {
	HallAddr         string //大厅地址
	GSMgmAddr        string //游戏服务器注册地址
	NotifyServerAddr string //协调服务器地址
	ServerID         int32  //服务器ID
	DBServerAddr     string //DBServer地址
	StrategyList     []*ServerAllocStrategyList
	MaxUser          int32
}

//服务器分配策略, 百人桌, 配桌型
type ServerAllocStrategyList struct {
	GameIdList map[int32]*int32
	MaxUser    *int32
}

//获取大厅配置
func GetHallServerConfig(cfg *config.Config) *HallServerConfig {
	halladdr := cfg.GetConfigValue("server", "halladdr")
	gsaddr := cfg.GetConfigValue("server", "gsaddr")
	dbaddr := cfg.GetConfigValue("dbserver", "addr")
	serverid := cfg.GetConfigInt("server", "serverid")
	nsaddr := cfg.GetConfigValue("server", "nsaddr")

	size := cfg.GetConfigInt("serverallocstrategy", "size")

	serverCfg := new(HallServerConfig)
	serverCfg.HallAddr = halladdr
	serverCfg.GSMgmAddr = gsaddr
	serverCfg.ServerID = int32(serverid)
	serverCfg.DBServerAddr = dbaddr
	serverCfg.NotifyServerAddr = nsaddr

	for i := 1; i <= size; i++ {
		cname := fmt.Sprintf("c%d", i)
		gameidval := cfg.GetConfigValue(cname, "gameidlist")
		gameidlist := strings.Split(gameidval, ",")
		strategy := new(ServerAllocStrategyList)
		strategy.GameIdList = make(map[int32]*int32, 0)
		for _, v := range gameidlist {
			num, _ := strconv.Atoi(v)
			strategy.GameIdList[int32(num)] = proto.Int32(int32(num))
		}
		strategy.MaxUser = proto.Int32(cfg.GetConfigInt32(cname, "maxuser"))
		serverCfg.StrategyList = append(serverCfg.StrategyList, strategy)
	}

	return serverCfg
}
