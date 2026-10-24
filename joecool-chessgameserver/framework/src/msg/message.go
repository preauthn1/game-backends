package msg

import (
	"fmt"
)

//网络消息对象定义
type Message struct {
	BClassID int32       //一级协议号
	SClassID int32       //二级协议号
	MsgData  interface{} //消息数据
}

func (msg *Message) String() string {
	return fmt.Sprintf("%d:%d", msg.BClassID, msg.SClassID)
}

//创建消息
func NewMessage(bclsID int32, sclsID int32, data interface{}) *Message {
	msg := new(Message)
	msg.BClassID = bclsID
	msg.SClassID = sclsID
	msg.MsgData = data

	return msg
}

//消息ID
type MessageID struct {
	BClassID int32
	SClassID int32
}

//转发消息ID
type RPCMessageID struct {
	MsgID     *MessageID //消息ID
	NeedLogon bool       //是否需要登录
}

//创建RPCMessageID 对象
func NewRPCMessageID(bclsid int32, sclsid int32, needlogon bool) *RPCMessageID {
	rpcmsgid := new(RPCMessageID)
	rpcmsgid.MsgID = new(MessageID)
	rpcmsgid.MsgID.BClassID = bclsid
	rpcmsgid.MsgID.SClassID = sclsid
	rpcmsgid.NeedLogon = needlogon

	return rpcmsgid
}

//消息ID是否一致
func (msgid *MessageID) Equals(othermsgid *MessageID) bool {
	return msgid.BClassID == othermsgid.BClassID && msgid.SClassID == othermsgid.SClassID
}
