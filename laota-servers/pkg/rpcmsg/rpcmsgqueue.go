package rpcmsg

import (
	"time"

	"example.invalid/ServerTeam/Servers/pkg/command"
	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/msg"
	"example.invalid/ServerTeam/Servers/pkg/msgserver"
	"example.invalid/ServerTeam/Servers/pkg/netproto"
	"example.invalid/ServerTeam/Servers/pkg/network"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//远程消息队列
type RPCMsgQueue struct {
	tcpMsgClient *msgserver.TcpMsgClient
	innerQueue   *MsgQueue
	evtchan      *msgserver.MsgEvtChan
	stopSig      chan bool
	rpcmsgParser msg.MsgParser
	OnRPCMesasge chan *msg.Message
}

//创建远程消息队列
func NewRPCMsgQueue() *RPCMsgQueue {

	msc := new(RPCMsgQueue)
	msc.rpcmsgParser = new(RPCMessageParser)

	evtchan := msgserver.NewMsgEvtChan(100)

	msc.tcpMsgClient = msgserver.NewTcpMsgClient(network.NewHeadMsgPacker(1024*500, true), evtchan, msc.rpcmsgParser, true)
	msc.innerQueue = NewMsgQueue(200000, 100000, 60, 0)
	msc.OnRPCMesasge = make(chan *msg.Message, 100)
	msc.evtchan = evtchan

	msc.stopSig = make(chan bool, 1)

	//添加rpcmsgquque 命令
	command.AddCommand(NewRPCCommand(msc))

	return msc
}

//连接远程地址
func (rq *RPCMsgQueue) Connect(addr string) error {
	err := rq.tcpMsgClient.Connect(addr)

	if err != nil {
		return err
	}

	go func() {
		defer util.PanicErrStack()

		for {
			select {
			case <-rq.stopSig:
				break
			default:
			}

			select {
			case clt := <-rq.evtchan.ConnectedSig:
				mlog.System("连接远程RPC服务器成功,%v", clt)
			case clt := <-rq.evtchan.DisconnectedSig:
				mlog.System("远程RPC服务器连接断开,%v", clt)
			case <-rq.evtchan.DataSig:

			case msg := <-rq.evtchan.MsgSig:
				//example.invalid("收到远程RPC服务器消息,%v", msg.MsgData)

				rpcmsg := msg.MsgData.(*RPCMessage)
				//通知消息处理结果
				rq.innerQueue.DealMsg(int(rpcmsg.RPCInfo.GetQueueID()), MsgSendSuccess)

				//通知回发消息
				rq.OnRPCMesasge <- msg
			}
		}
	}()

	return nil
}

//启动
func (rq *RPCMsgQueue) Start() {
	rq.innerQueue.Start()

	go func() {
		defer util.PanicErrStack()

		for {
			select {
			case <-rq.stopSig:
				break
			default:

			}

			//检查远程连接状态
			for {
				if rq.tcpMsgClient.IsConnected() {
					break
				} else {
					time.Sleep(time.Duration(100) * time.Millisecond)
				}
			}

			qmsg := <-rq.innerQueue.OnMessageSig
			if rq.tcpMsgClient.IsConnected() {

				//设置队列ID
				rpcmsg := qmsg.Message.MsgData.(*RPCMessage)
				qmsgid := int32(qmsg.MsgID)
				rpcmsg.RPCInfo.QueueID = &qmsgid

				err := rq.tcpMsgClient.SendMessage(qmsg.Message)
				//example.invalid("发送消息%v到RPC服务器.消息内容%v", qmsg, qmsg.Message)

				if err != nil {
					mlog.Error("发送消息%v到RPC服务器错误%v", qmsg.Message, err)
				}

			}
		}
	}()
}

//停止
func (rq *RPCMsgQueue) Stop() {
	rq.innerQueue.Stop()

	rq.stopSig <- true
}

//添加消息到达送队列
func (rq *RPCMsgQueue) PushMessage(rpcinfo *netproto.RPCInfo, safe bool, msg *msg.Message) error {
	rpcmsg := new(RPCMessage)
	rpcmsg.RPCInfo = rpcinfo
	rpcmsg.MsgData = msg.MsgData

	msg.MsgData = rpcmsg

	rq.innerQueue.PushMsg(msg, safe)

	return nil
}

func (rq *RPCMsgQueue) IsConnected() bool {
	return rq.tcpMsgClient.IsConnected()
}
