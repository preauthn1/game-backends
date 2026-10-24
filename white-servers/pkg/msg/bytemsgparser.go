package msg

import (
	"errors"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

//基于[]byte消息内容的消息解析器
type ByteMsgParser struct {
}

func (bmp *ByteMsgParser) Marshal(msg *Message) ([]byte, error) {
	b1 := util.IntToBytes(int32(msg.BClassID), true)
	b2 := util.IntToBytes(int32(msg.SClassID), true)

	b3, ok := msg.MsgData.([]byte)

	if !ok {
		return nil, errors.New("数据类型必须为[]byte")
	}

	return append(append(b1, b2...), b3...), nil
}

func (bmp *ByteMsgParser) UnMarshal(data []byte) (*Message, error) {
	msg := new(Message)
	msg.BClassID = util.BytesToInt(data[:4], true)
	msg.SClassID = util.BytesToInt(data[4:8], true)

	msg.MsgData = data[8:]

	return msg, nil
}
