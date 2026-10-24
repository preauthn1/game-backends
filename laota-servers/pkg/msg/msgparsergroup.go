package msg

import (
	"errors"
	"fmt"

	"example.invalid/ServerTeam/Servers/pkg/util"
)

const (
	AllMsgBClassID = -1
	AllMsgSClassID = -1
)

//消息解析服务器组
type MsgParserGroup struct {
	innerParsers map[string]MsgParser
}

//创建消息解析器组对象
func NewMsgParserGroup() *MsgParserGroup {
	svr := new(MsgParserGroup)
	svr.innerParsers = make(map[string]MsgParser)

	return svr
}

func getParserKey(bClsID int32, sClsID int32) string {
	return fmt.Sprintf("%d_%d", bClsID, sClsID)
}

//注册解析器
func (server *MsgParserGroup) RegisterParser(bClsID int32, sClsID int32, parser MsgParser) error {
	key := getParserKey(bClsID, sClsID)
	if _, ok := server.innerParsers[key]; ok {
		return errors.New("消息解析器已经存在")
	}

	server.innerParsers[key] = parser
	return nil
}

//序列化消息
func (server *MsgParserGroup) Marshal(msg *Message) ([]byte, error) {
	//使用特定解析方式
	key := getParserKey(msg.BClassID, msg.SClassID)

	if parser, ok := server.innerParsers[key]; ok {
		return parser.Marshal(msg)
	}

	//使用通用解析方式
	allkey := getParserKey(AllMsgBClassID, AllMsgSClassID)

	if allparser, ok := server.innerParsers[allkey]; ok {
		return allparser.Marshal(msg)
	}

	return nil, errors.New("消息解析器不存在")
}

func (server *MsgParserGroup) UnMarshal(data []byte) (*Message, error) {
	if len(data) < 8 {
		return nil, errors.New("消息不正确")
	}

	//使用特定的解析方式
	//fmt.Println(data)
	bclsID := util.BytesToInt(data[:4], true)
	cclsID := util.BytesToInt(data[4:8], true)

	key := getParserKey(bclsID, cclsID)

	if parser, ok := server.innerParsers[key]; ok {
		return parser.UnMarshal(data)
	}

	//使用通用解析方式
	allkey := getParserKey(AllMsgBClassID, AllMsgSClassID)

	if allparser, ok := server.innerParsers[allkey]; ok {
		return allparser.UnMarshal(data)
	}

	errStr := fmt.Sprintf("消息解析器不存在 bclsID[%v] cclsID[%v]", bclsID, cclsID)
	return nil, errors.New(errStr)
}
