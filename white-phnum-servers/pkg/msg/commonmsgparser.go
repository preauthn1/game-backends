package msg

//公用的消息解析器
type CommonMsgParser struct {
	mf  func(msg *Message) ([]byte, error)
	umf func(data []byte) (*Message, error)
}

func (cmp *CommonMsgParser) Marshal(msg *Message) ([]byte, error) {
	if cmp.mf != nil {
		return cmp.mf(msg)
	}

	return nil, nil
}

func (cmp *CommonMsgParser) UnMarshal(data []byte) (*Message, error) {
	if cmp.umf != nil {
		return cmp.umf(data)
	}

	return nil, nil
}

//创建单向输入解析的消息解析器
func NewInputMsgParser(umf func(data []byte) (*Message, error)) *CommonMsgParser {
	cmp := new(CommonMsgParser)
	cmp.mf = nil
	cmp.umf = umf

	return cmp
}

//创建单向输出解析的消息解析器
func NewOutputMsgParser(mf func(msg *Message) ([]byte, error)) *CommonMsgParser {
	cmp := new(CommonMsgParser)
	cmp.mf = mf
	cmp.umf = nil

	return cmp
}

//创建消息解析器
func NewCommonMsgParser(mf func(msg *Message) ([]byte, error), umf func(data []byte) (*Message, error)) *CommonMsgParser {
	cmp := new(CommonMsgParser)
	cmp.mf = mf
	cmp.umf = umf

	return cmp
}
