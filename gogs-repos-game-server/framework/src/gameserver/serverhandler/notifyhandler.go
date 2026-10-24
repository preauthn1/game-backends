package serverhandler

import (
	"conf"
	"mlog"
	"msg"
	"msgserver"
	"netproto"
	"network"
	"util"
)

import (
	"event"
	proto "example.invalid/golang/protobuf/proto"
)

const (
	TimerName_NotifyReg = "gameserver_serverhandler_notifyreg"
)

//通知协调处理器
type NotifyHandler struct {
	nsAddr         string
	TcpMsgClient   *msgserver.TcpMsgClient
	evtchan        *msgserver.MsgEvtChan
	stopsig        chan bool
	serverID       int
	serverType     int
	gameid         int
	HostAndBackup  int32
	cgx            *GameServerCtx
	gameServerAddr string
	FlagID         string
	nfRegTicker    *event.TimerFunc
}

//创建通知协调处理器
func NewNotifyHandler(nsaddr string, serverid int, servertype int, gameid int, hostAndBackup int32, gameserverAddr string, flagId string) *NotifyHandler {
	ntfhandler := new(NotifyHandler)
	ntfhandler.nsAddr = nsaddr
	ntfhandler.stopsig = make(chan bool, 1)
	ntfhandler.serverID = serverid
	ntfhandler.serverType = servertype
	ntfhandler.gameid = gameid
	ntfhandler.HostAndBackup = hostAndBackup
	ntfhandler.gameServerAddr = gameserverAddr
	ntfhandler.FlagID = flagId

	return ntfhandler
}

func (nshandler *NotifyHandler) Start(cgx *GameServerCtx) {
	//创建到NsServer的TCP连接
	msgparser := msg.NewProtoMsgParser()
	msgpacker := network.NewHeadMsgPacker(conf.MSGPACKERBUF, true)

	nshandler.evtchan = msgserver.NewMsgEvtChan(100)
	nshandler.TcpMsgClient = msgserver.NewTcpMsgClient(msgpacker, nshandler.evtchan, msgparser, true)
	nshandler.cgx = cgx

	go nshandler.tcpEventChanFunc(nshandler.evtchan)

	err := nshandler.TcpMsgClient.Connect(nshandler.nsAddr)

	if err != nil {
		mlog.Error("向协调服务器%s注册失败%v", nshandler.nsAddr, err)
		return
	}

	//nshandler.nfRegTicker = cgx.TimerFuncManager.NewNamedTicker(30, nshandler.sendMsgNotify, TimerName_NotifyReg)
}

func (nshandler *NotifyHandler) sendMsgNotify() {
	data := []byte("test")
	nshandler.TcpMsgClient.SendData(data)
}

func (nshandler *NotifyHandler) tcpEventChanFunc(tcpevt *msgserver.MsgEvtChan) {

	defer util.PanicErrStack()

	for {
		select {
		case <-nshandler.stopsig:
			break
		default:
		}

		select {
		case clt := <-tcpevt.ConnectedSig:
			example.invalid("连接协调服务器成功,%s", clt.GetConn().RemoteAddr().String())

			//发送服务器信息
			nsregmsg := new(msg.Message)
			nsregmsg.BClassID = int32(netproto.MessageBClassID_NotifyServer)
			nsregmsg.SClassID = int32(netproto.NotifyServerClassID_ServerRegID)

			nsregdata := new(netproto.NotifyServerRegInfo)
			nsregmsg.MsgData = nsregdata
			nsregdata.ServerID = proto.Int32(int32(nshandler.serverID))
			nsregdata.ServerType = proto.Int32(int32(nshandler.serverType))
			nsregdata.HostAndBackup = proto.Int32(nshandler.HostAndBackup)
			nsregdata.GameServerAddr = proto.String(nshandler.gameServerAddr)
			nsregdata.FlagID = proto.String(nshandler.FlagID)

			if nshandler.gameid > 0 && nshandler.serverType == conf.ServerType_GameServer {
				nsregdata.GameID = proto.Int32(int32(nshandler.gameid))
			}

			nshandler.TcpMsgClient.SendMessage(nsregmsg)

		case clt := <-tcpevt.DisconnectedSig:
			mlog.Warn("协调服务器连接断开,%v", clt.GetConn().RemoteAddr().String())
		case <-tcpevt.DataSig:

		case msg := <-tcpevt.MsgSig:
			mlog.Debug("收到协调服务器消息:%v", msg)

			//分发通知事件
			notifymsgevt := msgserver.NewNotifyMessageEvent(msg)
			nshandler.cgx.MsgServer.EventChannel.OnAsyncEvent(notifymsgevt, nil)
		}
	}

	mlog.Warn("notifyserver 消息处理线程退出")
}

func (nshandler *NotifyHandler) Stop() {
	nshandler.stopsig <- true
}

func (nshandler *NotifyHandler) Name() string {
	return "notify"
}
