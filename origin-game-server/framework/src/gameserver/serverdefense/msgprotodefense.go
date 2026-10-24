package serverdefense

import (
	"fmt"
	"mlog"
	"msg"
	"network"
	"strconv"
	"strings"
	"sync"
	"time"
	"util"
)

//消息协议防护
type MsgProtoDefense struct {
	*DefenserBase
	defaultProtoLimit *DefenserLimit            //默认的协议发送频率限制
	specialProtoLimit map[string]*DefenserLimit //特殊协议发送频率限制

	protoDefData    map[string]*DefenserData //协议防护数据
	protoDataLocker *sync.RWMutex            //协议数据锁
}

//创建消息协议防护对象
func NewMsgProtoDefense(defaultLimit *DefenserLimit, specialLimit map[string]*DefenserLimit) *MsgProtoDefense {
	msgprotodef := new(MsgProtoDefense)
	msgprotodef.defaultProtoLimit = defaultLimit
	msgprotodef.specialProtoLimit = specialLimit

	msgprotodef.DefenserBase = NewDefenserBase()
	msgprotodef.protoDataLocker = &sync.RWMutex{}
	msgprotodef.protoDefData = make(map[string]*DefenserData)

	//5秒协议频率数据清理
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(5) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			for _, dd := range msgprotodef.protoDefData {
				dd.ClearSecData_5()
			}
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	//10秒协议频率数据清理
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(10) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			for _, dd := range msgprotodef.protoDefData {
				dd.ClearSecData_10()
			}
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	//30秒协议频率数据清理
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(30) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			for _, dd := range msgprotodef.protoDefData {
				dd.ClearSecData_30()
			}
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	//60秒协议频率数据清理
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(60) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			for _, dd := range msgprotodef.protoDefData {
				dd.ClearSecData_60()
			}
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	//300秒协议频率数据清理
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(300) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			for _, dd := range msgprotodef.protoDefData {
				dd.ClearSecData_300()
			}
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	//15分钟,清理数据
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(60*15) * time.Second)

			msgprotodef.protoDataLocker.Lock()
			msgprotodef.protoDefData = make(map[string]*DefenserData)
			msgprotodef.protoDataLocker.Unlock()
		}
	}()

	return msgprotodef
}

//添加协议限制
func (dd *MsgProtoDefense) AddProtoLimit(bclsid int32, sclsid int32, limit *DefenserLimit) {
	protosign := fmt.Sprintf("%d_%d", bclsid, sclsid)

	dd.specialProtoLimit[protosign] = limit
}

//客户端连接
func (dd *MsgProtoDefense) OnClientConnect(clt *network.ConnInfo) {

}

//客户端断开
func (dd *MsgProtoDefense) OnClientDisconnect(clt *network.ConnInfo) {

}

//收到数据
func (dd *MsgProtoDefense) OnClientData(data *network.ConnData) {

}

//收到消息
func (dd *MsgProtoDefense) OnClientMessage(clt *network.ConnInfo, msg *msg.Message) {
	//ip白名单
	if dd.checkWhiteIPList(clt.IPAddress) {
		return
	}

	//检查特定协议频率
	connsign := fmt.Sprintf("%s_%d_%d", clt.IPAddress, msg.BClassID, msg.SClassID)
	protosign := fmt.Sprintf("%d_%d", msg.BClassID, msg.SClassID)

	//检查特定协议
	if ddlimit, ok := dd.specialProtoLimit[protosign]; ok {
		dd.checkProtoData(clt, connsign, ddlimit, protosign)
		return
	}

	//	//检查大类协议
	//	bconnsign := fmt.Sprintf("%s_%d_*", clt.IPAddress, msg.BClassID)
	//	bprotosign := fmt.Sprintf("%d_*", msg.BClassID)

	//	//检查大类协议频率
	//	if ddlimit, ok := dd.specialProtoLimit[bprotosign]; ok {
	//		dd.checkProtoData(clt, bconnsign, ddlimit, bprotosign)
	//		return
	//	}

	//检查默认协议
	ddlimit := dd.defaultProtoLimit
	dd.checkProtoData(clt, connsign, ddlimit, protosign)
}

func (dd *MsgProtoDefense) checkProtoData(clt *network.ConnInfo, connsign string, deflimit *DefenserLimit, protosign string) {
	//获取计数器
	var defdata *DefenserData
	dd.protoDataLocker.Lock()
	if pdd, ok := dd.protoDefData[connsign]; ok {
		defdata = pdd
	} else {
		defdata = NewDefenserData()
		dd.protoDefData[connsign] = defdata
	}
	dd.protoDataLocker.Unlock()

	//计数器加1
	defdata.Increase(1)

	if deflimit == nil {
		return
	}

	//检查计数器是否超出设定
	if defdata.CheckLimit(deflimit) {
		dd.CloseConn(clt)

		dd.BlackIPList.BlockIP(clt.IPAddress, dd.BlockIPTime, fmt.Sprintf("单位时间发送协议%s超出设定次数.%s", protosign, defdata))
	}

}

//解析默认协议防护
func (dd *MsgProtoDefense) ParseDefaultProtoLimit(limit string) {
	lmt := dd.parseDefenseLimit(limit)
	if lmt == nil {
		return
	}

	dd.defaultProtoLimit = lmt
}

//解析特殊协议防护数据
func (dd *MsgProtoDefense) ParseSpecialProtoLimit(limit string) {

	defer func() {
		if err := recover(); err != nil {
			mlog.Error("解析特殊协议防护配置错误,%v", err)
		}
	}()
	dd.specialProtoLimit = make(map[string]*DefenserLimit)

	arr := strings.Split(limit, "|")

	for _, str := range arr {
		item := strings.Split(str, ":")

		//协议号
		itemproto := item[0]
		//防护配置
		itemlimit := item[1]

		itemprotoArr := strings.Split(itemproto, "_")
		bclsid, err := strconv.Atoi(itemprotoArr[0])

		if err != nil {
			mlog.Error("解析特殊协议防护配置错误,%s,%v", str, err)
			return
		}

		sclsid := itemprotoArr[1]

		protosign := fmt.Sprintf("%d_%s", bclsid, sclsid)
		protolimit := dd.parseDefenseLimit(itemlimit)

		if protolimit == nil {
			mlog.Error("解析特殊协议防护配置错误,%s", str)
			return
		}

		dd.specialProtoLimit[protosign] = protolimit
	}
}
