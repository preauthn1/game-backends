package serverdefense

import (
	"config"
)

//配置节名称
const (
	ConfigSectionName_Defense = "defense" //防护方案配置节名称
)

//防护配置
type DefenseConfig struct {
	BlockIPTime       int    //封IP时间
	MaxConn           int    //最大连接数
	ConnTimesLimit    string //连接次数
	SendDataLimint    string //发送数据包大小
	SendMsgLimit      string //发送消息数量
	ProtoLimitDefault string //协议发送频率默认限制
	ProtoLimitSpecial string //特殊协议发送频率
	WhiteIPList       string //ip白名单
	ServerDesc        string //服务器信息
	Enable            bool   //是否开启
}

//解析防护协议配置
func ParseDefenseConfig(cfg *config.Config) *DefenseConfig {
	defCfg := new(DefenseConfig)
	blockiptime := cfg.GetConfigInt(ConfigSectionName_Defense, "blocktime")
	whiteiplist := cfg.GetConfigValue(ConfigSectionName_Defense, "whileiplist")

	cc_maxconn := cfg.GetConfigInt(ConfigSectionName_Defense, "cc_ipmaxconn")
	cc_conntimes := cfg.GetConfigValue(ConfigSectionName_Defense, "cc_conntimes")
	cc_senddata := cfg.GetConfigValue(ConfigSectionName_Defense, "cc_senddata")
	cc_sendmsg := cfg.GetConfigValue(ConfigSectionName_Defense, "cc_sendmsg")

	proto_default := cfg.GetConfigValue(ConfigSectionName_Defense, "proto_default")
	proto_special := cfg.GetConfigValue(ConfigSectionName_Defense, "proto_special")

	server_desc := cfg.GetConfigValue(ConfigSectionName_Defense, "serverdesc")
	enable := cfg.GetConfigValue(ConfigSectionName_Defense, "enable")

	defCfg.BlockIPTime = blockiptime
	defCfg.MaxConn = cc_maxconn
	defCfg.ConnTimesLimit = cc_conntimes
	defCfg.SendDataLimint = cc_senddata
	defCfg.SendMsgLimit = cc_sendmsg
	defCfg.ProtoLimitDefault = proto_default
	defCfg.ProtoLimitSpecial = proto_special
	defCfg.WhiteIPList = whiteiplist
	defCfg.ServerDesc = server_desc

	if enable == "" {
		defCfg.Enable = true
	} else {
		defCfg.Enable = (enable == "1")
	}

	return defCfg
}
