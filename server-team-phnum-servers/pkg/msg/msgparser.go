package msg

//消息解析接口,消息对象和二进制数据之间的转换
type MsgParser interface {
	Marshal(msg *Message) ([]byte, error)    //序列化消息
	UnMarshal(data []byte) (*Message, error) //反序列化消息
}

//消息解析器信息
type MsgParserInfo struct {
	MsgParser MsgParser
	BClassID  int32
	SClassID  int32
}

// 创建消息解析器信息对象
func NewMsgParserInfo(bclsid int32, sclsid int32, parser MsgParser) *MsgParserInfo {
	mpi := new(MsgParserInfo)
	mpi.BClassID = bclsid
	mpi.SClassID = sclsid
	mpi.MsgParser = parser

	return mpi
}
