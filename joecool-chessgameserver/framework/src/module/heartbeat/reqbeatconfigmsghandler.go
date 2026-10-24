package heartbeat

import (
	"module"
	"msg"
	"netproto"
	"network"
)
import proto "example.invalid/golang/protobuf/proto"

const (
	HeartBeat_BeatInterval     int32 = 3  //心跳包时间间隔
	HeartBeat_ConnLoseTimeSpan int32 = 10 //心跳超时时间
)

//请求心跳包配置消息处理器
type ReqBeatConfigMsgHandler struct {
}

//创建请求心跳包配置处理器
func NewReqBeatConfigMsgHandler() *ReqBeatConfigMsgHandler {
	reqh := new(ReqBeatConfigMsgHandler)

	return reqh
}

func (bh *ReqBeatConfigMsgHandler) OnMessage(ctx *module.MouduleCtx, clt *network.ConnInfo, m *msg.Message) []*msg.Message {
	example.invalid("接收到请求心跳配置.")

	bcmsg := new(msg.Message)
	bcmsg.BClassID = int32(netproto.MessageBClassID_Common)
	bcmsg.SClassID = int32(netproto.PlatformCommonClassID_HeartBeatConfigID)

	bcmsgData := new(netproto.HeartBeatConfig)
	bcmsgData.BeatInterval = proto.Int32(HeartBeat_BeatInterval)
	bcmsgData.ConnLoseTimeSpan = proto.Int32(HeartBeat_ConnLoseTimeSpan)
	bcmsg.MsgData = bcmsgData

	return []*msg.Message{bcmsg}
}

func (bh *ReqBeatConfigMsgHandler) IsHook(bclsid int32, sclsid int32) bool {
	return bclsid == int32(netproto.MessageBClassID_Common) && sclsid == int32(netproto.PlatformCommonClassID_RequestHeartBeatConfigID)
}
