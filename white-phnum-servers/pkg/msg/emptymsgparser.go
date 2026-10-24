package msg

type EmptyMsgParser struct {
}

func (parser *EmptyMsgParser) Marshal(msg *Message) ([]byte, error) {
	return nil, nil
}

func (parser *EmptyMsgParser) UnMarshal(data []byte) (*Message, error) {
	return nil, nil
}
