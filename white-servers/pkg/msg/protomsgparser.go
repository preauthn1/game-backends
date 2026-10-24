package msg

import (
	"errors"

	"example.invalid/golang/protobuf/proto"

	"example.invalid/ServerTeam/Servers/pkg/mlog"
	"example.invalid/ServerTeam/Servers/pkg/util"
)

//基于protobuf的消息解析器
//消息内容兼容[]byte和proto.Message
type ProtoMsgParser struct {
	*ByteMsgParser
}

//创建基于protobuf的消息解析器
func NewProtoMsgParser() *ProtoMsgParser {
	pm := new(ProtoMsgParser)

	return pm
}

func (mp *ProtoMsgParser) Marshal(msg *Message) ([]byte, error) {
	b1 := util.IntToBytes(int32(msg.BClassID), true)
	b2 := util.IntToBytes(int32(msg.SClassID), true)

	var b3 []byte
	if msg.MsgData != nil {
		var err error

		if bm, ok := msg.MsgData.([]byte); ok {
			b3 = bm
		} else {
			d, ok := msg.MsgData.(proto.Message)
			if !ok {
				mlog.Error("序列化消息产生错误,消息数据类型不正确")
				return nil, errors.New("数据类型必须为proto.Message")
			}

			b3, err = proto.Marshal(d)
			if err != nil {
				mlog.NewDefaultLogger().SetCallDepLen(8).Error("序列化消息产生错误,%v", err)
				return nil, err
			}
		}
	}

	return append(append(b1, b2...), b3...), nil
}
