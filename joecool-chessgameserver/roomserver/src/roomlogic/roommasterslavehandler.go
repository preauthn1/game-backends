package roomlogic

import (
	"gamelogic"
	"example.invalid/golang/protobuf/proto"
	"mlog"
	"module"
	"msg"
	"msgserver"
	"netproto"
	"player"
	"roomutil"
	"rpcmsg"
	"time"
	"util"
)

//用 new *gamelogic.Room 透過RPC 更新 master slave, 參考NewNotifyRefreshVerHandler

type RoomMasterSlaveHandler struct {
	room        *RoomLogic
	rpcmsgqueue *rpcmsg.RPCMsgQueue
	gameid      int32
	serverid    int32
}

func NewRoomMasterSlaveHandler(room *RoomLogic, rpcmsgqueue *rpcmsg.RPCMsgQueue, gameid int32, serverid int32) *RoomMasterSlaveHandler {
	roomHandler := new(RoomMasterSlaveHandler)
	roomHandler.room = room
	roomHandler.rpcmsgqueue = rpcmsgqueue
	roomHandler.gameid = gameid
	roomHandler.serverid = serverid

	return roomHandler
}

func (roomHandler *RoomMasterSlaveHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_NotifyServer) && sclsid == int32(netproto.NotifyServerClassID_ServerHostAndBackUp)
}

func (roomHandler *RoomMasterSlaveHandler) OnNotifyMessage(m *msg.Message, ctx *module.MouduleCtx, playerMang *player.PlayerManager) {
	defer util.PanicErrStack()

	mmbyte := m.MsgData.([]byte)
	mm := &netproto.HostAndBackUp{}
	err := proto.Unmarshal(mmbyte, mm)

	if err != nil {
		mlog.Error("解析錯誤 HostAndBackUp")
	}

	if roomHandler.room.room.ServerID == *mm.Master.ServerID &&
		roomHandler.room.room.GameInfo.GameID == *mm.Master.GameID {
		roomHandler.room.UpdateHostAndBackUp(HostType_Master, "")
	} else {
		roomHandler.room.UpdateHostAndBackUp(HostType_Slave, mm.Slave.GetGameServerAddr())
	}

	roomHandler.room.regHallHandler.RegHallServer()

	roomHandler.stopGameServer(mm)
}

func (roomHandler *RoomMasterSlaveHandler) stopGameServer(mm *netproto.HostAndBackUp) {
	freeusers := make([]*player.Player, 0)
	roomHandler.room.gameServer.GameServerRunCtx.PlayerManager.Range(func(pr *player.Player) {
		if pr.IsGuest() {
			freeusers = append(freeusers, pr)
		} else {
			ur := pr.UserData.(*gamelogic.GameUser)
			if ur.IsFree() || ur.IsSit() || ur.IsReady() || ur.IsQueue() {
				freeusers = append(freeusers, pr)
			}
		}
	})
	for _, pr := range freeusers {
		kickUser(roomHandler.room.gameServer.GameServerRunCtx.MsgServer, mm.Slave.GetGameServerAddr(), pr.Conn.ConnID())
	}
}

func kickUser(ctx *msgserver.MsgServer, str string, connid string) {
	example.invalid("停服检查逻辑,%s 断开客户端%s", str, connid)

	var m *msg.Message
	if str != "" && str != "-" {
		m, _ = roomutil.GetTipMessageWithParam(str, roomutil.MsgTypeID_RoomKick, 6)
	}else {
		m, _ = roomutil.GetTipMessageWithParam("-", roomutil.MsgTypeID_RoomKick, 4)
	}


	ctx.SendMessageByConnID(connid, m)

	go func() {
		time.Sleep(time.Duration(1) * time.Second)

		ctx.CloseConnByConnID(connid)
	}()
}
