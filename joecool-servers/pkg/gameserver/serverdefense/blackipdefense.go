package serverdefense

import (
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/network"
)

//IP黑名单防护
type BlackIPDefense struct {
	*DefenserBase
}

//创建IP黑名单防护
func NewBlackIPDefense() *BlackIPDefense {
	bd := new(BlackIPDefense)
	bd.DefenserBase = NewDefenserBase()

	return bd
}

//客户端连接
func (dd *BlackIPDefense) OnClientConnect(clt *network.ConnInfo) {
	if dd.BlackIPList.CheckIP(clt.IPAddress) && !dd.checkWhiteIPList(clt.IPAddress) {
		example.invalid("客户端%v连接,但被IP黑名单断开", clt)
		dd.CloseConn(clt)
	}
}

//客户端断开
func (dd *BlackIPDefense) OnClientDisconnect(clt *network.ConnInfo) {

}

//收到数据
func (dd *BlackIPDefense) OnClientData(data *network.ConnData) {

}

//收到消息
func (dd *BlackIPDefense) OnClientMessage(clt *network.ConnInfo, msg *msg.Message) {
	if dd.BlackIPList.CheckIP(clt.IPAddress) && !dd.checkWhiteIPList(clt.IPAddress) {
		example.invalid("客户端%v发送消息,但被IP黑名单断开", clt)
		dd.CloseConn(clt)
	}
}
