package msghandler

import (
	"halllogic/hallutil"
	"module"
	"msg"
	"netproto"
	"network"
)

import (
	"fmt"
	proto "example.invalid/golang/protobuf/proto"
	"mlog"
)

type AllocGameServerHandler struct {
	svrProvider hallutil.GameServerProvider
}

//创建分配游戏服务器处理器对象
func NewAllocGameServerHandler(svrProvider hallutil.GameServerProvider) *AllocGameServerHandler {
	allochandler := new(AllocGameServerHandler)
	allochandler.svrProvider = svrProvider

	return allochandler
}

func (alloch *AllocGameServerHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Hall) && sclsid == int32(netproto.HallMsgClassID_RequestGameServerAddrID)
}

func (alloch *AllocGameServerHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {

	dd := m.MsgData.([]byte)
	mm := &netproto.RequestGameServerAddrInfo{}

	err := proto.Unmarshal(dd, mm)

	if err != nil {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[AllocGameServerHandler]客户端[%v]发送请求分配游戏服务器数据包解析错误[%v] 將關閉客戶端連接conn:", clt, err, clt)).Error("HallServer壓測時")
		ctx.Error("客户端%v发送请求分配游戏服务器数据包解析错误,%v", clt, err)

		ctx.Server.CloseConn(clt)
		return nil
	}

	flag := mm.GetServerFlag()
	gameid := mm.GetGameID()

	example.invalid("收到客户端%v请求分配服务器%s", clt, flag)

	retmsg := new(msg.Message)
	retmsg.BClassID = int32(netproto.MessageBClassID_Hall)
	retmsg.SClassID = int32(netproto.HallMsgClassID_GameServerAddrInfoID)

	svraddr := alloch.svrProvider.GetGameServerAddrByFlag(gameid, flag)

	if svraddr == nil {
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[AllocGameServerHandler]客户端%v请求分配服务器%d_%s,未获取到服务器信息", clt, gameid, flag)).Error("HallServer")
		example.invalid("客户端%v请求分配服务器%d_%s,未获取到服务器信息", clt, gameid, flag)
		return nil
	}
	retmsg.MsgData = svraddr

	example.invalid("为客户端%v分配服务器%v", clt, svraddr)

	return []*msg.Message{retmsg}
}
