package tfconfig

import (
	"config"
	"fmt"
	"mlog"
	"net/http"
	"net/url"
)

const (
	ConfigDirName                   = "config"
	ConfigTFCenterServerSectionName = "centerserver"
)

//转发配置
type TFConfig struct {
	ReportUrl              string //上报转发状态地址
	PortMapUrl             string //端口映射地址
	ReportInterval         int    //上报状态时间间隔
	RefreshPortMapInterval int    //刷新端口映射时间间隔
	CheckTFPortInterval    int    //转发端口检查时间间隔
	ReportIP               int    //建立通道時是否由TFSV上報連線來源IP
	NeedCheckKey		   int //需要驗證key的埠號
	NeedCheckKeySourceIP   string //需要驗證key的來源埠號
	ConnWatchInterval      int    //每 n 秒偵測一次連線數量
	ConnWatchExceeded      int    //每一分鐘超過50個連線數量的波動
	AlertBotAPI            string //報警群

}

var ConfigCtx *TFConfig

func init() {
	ConfigCtx = loadTFConfig()
}

func loadTFConfig() *TFConfig {
	cfgs := config.GetConfigGroup(ConfigDirName)
	cfg := cfgs.GetConfig("tfserver")

	tfcfg := new(TFConfig)
	tfcfg.PortMapUrl = cfg.GetConfigValue(ConfigTFCenterServerSectionName, "portmapurl")
	tfcfg.ReportUrl = cfg.GetConfigValue(ConfigTFCenterServerSectionName, "reporturl")
	tfcfg.ReportInterval = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "reportinterval")
	tfcfg.RefreshPortMapInterval = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "refreshportmapinterval")
	tfcfg.CheckTFPortInterval = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "checktfportinterval")
	tfcfg.ReportIP = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "reportip")
	tfcfg.NeedCheckKey = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "needcheckkey")
	tfcfg.NeedCheckKeySourceIP = cfg.GetConfigValue(ConfigTFCenterServerSectionName, "needcheckkeysourceip")

	tfcfg.ConnWatchInterval = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "connwatchinterval")
	tfcfg.ConnWatchExceeded = cfg.GetConfigInt(ConfigTFCenterServerSectionName, "connwatchexceeded")
	tfcfg.AlertBotAPI = cfg.GetConfigValue(ConfigTFCenterServerSectionName, "alertbotapi")

	tfcfg.PortMapUrl = fmt.Sprintf("http://%s", tfcfg.PortMapUrl)
	tfcfg.ReportUrl = fmt.Sprintf("http://%s", tfcfg.ReportUrl)
	tfcfg.AlertBotAPI = fmt.Sprintf("http://%s", tfcfg.AlertBotAPI)
	return tfcfg

}

func AlertMsg(m string) {
	s := url.QueryEscape(m)
	url := fmt.Sprintf(ConfigCtx.AlertBotAPI, s)
	mlog.Error(url)
	http.Get(url)
}
