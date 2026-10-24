package msghandler

import (
	"example.invalid/ServerTeam/Servers/internal/hallserver/halllogic/hallutil"
	"example.invalid/ServerTeam/Servers/pkg/module"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

import (
	"fmt"
	proto "example.invalid/golang/protobuf/proto"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
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
		mlog.NewDefaultLogger().SetData(fmt.Sprintf("[AllocGameServerHandler]客户端[%v]发送请求分配游戏服务器数据包解析错误[%v] 將關閉客戶端連接conn:%v", clt, err, clt)).Error("HallServer壓測時")
		ctx.Error("客户端%v发送请求分配游戏服务器数据包解析错误,%v", clt, err)

		ctx.Server.CloseConn(clt)
		return nil
	}

	//TODO client改為直連遊戲port, 不再透過大廳二次取得遊戲port 位置

	return []*msg.Message{nil}
}
