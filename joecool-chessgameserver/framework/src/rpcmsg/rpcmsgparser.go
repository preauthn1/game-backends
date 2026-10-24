package rpcmsg

import (
	"errors"

	proto "example.invalid/golang/protobuf/proto"
)

import (
	"msg"
	"netproto"
	"util"
)

type CreateProtoMsgFun func() proto.Message

//远程消息解析器
type RPCMessageParser struct {
}

func NewRPCMessageParser() *RPCMessageParser {
	p := new(RPCMessageParser)

	return p
}

func (rp *RPCMessageParser) Marshal(msg *msg.Message) ([]byte, error) {
	//一级协议号
	b1 := util.IntToBytes(int32(msg.BClassID), true)
	//二级协议号
	b2 := util.IntToBytes(int32(msg.SClassID), true)

	//消息转换为rpcmsg
	rpcmsg, ok := msg.MsgData.(*RPCMessage)
	if !ok {
		return nil, errors.New("消息内容不正确,必须为*RPCMessage类型")
	}

	rpcdata, err := proto.Marshal(rpcmsg.RPCInfo)
	if err != nil {
		return nil, err
	}

	var msgdata []byte

	if ptmdd, ok := rpcmsg.MsgData.(proto.Message); ok {
		msgdata, err = proto.Marshal(ptmdd)

		if err != nil {
			return nil, err
		}
	}

	if btmdd, ok := rpcmsg.MsgData.([]byte); ok {
		msgdata = btmdd
	}

	//	if msgdata == nil {
	//		return nil, errors.New("数据格式不正确.")
	//	}

	//消息头
	bh := append(b1, b2...)
	//rpc 消息头
	rpclen := int32(len(rpcdata))
	ret := append(bh, util.IntToBytes(rpclen, true)...)
	ret = append(ret, rpcdata...)
	if msgdata != nil {
		ret = append(ret, msgdata...)
	}

	return ret, nil
}

func (rp *RPCMessageParser) UnMarshal(data []byte) (*msg.Message, error) {
	m1 := new(msg.Message)
	m1.BClassID = util.BytesToInt(data[:4], true)
	m1.SClassID = util.BytesToInt(data[4:8], true)

	msgdata := data[8:]
	rpcdatalen := util.BytesToInt(msgdata[:4], true)

	if rpcdatalen+4 > int32(len(msgdata)) {
		return nil, errors.New("解析消息错误,rpc数据头长度不正确")
	}

	rpcdata := msgdata[4 : 4+rpcdatalen]

	rpcinfo := &netproto.RPCInfo{}
	err := proto.Unmarshal(rpcdata, rpcinfo)

	if err != nil {
		return nil, err
	}

	rpcmsg := new(RPCMessage)
	rpcmsg.RPCInfo = rpcinfo
	rpcmsg.MsgData = msgdata[4+rpcdatalen:]

	m1.MsgData = rpcmsg

	return m1, nil

}
