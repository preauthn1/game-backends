package heartbeat

import (
	"sync"
	"time"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//心跳信息
type HeartBeatInfo struct {
	ConnID       string    //连接ID
	LastBeatTime time.Time //上次心跳时间
}

//心跳包处理器
type HeartBeatManager struct {
	beatLog    map[string]time.Time
	msgServer  *msgserver.MsgServer
	beatLocker *sync.RWMutex
}

//创建心跳包处理器
func NewHeartBeatManager(msgServer *msgserver.MsgServer) *HeartBeatManager {
	bmh := new(HeartBeatManager)
	bmh.beatLog = make(map[string]time.Time)
	bmh.msgServer = msgServer
	bmh.beatLocker = &sync.RWMutex{}

	//检查心跳超时的客户端
	go func() {
		defer util.PanicErrStack()

		for {
			time.Sleep(time.Duration(HeartBeat_BeatInterval) * time.Second)

			bmh.checkBeatTimeout()
		}
	}()

	return bmh
}

func (bmh *HeartBeatManager) checkBeatTimeout() {
	bmh.beatLocker.Lock()
	defer bmh.beatLocker.Unlock()

	for connid, t := range bmh.beatLog {
		tt := time.Now().Sub(t)

		if int32(tt.Seconds()) >= HeartBeat_ConnLoseTimeSpan {
			mlog.Debug("客户端%v心跳包超时,断开连接", connid)

			//处理连上马上断开的情况
			if bmh.msgServer.GetClientByID(connid) == nil {
				delete(bmh.beatLog, connid)
			}

			bmh.msgServer.CloseConnByConnID(connid)
		}
	}
}

func (bmh *HeartBeatManager) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	bmh.beatLocker.Lock()
	defer bmh.beatLocker.Unlock()

	connid := clt.ConnID()

	if _, ok := bmh.beatLog[connid]; ok {
		bmh.beatLog[connid] = time.Now()
	}

	//example.invalid("收到客户端%v心跳消息", clt)

	//回复心跳消息
	hbmsg := new(msg.Message)
	hbmsg.BClassID = int32(netproto.MessageBClassID_Common)
	hbmsg.SClassID = int32(netproto.PlatformCommonClassID_HeartBeatReturnID)

	return []*msg.Message{hbmsg}

}

func (bmh *HeartBeatManager) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Common) && sclsid == int32(netproto.PlatformCommonClassID_HeartBeatID)
}

//客户端连接断开
func (bmh *HeartBeatManager) OnClientDisconnect(connid string) {
	bmh.beatLocker.Lock()
	defer bmh.beatLocker.Unlock()

	delete(bmh.beatLog, connid)
}

//客户端连接
func (bmh *HeartBeatManager) OnClientConnect(connid string) {
	bmh.beatLocker.Lock()
	defer bmh.beatLocker.Unlock()

	bmh.beatLog[connid] = time.Now()
}
